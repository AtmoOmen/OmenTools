namespace OmenTools.Helpers;

public class FrameThrottler<T>(Func<long> frameCountProvider) : ThrottlerBase<T> where T : notnull
{
    protected override long GetCurrentTime() => frameCountProvider();

    public bool Throttle(T name, int frames = 60, bool reThrottle = false) =>
        base.Throttle(name, frames, reThrottle);
}
