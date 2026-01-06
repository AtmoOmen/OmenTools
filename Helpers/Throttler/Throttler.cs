using System.Collections.Concurrent;
using System.Diagnostics;

namespace OmenTools.Helpers;

/// <summary>
///     高精度节流器实现，使用 Stopwatch.GetTimestamp() 作为时间基准
/// </summary>
/// <typeparam name="T">节流标识符的类型，必须为非空类型</typeparam>
public class Throttler<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, long> throttlers = new();

    /// <summary>
    ///     获取当前所有被节流的标识符集合
    /// </summary>
    public ICollection<T> ThrottleNames => throttlers.Keys;

    /// <summary>
    ///     获取当前时间戳（Tick）
    /// </summary>
    protected static long GetCurrentTimestamp() => 
        Stopwatch.GetTimestamp();

    /// <summary>
    ///     对指定标识符进行节流操作
    /// </summary>
    /// <param name="name">要节流的标识符</param>
    /// <param name="milliseconds">节流持续时间（毫秒），默认 500 毫秒</param>
    /// <param name="reThrottle">是否重新开始节流计时</param>
    /// <returns>如果成功应用节流返回 true，如果已在节流中且 reThrottle 为 false 则返回 false</returns>
    public bool Throttle(T name, uint milliseconds = 500, bool reThrottle = false)
    {
        var durationTicks = (long)(milliseconds * Stopwatch.Frequency / 1000.0);
        return ThrottleInternal(name, durationTicks, reThrottle);
    }

    /// <summary>
    ///     对指定标识符进行节流操作
    /// </summary>
    /// <param name="name">要节流的标识符</param>
    /// <param name="duration">节流持续时间</param>
    /// <param name="reThrottle">是否重新开始节流计时</param>
    /// <returns>如果成功应用节流返回 true，如果已在节流中且 reThrottle 为 false 则返回 false</returns>
    public bool Throttle(T name, TimeSpan duration, bool reThrottle = false)
    {
        var durationTicks = (long)(duration.TotalSeconds * Stopwatch.Frequency);
        return ThrottleInternal(name, durationTicks, reThrottle);
    }

    private bool ThrottleInternal(T name, long durationTicks, bool reThrottle)
    {
        var currentTimestamp = GetCurrentTimestamp();
        var newExpiration    = currentTimestamp + durationTicks;

        return throttlers.AddOrUpdate(name,
                                      _ => newExpiration,
                                      (_, existingExpiration) =>
                                      {
                                          if (reThrottle || currentTimestamp > existingExpiration)
                                              return newExpiration;
                                          return existingExpiration;
                                      }) == newExpiration;
    }

    /// <summary>
    ///     检查指定标识符是否处于节流状态
    /// </summary>
    /// <param name="name">要检查的标识符</param>
    /// <returns>如果未节流或节流已过期返回 true，如果仍在节流期内返回 false</returns>
    public bool Check(T name) =>
        !throttlers.TryGetValue(name, out var expirationTime) || GetCurrentTimestamp() > expirationTime;

    /// <summary>
    ///     获取指定标识符剩余的节流时间（毫秒）
    /// </summary>
    /// <param name="name">要查询的标识符</param>
    /// <param name="allowNegative">是否允许返回负值（true：返回实际剩余时间，可能为负；false：返回0或正数）</param>
    /// <returns>剩余的节流时间（毫秒），如果 allowNegative 为 false 则最小返回0</returns>
    public long GetRemainingTime(T name, bool allowNegative = false)
    {
        var currentTimestamp = GetCurrentTimestamp();

        if (!throttlers.TryGetValue(name, out var expirationTime))
            return allowNegative ? -(long)(currentTimestamp * 1000.0 / Stopwatch.Frequency) : 0;

        var remainingTicks = expirationTime - currentTimestamp;
        var remainingMs    = (long)(remainingTicks * 1000.0 / Stopwatch.Frequency);

        return allowNegative ? remainingMs : Math.Max(remainingMs, 0);
    }

    /// <summary>
    ///     移除指定标识符的节流限制
    /// </summary>
    /// <param name="name">要移除节流的标识符</param>
    /// <returns>如果成功移除返回 true，如果标识符不存在返回 false</returns>
    public bool Remove(T name) => 
        throttlers.TryRemove(name, out _);

    /// <summary>
    ///     清除所有节流限制
    /// </summary>
    public void Clear() => throttlers.Clear();
}
