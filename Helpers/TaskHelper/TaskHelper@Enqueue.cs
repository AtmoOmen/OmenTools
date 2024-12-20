namespace OmenTools.Helpers;

public partial class TaskHelper
{
    public void Enqueue(Func<bool?> task, string? name = null, int? timeLimitMs = null, bool? abortOnTimeout = null, uint weight = 0)
    {
        EnsureQueueExists(weight);
        var queue = Queues.First(q => q.Weight == weight);
        queue.Tasks.Add(new TaskHelperTask(task, timeLimitMs ?? TimeLimitMS, abortOnTimeout ?? AbortOnTimeout, name));
        hasPendingTask = true;
    }

    public void Enqueue(Action task, string? name = null, int? timeLimitMs = null, bool? abortOnTimeout = null, uint weight = 0)
    {
        Enqueue(() => { task(); return true; }, name, timeLimitMs, abortOnTimeout, weight);
    }

    private void EnsureQueueExists(uint weight)
    {
        if (Queues.All(q => q.Weight != weight))
        {
            Queues.Add(new TaskHelperQueue(weight));
        }
    }

    public void DelayNext(int delayMS, string uniqueName = "DelayNextEnqueue", 
        bool useFrameThrottler = false, uint weight = 0)
    {
        IThrottler<string> throttler = useFrameThrottler ? FrameThrottler : Throttler;

        Enqueue(() => throttler.Throttle(uniqueName, delayMS),
            $"{uniqueName} (Delay)",
            weight: weight);
        Enqueue(() => throttler.Check(uniqueName),
            $"{uniqueName} (DelayCheck)",
            weight: weight);

        hasPendingTask = true;
    }
}
