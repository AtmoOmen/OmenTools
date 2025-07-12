namespace OmenTools.Helpers;

public partial class TaskHelper
{
    public void Insert(Func<bool?> task, string? name = null, int? timeLimitMs = null, bool? abortOnTimeout = null, int weight = 0) => 
        InsertQueueTask(new TaskHelperTask(task, timeLimitMs ?? TimeLimitMS, abortOnTimeout ?? AbortOnTimeout, name), weight);

    public void Insert(Action task, string? name = null, int? timeLimitMs = null, bool? abortOnTimeout = null, int weight = 0) => 
        Insert(() => { task(); return true; }, name, timeLimitMs, abortOnTimeout, weight);

    public void InsertAsync(Func<CancellationToken, Task<bool?>> asyncTask, string? name = null, int? timeLimitMs = null, bool? abortOnTimeout = null, int weight = 0) => 
        InsertQueueTask(new TaskHelperTask(asyncTask, timeLimitMs ?? TimeLimitMS, abortOnTimeout ?? AbortOnTimeout, name), weight);

    public void InsertAsync(Func<CancellationToken, Task> asyncTask, string? name = null, int? timeLimitMs = null, bool? abortOnTimeout = null, int weight = 0) => 
        InsertAsync(async ct => { await asyncTask(ct); return true; }, name, timeLimitMs, abortOnTimeout, weight);

    public void InsertAsync(Func<Task<bool?>> asyncTask, string? name = null, int? timeLimitMs = null, bool? abortOnTimeout = null, int weight = 0) => 
        InsertAsync(ct => asyncTask(), name, timeLimitMs, abortOnTimeout, weight);

    public void InsertAsync(Func<Task> asyncTask, string? name = null, int? timeLimitMs = null, bool? abortOnTimeout = null, int weight = 0) => 
        InsertAsync(async ct => { await asyncTask(); return true; }, name, timeLimitMs, abortOnTimeout, weight);

    private void InsertQueueTask(TaskHelperTask task, int weight)
    {
        var queue = Queues.FirstOrDefault(q => q.Weight == weight) ?? AddQueueAndGet(weight);
        queue.Tasks.Insert(0, task);
        HasPendingTask = true;
    }

    private TaskHelperQueue AddQueueAndGet(int weight)
    {
        var newQueue = new TaskHelperQueue(weight);
        Queues.Add(newQueue);
        return newQueue;
    }

    public void InsertDelayNext(int delayMS, string uniqueName = "DelayNextInsert", bool useFrameThrottler = false, int weight = 0)
    {
        IThrottler<string> throttler = useFrameThrottler ? FrameThrottler : Throttler;

        Insert(() => throttler.Check(uniqueName),             $"{uniqueName} (DelayCheck)", weight: weight);
        Insert(() => throttler.Throttle(uniqueName, delayMS), $"{uniqueName} (Delay)", weight: weight);

        HasPendingTask = true;
    }
}
