using AxleLoadSystem.Models;
using Blazored.LocalStorage;
using BOL.CustomModels;
using Microsoft.AspNetCore.Components;
using static AxleLoadSystem.Models.Notification;

namespace AxleLoadSystem.Store;

public interface IAppState
{
    event Action OnChange;
    event Action OnProfileImageChange;
    void SetNotification(ComponentBase source, Notification notification);
    void SetNotification(ComponentBase source, string message, NotificationType type);
    void NotifyProfileImageChange();
    Notification GetNotification();

    Task SetReportParameters(ReportParameters param);
    Task<ReportParameters> GetReportParameters();
}
public class AppState : IAppState
{
    public event Action OnChange;
    public event Action OnProfileImageChange;

    private readonly ILocalStorageService _storage;

    private Notification _notification { get; set; }

    private ReportParameters _reportParameters { get; set; }

    public AppState(ILocalStorageService storageService)
    {
        _storage = storageService;
    }

    #region Global Notification
    public void SetNotification(ComponentBase source, Notification notification)
    {
        _notification = notification;
        OnChange?.Invoke();
    }
    public void SetNotification(ComponentBase source, string message, NotificationType type)
    {
        _notification = new(message, type);
        OnChange?.Invoke();
    }
    public Notification GetNotification()
    {
        return _notification;
    }
    #endregion

    public void NotifyProfileImageChange()
    {
        OnProfileImageChange?.Invoke();
    }

    #region Report Parameters
    public async Task SetReportParameters(ReportParameters param)
    {
        await _storage.SetItemAsync("ReportSelection", param);
        _reportParameters = param;
    }
    public async Task<ReportParameters> GetReportParameters()
    {
        if (await _storage.ContainKeyAsync("ReportSelection"))
        {
            _reportParameters = await _storage.GetItemAsync<ReportParameters>("ReportSelection") ?? new();
            if(_reportParameters.Stations is null)
            {
                _reportParameters.Stations = new();
            }
            return _reportParameters;
        }

        return new();
    }
    #endregion
}
