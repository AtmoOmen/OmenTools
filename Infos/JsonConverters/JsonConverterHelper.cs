using Newtonsoft.Json;

namespace OmenTools.Infos;

public partial class InfosOm
{
    public static JsonSerializerSettings JsonSettings { get; } = new()
    {
        Converters =
        {
            new Vector2Converter(),
            new Vector3Converter(),
            new Vector4Converter(),
            new TimeSpanConverter(),
            new VersionConverter()
        }
    };
}
