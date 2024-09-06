using System.Collections.Concurrent;

namespace OmenTools.Helpers;

public abstract class ThrottlerBase<T> : IThrottler<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, long> throttlers = new();
    public           ICollection<T>                ThrottleNames => throttlers.Keys;

    protected abstract long GetCurrentTime();

    public virtual void Clear() => throttlers.Clear();
    
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

    public virtual bool Check(T name) =>
        !throttlers.TryGetValue(name, out var expirationTime) || GetCurrentTime() > expirationTime;

    public virtual long GetRemainingTime(T name, bool allowNegative = false)
    {
        var currentTime = GetCurrentTime();
        if (!throttlers.TryGetValue(name, out var expirationTime))
            return allowNegative ? -currentTime : 0;

        var remainingTime = expirationTime - currentTime;
        return allowNegative ? remainingTime : Math.Max(remainingTime, 0);
    }
}
