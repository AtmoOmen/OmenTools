namespace OmenTools.Helpers;

public class Throttler<T> : ThrottlerBase<T> where T : notnull
{
    protected override long GetCurrentTime() => Environment.TickCount64;

    public bool Throttle(T name, int milliseconds = 500, bool reThrottle = false) =>
        base.Throttle(name, milliseconds, reThrottle);
}
