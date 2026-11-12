import socket
import struct
import sys

UDP_IP = "127.0.0.1"
UDP_PORT = 5006

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))

print(f"=== UZAK AĞ ALICISI (NETWORK RECEIVER) ===")
print(f"UDP Port {UDP_PORT} dinleniyor...\n")

try:
    while True:
        data, addr = sock.recvfrom(65535)
        
        # Header Ayrıştırma: 4 Byte uint (Sequence) + 8 Byte long (Timestamp) = 12 Byte
        if len(data) >= 12:
            header = data[:12]
            pcm_payload = data[12:]
            
            seq_num, time_stamp = struct.unpack('<IQ', header)
            
            print(f"[AĞ PAKETİ ALINDI] Gönderen: {addr[0]}:{addr[1]} | Sıra No: #{seq_num} | Zaman Damgası: {time_stamp} ms | Payload Boyutu: {len(pcm_payload)} Byte")
except KeyboardInterrupt:
    print("\nAlıcı kapatıldı.")