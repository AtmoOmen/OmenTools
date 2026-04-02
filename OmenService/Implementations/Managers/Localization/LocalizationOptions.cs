using System.Collections.Frozen;
using Lumina.Data;
using OmenTools.Localization.Abstractions;

namespace OmenTools.OmenService;

public sealed class LocalizationOptions
{
    public required FrozenDictionary<Language, string> SupportedLanguages { get; init; }

    public required Language DefaultLanguage { get; init; }

    public required Func<Language, string> FileNameResolver { get; init; }

    public required ILocalizationSource Source { get; init; }

    public required ILocalizationParser Parser { get; init; }

    public required Func<Language, IEnumerable<Language>> FallbackResolver { get; init; }

    public bool EnableHotReload { get; init; } = true;

    public TimeSpan ReloadDebounce { get; init; } = TimeSpan.FromSeconds(3);

    public string LoggerTag { get; init; } = nameof(LocalizationManager);
}
