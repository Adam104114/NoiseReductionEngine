# NoiseReductionEngine 🎙️⚡

A high-performance, real-time noise reduction and remote audio streaming engine built with **C# (.NET 8)**, **Linux ALSA Native Interop**, **Triple-Stage DSP (Time & Frequency Domain)**, and **Python**.

## 📌 Architecture Overview

NoiseReductionEngine follows Clean Architecture principles to separate low-level Linux audio capture, real-time digital signal processing, and UDP network delivery:

   **Infrastructure Layer (Native Hardware Interop):** `AlsaAudioCaptureService` captures raw 16-bit PCM (44.1 kHz, Mono) audio directly via `libasound.so.2` (P/Invoke).
   **Core Layer (Triple DSP Pipeline):**
   - `MovingAverageFilter`: Time-domain high-frequency spike smoothing.
   - `NoiseGateFilter`: Low-amplitude background noise suppression during silence.
   - `FftSpectrumFilter`: MathNet Forward/Inverse FFT Band-Pass filter (80 Hz - 4000 Hz) isolating human voice and suppressing hum/hiss.
   **Infrastructure Layer (Network Streaming):**
   - `UdpSignalStreamService`: Streams raw and filtered waveforms to UDP Port `5005`.
   - `NetworkAudioStreamService`: Streams 12-byte custom header (`SequenceNumber` + `Timestamp`) + PCM audio to UDP Port `5006`.
   **Python Clients:**
   - `oscilloscope.py`: 100 FPS real-time PyQtGraph visualizer.
   - `network_audio_player.py`: Zero-latency live audio output player.

## 🛠️  Kurulum (Installation)

Linux ortamında gerekli sistem paketlerini kurun, projeyi derleyin ve Python ortamını hazırlayın:

    sudo apt update
    sudo apt install -y build-essential libasound2-dev libportaudio2 python3-venv
    git clone [https://github.com/Adam104114/NoiseReductionEngine.git](https://github.com/Adam104114/NoiseReductionEngine.git)
    cd NoiseReductionEngine
    dotnet build NoiseReductionEngine.sln
    python3 -m venv .venv
    source .venv/bin/activate
    pip install numpy sounddevice pyqtgraph PyQt5

## 🚀  Nasıl Çalıştırılır? (Step-by-Step Usage Guide)

Bu proje bir arayüz (GUI) yerine paralel çalışan servislerden oluşur. Sistemi çalıştırmak için **3 ayrı terminal sekmesi** açın ve aşağıdaki sırayı takip edin (**Önce Python alıcılarını başlatın, en son C# motorunu çalıştırın.** Yankı olmaması için kulaklık kullanmanız önerilir):

    Terminal1 — Canlı Ses Oynatıcı (UDP Port 5006):**

    source .venv/bin/activate
    python3 network_audio_player.py

    Terminal2 — Canlı Osiloskop Ekranı (UDP Port 5005 - İsteğe Bağlı):**

    source .venv/bin/activate
    python3 oscilloscope.py

    Terminal3 — Ana C# Ses İşleme Motoru:**

    dotnet run --project NoiseReductionEngine.CLI

## 🔧 Calibration & Fine-Tuning (Program.cs)

Ortam gürültüsüne göre filtre hassasiyetini `NoiseReductionEngine.CLI/Program.cs` dosyasından özelleştirebilirsiniz:
- **Noise Gate Threshold (default: 600):** Sessiz ortamlarda `200-300` seviyesine düşürün; gürültülü ortamlarda `800-1000` seviyesine çıkarın.
- **FFT Band-Pass Cutoff (default: 80 Hz - 4000 Hz):** İnsan sesi frekans aralığı için optimize edilmiştir.

## 📂 Project Structure

- `NoiseReductionEngine.Core/` — DSP Interfaces, Models & Filters (FFT, Gate, Smoothing)
- `NoiseReductionEngine.Infrastructure/` — ALSA Hardware Interop & UDP Network Streamers
- `NoiseReductionEngine.CLI/` — Engine Application Pipeline & Execution Entrypoint
- `oscilloscope.py` — Real-time PyQtGraph Dual-Waveform Visualizer
- `network_audio_player.py` — UDP Live Audio Output Client

## 📜 License

Distributed under the MIT License.
