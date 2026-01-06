namespace OmenTools.Helpers;

/// <summary>
/// 节流器辅助类，提供常用的字符串类型节流器实例
/// </summary>
public static class ThrottlerHelper
{
    /// <summary>
    /// 获取基于毫秒的字符串节流器实例
    /// </summary>
    public static Throttler<string> Throttler { get; } = new();

    internal static void Uninit() => 
        Throttler.Clear();
}
