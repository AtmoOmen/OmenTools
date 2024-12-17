using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.ImGuiNotification;
using FFXIVClientStructs.FFXIV.Client.UI;
namespace OmenTools.Helpers;

public static class NotifyHelper
{
    public static unsafe void ContentHintBlue(string message, int hundredMS = 30) =>
        RaptureAtkModule.Instance()->ShowTextGimmickHint(message, RaptureAtkModule.TextGimmickHintStyle.Info, hundredMS);

    public static unsafe void ContentHintRed(string message, int hundredMS = 30) =>
        RaptureAtkModule.Instance()->ShowTextGimmickHint(message, RaptureAtkModule.TextGimmickHintStyle.Warning, hundredMS);

    public static void NotificationSuccess(string message, string? title = null) => DService.DNotice.AddNotification(new()
    {
        Title = title ?? message,
        Content = message,
        Type = NotificationType.Success,
        Minimized = false,
        InitialDuration = TimeSpan.FromSeconds(3),
        ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
    });

    public static void NotificationWarning(string message, string? title = null) => DService.DNotice.AddNotification(new()
    {
        Title = title ?? message,
        Content = message,
        Type = NotificationType.Warning,
        Minimized = false,
        InitialDuration = TimeSpan.FromSeconds(3),
        ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
    });

    public static void NotificationError(string message, string? title = null) => DService.DNotice.AddNotification(new()
    {
        Title = title ?? message,
        Content = message,
        Type = NotificationType.Error,
        Minimized = false,
        InitialDuration = TimeSpan.FromSeconds(3),
        ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
    });

    public static void NotificationInfo(string message, string? title = null) => DService.DNotice.AddNotification(new()
    {
        Title = title ?? message,
        Content = message,
        Type = NotificationType.Info,
        Minimized = false,
        InitialDuration = TimeSpan.FromSeconds(3),
        ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
    });

    public static void ChatError(string message, SeString prefix) =>
        DService.Chat.PrintError(new SeStringBuilder().Append(prefix).AddUiForeground($" {message}", 518).Build());

    public static void ChatError(SeString message, SeString prefix)
    {
        var builder = new SeStringBuilder();
        builder.Append(prefix).Append(" ");
        foreach (var payload in message.Payloads)
            if (payload.Type == PayloadType.RawText)
                builder.AddUiForeground($" {((TextPayload)payload).Text}", 518);
            else builder.Add(payload);
    }

    public static void Chat(string message, SeString prefix) =>
        DService.Chat.Print(new SeStringBuilder().Append(prefix).Append($" {message}").Build());

    public static void Chat(SeString message, SeString prefix) =>
        DService.Chat.Print(new SeStringBuilder().Append(prefix).Append(" ").Append(message).Build());

    public static void Verbose(string message) => DService.Log.Verbose(message);

    public static void Verbose(string message, Exception ex) => DService.Log.Verbose(ex, message);

    public static void Debug(string message) => DService.Log.Debug(message);

    public static void Debug(string message, Exception ex) => DService.Log.Debug(ex, message);

    public static void Warning(string message) => DService.Log.Warning(message);

    public static void Warning(string message, Exception ex) => DService.Log.Warning(ex, message);

    public static void Error(string message) => DService.Log.Error(message);

    public static void Error(string message, Exception ex) => DService.Log.Error(ex, message);
}
