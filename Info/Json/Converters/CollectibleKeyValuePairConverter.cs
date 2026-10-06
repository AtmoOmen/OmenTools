using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OmenTools.Info.Json.Converters;

internal sealed class CollectibleKeyValuePairConverter
(
    Type type
) : JsonConverter
{
    private readonly Type[]          arguments     = type.GenericTypeArguments;
    private readonly PropertyInfo    keyProperty   = type.GetProperty("Key");
    private readonly PropertyInfo    valueProperty = type.GetProperty("Value");
    private readonly ConstructorInfo constructor   = type.GetConstructor(type.GenericTypeArguments);

    private readonly Action<JsonReader, JsonContract, bool> readForType =
        typeof(JsonReader).GetMethod
                          (
                              "ReadForTypeAndAssert",
                              BindingFlags.Instance | BindingFlags.NonPublic
                          )
                          .CreateDelegate<Action<JsonReader, JsonContract, bool>>();

    public override bool CanConvert
    (
        Type objectType
    )
    {
        var type = Nullable.GetUnderlyingType(objectType) ?? objectType;
        return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
    }

    public override void WriteJson
    (
        JsonWriter     writer,
        object?        value,
        JsonSerializer serializer
    )
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var resolver = serializer.ContractResolver as DefaultContractResolver;
        writer.WriteStartObject();
        writer.WritePropertyName(resolver?.GetResolvedPropertyName("Key") ?? "Key");
        serializer.Serialize(writer, keyProperty.GetValue(value), arguments[0]);
        writer.WritePropertyName(resolver?.GetResolvedPropertyName("Value") ?? "Value");
        serializer.Serialize(writer, valueProperty.GetValue(value), arguments[1]);
        writer.WriteEndObject();
    }

    public override object ReadJson
    (
        JsonReader     reader,
        Type           objectType,
        object?        existingValue,
        JsonSerializer serializer
    )
    {
        var nullableType = Nullable.GetUnderlyingType(objectType);

        if (reader.TokenType == JsonToken.Null)
        {
            return nullableType != null ?
                       null :
                       throw new JsonSerializationException("Cannot convert null value to KeyValuePair.");
        }

        var    keyContract   = serializer.ContractResolver.ResolveContract(arguments[0]);
        var    valueContract = serializer.ContractResolver.ResolveContract(arguments[1]);
        object key           = null;
        object value         = null;
        if (!reader.Read())
            throw new JsonSerializationException("Unexpected end when reading KeyValuePair.");

        while (reader.TokenType == JsonToken.PropertyName)
        {
            var name = reader.Value?.ToString();

            if (string.Equals(name, "Key", StringComparison.OrdinalIgnoreCase))
            {
                readForType(reader, keyContract, false);
                key = serializer.Deserialize(reader, arguments[0]);
            }
            else if (string.Equals(name, "Value", StringComparison.OrdinalIgnoreCase))
            {
                readForType(reader, valueContract, false);
                value = serializer.Deserialize(reader, arguments[1]);
            }
            else
                reader.Skip();

            if (!reader.Read())
                throw new JsonSerializationException("Unexpected end when reading KeyValuePair.");
        }

        return constructor.Invoke([key, value]);
    }
}
