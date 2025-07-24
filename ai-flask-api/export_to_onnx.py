import torch
from transformers import AutoTokenizer, AutoModelForSequenceClassification
import os
from onnxruntime.quantization import quantize_dynamic, QuantType

def convert_to_onnx(model_path, output_dir="onnx_model"):
    # Çıktı dizinini oluştur
    os.makedirs(output_dir, exist_ok=True)
    output_path = os.path.join(output_dir, "model.onnx")
    
    # Model ve tokenizer yükle
    tokenizer = AutoTokenizer.from_pretrained(model_path)
    model = AutoModelForSequenceClassification.from_pretrained(model_path)
    model.eval()
    
    # Dinamik boyutlar için örnek giriş
    sample_text = "Örnek şikayet metni buraya yazılacak"
    inputs = tokenizer(
        sample_text, 
        padding=True,
        truncation=True,
        max_length=256,
        return_tensors="pt"
    )
    
    # ONNX export için gerekenler
    input_names = ["input_ids", "attention_mask"]
    output_names = ["logits"]
    dynamic_axes = {
        'input_ids': {0: 'batch_size', 1: 'sequence_length'},
        'attention_mask': {0: 'batch_size', 1: 'sequence_length'},
        'logits': {0: 'batch_size'}
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
        export_params=True,
        verbose=True
    )
    
    # Quantization (optimize_model parametresi kaldırıldı)
    quantized_path = os.path.join(output_dir, "model_quant.onnx")
    quantize_dynamic(
        model_input=output_path,
        model_output=quantized_path,
        weight_type=QuantType.QUInt8
    )
    
    print(f"ONNX modeli kaydedildi: {output_path}")
    print(f"Quantized model kaydedildi: {quantized_path}")
    return output_path, quantized_path

if __name__ == "__main__":
    # Eğitilmiş model yolu
    trained_model_path = "./trained_model"
    
    # ONNX'e dönüştür
    print("ONNX export başlıyor...")
    onnx_path, quantized_path = convert_to_onnx(trained_model_path)
    print(f"\nONNX model başarıyla oluşturuldu: {onnx_path}")
    print(f"Quantized model oluşturuldu: {quantized_path}")
    
    # Boyut karşılaştırması
    original_size = os.path.getsize(onnx_path) / (1024 * 1024)
    quantized_size = os.path.getsize(quantized_path) / (1024 * 1024)
    
    print(f"\nBoyut karşılaştırması:")
    print(f"Original ONNX: {original_size:.2f} MB")
    print(f"Quantized ONNX: {quantized_size:.2f} MB")
    print(f"Küçültme Oranı: {original_size / quantized_size:.1f}x")
    print("ONNX export işlemi tamamlandı.")