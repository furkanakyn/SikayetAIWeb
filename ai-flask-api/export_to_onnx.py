import torch
from transformers import AutoTokenizer, AutoModelForSequenceClassification
from pathlib import Path
import os
from onnxruntime.quantization import quantize_dynamic, QuantType

def convert_to_onnx(model_path, output_path="model.onnx"):
    # Model ve tokenizer yükle
    tokenizer = AutoTokenizer.from_pretrained(model_path)
    model = AutoModelForSequenceClassification.from_pretrained(model_path)
    model.eval()
    
    # Örnek giriş
    sample_text = "Çöpler toplanmadı"
    inputs = tokenizer(sample_text, return_tensors="pt")
    
    # ONNX export için gerekenler
    input_names = ["input_ids", "attention_mask"]
    output_names = ["logits"]
    dynamic_axes = {
        'input_ids': {0: 'batch', 1: 'sequence'},
        'attention_mask': {0: 'batch', 1: 'sequence'},
        'logits': {0: 'batch'}
    }
    
    # ONNX'e dönüştürme
    torch.onnx.export(
        model,
        (inputs["input_ids"], inputs["attention_mask"]),
        output_path,
        opset_version=14,
        input_names=input_names,
        output_names=output_names,
        dynamic_axes=dynamic_axes,
        do_constant_folding=True,
        export_params=True
    )
    
   

if __name__ == "__main__":
    # Eğitilmiş model yolu
    trained_model_path = "./trained_model"
    
    # ONNX'e dönüştür
    print("ONNX export başlıyor...")
    onnx_path = convert_to_onnx(trained_model_path)
    print(f"Model başarıyla {onnx_path} olarak kaydedildi")