import os
import torch
import warnings
import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.metrics import f1_score, accuracy_score
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

# Uyarıları filtrele
warnings.filterwarnings("ignore", message="Could not find image processor class")
warnings.filterwarnings("ignore", category=UserWarning)

# Dataset sınıfı
class ComplaintDataset(Dataset):
    def __init__(self, encodings, labels):
        self.encodings = encodings
        self.labels = labels

    def __getitem__(self, idx):
        item = {key: torch.tensor(val[idx]) for key, val in self.encodings.items()}
        item["labels"] = torch.tensor(self.labels[idx], dtype=torch.float)
        return item

    def __len__(self):
        return len(self.labels)

# Metrikler
def compute_metrics(pred):
    sigmoid = torch.nn.Sigmoid()
    probs = sigmoid(torch.tensor(pred.predictions))
    y_pred = np.where(probs >= 0.5, 1, 0)
    y_true = pred.label_ids

    return {
        "f1_micro": f1_score(y_true, y_pred, average="micro"),
        "f1_macro": f1_score(y_true, y_pred, average="macro"),
        "accuracy": accuracy_score(y_true, y_pred)
    }

# Ana eğitim fonksiyonu
def train_model():
    # Veri yükleme
    df = pd.read_csv("data/complaints.csv")
    df = df.dropna(subset=["şikayet_metni", "kategori"])

    # Çoklu etiketleri ayır
    df["etiketler"] = df["kategori"].apply(lambda x: [label.strip() for label in x.split(",")])
    
    texts = df["şikayet_metni"].tolist()
    multilabels = df["etiketler"].tolist()

    # Label'ları dönüştür
    mlb = MultiLabelBinarizer()
    binary_labels = mlb.fit_transform(multilabels)
    label_list = mlb.classes_

    # Eğitim/Doğrulama bölme
    train_texts, val_texts, train_labels, val_labels = train_test_split(
        texts, binary_labels,
        test_size=0.15,
        random_state=42
    )

    # Tokenizer
    tokenizer = AutoTokenizer.from_pretrained("dbmdz/bert-base-turkish-128k-cased")

    # Tokenizasyon
    train_encodings = tokenizer(train_texts, truncation=True, padding=True, max_length=128)
    val_encodings = tokenizer(val_texts, truncation=True, padding=True, max_length=128)

    # Dataset
    train_dataset = ComplaintDataset(train_encodings, train_labels)
    val_dataset = ComplaintDataset(val_encodings, val_labels)

    # Config
    config = AutoConfig.from_pretrained(
        "dbmdz/bert-base-turkish-128k-cased",
        num_labels=len(label_list),
        problem_type="multi_label_classification"
    )

    # Model
    model = AutoModelForSequenceClassification.from_pretrained(
        "dbmdz/bert-base-turkish-128k-cased",
        config=config
    )

    # Eğitim ayarları
    training_args = TrainingArguments(
        output_dir="./model_output",
        num_train_epochs=8,
        per_device_train_batch_size=16,
        per_device_eval_batch_size=32,
        learning_rate=2e-5,
        weight_decay=0.01,
        warmup_ratio=0.06,
        eval_strategy="epoch",
        save_strategy="epoch",
        load_best_model_at_end=True,
        metric_for_best_model="f1_macro",
        greater_is_better=True,
        fp16=torch.cuda.is_available(),
        logging_dir="./logs",
        logging_steps=30,
        report_to="tensorboard",
        save_total_limit=2,
        seed=42
    )

    # Trainer
    trainer = Trainer(
        model=model,
        args=training_args,
        train_dataset=train_dataset,
        eval_dataset=val_dataset,
        tokenizer=tokenizer,
        compute_metrics=compute_metrics,
        callbacks=[EarlyStoppingCallback(early_stopping_patience=2)]
    )

    # Eğitimi başlat
    print("BERTurk model çoklu etiket eğitimi başlıyor...")
    trainer.train()

    # Kaydet
    output_dir = "./trained_model"
    trainer.save_model(output_dir)
    tokenizer.save_pretrained(output_dir)

    # Değerlendirme
    eval_results = trainer.evaluate()
    print("\n=== Eğitim Sonuçları ===")
    print(f"Accuracy: {eval_results['eval_accuracy']:.4f}")
    print(f"F1 Micro: {eval_results['eval_f1_micro']:.4f}")
    print(f"F1 Macro: {eval_results['eval_f1_macro']:.4f}")
    print(f"Loss: {eval_results['eval_loss']:.4f}")

    return output_dir

if __name__ == "__main__":
    torch.cuda.empty_cache()
    torch.backends.cudnn.benchmark = True
    model_path = train_model()
    print(f"\nModel başarıyla kaydedildi: {model_path}")
