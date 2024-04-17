using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace BOL.Helpers;
public sealed class JsonDateTimeFormatAttribute : JsonConverterAttribute
{
    private readonly string format;

    public JsonDateTimeFormatAttribute(string format)
    {
        this.format = format;
    }

    public string Format => this.format;

    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        return new DateTimeFormatConverter(this.format);
    }
}

public class DateTimeFormatConverter : JsonConverter<DateTime>
{
    private readonly string format;

    public DateTimeFormatConverter(string format)
    {
        this.format = format;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string strValue = reader.GetString();
        return DateTime.ParseExact(
            strValue,
            this.format,
            CultureInfo.CurrentCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));

        writer.WriteStringValue(value
            .ToUniversalTime()
            .ToString(
                this.format,
                CultureInfo.InvariantCulture));
    }
}
