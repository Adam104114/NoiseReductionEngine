using System.Runtime.InteropServices;
using NoiseReductionEngine.Core.Events;
using NoiseReductionEngine.Core.Interfaces;
using NoiseReductionEngine.Core.Models;

namespace NoiseReductionEngine.Infrastructure.Services;

public class AlsaAudioCaptureService : IAudioCaptureService
{
    public event EventHandler<AudioCapturedEventArgs>? AudioCaptured;
    public bool IsCapturing { get; private set; }

    private CancellationTokenSource? _cts;

    // ALSA C-API Native P/Invoke Bağlantıları (libasound.so.2)
    [DllImport("libasound.so.2")]
    private static extern int snd_pcm_open(out IntPtr pcm, string name, int stream, int mode);

    [DllImport("libasound.so.2")]
    private static extern int snd_pcm_set_params(IntPtr pcm, int format, int access, int channels, int rate, int soft_resample, int latency);

    [DllImport("libasound.so.2")]
    private static extern int snd_pcm_readi(IntPtr pcm, byte[] buffer, ulong frames);

    [DllImport("libasound.so.2")]
    private static extern int snd_pcm_close(IntPtr pcm);

    public void StartCapture()
    {
        if (IsCapturing) return;

        IsCapturing = true;
        _cts = new CancellationTokenSource();

        var token = _cts.Token;
        Task.Run(() => CaptureLoop(token), token);
    }

    private void CaptureLoop(CancellationToken token)
    {
        IntPtr pcmHandle = IntPtr.Zero;

        // ALSA üzerinden varsayılan yakalama cihazını (mikrofon) açıyoruz
        // Stream: 1 (SND_PCM_STREAM_CAPTURE), Mode: 0 (Bloklayan)
        int err = snd_pcm_open(out pcmHandle, "default", 1, 0);
        if (err < 0)
        {
            Console.WriteLine($"[ALSA Hata] Cihaz açılamadı. Hata Kodu: {err}");
            IsCapturing = false;
            return;
        }

        // 44100Hz, 16-bit LE (format=2), 1 Kanal (Mono), 20000us (20ms gecikme)
        err = snd_pcm_set_params(pcmHandle, 2, 3, 1, 44100, 1, 20000);
        if (err < 0)
        {
            Console.WriteLine($"[ALSA Hata] Ses parametreleri ayarlanamadı. Hata Kodu: {err}");
            snd_pcm_close(pcmHandle);
            IsCapturing = false;
            return;
        }

        // 20ms için 16-bit Mono (2 byte/örnek) buffer boyutu: 44100 * 0.02 * 2 = 1764 bytes
        byte[] buffer = new byte[1764];
        ulong periodFrames = 882;

        while (!token.IsCancellationRequested)
        {
            // Doğrudan mikrofon donanım tamponundan okuma yapıyoruz
            long readFrames = snd_pcm_readi(pcmHandle, buffer, periodFrames);

            if (readFrames > 0)
            {
                var frame = new AudioFrame(buffer, 44100, 1);
                AudioCaptured?.Invoke(this, new AudioCapturedEventArgs(frame));
            }
            else if (readFrames < 0)
            {
                // Buffer taşması/kesintisi (Overrun) durumunda ALSA cihazını hızlıca resetliyoruz
                snd_pcm_close(pcmHandle);
                snd_pcm_open(out pcmHandle, "default", 1, 0);
                snd_pcm_set_params(pcmHandle, 2, 3, 1, 44100, 1, 20000);
            }
        }

        snd_pcm_close(pcmHandle);
    }

    public void StopCapture()
    {
        if (!IsCapturing) return;
        _cts?.Cancel();
        IsCapturing = false;
    }
}