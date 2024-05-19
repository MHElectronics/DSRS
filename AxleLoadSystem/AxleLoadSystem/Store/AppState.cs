using AxleLoadSystem.Models;
using Microsoft.AspNetCore.Components;

namespace AxleLoadSystem.Store;

public interface IAppState
{
    event Action OnChange;
    void SetNotification(ComponentBase source, Notification notification);
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
    public Notification GetNotification()
    {
        return _notification;
    }
}
