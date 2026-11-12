import socket
import json
import sys
import numpy as np
import pyqtgraph as pg
from pyqtgraph.Qt import QtCore, QtWidgets

UDP_IP = "127.0.0.1"
UDP_PORT = 5005

# UDP Socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))
sock.setblocking(False)

# Graphic Window
app = QtWidgets.QApplication(sys.argv)
win = pg.GraphicsLayoutWidget(show=True, title="NoiseReductionEngine - Canlı Osiloskop")
win.resize(1000, 500)

plot = win.addPlot(title="16-bit PCM Canlı Ses Sinyali (Kırmızı: Ham | Mavi: Temizlenmiş)")
plot.setYRange(-5000, 5000)
plot.showGrid(x=True, y=True)

raw_curve = plot.plot(pen=pg.mkPen('r', width=1.5), name="Ham Sinyal")
filtered_curve = plot.plot(pen=pg.mkPen('b', width=2), name="Temizlenmiş Sinyal")

def update():
    try:
        data = None
        while True:
            try:
                pkt, _ = sock.recvfrom(65535)
                data = pkt
            except (BlockingIOError, socket.error):
                break
        
        if data:
            payload = json.loads(data.decode('utf-8'))
            raw_samples = payload['raw']
            filtered_samples = payload['filtered']

            raw_curve.setData(np.array(raw_samples, dtype=np.int16))
            filtered_curve.setData(np.array(filtered_samples, dtype=np.int16))
    except Exception:
        pass

timer = QtCore.QTimer()
timer.timeout.connect(update)
timer.start(20) # 20ms

if __name__ == '__main__':
    sys.exit(app.exec_())