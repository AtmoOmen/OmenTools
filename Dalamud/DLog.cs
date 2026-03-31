namespace OmenTools.Dalamud;

/// <summary>
///     对 Dalamud 日志信息输出的包装
/// </summary>
public static class DLog
{
    public static void Verbose(string message) =>
        DService.Instance().Log.Verbose(message);

    public static void Verbose(string message, Exception ex) =>
        DService.Instance().Log.Verbose(ex, message);

    public static void Verbose(Exception ex) =>
        DService.Instance().Log.Verbose(ex, ex.ToString());


    public static void Debug(string message) =>
        DService.Instance().Log.Debug(message);

    public static void Debug(string message, Exception ex) =>
        DService.Instance().Log.Debug(ex, message);

    public static void Debug(Exception ex) =>
        DService.Instance().Log.Debug(ex, ex.ToString());


    public static void Warning(string message) =>
        DService.Instance().Log.Warning(message);

    public static void Warning(string message, Exception ex) =>
        DService.Instance().Log.Warning(ex, message);

    public static void Warning(Exception ex) =>
        DService.Instance().Log.Warning(ex, ex.ToString());


    public static void Error(string message) =>
        DService.Instance().Log.Error(message);

    public static void Error(string message, Exception ex) =>
        DService.Instance().Log.Error(ex, message);

    public static void Error(Exception ex) =>
        DService.Instance().Log.Error(ex, ex.ToString());
}
