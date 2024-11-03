using System.ComponentModel;
using System.Text.Json;

namespace OmenTools.Infos;

public partial class InfosOm
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters =
        {
            new Vector2Converter(),
            new Vector3Converter(),
            new Vector4Converter(),
            new DateTimeConverter(),
            new TimeSpanConverter(),
            new VersionConverter(),
        }
    };
}