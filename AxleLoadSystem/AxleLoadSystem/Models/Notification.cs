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

    public string AlertClass
    {
        get
        {
            if(this.Type is null)
            {
                return "alert-primary";
            }
            if (this.Type == Notification.NotificationType.Info)
            {
                return "alert-info";
            }
            else if (this.Type == Notification.NotificationType.Success)
            {
                return "alert-success";
            }
            else if (this.Type == Notification.NotificationType.Warning)
            {
                return "alert-warning";
            }
            else if (this.Type == Notification.NotificationType.Failure)
            {
                return "alert-danger";
            }
            return "alert-primary";
        }
    }
}
