using System;
using NoiseReductionEngine.Core.Interfaces;
using NoiseReductionEngine.Core.Models;

namespace NoiseReductionEngine.Core.Filters;

public class NoiseGateFilter : INoiseFilter
{
    private readonly short _threshold;

    public NoiseGateFilter(short threshold = 500)
    {
        _threshold = threshold;
    }

    public AudioFrame Process(AudioFrame inputFrame)
    {
        byte[] inputData = inputFrame.Data;
        int sampleCount = inputData.Length / 2;
        short[] samples = new short[sampleCount];


        for (int i =0; i< sampleCount; i++)
        {
            samples[i] = BitConverter.ToInt16(inputData, i*2);
        }

        short[] outputSamples = new short[sampleCount];

        for (int i=0; i<sampleCount; i++)
        {
            if(Math.Abs(samples[i]) < _threshold)
            {
                outputSamples[i]= 0;
            }
            else
            {
                outputSamples[i] = samples[i];
            }
        }

        byte[] outputData = new byte[inputData.Length];
        for( int i =0; i< sampleCount; i++)
        {
            byte[] bytes =BitConverter.GetBytes(outputSamples[i]);
            outputData[i*2] = bytes[0];
            outputData[i*2 +1]= bytes[1];
        }

        return new AudioFrame(outputData, inputFrame.SampleRate, inputFrame.Channels);
    }
}