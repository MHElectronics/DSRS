using AxleLoadSystem.Models;
using Microsoft.AspNetCore.Components;
using static AxleLoadSystem.Models.Notification;

namespace AxleLoadSystem.Store;

public interface IAppState
{
    event Action OnChange;
    void SetNotification(ComponentBase source, Notification notification);
    void SetNotification(ComponentBase source, string message, NotificationType type);
    Notification GetNotification();
}
public class AppState : IAppState
{
    public event Action OnChange;
    private Notification _notification { get; set; }

    public void SetNotification(ComponentBase source, Notification notification)
    {
        _notification = notification;
        OnChange?.Invoke();
    }
    public void SetNotification(ComponentBase source, string message, NotificationType type)
    {
        _notification = new(message , type);
        OnChange?.Invoke();
    }
    public Notification GetNotification()
    {
        return _notification;
    }
}
