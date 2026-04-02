using Lumina.Data;

namespace OmenTools.Localization.Abstractions;

public interface ILocalizationSource : IDisposable
{
    bool SupportsChangeNotifications { get; }

    event EventHandler<LocalizationSourceChangedEventArgs>? ResourceChanged;

    bool Exists(Language language, string resourceName);

    Stream? OpenRead(Language language, string resourceName);
}
