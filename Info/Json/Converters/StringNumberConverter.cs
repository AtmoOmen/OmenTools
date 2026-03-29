using System.Globalization;
using Newtonsoft.Json;

namespace OmenTools.Info.Json.Converters;

public class StringNumberConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(int)    ||
        objectType == typeof(long)   ||
        objectType == typeof(uint)   ||
        objectType == typeof(ulong)  ||
        objectType == typeof(float)  ||
        objectType == typeof(double) ||
        objectType == typeof(decimal);

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return reader.TokenType switch
        {
            JsonToken.String  => ReadFromString(reader.Value?.ToString() ?? string.Empty, objectType),
            JsonToken.Integer => ConvertFromNumber(reader.Value, objectType),
            JsonToken.Float   => ConvertFromNumber(reader.Value, objectType),
            _                 => 0
        };
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        writer.WriteValue(Convert.ToString(value, CultureInfo.InvariantCulture));

    private static object ReadFromString(string value, Type objectType)
    {
        if (objectType == typeof(int)) return int.Parse(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(long)) return long.Parse(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(uint)) return uint.Parse(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(ulong)) return ulong.Parse(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(float)) return float.Parse(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(double)) return double.Parse(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(decimal)) return decimal.Parse(value, CultureInfo.InvariantCulture);

        return 0;
    }

    private static object ConvertFromNumber(object? value, Type objectType)
    {
        if (objectType == typeof(int)) return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(long)) return Convert.ToInt64(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(uint)) return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(ulong)) return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(float)) return Convert.ToSingle(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(double)) return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        if (objectType == typeof(decimal)) return Convert.ToDecimal(value, CultureInfo.InvariantCulture);

        return 0;
    }
}
