using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace BOL.Helpers;
public sealed class JsonDateTimeFormatAttribute : JsonConverterAttribute
{
    public static readonly string GlobalDateFormat = "MM-dd-yyyy h:mm:ss tt";

    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        return new DateTimeFormatConverter();
    }
}

public class DateTimeFormatConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string strValue = reader.GetString();
        return DateTime.ParseExact(
            strValue,
            JsonDateTimeFormatAttribute.GlobalDateFormat,
            CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));

        writer.WriteStringValue(value
            .ToUniversalTime()
            .ToString(
                JsonDateTimeFormatAttribute.GlobalDateFormat,
                CultureInfo.InvariantCulture));
    }
}
