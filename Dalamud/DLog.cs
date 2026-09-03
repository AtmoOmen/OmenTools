namespace OmenTools.Dalamud;

/// <summary>
///     对 Dalamud 日志信息输出的包装
/// </summary>
public static class DLog
{
    public static void Verbose(string message) =>
        IPluginLog.Instance().Verbose(message);

    public static void Verbose(string message, Exception ex) =>
        IPluginLog.Instance().Verbose(ex, message);

    public static void Verbose(Exception ex) =>
        IPluginLog.Instance().Verbose(ex, ex.ToString());


    public static void Debug(string message) =>
        IPluginLog.Instance().Debug(message);

    public static void Debug(string message, Exception ex) =>
        IPluginLog.Instance().Debug(ex, message);

    public static void Debug(Exception ex) =>
        IPluginLog.Instance().Debug(ex, ex.ToString());


    public static void Warning(string message) =>
        IPluginLog.Instance().Warning(message);

    public static void Warning(string message, Exception ex) =>
        IPluginLog.Instance().Warning(ex, message);

    public static void Warning(Exception ex) =>
        IPluginLog.Instance().Warning(ex, ex.ToString());


    public static void Error(string message) =>
        IPluginLog.Instance().Error(message);

    public static void Error(string message, Exception ex) =>
        IPluginLog.Instance().Error(ex, message);

    public static void Error(Exception ex) =>
        IPluginLog.Instance().Error(ex, ex.ToString());
}
