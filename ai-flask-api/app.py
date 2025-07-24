from flask import Flask, request, jsonify
import numpy as np
from transformers import AutoTokenizer
from onnxruntime import InferenceSession
import os
import time
import json

app = Flask(__name__)

# Model yolu
model_dir = os.path.abspath("./trained_model")
tokenizer = AutoTokenizer.from_pretrained(model_dir, local_files_only=True)

# Tokenizer ve ONNX modeli yükle
tokenizer = AutoTokenizer.from_pretrained(model_dir)
onnx_session = InferenceSession("./onnx_model/model.onnx")

# config.json'dan id2label çek
with open(os.path.join(model_dir, "config.json"), "r", encoding="utf-8") as f:
    config = json.load(f)
id2label = {int(k): v for k, v in config.get("id2label", {}).items()}

# Tahmin için eşik değeri
THRESHOLD = 0.4

@app.route('/predict', methods=['POST'])
def predict():
    try:
        data = request.json
        text = data['text']

        # Tokenize
        inputs = tokenizer(
            text,
            truncation=True,
            padding=True,
            max_length=128,
            return_tensors="np"
        )

        # ONNX girişleri
        inputs_onnx = {
            "input_ids": inputs["input_ids"].astype(np.int64),
            "attention_mask": inputs["attention_mask"].astype(np.int64)
        }

        # Tahmin süreci
        start_time = time.time()
        logits = onnx_session.run(None, inputs_onnx)[0]  # (1, label_count)

        # Sigmoid ile olasılık hesapla
        probs = 1 / (1 + np.exp(-logits))

        # Eşik üstü olan etiketleri al
        pred_indices = np.where(probs[0] >= THRESHOLD)[0]

        if len(pred_indices) == 0:
            labels = ["Eşik altında: Etiket yok"]
            confidences = []
        else:
            labels = [id2label[idx] for idx in pred_indices]
            confidences = [float(probs[0][idx]) for idx in pred_indices]

        latency = (time.time() - start_time) * 1000  # ms cinsinden

        response = {
            "labels": labels,
            "confidences": confidences,
            "latency_ms": latency,
            "status": "success"
        }
        print("Tahmin sonucu:", response)

    except Exception as e:
        response = {
            "status": "error",
            "message": str(e)
        }

    return jsonify(response)

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
