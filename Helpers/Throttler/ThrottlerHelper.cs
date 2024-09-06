namespace OmenTools.Helpers;

public static class ThrottlerHelper
{
    public static Throttler<string> Throttler { get; } = new();
    public static FrameThrottler<string> FrameThrottler { get; } = new(() => (long)DService.UiBuilder.FrameCount);

    public static void Uninit()
    {
        Throttler.Clear();
        FrameThrottler.Clear();
    }
}
