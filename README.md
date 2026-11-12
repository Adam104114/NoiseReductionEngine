# NoiseReductionEngine 🎙️⚡

A high-performance, real-time noise reduction system built with **C# (.NET 8)**, **Linux ALSA Native Interop**, **Triple-Stage DSP (Time & Frequency Domain)**, and **UDP Remote Audio Streaming**.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Linux%20%7C%20ALSA-orange.svg)

---

## 📌 Architecture Overview

`NoiseReductionEngine` follows **Clean Architecture** principles to separate low-level Linux audio capture, high-performance signal processing, and low-latency network delivery.

```text
[ Hardware: Microphone ]
           │
           ▼
┌─────────────────────────────────────────────────────────┐
│    INFRASTRUCTURE LAYER (Native Hardware Interop)       │
│    • AlsaAudioCaptureService (libasound.so.2 P/Invoke)  │
│    • 16-bit PCM (44.1 kHz, Mono) Buffer Stream          │
└──────────────────────────┬──────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│    CORE LAYER (Triple DSP & Frequency Domain Pipeline)  │
│    • MovingAverageFilter (Time-Domain Smoothing)        │
│    • NoiseGateFilter (Threshold Level Suppression)      │
│    • FftSpectrumFilter (MathNet Forward/Inverse FFT)    │
│      └─► Band-Pass: 80 Hz - 4000 Hz                     │
└──────────────────────────┬──────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│    INFRASTRUCTURE LAYER (Network Streaming Services)    │
│    • UdpSignalStreamService (Oscilloscope Stream - 5005)│
│    • NetworkAudioStreamService (Header + PCM - 5006)    │
│      └─► Header: Sequence No (4B) + Timestamp (8B)      │
└──────────────────────────┬──────────────────────────────┘
                           │
         ┌─────────────────┴─────────────────┐
         ▼                                   ▼
┌─────────────────┐                 ┌──────────────────┐
│ Python Client 1 │                 │ Python Client 2  │
│ oscilloscope.py │                 │ network_player.py│
│ (PyQtGraph GUI) │                 │ (Live Audio Out) │
└─────────────────┘                 └──────────────────┘