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

public enum UploadedFileType
{
    [Description("Axle Load")]
    LoadData = 1,
    [Description("Fine Payment")]
    FineData = 2
}
public enum WIMType
{
    [Description("Slow Speed")]
    SlowSpeed = 1,
    [Description("Static")]
    Static = 2,
    [Description("High Speed")]
    HighSpeed = 3,
    [Description("Middle Speed")]
    MiddleSpeed = 4,
    [Description("Others")]
    Others = 5
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
