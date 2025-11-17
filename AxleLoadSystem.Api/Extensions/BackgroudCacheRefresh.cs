using BOL;
using Services;

namespace DSRSystem.Api.Extensions;

public class BackgroudCacheRefresh : IHostedService, IDisposable
{
    private Timer _timer;
    private List<Station> _stations;
    private readonly IStationService _stationService;
    public BackgroudCacheRefresh(IStationService stationService)
    {
        _stationService = stationService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(AddToCache, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        return Task.CompletedTask;
    }

    private void AddToCache(object? state)
    {
        _stations = _stationService!.Get().GetAwaiter().GetResult().ToList();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
