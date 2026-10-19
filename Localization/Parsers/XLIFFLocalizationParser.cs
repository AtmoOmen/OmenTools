using System.Collections.Frozen;
using System.Xml;
using System.Xml.Linq;
using OmenTools.Localization.Abstractions;

namespace OmenTools.Localization.Parsers;

public sealed class XLIFFLocalizationParser : ILocalizationParser
{
    public FrozenDictionary<string, string> Parse
    (
        Stream stream
    )
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = XmlReader.Create
        (
            stream,
            new()
            {
                CloseInput     = false,
                DtdProcessing  = DtdProcessing.Prohibit,
                IgnoreComments = true,
                XmlResolver    = null
            }
        );

        var        document     = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
        XNamespace xmlNamespace = XLIFF_NAMESPACE;
        if (document.Root?.Name != xmlNamespace + "xliff")
            throw new XmlException("Expected an XLIFF 1.2 document.");

        Dictionary<string, string> result = new(StringComparer.Ordinal);

        foreach (var file in document.Root.Elements(xmlNamespace + "file"))
        {
            var valueName = xmlNamespace +
                            (file.Attribute("target-language") == null ?
                                 "source" :
                                 "target");

            foreach (var entry in file.Descendants(xmlNamespace + "trans-unit"))
            {
                var key = entry.Attribute("id")?.Value;
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                var value = entry.Element(valueName);
                if (value != null)
                    result[key] = value.Value;
            }
        }

        return result.ToFrozenResource();
    }

    #region 常量

    private const string XLIFF_NAMESPACE = "urn:oasis:names:tc:xliff:document:1.2";

    #endregion
}
