using NoiseReductionEngine.Core.Filters;
using NoiseReductionEngine.Core.Interfaces;
using NoiseReductionEngine.Infrastructure.Services;

Console.WriteLine("=== SES GÜRÜLTÜ AZALTMA MOTORU (REMOTE NETWORK STREAMING) ===");
Console.WriteLine("1. Osiloskop Yayını: 127.0.0.1:5005");
Console.WriteLine("2. Network Ses Akışı: 127.0.0.1:5006\n");

IAudioCaptureService captureService = new AlsaAudioCaptureService();

// Triple DSP Pipeline (Time-Domain + Frequency-Domain FFT)
INoiseFilter smoothingFilter = new MovingAverageFilter(windowSize: 10);
INoiseFilter gateFilter = new NoiseGateFilter(threshold: 600);
INoiseFilter fftFilter = new FftSpectrumFilter(lowCutoffHz: 120, highCutoffHz: 4000);

// Servisler
using var udpOsiloskopStreamer = new UdpSignalStreamService("127.0.0.1", 5005);
using var networkStreamer = new NetworkAudioStreamService("127.0.0.1", 5006);

captureService.AudioCaptured += (sender, e) =>
{
    var rawFrame = e.Frame;

    // 1. DSP Filtreleme Boru Hattı
    var smoothedFrame = smoothingFilter.Process(rawFrame);
    var gatedFrame = gateFilter.Process(smoothedFrame);
    var finalFrame = fftFilter.Process(gatedFrame); // Temizlenmiş nihai ses verisi

    // 2. Osiloskop Çizimi İçin Örnek Dizileri (UDP 5005)
    int sampleCount = rawFrame.Data.Length / 2;
    short[] rawSamples = new short[sampleCount];
    short[] finalSamples = new short[sampleCount];

    for (int i = 0; i < sampleCount; i++)
    {
        rawSamples[i] = BitConverter.ToInt16(rawFrame.Data, i * 2);
        finalSamples[i] = BitConverter.ToInt16(finalFrame.Data, i * 2);
    }
    udpOsiloskopStreamer.SendSignalData(rawSamples, finalSamples);

    // 3. Ağ Akışı (Network Stream - UDP 5006): İşlenmiş pürüzsüz baytları fırlatıyoruz
    networkStreamer.StreamAudioFrame(finalFrame.Data);

    Console.WriteLine($"[{rawFrame.TimeStamp:HH:mm:ss.fff}] FFT Temiz Ses -> Osiloskop (5005) & Network (5006) Akıtıldı.");
};

captureService.StartCapture();

await Task.Delay(30000); // 30 saniye yayın

captureService.StopCapture();
Console.WriteLine("\nYayın tamamlandı.");