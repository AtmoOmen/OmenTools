using System.Collections.Concurrent;

namespace OmenTools.Helpers;

/// <summary>
/// 节流器基类，提供基于时间的操作限制功能的通用实现
/// </summary>
/// <typeparam name="T">节流标识符的类型，必须为非空类型</typeparam>
public abstract class ThrottlerBase<T> : IThrottler<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, long> throttlers = new();
    
    /// <summary>
    /// 获取当前所有被节流的标识符集合
    /// </summary>
    public ICollection<T> ThrottleNames => throttlers.Keys;

    /// <summary>
    /// 抽象方法，由具体实现提供当前时间获取逻辑
    /// </summary>
    /// <returns>当前时间值（单位取决于具体实现）</returns>
    protected abstract long GetCurrentTime();

    /// <summary>
    /// 清除所有节流限制
    /// </summary>
    public virtual void Clear() => 
        throttlers.Clear();
    
    /// <summary>
    /// 对指定标识符进行节流操作
    /// </summary>
    /// <param name="name">要节流的标识符</param>
    /// <param name="duration">节流持续时间</param>
    /// <param name="reThrottle">是否重新开始节流计时</param>
    /// <returns>如果成功应用节流返回true，如果已在节流中且 reThrottle 为 false 则返回 false</returns>
    public virtual bool Throttle(T name, long duration, bool reThrottle = false)
    {
        var currentTime = GetCurrentTime();
        return throttlers.AddOrUpdate(name,
                                      _ => currentTime + duration,
                                      (_, existingTime) =>
                                      {
                                          if (currentTime > existingTime || reThrottle)
                                              return currentTime + duration;
                                          return existingTime;
                                      }) == currentTime + duration;
    }

    /// <summary>
    /// 检查指定标识符是否处于节流状态
    /// </summary>
    /// <param name="name">要检查的标识符</param>
    /// <returns>如果未节流或节流已过期返回 true，如果仍在节流期内返回 false</returns>
    public virtual bool Check(T name) =>
        !throttlers.TryGetValue(name, out var expirationTime) || GetCurrentTime() > expirationTime;

    /// <summary>
    /// 获取指定标识符剩余的节流时间
    /// </summary>
    /// <param name="name">要查询的标识符</param>
    /// <param name="allowNegative">是否允许返回负值</param>
    /// <returns>剩余的节流时间，如果 allowNegative 为 false 则最小返回 0</returns>
    public virtual long GetRemainingTime(T name, bool allowNegative = false)
    {
        var currentTime = GetCurrentTime();
        if (!throttlers.TryGetValue(name, out var expirationTime))
            return allowNegative ? -currentTime : 0;

        var remainingTime = expirationTime - currentTime;
        return allowNegative ? remainingTime : Math.Max(remainingTime, 0);
    }

    /// <summary>
    /// 移除指定标识符的节流限制
    /// </summary>
    /// <param name="name">要移除节流的标识符</param>
    /// <returns>如果成功移除返回 true，如果标识符不存在返回 false</returns>
    public virtual bool Remove(T name) => 
        throttlers.TryRemove(name, out _);
}
