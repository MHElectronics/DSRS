using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AxleLoadSystem.Data
{
    // Simple local TCP simulator that responds with sample meter JSON.
    // The service will start a background listener on 127.0.0.1:5000.
    public class TcpSimulator
    {
        private TcpListener? _listener;
        private bool _running;

        public void Start(int port = 5000)
        {
            if (_running) return;
            _running = true;
            _listener = new TcpListener(IPAddress.Loopback, port);
            _listener.Start();
            Task.Run(async () =>
            {
                while (_running)
                {
                    try
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        _ = HandleClientAsync(client);
                    }
                    catch { }
                }
            });
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using var stream = client.GetStream();
            var buffer = new byte[8192];
            try
            {
                var len = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (len <= 0) return;
                var req = Encoding.UTF8.GetString(buffer, 0, len);
                // build sample response with some randomized values
                var resp = new
                {
                    MeterNO = "0000000001",
                    cmdType = 0,
                    responseCode = 0,
                    meterType = "0001",
                    meterStatus = "02",
                    version = "000001",
                    currentSumUsedWork = Math.Round(50 + new Random().NextDouble() * 200, 2),
                    currentTemperature = 25,
                    batteryCapacity = new Random().Next(40, 100).ToString(),
                    real_time = DateTime.UtcNow.ToString("yyyyMMddHHmmss")
                };
                var respJson = JsonSerializer.Serialize(resp);
                var outBytes = Encoding.UTF8.GetBytes(respJson);
                await stream.WriteAsync(outBytes, 0, outBytes.Length);
            }
            catch { }
            finally
            {
                client.Close();
            }
        }

        public void Stop()
        {
            _running = false;
            _listener?.Stop();
        }
    }
}
