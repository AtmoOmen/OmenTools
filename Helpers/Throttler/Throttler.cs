namespace OmenTools.Helpers;

/// <summary>
/// 基于毫秒的节流器实现，使用系统 Tick 计数作为时间基准
/// </summary>
/// <typeparam name="T">节流标识符的类型，必须为非空类型</typeparam>
public class Throttler<T> : ThrottlerBase<T> where T : notnull
{
    /// <summary>
    /// 获取当前系统 Tick 计数（毫秒）
    /// </summary>
    /// <returns>从系统启动开始计算的毫秒数</returns>
    protected override long GetCurrentTime() => Environment.TickCount64;

    /// <summary>
    /// 对指定标识符进行毫秒级节流操作
    /// </summary>
    /// <param name="name">要节流的标识符</param>
    /// <param name="milliseconds">节流持续时间（毫秒），默认 500 毫秒</param>
    /// <param name="reThrottle">是否重新开始节流计时</param>
    /// <returns>如果成功应用节流返回 true，如果已在节流中且 reThrottle 为 false 则返回 false</returns>
    public bool Throttle(T name, int milliseconds = 500, bool reThrottle = false) =>
        base.Throttle(name, milliseconds, reThrottle);
}
