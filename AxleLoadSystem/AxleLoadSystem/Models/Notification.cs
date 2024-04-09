namespace AxleLoadSystem.Models;

public class Notification
{
    public NotificationType? Type { get; set; } = null;
    public string Message { get; set; } = string.Empty;

    public enum NotificationType
    {
        Info,
        Success,
        Failure,
        Warning
    }
}
