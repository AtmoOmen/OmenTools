using Newtonsoft.Json;
using OmenTools.Info.Json;
using OmenTools.Info.Json.Converters;

namespace OmenTools.Extensions;

public static class JsonSerializerSettingsExtension
{
    private static JsonSerializerSettings SharedJSONSettings { get; } = new()
    {
        ContractResolver = new CollectibleContractResolver(),
        Converters =
        {
            new Vector2Converter(),
            new Vector3Converter(),
            new Vector4Converter(),
            new TimeSpanConverter(),
            new VersionConverter()
        }
    };

    extension
    (
        JsonSerializerSettings
    )
    {
        public static JsonSerializerSettings GetShared() => SharedJSONSettings;
    }
}
