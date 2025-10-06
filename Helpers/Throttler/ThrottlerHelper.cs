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
    
    /// <summary>
    /// 获取基于帧数的字符串节流器实例，使用 UI 构建器的帧计数
    /// </summary>
    public static FrameThrottler<string> FrameThrottler { get; } = new(() => (long)DService.UIBuilder.FrameCount);

    /// <summary>
    /// 清理所有节流器状态，通常在插件卸载时调用
    /// </summary>
    public static void Uninit()
    {
        Throttler.Clear();
        FrameThrottler.Clear();
    }
}
