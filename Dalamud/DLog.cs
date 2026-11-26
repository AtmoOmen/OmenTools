#if DEBUG
using System.Runtime.CompilerServices;
#endif

namespace OmenTools.Dalamud;

/// <summary>
///     对 Dalamud 日志信息输出的包装。
/// </summary>
public static class DLog
{
#if DEBUG
    /// <summary>记录详细日志，并附加调用处的文件名、行号和成员名。</summary>
    /// <param name="message">日志消息。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Verbose
    (
        string                    message,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Verbose($"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] {message}");

    /// <summary>记录详细异常日志及补充消息，并附加调用位置。</summary>
    /// <param name="message">补充日志消息。</param>
    /// <param name="ex">需要记录的异常。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Verbose
    (
        string                    message,
        Exception                 ex,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Verbose(ex, $"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] 发生异常。\n{message}");

    /// <summary>记录详细异常日志，并附加调用位置。</summary>
    /// <param name="ex">需要记录的异常。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Verbose
    (
        Exception                 ex,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Verbose(ex, $"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] 发生异常。");


    /// <summary>记录调试日志，并附加调用处的文件名、行号和成员名。</summary>
    /// <param name="message">日志消息。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Debug
    (
        string                    message,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Debug($"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] {message}");

    /// <summary>记录调试异常日志及补充消息，并附加调用位置。</summary>
    /// <param name="message">补充日志消息。</param>
    /// <param name="ex">需要记录的异常。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Debug
    (
        string                    message,
        Exception                 ex,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Debug(ex, $"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] 发生异常。\n{message}");

    /// <summary>记录调试异常日志，并附加调用位置。</summary>
    /// <param name="ex">需要记录的异常。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Debug
    (
        Exception                 ex,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Debug(ex, $"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] 发生异常。");


    /// <summary>记录警告日志，并附加调用处的文件名、行号和成员名。</summary>
    /// <param name="message">日志消息。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Warning
    (
        string                    message,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Warning($"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] {message}");

    /// <summary>记录警告异常日志及补充消息，并附加调用位置。</summary>
    /// <param name="message">补充日志消息。</param>
    /// <param name="ex">需要记录的异常。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Warning
    (
        string                    message,
        Exception                 ex,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Warning(ex, $"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] 发生异常。\n{message}");

    /// <summary>记录警告异常日志，并附加调用位置。</summary>
    /// <param name="ex">需要记录的异常。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Warning
    (
        Exception                 ex,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Warning(ex, $"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] 发生异常。");


    /// <summary>记录错误日志，并附加调用处的文件名、行号和成员名。</summary>
    /// <param name="message">日志消息。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Error
    (
        string                    message,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Error($"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] {message}");

    /// <summary>记录错误异常日志及补充消息，并附加调用位置。</summary>
    /// <param name="message">补充日志消息。</param>
    /// <param name="ex">需要记录的异常。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Error
    (
        string                    message,
        Exception                 ex,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Error(ex, $"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] 发生异常。\n{message}");

    /// <summary>记录错误异常日志，并附加调用位置。</summary>
    /// <param name="ex">需要记录的异常。</param>
    /// <param name="callerMemberName">由编译器填写的调用成员名。</param>
    /// <param name="callerFilePath">由编译器填写的调用源文件路径。</param>
    /// <param name="callerLineNumber">由编译器填写的调用源代码行号。</param>
    public static void Error
    (
        Exception                 ex,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath]   string callerFilePath   = "",
        [CallerLineNumber] int    callerLineNumber = 0
    ) =>
        IPluginLog.Instance().Error(ex, $"[{Path.GetFileName(callerFilePath)}:{callerLineNumber} {callerMemberName}] 发生异常。");
#else
    /// <summary>记录详细日志。</summary>
    /// <param name="message">日志消息。</param>
    public static void Verbose(string message) =>
        IPluginLog.Instance().Verbose(message);

    /// <summary>记录详细异常日志及补充消息。</summary>
    /// <param name="message">补充日志消息。</param>
    /// <param name="ex">需要记录的异常。</param>
    public static void Verbose(string message, Exception ex) =>
        IPluginLog.Instance().Verbose(ex, $"发生异常。\n{message}");

    /// <summary>记录详细异常日志。</summary>
    /// <param name="ex">需要记录的异常。</param>
    public static void Verbose(Exception ex) =>
        IPluginLog.Instance().Verbose(ex, "发生异常。");


    /// <summary>记录调试日志。</summary>
    /// <param name="message">日志消息。</param>
    public static void Debug(string message) =>
        IPluginLog.Instance().Debug(message);

    /// <summary>记录调试异常日志及补充消息。</summary>
    /// <param name="message">补充日志消息。</param>
    /// <param name="ex">需要记录的异常。</param>
    public static void Debug(string message, Exception ex) =>
        IPluginLog.Instance().Debug(ex, $"发生异常。\n{message}");

    /// <summary>记录调试异常日志。</summary>
    /// <param name="ex">需要记录的异常。</param>
    public static void Debug(Exception ex) =>
        IPluginLog.Instance().Debug(ex, "发生异常。");


    /// <summary>记录警告日志。</summary>
    /// <param name="message">日志消息。</param>
    public static void Warning(string message) =>
        IPluginLog.Instance().Warning(message);

    /// <summary>记录警告异常日志及补充消息。</summary>
    /// <param name="message">补充日志消息。</param>
    /// <param name="ex">需要记录的异常。</param>
    public static void Warning(string message, Exception ex) =>
        IPluginLog.Instance().Warning(ex, $"发生异常。\n{message}");

    /// <summary>记录警告异常日志。</summary>
    /// <param name="ex">需要记录的异常。</param>
    public static void Warning(Exception ex) =>
        IPluginLog.Instance().Warning(ex, "发生异常。");


    /// <summary>记录错误日志。</summary>
    /// <param name="message">日志消息。</param>
    public static void Error(string message) =>
        IPluginLog.Instance().Error(message);

    /// <summary>记录错误异常日志及补充消息。</summary>
    /// <param name="message">补充日志消息。</param>
    /// <param name="ex">需要记录的异常。</param>
    public static void Error(string message, Exception ex) =>
        IPluginLog.Instance().Error(ex, $"发生异常。\n{message}");

    /// <summary>记录错误异常日志。</summary>
    /// <param name="ex">需要记录的异常。</param>
    public static void Error(Exception ex) =>
        IPluginLog.Instance().Error(ex, "发生异常。");
#endif
}
