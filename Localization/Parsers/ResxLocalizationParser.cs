using System.Collections.Frozen;
using System.Xml;
using System.Xml.Linq;
using OmenTools.Localization.Abstractions;

namespace OmenTools.Localization.Parsers;

public sealed class ResxLocalizationParser : ILocalizationParser
{
    public FrozenDictionary<string, string> Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = XmlReader.Create
        (
            stream,
            new()
            {
                CloseInput       = false,
                DtdProcessing    = DtdProcessing.Ignore,
                IgnoreComments   = true,
                IgnoreWhitespace = true,
                XmlResolver      = null
            }
        );

        var                        document = XDocument.Load(reader);
        Dictionary<string, string> result   = new(StringComparer.Ordinal);

        foreach (var entry in document.Root?.Elements("data") ?? [])
        {
            var key = entry.Attribute("name")?.Value;
            if (string.IsNullOrWhiteSpace(key))
                continue;

            var valueElement = entry.Element("value");
            if (valueElement == null)
                continue;

            result[key] = valueElement.Value;
        }

        return result.ToFrozenResource();
    }
}
