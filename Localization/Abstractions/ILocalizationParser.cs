using System.Collections.Frozen;

namespace OmenTools.Localization.Abstractions;

public interface ILocalizationParser
{
    FrozenDictionary<string, string> Parse(Stream stream);
}
