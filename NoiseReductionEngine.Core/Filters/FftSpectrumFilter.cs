using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using NoiseReductionEngine.Core.Interfaces;
using NoiseReductionEngine.Core.Models;

namespace NoiseReductionEngine.Core.Filters;

public class FftSpectrumFilter : INoiseFilter
{
    private readonly int _lowCutoffHz;
    private readonly int _highCutoffHz;

    /// <summary>
    /// FFT Band-Pass (Frekans Süzgeci) Filtresi
    /// </summary>
    /// <param name="lowCutoffHz">Bu frekansın altındaki sesler (ör. 80 Hz dip uğultu) kesilir.</param>
    /// <param name="highCutoffHz">Bu frekansın üstündeki sesler (ör. 4000 Hz yüksek cızırtı) kesilir.</param>
    public FftSpectrumFilter(int lowCutoffHz = 80, int highCutoffHz = 4000)
    {
        _lowCutoffHz = lowCutoffHz;
        _highCutoffHz = highCutoffHz;
    }

    public AudioFrame Process(AudioFrame inputFrame)
    {
        byte[] inputData = inputFrame.Data;
        int sampleCount = inputData.Length / 2;

        // FFT için örnek sayısının 2'nin kuvveti (Power of 2) olması gerekir.
        // En yakın 2'nin kuvvetini buluyoruz (ör. 512, 1024)
        int fftSize = 1;
        while (fftSize < sampleCount) fftSize *= 2;

        Complex32[] complexBuffer = new Complex32[fftSize];

        // 1. 16-bit PCM verisini Karmaşık Sayılara (Complex32) çeviriyoruz
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(inputData, i * 2);
            complexBuffer[i] = new Complex32(sample, 0);
        }

        // 2. ZAMAN ALANINDAN FREKANS ALANINA GEÇİŞ (Forward FFT)
        Fourier.Forward(complexBuffer, FourierOptions.Matlab);

        // 3. Frekans Filtreleme (Spesifik bantları sıfırlama)
        double hzPerBin = (double)inputFrame.SampleRate / fftSize;

        for (int i = 0; i < fftSize; i++)
        {
            double currentHz = i * hzPerBin;
            // Nyquist frekansı üzerindeki simetrik bölgeyi de korumak için simetri hesabı
            if (currentHz > inputFrame.SampleRate / 2.0)
            {
                currentHz = inputFrame.SampleRate - currentHz;
            }

            // Belirlenen bantların dışındaki frekansları tamamen sıfırlıyoruz
            if (currentHz < _lowCutoffHz || currentHz > _highCutoffHz)
            {
                complexBuffer[i] = Complex32.Zero;
            }
        }

        // 4. FREKANS ALANINDAN ZAMAN ALANINA GERİ DÖNÜŞ (Inverse FFT)
        Fourier.Inverse(complexBuffer, FourierOptions.Matlab);

        // 5. İşlenmiş karmaşık sayıları tekrar 16-bit PCM byte[] dizisine çeviriyoruz
        byte[] outputData = new byte[inputData.Length];
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = (short)Math.Clamp(complexBuffer[i].Real, short.MinValue, short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(sample);
            outputData[i * 2] = bytes[0];
            outputData[i * 2 + 1] = bytes[1];
        }

        return new AudioFrame(outputData, inputFrame.SampleRate, inputFrame.Channels);
    }
}