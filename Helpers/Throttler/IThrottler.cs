namespace OmenTools.Helpers;

public interface IThrottler<T> where T : notnull
{
    ICollection<T> ThrottleNames { get; }
    bool           Throttle(T         name, long duration, bool reThrottle = false);
    bool           Check(T            name);
    long           GetRemainingTime(T name, bool allowNegative = false);
}
