using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace NoiseReductionEngine.Infrastructure.Services;

public class UdpSignalStreamService : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly string _hostname;
    private readonly int _port;


    public UdpSignalStreamService(string hostname = "127.0.0.1", int port = 5005)
    {
        _hostname = hostname;
        _port = port;
        _udpClient = new UdpClient();
    }


    public void SendSignalData(short[] rawSamples, short[] filteredSamples)
    {
        try
        {
            var payload = new
            {
                raw= rawSamples,
                filtered = filteredSamples
            };

            string json = JsonSerializer.Serialize(payload);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            _udpClient.Send(bytes, bytes.Length, _hostname, _port);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UDP Hata] Yayın gönderilemedi: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
    }
}