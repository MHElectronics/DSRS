using DSRSystem.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using static DSRSystem.Models.Notification;

namespace DSRSystem.Store;

public interface IAppState
{
    event Action OnChange;
    event Action OnProfileImageChange;
    void SetNotification(ComponentBase source, Notification notification);
    void SetNotification(ComponentBase source, string message, NotificationType type);
    void NotifyProfileImageChange();
    Notification GetNotification();

}
public class AppState : IAppState
{
    public event Action OnChange;
    public event Action OnProfileImageChange;

    private readonly ILocalStorageService _storage;

    private Notification _notification { get; set; }


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
   
    #endregion
}
