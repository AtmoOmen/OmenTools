namespace OmenTools.Helpers;

/// <summary>
/// 节流器接口，提供基于时间的操作限制功能
/// </summary>
/// <typeparam name="T">节流标识符的类型，必须为非空类型</typeparam>
public interface IThrottler<T> where T : notnull
{
    /// <summary>
    /// 获取当前所有被节流的标识符集合
    /// </summary>
    ICollection<T> ThrottleNames { get; }

    /// <summary>
    /// 对指定标识符进行节流操作
    /// </summary>
    /// <param name="name">要节流的标识符</param>
    /// <param name="duration">节流持续时间（单位取决于具体实现）</param>
    /// <param name="reThrottle">是否重新开始节流计时（true：即使已在节流中也会重置计时；false：只在未节流时生效）</param>
    /// <returns>如果成功应用节流返回 true，如果已在节流中且 reThrottle 为 false 则返回 false</returns>
    bool Throttle(T name, long duration, bool reThrottle = false);

    /// <summary>
    /// 检查指定标识符是否处于节流状态
    /// </summary>
    /// <param name="name">要检查的标识符</param>
    /// <returns>如果未节流或节流已过期返回 true，如果仍在节流期内返回 false</returns>
    bool Check(T name);

    /// <summary>
    /// 获取指定标识符剩余的节流时间
    /// </summary>
    /// <param name="name">要查询的标识符</param>
    /// <param name="allowNegative">是否允许返回负值（true：返回实际剩余时间，可能为负；false：返回0或正数）</param>
    /// <returns>剩余的节流时间，如果 allowNegative 为 false 则最小返回0</returns>
    long GetRemainingTime(T name, bool allowNegative = false);

    /// <summary>
    /// 移除指定标识符的节流限制
    /// </summary>
    /// <param name="name">要移除节流的标识符</param>
    /// <returns>如果成功移除返回 true，如果标识符不存在返回 false</returns>
    bool Remove(T name);

    /// <summary>
    /// 清除所有节流限制
    /// </summary>
    void Clear();
}
