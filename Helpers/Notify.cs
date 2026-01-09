using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static unsafe void ContentHintBlue(string message, int hundredMS = 30) =>
        RaptureAtkModule.Instance()->ShowTextGimmickHint(message, RaptureAtkModule.TextGimmickHintStyle.Info, hundredMS);

    public static unsafe void ContentHintRed(string message, int hundredMS = 30) =>
        RaptureAtkModule.Instance()->ShowTextGimmickHint(message, RaptureAtkModule.TextGimmickHintStyle.Warning, hundredMS);

    public static void NotificationSuccess(string message, string? title = null)
    {
        DService.Instance().DalamudNotification.AddNotification(new()
        {
            Title                              = title ?? message,
            Content                            = message,
            Type                               = NotificationType.Success,
            Minimized                          = false,
            IconTexture                        = DService.Instance().DalamudNotificationIcon,
            InitialDuration                    = TimeSpan.FromSeconds(3),
            ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
        });
        
        if (!GameState.IsForeground && DService.Instance().TrayNotification is { } tray) 
            tray.ShowBalloonTip(title ?? message, message);
    }

    public static void NotificationWarning(string message, string? title = null)
    {
        DService.Instance().DalamudNotification.AddNotification(new()
        {
            Title                              = title ?? message,
            Content                            = message,
            Type                               = NotificationType.Warning,
            Minimized                          = false,
            IconTexture                        = DService.Instance().DalamudNotificationIcon,
            InitialDuration                    = TimeSpan.FromSeconds(3),
            ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
        });
        
        if (!GameState.IsForeground && DService.Instance().TrayNotification is { } tray) 
            tray.ShowBalloonTip(title ?? message, message, ToolTipIcon.Warning);
    }

    public static void NotificationError(string message, string? title = null)
    {
        DService.Instance().DalamudNotification.AddNotification(new()
        {
            Title                              = title ?? message,
            Content                            = message,
            Type                               = NotificationType.Error,
            Minimized                          = false,
            IconTexture                        = DService.Instance().DalamudNotificationIcon,
            InitialDuration                    = TimeSpan.FromSeconds(3),
            ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
        });
        
        if (!GameState.IsForeground && DService.Instance().TrayNotification is { } tray) 
            tray.ShowBalloonTip(title ?? message, message, ToolTipIcon.Error);
    }

    public static void NotificationInfo(string message, string? title = null)
    {
        DService.Instance().DalamudNotification.AddNotification(new()
        {
            Title                              = title ?? message,
            Content                            = message,
            Type                               = NotificationType.Info,
            Minimized                          = false,
            IconTexture                        = DService.Instance().DalamudNotificationIcon,
            InitialDuration                    = TimeSpan.FromSeconds(3),
            ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
        });
        
        if (!GameState.IsForeground && DService.Instance().TrayNotification is { } tray) 
            tray.ShowBalloonTip(title ?? message, message);
    }

    public static void ChatError(string message, SeString? prefix = null)
    {
        var builder = new SeStringBuilder();
        if (prefix != null) 
            builder.Append(prefix).Append(" ");
        builder.AddUiForeground($"{message}", 518);
        DService.Instance().Chat.PrintError(builder.Build());
    }

    public static void ChatError(SeString message, SeString? prefix = null)
    {
        var builder = new SeStringBuilder();
        if (prefix != null) 
            builder.Append(prefix);

        var isFirstPayload = prefix != null;
        foreach (var payload in message.Payloads)
        {
            if (isFirstPayload)
            {
                builder.Append(" ");
                isFirstPayload = false;
            }
            
            if (payload.Type == PayloadType.RawText)
                builder.AddUiForeground($"{((TextPayload)payload).Text}", 518);
            else 
                builder.Add(payload);
        }
        
        DService.Instance().Chat.PrintError(builder.Build());
    }

    public static void Chat(string message, SeString? prefix = null)
    {
        var builder = new SeStringBuilder();
        if (prefix != null) 
            builder.Append(prefix).Append(" ");
        builder.Append($"{message}");
        DService.Instance().Chat.Print(builder.Build());
    }

    public static void Chat(SeString message, SeString? prefix = null)
    {
        var builder = new SeStringBuilder();
        if (prefix != null) 
            builder.Append(prefix);

        var isFirstPayload = prefix != null;
        foreach (var payload in message.Payloads)
        {
            if (isFirstPayload)
            {
                builder.Append(" ");
                isFirstPayload = false;
            }
            
            builder.Add(payload);
        }
        
        DService.Instance().Chat.Print(builder.Build());
    }

    public static void Verbose(string message) 
        => DService.Instance().Log.Verbose(message);

    public static void Verbose(string message, Exception ex) 
        => DService.Instance().Log.Verbose(ex, message);

    public static void Debug(string message) 
        => DService.Instance().Log.Debug(message);

    public static void Debug(string message, Exception ex) 
        => DService.Instance().Log.Debug(ex, message);

    public static void Warning(string message) 
        => DService.Instance().Log.Warning(message);

    public static void Warning(string message, Exception ex) 
        => DService.Instance().Log.Warning(ex, message);

    public static void Error(string message) 
        => DService.Instance().Log.Error(message);

    public static void Error(string message, Exception ex) 
        => DService.Instance().Log.Error(ex, message);
}
