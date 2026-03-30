using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Textures;

namespace OmenTools.OmenService;

/// <summary>
///     单次通知的可选覆盖项。
/// </summary>
public sealed record NotificationOptions
{
    public string? MinimizedText { get; init; }

    public INotificationIcon? IconSource { get; init; }

    public ISharedImmediateTexture? Icon { get; init; }

    public DateTime? HardExpiry { get; init; }

    public TimeSpan? InitialDuration { get; init; }

    public TimeSpan? ExtensionDuration { get; init; }

    public bool? ShowIndeterminateIfNoExpiry { get; init; }

    public bool? RespectUIHidden { get; init; }

    public bool? Minimized { get; init; }

    public bool? UserDismissable { get; init; }

    public float? Progress { get; init; }

    public bool? ShowTray { get; init; }

    public bool? TrayOnlyWhenBackground { get; init; }

    public ToolTipIcon? TrayIcon { get; init; }
}
