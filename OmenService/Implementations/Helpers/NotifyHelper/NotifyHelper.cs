using System.Runtime.CompilerServices;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Client.UI;
using OmenTools.Interop.Windows;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public class NotifyHelper : OmenServiceBase<NotifyHelper>
{
    public string? NotificationMinimizedText { get; set; }

    public INotificationIcon? NotificationIconSource { get; set; }

    public ISharedImmediateTexture? NotificationIcon { get; set; }

    public DateTime NotificationHardExpiry { get; set; } = DateTime.MaxValue;

    public TimeSpan NotificationInitialDuration { get; set; } = TimeSpan.FromSeconds(3);

    public TimeSpan NotificationExtensionDuration { get; set; } = TimeSpan.FromSeconds(1);

    public bool NotificationShowIndeterminateIfNoExpiry { get; set; } = true;

    public bool NotificationRespectUIHidden { get; set; } = true;

    public bool NotificationMinimized { get; set; }

    public bool UserDismissable { get; set; } = true;

    public float NotificationProgress { get; set; } = 1f;

    public bool EnableTrayNotification { get; set; }

    public TimeSpan ContentHintDuration { get; set; }

    public TrayNotifier? TrayNotifier { get; set; }

    public SeString? ChatPrefix { get; set; }

    public ushort ChatErrorTextColor { get; set; }

    public ushort ChatErrorSeStringTextColor { get; set; }

    protected override void Uninit()
    {
        TrayNotifier?.Dispose();

        NotificationMinimizedText  = null;
        NotificationIconSource     = null;
        TrayNotifier               = null;
        NotificationIcon           = null;
        ChatPrefix                 = null;
        ChatErrorTextColor         = 0;
        ChatErrorSeStringTextColor = 0;
    }

    #region Toast

    public static void Toast(string message, ToastOptions? options = null) =>
        DService.Instance().Toast.ShowNormal(message, options);

    public static void Toast(SeString message, ToastOptions? options = null) =>
        DService.Instance().Toast.ShowNormal(message, options);

    public static void ToastError(string message) =>
        DService.Instance().Toast.ShowError(message);

    public static void ToastError(SeString message) =>
        DService.Instance().Toast.ShowError(message);

    public static void ToastQuest(string message, QuestToastOptions? options = null) =>
        DService.Instance().Toast.ShowQuest(message, options);

    public static void ToastQuest(SeString message, QuestToastOptions? options = null) =>
        DService.Instance().Toast.ShowQuest(message, options);

    #endregion

    #region Content Hint

    /// <summary>
    ///     显示游戏内悬浮文本提示。
    /// </summary>
    public static unsafe void ContentHint
    (
        string                                message,
        RaptureAtkModule.TextGimmickHintStyle style    = RaptureAtkModule.TextGimmickHintStyle.Info,
        TimeSpan?                             duration = null
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        var hundredMilliseconds = ToHundredMilliseconds(duration ?? Instance().ContentHintDuration);
        RaptureAtkModule.Instance()->ShowTextGimmickHint(message, style, hundredMilliseconds);
    }

    public static void ContentHintBlue(string message, TimeSpan? duration = null) =>
        ContentHint(message, RaptureAtkModule.TextGimmickHintStyle.Info, duration);

    public static void ContentHintRed(string message, TimeSpan? duration = null) =>
        ContentHint(message, RaptureAtkModule.TextGimmickHintStyle.Warning, duration);

    #endregion

    #region Notification

    /// <summary>
    ///     发送 Dalamud 通知，并按需补充系统托盘提醒。
    /// </summary>
    public static void Notify
    (
        string               message,
        NotificationType     type    = NotificationType.Info,
        string?              title   = null,
        NotificationOptions? options = null
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        var helper = Instance();
        var plan   = BuildNotificationPlan(helper, message, type, title, options);

        DService.Instance().DalamudNotification.AddNotification
        (
            new()
            {
                Title                              = plan.Title,
                MinimizedText                      = plan.MinimizedText,
                Content                            = plan.Message,
                Type                               = plan.Type,
                Icon                               = plan.IconSource,
                Minimized                          = plan.Minimized,
                UserDismissable                    = plan.UserDismissable,
                Progress                           = plan.Progress,
                IconTexture                        = plan.Icon,
                HardExpiry                         = plan.HardExpiry,
                InitialDuration                    = plan.InitialDuration,
                ExtensionDurationSinceLastInterest = plan.ExtensionDuration,
                ShowIndeterminateIfNoExpiry        = plan.ShowIndeterminateIfNoExpiry,
                RespectUiHidden                    = plan.RespectUIHidden
            }
        );

        TryShowTrayNotification(helper, plan);
    }

    public static void NotificationSuccess(string message, string? title = null, NotificationOptions? options = null) =>
        Notify(message, NotificationType.Success, title, options);

    public static void NotificationWarning(string message, string? title = null, NotificationOptions? options = null) =>
        Notify(message, NotificationType.Warning, title, options);

    public static void NotificationError(string message, string? title = null, NotificationOptions? options = null) =>
        Notify(message, NotificationType.Error, title, options);

    public static void NotificationInfo(string message, string? title = null, NotificationOptions? options = null) =>
        Notify(message, NotificationType.Info, title, options);

    #endregion

    #region Chat

    /// <summary>
    ///     输出错误聊天文本，可选前缀与颜色。
    /// </summary>
    public static void ChatError(string message, SeString? prefix = null, ushort? textColor = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        PrintChat(message, prefix, true, textColor ?? Instance().ChatErrorTextColor);
    }

    /// <summary>
    ///     输出带富文本的错误聊天消息，仅对纯文本片段着色。
    /// </summary>
    public static void ChatError(SeString message, SeString? prefix = null, ushort? rawTextColor = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        PrintChat(message, prefix, true, rawTextColor ?? Instance().ChatErrorSeStringTextColor);
    }

    /// <summary>
    ///     输出普通聊天文本，可选前缀与颜色。
    /// </summary>
    public static void Chat(string message, SeString? prefix = null, ushort? textColor = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        PrintChat(message, prefix, false, textColor);
    }

    /// <summary>
    ///     输出带富文本的普通聊天消息。
    /// </summary>
    public static void Chat(SeString message, SeString? prefix = null, ushort? rawTextColor = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        PrintChat(message, prefix, false, rawTextColor);
    }

    #endregion

    #region Helpers

    private static NotificationPlan BuildNotificationPlan
    (
        NotifyHelper         helper,
        string               message,
        NotificationType     type,
        string?              title,
        NotificationOptions? options
    )
    {
        var resolvedTitle = string.IsNullOrWhiteSpace(title) ? message : title;

        return new
        (
            resolvedTitle,
            message,
            type,
            options?.MinimizedText               ?? helper.NotificationMinimizedText,
            options?.IconSource                  ?? helper.NotificationIconSource,
            options?.Minimized                   ?? helper.NotificationMinimized,
            options?.Icon                        ?? helper.NotificationIcon,
            options?.HardExpiry                  ?? helper.NotificationHardExpiry,
            options?.InitialDuration             ?? helper.NotificationInitialDuration,
            options?.ExtensionDuration           ?? helper.NotificationExtensionDuration,
            options?.ShowIndeterminateIfNoExpiry ?? helper.NotificationShowIndeterminateIfNoExpiry,
            options?.RespectUIHidden             ?? helper.NotificationRespectUIHidden,
            options?.UserDismissable             ?? helper.UserDismissable,
            options?.Progress                    ?? helper.NotificationProgress,
            options?.ShowTray                    ?? helper.EnableTrayNotification,
            options?.TrayOnlyWhenBackground      ?? type is not NotificationType.Success,
            options?.TrayIcon                    ?? GetTrayIcon(type)
        );
    }

    private static void TryShowTrayNotification(NotifyHelper helper, NotificationPlan plan)
    {
        if (!plan.ShowTray || helper.TrayNotifier is not { } trayNotifier)
            return;

        if (plan.TrayOnlyWhenBackground && GameState.IsForeground)
            return;

        trayNotifier.ShowBalloonTip(plan.Title, plan.Message, plan.TrayIcon);
    }

    private static ToolTipIcon GetTrayIcon(NotificationType type) => type switch
    {
        NotificationType.Warning => ToolTipIcon.Warning,
        NotificationType.Error   => ToolTipIcon.Error,
        _                        => ToolTipIcon.Info
    };

    private static void PrintChat(string message, SeString? prefix, bool isError, ushort? textColor)
    {
        var builder = new SeStringBuilder();
        AppendPrefix(builder, prefix ?? Instance().ChatPrefix);

        if (textColor is { } color)
            builder.AddUiForeground(message, color);
        else
            builder.Append(message);

        var chat = DService.Instance().Chat;
        if (isError)
            chat.PrintError(builder.Build());
        else
            chat.Print(builder.Build());
    }

    private static void PrintChat(SeString message, SeString? prefix, bool isError, ushort? rawTextColor)
    {
        var builder = new SeStringBuilder();
        AppendPrefix(builder, prefix ?? Instance().ChatPrefix);

        foreach (var payload in message.Payloads)
        {
            if (rawTextColor is { } color && payload is TextPayload textPayload)
            {
                builder.AddUiForeground(textPayload.Text, color);
                continue;
            }

            builder.Add(payload);
        }

        var chat = DService.Instance().Chat;
        if (isError)
            chat.PrintError(builder.Build());
        else
            chat.Print(builder.Build());
    }

    private static void AppendPrefix(SeStringBuilder builder, SeString? prefix)
    {
        if (prefix is null)
            return;

        builder.Append(prefix).Append(" ");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToHundredMilliseconds(TimeSpan ts)
    {
        const long TICKS_PER_HUNDRED_MILLISECONDS = TimeSpan.TicksPerMillisecond * 100;

        if (ts <= TimeSpan.Zero)
            return 0;

        var result = (int)(ts.Ticks / TICKS_PER_HUNDRED_MILLISECONDS);
        return result == 0 ? 1 : result;
    }

    private readonly record struct NotificationPlan
    (
        string                   Title,
        string                   Message,
        NotificationType         Type,
        string?                  MinimizedText,
        INotificationIcon?       IconSource,
        bool                     Minimized,
        ISharedImmediateTexture? Icon,
        DateTime                 HardExpiry,
        TimeSpan                 InitialDuration,
        TimeSpan                 ExtensionDuration,
        bool                     ShowIndeterminateIfNoExpiry,
        bool                     RespectUIHidden,
        bool                     UserDismissable,
        float                    Progress,
        bool                     ShowTray,
        bool                     TrayOnlyWhenBackground,
        ToolTipIcon              TrayIcon
    );

    #endregion
}
