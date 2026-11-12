using System.Net;
using System.Net.Sockets;

namespace NoiseReductionEngine.Infrastructure.Services;

public class NetworkAudioStreamService : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _remoteEndPoint;
    private uint _sequenceNumber = 0;

    /// <summary>
    /// İşlenmiş ses paketlerini hedef IP ve Porta fırlatan ağ akış servisi.
    /// </summary>
    /// <param name="targetIp">Hedef sunucu IP adresi (ör. "192.168.1.50" veya "127.0.0.1")</param>
    /// <param name="targetPort">Hedef dinleme portu (ör. 5006)</param>
    public NetworkAudioStreamService(string targetIp = "127.0.0.1", int targetPort = 5006)
    {
        _udpClient = new UdpClient();
        _remoteEndPoint = new IPEndPoint(IPAddress.Parse(targetIp), targetPort);
    }

    public void StreamAudioFrame(byte[] pcmData)
    {
        try
        {
            _sequenceNumber++;
            long timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Header Yapısı: [SequenceNumber (4 byte)] + [TimeStamp (8 byte)] = 12 Byte Header
            byte[] headerBytes = new byte[12];
            BitConverter.GetBytes(_sequenceNumber).CopyTo(headerBytes, 0);
            BitConverter.GetBytes(timeStamp).CopyTo(headerBytes, 4);

            // Payload = Header + PCM Data
            byte[] packet = new byte[headerBytes.Length + pcmData.Length];
            Buffer.BlockCopy(headerBytes, 0, packet, 0, headerBytes.Length);
            Buffer.BlockCopy(pcmData, 0, packet, headerBytes.Length, pcmData.Length);

            _udpClient.Send(packet, packet.Length, _remoteEndPoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Network Stream Hata] Paket gönderilemedi: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
    }
}