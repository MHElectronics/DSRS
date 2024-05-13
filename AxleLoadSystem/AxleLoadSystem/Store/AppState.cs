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
    public event Action<ComponentBase, string> StateChanged;
    private void NotifyStateChanged(ComponentBase source, string property) { StateChanged?.Invoke(source, property); }

    public void SetNotification(ComponentBase source, Notification notification)
    {
        _notification = notification;
        OnChange?.Invoke();
        NotifyStateChanged(source, "Notification");
    }
    public Notification GetNotification()
    {
        return _notification;
    }
}
