
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace ACE.PMS.Infrastructure.Conversions;

#nullable disable warnings
public static class ValueConversionExtensions
{
    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> builder) //where T : class
    {       
        var converter = new ValueConverter<T, string>(
           v => JsonSerializer.Serialize(v, JsonSerializerOptions.Web),
           //v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented=false}),
           v => string.IsNullOrEmpty(v) ? default : JsonSerializer.Deserialize<T>(v, JsonSerializerOptions.Web));

        var comparer = new ValueComparer<T>(
            (l, r) => JsonSerializer.Serialize(l, JsonSerializerOptions.Web) == JsonSerializer.Serialize(r, JsonSerializerOptions.Web),
            v => v == null ? 0 : JsonSerializer.Serialize(v, JsonSerializerOptions.Web).GetHashCode(),
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, JsonSerializerOptions.Web), JsonSerializerOptions.Web));

        builder.HasConversion(converter);
        builder.Metadata.SetValueComparer(comparer);

        return builder;
    }    
}



public static class JsonConversionExtensions
{
    public readonly static JavaScriptEncoder Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

    public static JsonSerializerOptions CreateJsonSerializerOptions(bool camelCase = false)
    {
        JsonSerializerOptions options = new() 
        { 
            Encoder = Encoder 
        };
        if (camelCase)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        }
        options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
        return options;
    }

    public static string ToJsonString(this RateContent content, bool camelCase = false)
    {
        var options = CreateJsonSerializerOptions(camelCase);
        return JsonSerializer.Serialize(content, options);
    }

    public static T? ParseJson<T>(this string json, bool camelCase = false)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;

        var options = CreateJsonSerializerOptions(camelCase);
        return JsonSerializer.Deserialize<T>(json, options);
    }
}

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    private readonly string _dateFormat;

    public DateTimeJsonConverter()
    {
        _dateFormat = "yyyy-MM-dd HH:mm:ss";
    }

    public DateTimeJsonConverter(string dateFormat)
    {
        _dateFormat = dateFormat;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.ParseExact(reader.GetString()!, _dateFormat, null);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_dateFormat));
        //writer.WriteStringValue(value.ToString(_dateFormat, System.Globalization.CultureInfo.InvariantCulture));
        //writer.WriteStringValue(value.ToUniversalTime().ToString(_dateFormat));
    }
}