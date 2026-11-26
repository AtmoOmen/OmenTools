using System.Runtime.CompilerServices;

namespace OmenTools.Dalamud;

/// <summary>
///     对 Dalamud 日志信息输出的包装。
/// </summary>
public static class DLog
{
    public static void Verbose(string message) =>
        IPluginLog.Instance().Verbose(message);

    public static void Verbose(string message, Exception ex, [CallerMemberName] string callerMemberName = "") =>
        IPluginLog.Instance().Verbose(ex, $"{callerMemberName} 发生异常。\n{message}");

    public static void Verbose(Exception ex, [CallerMemberName] string callerMemberName = "") =>
        IPluginLog.Instance().Verbose(ex, $"{callerMemberName} 发生异常。");


    public static void Debug(string message) =>
        IPluginLog.Instance().Debug(message);

    public static void Debug(string message, Exception ex, [CallerMemberName] string callerMemberName = "") =>
        IPluginLog.Instance().Debug(ex, $"{callerMemberName} 发生异常。\n{message}");

    public static void Debug(Exception ex, [CallerMemberName] string callerMemberName = "") =>
        IPluginLog.Instance().Debug(ex, $"{callerMemberName} 发生异常。");


    public static void Warning(string message) =>
        IPluginLog.Instance().Warning(message);

    public static void Warning(string message, Exception ex, [CallerMemberName] string callerMemberName = "") =>
        IPluginLog.Instance().Warning(ex, $"{callerMemberName} 发生异常。\n{message}");

    public static void Warning(Exception ex, [CallerMemberName] string callerMemberName = "") =>
        IPluginLog.Instance().Warning(ex, $"{callerMemberName} 发生异常。");


    public static void Error(string message) =>
        IPluginLog.Instance().Error(message);

    public static void Error(string message, Exception ex, [CallerMemberName] string callerMemberName = "") =>
        IPluginLog.Instance().Error(ex, $"{callerMemberName} 发生异常。\n{message}");

    public static void Error(Exception ex, [CallerMemberName] string callerMemberName = "") =>
        IPluginLog.Instance().Error(ex, $"{callerMemberName} 发生异常。");
}
