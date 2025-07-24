import os
import torch
import warnings
import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.metrics import f1_score, accuracy_score, classification_report
from sklearn.preprocessing import MultiLabelBinarizer
from torch.utils.data import Dataset
from transformers import (
    AutoTokenizer,
    AutoModelForSequenceClassification,
    Trainer,
    TrainingArguments,
    AutoConfig,
    EarlyStoppingCallback
)
from torch.nn import BCEWithLogitsLoss

# Uyarıları filtrele
warnings.filterwarnings("ignore")

# Sabitler
MAX_LENGTH = 256
BATCH_SIZE = 16 if torch.cuda.is_available() else 8
LEARNING_RATE = 3e-5
NUM_EPOCHS = 10

# 1. Özelleştirilmiş Dataset Sınıfı
class MultiLabelComplaintDataset(Dataset):
    def __init__(self, encodings, labels):
        self.encodings = encodings
        self.labels = labels

    def __getitem__(self, idx):
        item = {key: torch.tensor(val[idx]) for key, val in self.encodings.items()}
        item["labels"] = torch.tensor(self.labels[idx], dtype=torch.float)
        return item

    def __len__(self):
        return len(self.labels)

# 2. Metrik Hesaplama Fonksiyonu
def compute_metrics(pred):
    sigmoid = torch.nn.Sigmoid()
    probs = sigmoid(torch.tensor(pred.predictions))
    y_pred = np.zeros(probs.shape)
    y_pred[np.where(probs >= 0.5)] = 1
    y_true = pred.label_ids
    
    return {
        "accuracy": accuracy_score(y_true, y_pred),
        "f1_micro": f1_score(y_true, y_pred, average="micro"),
        "f1_macro": f1_score(y_true, y_pred, average="macro"),
        "f1_weighted": f1_score(y_true, y_pred, average="weighted")
    }

# 3. Ana Eğitim Fonksiyonu
def train_multi_label_model():
    # 1. Veri yükleme ve ön işleme
    try:
        df = pd.read_csv("data/complaints.csv")
        df = df.dropna(subset=["şikayet_metni", "kategori"])
    except Exception as e:
        print(f"Veri yükleme hatası: {e}")
        return None

    # 2. Çoklu etiketleri ayır
    df["etiketler"] = df["kategori"].apply(lambda x: [label.strip() for label in x.split(",") if label.strip()])
    
    # 3. Etiketleri binary formata çevir
    mlb = MultiLabelBinarizer()
    binary_labels = mlb.fit_transform(df["etiketler"])
    label_list = mlb.classes_
    
    print(f"\n{'='*50}\nEtiket Dağılımı:\n{pd.Series(df['etiketler'].explode()).value_counts()}\n{'='*50}")
    print(f"Toplam {len(label_list)} etiket: {label_list}")

    # 4. Veriyi böl (stratify çoklu etiketlerde zor olduğu için basit bölme)
    train_texts, val_texts, train_labels, val_labels = train_test_split(
        df["şikayet_metni"].tolist(), 
        binary_labels,
        test_size=0.15,
        random_state=42
    )

    # 5. Tokenizer
    tokenizer = AutoTokenizer.from_pretrained("dbmdz/bert-base-turkish-128k-cased")

    # 6. Tokenizasyon (daha verimli bellek kullanımı için batch encoding)
    def batch_encode(texts, tokenizer, max_length=MAX_LENGTH):
        return tokenizer(
            texts,
            truncation=True,
            padding=True,
            max_length=max_length,
            return_tensors="pt"
        )

    train_encodings = batch_encode(train_texts, tokenizer)
    val_encodings = batch_encode(val_texts, tokenizer)

    # 7. Dataset oluşturma
    train_dataset = MultiLabelComplaintDataset(train_encodings, train_labels)
    val_dataset = MultiLabelComplaintDataset(val_encodings, val_labels)

    # 8. Model config
    config = AutoConfig.from_pretrained(
        "dbmdz/bert-base-turkish-128k-cased",
        num_labels=len(label_list),
        problem_type="multi_label_classification"
    )

    # 9. Model yükleme
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    model = AutoModelForSequenceClassification.from_pretrained(
        "dbmdz/bert-base-turkish-128k-cased",
        config=config
    ).to(device)

    # 10. Sınıf ağırlıklarını otomatik hesapla
    label_counts = binary_labels.sum(axis=0)
    pos_weights = (len(binary_labels) - label_counts) / (label_counts + 1e-5)  # Sıfır bölme hatası önleme
    pos_weights = torch.tensor(pos_weights, dtype=torch.float32).to(device)
    model.loss_fct = BCEWithLogitsLoss(pos_weight=pos_weights)

    # 11. Eğitim parametreleri
    training_args = TrainingArguments(
        output_dir="./model_output",
        num_train_epochs=NUM_EPOCHS,
        per_device_train_batch_size=BATCH_SIZE,
        per_device_eval_batch_size=BATCH_SIZE*2,
        learning_rate=LEARNING_RATE,
        weight_decay=0.01,
        warmup_ratio=0.1,
        eval_strategy="epoch",
        save_strategy="epoch",
        load_best_model_at_end=True,
        metric_for_best_model="f1_macro",
        greater_is_better=True,
        fp16=torch.cuda.is_available(),
        logging_dir="./logs",
        logging_steps=50,
        report_to="tensorboard",
        save_total_limit=2,
        seed=42
    )

    # 12. Trainer
    trainer = Trainer(
        model=model,
        args=training_args,
        train_dataset=train_dataset,
        eval_dataset=val_dataset,
        tokenizer=tokenizer,
        compute_metrics=compute_metrics,
        callbacks=[EarlyStoppingCallback(early_stopping_patience=3)]
    )

    # 13. Eğitimi başlat
    print("\nModel eğitimi başlıyor...")
    trainer.train()

    # 14. Modeli kaydet
    output_dir = "./trained_model"
    trainer.save_model(output_dir)
    tokenizer.save_pretrained(output_dir)
    print(f"\nModel başarıyla kaydedildi: {output_dir}")

    # 15. Detaylı değerlendirme
    print("\nModel Değerlendirme Sonuçları:")
    eval_results = trainer.evaluate()
    for key, value in eval_results.items():
        print(f"{key}: {value:.4f}")

        # 16. Örnek tahmin fonksiyonu
    def predict_example(text, model, tokenizer, mlb, threshold=0.4):
        inputs = tokenizer(text, return_tensors="pt", truncation=True, max_length=MAX_LENGTH).to(device)
        
        model.eval()
        with torch.no_grad():
            outputs = model(**inputs)
            probs = torch.sigmoid(outputs.logits.squeeze().cpu())
            predictions = (probs >= threshold).numpy().astype(int)
        
        # mlb.inverse_transform için uygun formata getir
        return mlb.inverse_transform(predictions.reshape(1, -1))[0]

    # Test tahminleri
    test_texts = [
        "Yollar çok kötü ve otobüsler zamanında gelmiyor",
        "Çöp konteynerleri taşmış ve etraf pis kokuyor",
        "Gece yarısı yüksek sesle müzik çalıyorlar",
        "Su kesintisi var ve musluklardan çamurlu su akıyor",
        "Parktaki banklar kırık ve çimler biçilmemiş"
    ]

    print("\nTest Tahminleri:")
    for text in test_texts:
        predicted = predict_example(text, model, tokenizer, mlb)
        print(f"\nMetin: '{text}'\nTahmin: {predicted}")

    return output_dir

if __name__ == "__main__":
    torch.cuda.empty_cache()
    torch.backends.cudnn.benchmark = True
    
    model_path = train_multi_label_model()