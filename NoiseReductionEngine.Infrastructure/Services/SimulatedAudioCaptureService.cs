using NoiseReductionEngine.Core.Events;
using NoiseReductionEngine.Core.Interfaces;
using NoiseReductionEngine.Core.Models;

namespace NoiseReductionEngine.Infrastructure.Services;

public class SimulatedAudioCaptureService : IAudioCaptureService
{
    public event EventHandler<AudioCapturedEventArgs>? AudioCaptured;
    public bool IsCapturing { get; private set; }

    private CancellationTokenSource? _cts;

    public void StartCapture()
    {
        if (IsCapturing) return;

        IsCapturing = true;
        _cts = new CancellationTokenSource();

        var token = _cts.Token;
        Task.Run(async () =>
        {
            int sampleRate = 44100;
            int channels = 1;
            byte[] dummyBuffer = new byte[1764];

            while (!token.IsCancellationRequested)
            {
                try
                {
                    Random.Shared.NextBytes(dummyBuffer);
                    var frame = new AudioFrame(dummyBuffer, sampleRate, channels);

                    // Olayı güvenle fırlatıyoruz
                    AudioCaptured?.Invoke(this, new AudioCapturedEventArgs(frame));

                    // 20 ms bekleme
                    await Task.Delay(20, token);
                }
                catch (TaskCanceledException)
                {
                    // Durdurma istendiğinde döngüyü sessizce kırıyoruz
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Simülatör Hatası: {ex.Message}");
                }
            }
        }, token);
    }

    public void StopCapture()
    {
        if (!IsCapturing) return;

        _cts?.Cancel();
        IsCapturing = false;
    }
}