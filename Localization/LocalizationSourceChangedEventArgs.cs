namespace OmenTools.Localization;

public sealed class LocalizationSourceChangedEventArgs
(
    string             resourceName,
    WatcherChangeTypes changeType
) : EventArgs
{
    public string ResourceName { get; } = resourceName;

    public WatcherChangeTypes ChangeType { get; } = changeType;
}
