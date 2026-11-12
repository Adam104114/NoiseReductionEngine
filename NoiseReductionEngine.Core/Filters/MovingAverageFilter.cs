using NoiseReductionEngine.Core.Interfaces;
using NoiseReductionEngine.Core.Models;

namespace NoiseReductionEngine.Core.Filters;

public class MovingAverageFilter : INoiseFilter
{
    private readonly int _windowSize;

    public MovingAverageFilter(int windowSize = 10) // Pencereyi 10 yaparak etkisini artıralım
    {
        _windowSize = windowSize;
    }

    public AudioFrame Process(AudioFrame inputFrame)
    {
        byte[] inputData = inputFrame.Data;
        
        // byte[] dizisini 16-bit short[] ses dizisine dönüştürüyoruz (C# Little Endian)
        int sampleCount = inputData.Length / 2;
        short[] samples = new short[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = BitConverter.ToInt16(inputData, i * 2);
        }

        short[] filteredSamples = new short[sampleCount];

        // 16-bit ses genlikleri üzerinde Moving Average (Hareketli Ortalama) uyguluyoruz
        for (int i = 0; i < sampleCount; i++)
        {
            int sum = 0;
            int count = 0;

            for (int j = 0; j < _windowSize && (i - j) >= 0; j++)
            {
                sum += samples[i - j];
                count++;
            }

            filteredSamples[i] = (short)(sum / count);
        }

        // Filtrelenmiş short[] dizisini tekrar byte[] dizisine geri çeviriyoruz
        byte[] outputData = new byte[inputData.Length];
        for (int i = 0; i < sampleCount; i++)
        {
            byte[] bytes = BitConverter.GetBytes(filteredSamples[i]);
            outputData[i * 2] = bytes[0];
            outputData[i * 2 + 1] = bytes[1];
        }

        return new AudioFrame(outputData, inputFrame.SampleRate, inputFrame.Channels);
    }
}