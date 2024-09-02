using AxleLoadSystem.Models;
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

    void SetReportParameters(ReportParameters param);
    ReportParameters GetReportParameters();
}
public class AppState : IAppState
{
    public event Action OnChange;
    public event Action OnProfileImageChange;
    private Notification _notification { get; set; }

    private ReportParameters _reportParameters { get; set; }

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

    public void NotifyProfileImageChange()
    {
        OnProfileImageChange?.Invoke();
    }
    #endregion

    public void SetReportParameters(ReportParameters param)
    {
        _reportParameters = param;
    }
    public ReportParameters GetReportParameters()
    {
        return _reportParameters ?? new();
    }
}
