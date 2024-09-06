namespace OmenTools.Helpers;

public partial class TaskHelper
{
    public void Insert(Func<bool?> task, string? name = null, int? timeLimitMs = null, bool? abortOnTimeout = null, uint weight = 0)
    {
        InsertQueueTask(new TaskHelperTask(task, timeLimitMs ?? TimeLimitMS, abortOnTimeout ?? AbortOnTimeout, name), weight);
    }

    public void Insert(Action task, string? name = null, int? timeLimitMs = null, bool? abortOnTimeout = null, uint weight = 0)
    {
        Insert(() => { task(); return true; }, name, timeLimitMs, abortOnTimeout, weight);
    }

    private void InsertQueueTask(TaskHelperTask task, uint weight)
    {
        var queue = Queues.FirstOrDefault(q => q.Weight == weight) ?? AddQueueAndGet(weight);
        queue.Tasks.Insert(0, task);
        MaxTasks++;
    }

    private TaskHelperQueue AddQueueAndGet(uint weight)
    {
        var newQueue = new TaskHelperQueue(weight);
        Queues.Add(newQueue);
        return newQueue;
    }

    public void InsertDelayNext(int delayMS, string uniqueName = "DelayNextInsert", 
        bool useFrameThrottler = false, uint weight = 0)
    {
        IThrottler<string> throttler = useFrameThrottler ? FrameThrottler : Throttler;

        Insert(() => throttler.Check(uniqueName),
               $"{throttler.GetType().Name}.Check({uniqueName})",
               weight: weight);
        Insert(() => throttler.Throttle(uniqueName, delayMS),
               $"{throttler.GetType().Name}.Throttle({uniqueName}, {delayMS})",
               weight: weight);

        MaxTasks += 2;
    }
}
