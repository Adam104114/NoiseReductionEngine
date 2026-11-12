namespace  NoiseReductionEngine.Core.Models;

public class AudioFrame
{
    public byte[] Data {get;}

    public int SampleRate {get;}

    public int Channels {get;}// kanal sayısı

    public DateTime TimeStamp {get;} //verinin alındığı an


    public AudioFrame(byte[] data, int sampleRate, int channels)//constructor
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        SampleRate = sampleRate;
        Channels = channels;
        TimeStamp = DateTime.UtcNow;
    }
}