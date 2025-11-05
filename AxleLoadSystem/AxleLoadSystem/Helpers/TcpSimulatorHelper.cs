using AxleLoadSystem.Data;
namespace AxleLoadSystem.Helpers;


public class TcpSimulatorHelper
{
    private readonly TcpSimulator _sim = new TcpSimulator();
    private bool _started = false;
    public void Start(int port = 5000)
    {
        if (_started) return;
        _sim.Start(port);
        _started = true;
    }
    public void Stop() => _sim.Stop();
}
