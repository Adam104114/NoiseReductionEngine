using NoiseReductionEngine.Core.Models;

namespace NoiseReductionEngine.Core.Events;

public class AudioCapturedEventArgs : EventArgs
{

    public AudioFrame Frame { get; }

    public AudioCapturedEventArgs(AudioFrame frame)
    {
        Frame = frame ?? throw new ArgumentNullException(nameof(frame));
    }
}