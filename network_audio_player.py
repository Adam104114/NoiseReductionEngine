import socket
import struct
import numpy as np
import sounddevice as sd

UDP_IP = "127.0.0.1"
UDP_PORT = 5006
SAMPLE_RATE = 44100
CHANNELS = 1

# Ses çıkış akışını (Audio Stream) başlatıyoruz
audio_stream = sd.OutputStream(
    samplerate=SAMPLE_RATE,
    channels=CHANNELS,
    dtype='int16'
)
audio_stream.start()

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))

print("=== CANLI SES OYNATICI İSTEMCİ (NETWORK AUDIO PLAYER) ===")
print(f"UDP Port {UDP_PORT} dinleniyor... Filtrelenmiş ses hoparlöre aktarılıyor.\n")

try:
    while True:
        data, addr = sock.recvfrom(65535)
        
        # 12 Byte Header: [SequenceNumber (4B)] + [Timestamp (8B)]
        if len(data) > 12:
            header = data[:12]
            pcm_payload = data[12:]
            
            seq_num, time_stamp = struct.unpack('<IQ', header)
            
            # Byte dizisini 16-bit PCM numpy dizisine çevirip hoparlöre basıyoruz
            audio_samples = np.frombuffer(pcm_payload, dtype=np.int16)
            audio_stream.write(audio_samples)

except KeyboardInterrupt:
    print("\nSes oynatıcı kapatıldı.")
    audio_stream.stop()
    audio_stream.close()