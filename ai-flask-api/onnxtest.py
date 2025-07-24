import onnxruntime as ort
from transformers import AutoTokenizer
import numpy as np
import json
import os

class ONNXModelTester:
    def __init__(self, model_path, tokenizer_path, config_path):
        self.session = ort.InferenceSession(model_path)
        self.tokenizer = AutoTokenizer.from_pretrained(tokenizer_path)
        
        # id2label sözlüğünü yükle
        with open(config_path, "r", encoding="utf-8") as f:
            config = json.load(f)
            self.id2label = {int(k): v for k, v in config.get("id2label", {}).items()}
        
        if not self.id2label:
            raise ValueError("Config dosyasında 'id2label' bulunamadı!")

    def predict(self, text, threshold=0.5):
        # Tokenize et
        inputs = self.tokenizer(
            text,
            padding='max_length',
            truncation=True,
            max_length=64,
            return_tensors='pt'
        )
        
        # Torch tensor → numpy (int64)
        ort_inputs = {
            'input_ids': inputs['input_ids'].numpy().astype(np.int64),
            'attention_mask': inputs['attention_mask'].numpy().astype(np.int64)
        }
        
        # Tahmin al
        outputs = self.session.run(None, ort_inputs)
        logits = outputs[0]
        
        # Sigmoid uygula
        probs = 1 / (1 + np.exp(-logits))
        
        # Threshold ile birden fazla etiket seç
        pred_ids = np.where(probs[0] >= threshold)[0]
        
        if len(pred_ids) == 0:
            return ["(Eşik altında: Etiket yok)"]
        
        results = [self.id2label[i] for i in pred_ids]
        return results

if __name__ == "__main__":
    model_path = "onnx_model/model_quant.onnx"  # Quantized model yolu
    tokenizer_path = "./trained_model"
    config_path = os.path.join(tokenizer_path, "config.json")

    tester = ONNXModelTester(model_path, tokenizer_path, config_path)

    test_texts = [
        "3 gündür su yok",
        "Yolda büyük bir çukur var",
        "Çöp konteyneri taşmış",
        "Gece yüksek sesle müzik var",
        "Parkta ağaçlar kurumaya başladı",
        "Köpekler çok havlıyor",
        "Yollar hep çukur olmuş",
        "Yollar çukurlu",
        "Belediye hizmetlerinde personel sayısı yetersiz.",
        "Şüpheli araçlar günlerce aynı yerde park ediliyor.",
        "Otobüslerdeki tutunma halkaları çok yüksek, kısa boylular yetişemiyor.",
        "Yaya bölgelerinde şarj istasyonu bulunmuyor"
    ]

    for text in test_texts:
        predictions = tester.predict(text, threshold=0.3)
        print(f"Metin: '{text}' -> Tahminler: {predictions}")
