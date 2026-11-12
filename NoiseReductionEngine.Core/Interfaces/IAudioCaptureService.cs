using NoiseReductionEngine.Core.Events;

namespace NoiseReductionEngine.Core.Interfaces;

public interface IAudioCaptureService
{
    // Ses verisi her yakalandığında tetiklenecek Event-Driven olayımız
    event EventHandler<AudioCapturedEventArgs> AudioCaptured;

    bool IsCapturing { get;}

    void StartCapture();
    void StopCapture();
}