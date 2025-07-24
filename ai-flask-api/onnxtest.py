import onnxruntime as ort
from transformers import AutoTokenizer, AutoConfig
import numpy as np
import json
import os

class ONNXModelTester:
    def __init__(self, model_path, tokenizer_path, config_path):
        self.session = ort.InferenceSession(model_path)
        self.tokenizer = AutoTokenizer.from_pretrained(tokenizer_path)
        
        # Config dosyasından id2label okuma
        with open(config_path, "r", encoding="utf-8") as f:
            config = json.load(f)
            # id2label dict int-key string-value olmalı
            self.id2label = {int(k): v for k, v in config.get("id2label", {}).items()}
        
        if not self.id2label:
            raise ValueError("Config dosyasında 'id2label' bulunamadı!")

    def predict(self, text):
        inputs = self.tokenizer(
            text,
            padding='max_length',
            truncation=True,
            max_length=64,
            return_tensors='np'
        )
        ort_inputs = {
            'input_ids': inputs['input_ids'].astype(np.int64),
            'attention_mask': inputs['attention_mask'].astype(np.int64)
        }
        outputs = self.session.run(None, ort_inputs)
        logits = outputs[0]
        pred_id = int(np.argmax(logits, axis=1)[0])
        return self.id2label.get(pred_id, "Unknown")

if __name__ == "__main__":
    model_path = "model.onnx"
    tokenizer_path = "./trained_model"
    config_path = os.path.join(tokenizer_path, "config.json")

    tester = ONNXModelTester(model_path, tokenizer_path, config_path)

    test_texts = [
        "3 gündür su yok",
        "Yolda büyük bir çukur var",
        "Çöp konteyneri taşmış",
        "Gece yüksek sesle müzik var",
        "Parkta ağaçlar kurumaya başladı",
        "köpekler çok havlıyor",
        "Yollar hep çukur olmuş",
        "Yollar çukurlu"
    ]

    for text in test_texts:
        prediction = tester.predict(text)
        print(f"Metin: '{text}' -> Tahmin: '{prediction}'")
