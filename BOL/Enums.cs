using System.ComponentModel;

namespace BOL;

public enum FormMode
{
    Details,
    Add,
    Edit
}
public enum UserRole
{
    Admin,
    User,
    AdvancedUser
}
public enum LogActivity
{ 
    Login = 1,
    Logout = 2,
    Insert = 3, 
    Update = 4, 
    Delete = 5,
    Download = 6,
    Get = 7
}

public enum UploadedFileType
{
    [Description("Axle Load")]
    LoadData = 1,
    [Description("Fine Payment")]
    FineData = 2
}
public enum WIMType
{
    [Description("SSWIM")]
    SlowSpeed = 1,
    [Description("Static")]
    Static = 2,
    [Description("HSWIM")]
    HighSpeed = 3,
    [Description("MSWIM")]
    MiddleSpeed = 4,
    [Description("Others")]
    Others = 5
}
public enum MeterType
{
    [Description("Gas Meter")]
    GasMeter = 1,
    [Description("GPRS Meter")]
    GPRSMeter = 2
}

public static class EnumHelper
{
    public static string ToDescription(this Enum value)
    {
        var attributes = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : value.ToString();
    }
}
public static class CacheKeys
{
    public static string Stations { get; set; } = "Stations";
}
