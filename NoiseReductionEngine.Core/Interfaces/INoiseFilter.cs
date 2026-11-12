using NoiseReductionEngine.Core.Models;

namespace NoiseReductionEngine.Core.Interfaces;

public interface INoiseFilter
{
    AudioFrame Process(AudioFrame inputFrame);
}