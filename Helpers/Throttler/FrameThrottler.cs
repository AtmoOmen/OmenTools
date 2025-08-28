namespace OmenTools.Helpers;

/// <summary>
/// 基于帧数的节流器实现，使用外部提供的帧计数作为时间基准
/// </summary>
/// <typeparam name="T">节流标识符的类型，必须为非空类型</typeparam>
/// <param name="frameCountProvider">帧计数提供函数，用于获取当前帧数</param>
public class FrameThrottler<T>(Func<long> frameCountProvider) : ThrottlerBase<T> where T : notnull
{
    /// <summary>
    /// 获取当前帧数
    /// </summary>
    /// <returns>由 frameCountProvider 提供的当前帧数值</returns>
    protected override long GetCurrentTime() => frameCountProvider();

    /// <summary>
    /// 对指定标识符进行帧数级节流操作
    /// </summary>
    /// <param name="name">要节流的标识符</param>
    /// <param name="frames">节流持续时间（帧数），默认60帧</param>
    /// <param name="reThrottle">是否重新开始节流计时</param>
    /// <returns>如果成功应用节流返回 true，如果已在节流中且 reThrottle 为 false 则返回 false</returns>
    public bool Throttle(T name, int frames = 60, bool reThrottle = false) =>
        base.Throttle(name, frames, reThrottle);
}
