namespace OmenTools.Helpers;

public partial class TaskHelper : IDisposable
{
    private static readonly List<TaskHelper>       Instances = [];
    private readonly        FrameThrottler<string> FrameThrottler;
    private readonly        Throttler<string>      Throttler;

    public TaskHelper()
    {
        FrameThrottler = new(() => (long)DService.UiBuilder.FrameCount);
        Throttler = new();
        DService.Framework.Update += Tick;
        Instances.Add(this);
    }

    public  TaskHelperTask?            CurrentTask     { get; set; }
    public  string                     CurrentTaskName => CurrentTask?.Name ?? string.Empty;
    private SortedSet<TaskHelperQueue> Queues          { get; } = [new(1), new(0)];
    public  List<string>               TaskStack       => Queues.SelectMany(q => q.Tasks.Select(t => t.Name)).ToList();
    public  int                        NumQueuedTasks => Queues.Sum(q => q.Tasks.Count) + (CurrentTask == null ? 0 : 1);
    public  bool                       IsBusy => CurrentTask != null || Queues.Any(q => q.Tasks.Count > 0);
    public  int                        MaxTasks        { get; private set; }
    public  bool                       AbortOnTimeout  { get; set; } = true;
    public  long                       AbortAt         { get; private set; }
    public  bool                       ShowDebug       { get; set; } = false;
    public  int                        TimeLimitMS { get; set; } = 10000;

    private void Tick(object? _)
    {
        if (CurrentTask == null)
        {
            ProcessNextTask();
        }
        else
        {
            ExecuteCurrentTask();
        }
    }

    private void ProcessNextTask()
    {
        foreach (var queue in Queues)
        {
            if(!queue.Tasks.TryDequeue(out var task)) continue;
            
            CurrentTask = task;
            if (ShowDebug) DService.Log.Debug($"开始执行任务: {CurrentTask?.Name ?? "(无名称)"}");

            AbortAt = Environment.TickCount64 + CurrentTask?.TimeLimitMS ?? 0;
            break;
        }

        if (CurrentTask == null) MaxTasks = 0;
    }

    private void ExecuteCurrentTask()
    {
        try
        {
            if (CurrentTask == null) return;
            var result = CurrentTask.Action();
            switch (result)
            {
                case true:
                    CompleteTask();
                    break;
                case false:
                    CheckForTimeout();
                    break;
                default:
                    AbortAllTasks();
                    break;
            }
        }
        catch (TimeoutException e)
        {
            HandleTimeout(e);
        }
        catch (Exception e)
        {
            HandleError(e);
        }
    }

    private void CompleteTask()
    {
        if (ShowDebug) DService.Log.Debug($"已完成任务: {CurrentTask?.Name ?? "(无名称)"}");
        
        CurrentTask = null;
    }

    private void CheckForTimeout()
    {
        if(Environment.TickCount64 <= AbortAt) return;
        if (CurrentTask?.AbortOnTimeout ?? true)
            AbortAllTasks($"任务 {CurrentTask?.Name ?? "(无名称)"} 执行时间过长");
        else
            DService.Log.Warning($"任务 {CurrentTask?.Name ?? "(无名称)"} 执行时间过长，但设置为不终止其他任务。");
    }

    private void AbortAllTasks(string reason = "无")
    {
        DService.Log.Warning($"正在清理所有剩余任务 (原因: {reason})");
        Abort();
    }

    private void HandleTimeout(Exception e)
    {
        DService.Log.Error("执行任务超时", e);
        CurrentTask = null;
    }

    private void HandleError(Exception e)
    {
        DService.Log.Error("执行任务过程中出现错误", e);
        CurrentTask = null;
    }

    public void SetStepMode(bool enabled)
    {
        DService.Framework.Update -= Tick;
        if (!enabled) DService.Framework.Update += Tick;
    }

    public void Step() => Tick(null);

    public bool AddQueue(uint weight)
    {
        if (Queues.Any(q => q.Weight == weight)) return false;
        Queues.Add(new TaskHelperQueue(weight));
        return true;
    }

    public bool RemoveQueue(uint weight) => Queues.RemoveWhere(q => q.Weight == weight) > 0;

    public void RemoveAllTasks(uint weight) =>
        Queues.FirstOrDefault(q => q.Weight == weight)?.Tasks.Clear();

    public bool RemoveFirstTask(uint weight) =>
        Queues.FirstOrDefault(q => q.Weight == weight)?.Tasks.TryDequeue(out _) ?? false;

    public bool RemoveLastTask(uint weight)
    {
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);
        if(!(queue?.Tasks.Count > 0)) return false;
        
        queue.Tasks.RemoveAt(queue.Tasks.Count - 1);
        return true;
    }

    public bool RemoveFirstNTasks(uint weight, int count)
    {
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);
        if (queue?.Tasks.Count > 0)
        {
            var actualCountToRemove = Math.Min(count, queue.Tasks.Count);
            queue.Tasks.RemoveRange(0, actualCountToRemove);
            return true;
        }
        return false;
    }

    public void Abort()
    {
        foreach (var queue in Queues)
            queue.Tasks.Clear();
        CurrentTask = null;
    }

    public void Dispose()
    {
        DService.Framework.Update -= Tick;
        Instances.Remove(this);
    }

    public static void DisposeAll()
    {
        var disposedCount = 0;
        foreach (var instance in Instances)
        {
            DService.Framework.Update -= instance.Tick;
            disposedCount++;
        }

        if (disposedCount > 0)
            DService.Log.Debug($"已自动清理了 {disposedCount} 个队列管理器");

        Instances.Clear();
    }
}
