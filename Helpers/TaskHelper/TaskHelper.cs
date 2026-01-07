using System.Collections.Concurrent;
using Dalamud.Plugin.Services;

namespace OmenTools.Helpers;

public partial class TaskHelper : IDisposable
{
    private static readonly List<TaskHelper> Instances = [];
    
    public TaskHelper()
    {
        DService.Framework.Update += Tick;
        Instances.Add(this);
    }
    
    public void Dispose()
    {
        DService.Framework.Update -= Tick;
        
        foreach (var kvp in RunningAsyncTasks)
        {
            kvp.Key.CancellationTokenSource?.Cancel();
            kvp.Key.CancellationTokenSource?.Dispose();
        }
        
        RunningAsyncTasks.Clear();
        
        foreach (var queue in Queues)
        {
            foreach (var task in queue.Tasks.Where(t => t.IsAsync))
            {
                task.CancellationTokenSource?.Cancel();
                task.CancellationTokenSource?.Dispose();
            }
        }
        
        Instances.Remove(this);
    }

    public  string CurrentTaskName => CurrentTask?.Name ?? string.Empty;
    public  int    QueuedTaskCount => Queues.Sum(q => q.Tasks.Count) + (CurrentTask == null ? 0 : 1);
    public  bool   IsBusy          => CurrentTask != null || Queues.Any(q => q.Tasks.Count > 0) || !RunningAsyncTasks.IsEmpty;
    
    public  bool   AbortOnTimeout  { get; set; } = true;
    public  long   AbortAt         { get; private set; }
    public  bool   ShowDebug       { get; set; }
    public  int    TimeLimitMS     { get; set; } = 10_000;

    private TaskHelperTask?                                   CurrentTask       { get; set; }
    private SortedSet<TaskHelperQueue>                        Queues            { get; }      = [new(1), new(0)];
    private ConcurrentDictionary<TaskHelperTask, Task<bool?>> RunningAsyncTasks { get; set; } = [];
    private bool                                              HasPendingTask    { get; set; }
    
    private void Tick(IFramework framework)
    {
        if (CurrentTask == null)
            ProcessNextTask();
        else
            ExecuteCurrentTask(framework);
    }

    private void ProcessNextTask()
    {
        if (!HasPendingTask) return;
        
        foreach (var queue in Queues)
        {
            if (!queue.Tasks.TryDequeue(out var task)) continue;
            
            CurrentTask = task;
            Log($"开始执行任务: {CurrentTask?.Name ?? "(无名称)"}");

            AbortAt = Environment.TickCount64 + CurrentTask?.TimeLimitMS ?? 0;
            break;
        }

        if (CurrentTask == null) 
            HasPendingTask = false;
    }

    private void ExecuteCurrentTask(IFramework framework)
    {
        try
        {
            if (CurrentTask == null) return;

            bool? result;
            
            if (CurrentTask.IsAsync)
            {
                if (!RunningAsyncTasks.TryGetValue(CurrentTask, out var task))
                {
                    var asyncTask = framework.Run(async () => await CurrentTask.AsyncAction(CurrentTask.CancellationTokenSource.Token),
                                                  CurrentTask.CancellationTokenSource.Token);
                    RunningAsyncTasks[CurrentTask] = asyncTask;
                    
                    Log($"启动异步任务: {CurrentTask.Name}");
                    
                    return;
                }
                
                if (!task.IsCompleted)
                {
                    CheckForTimeout();
                    return;
                }
                
                RunningAsyncTasks.TryRemove(CurrentTask, out _);
                
                if (task.IsFaulted)
                {
                    HandleError(CurrentTask.Name, task.Exception?.GetBaseException() ?? new Exception("异步任务失败"));
                    return;
                }
                
                if (task.IsCanceled)
                {
                    Log($"异步任务已取消: {CurrentTask.Name}");
                    
                    CurrentTask = null;
                    return;
                }
                
                result = task.Result;
            }
            else
                result = CurrentTask.Action!();
            
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
        catch (Exception e)
        {
            HandleError(CurrentTask?.Name ?? "(无名称)", e);
        }
    }

    private void CompleteTask()
    {
        Log($"已完成任务: {CurrentTask?.Name ?? "(无名称)"}");
        
        CurrentTask = null;
    }

    private void CheckForTimeout()
    {
        if (Environment.TickCount64 <= AbortAt) return;

        var reason = $"任务 {CurrentTask?.Name ?? "(无名称)"} 执行时间过长";
        
        if (CurrentTask is { IsAsync: true, CancellationTokenSource: not null })
        {
            CurrentTask.CancellationTokenSource.Cancel();
            RunningAsyncTasks.TryRemove(CurrentTask, out _);
        }
        
        if (CurrentTask?.AbortOnTimeout ?? true)
            AbortAllTasks(reason);
        else
            HandleTimeout(reason);
    }

    private void AbortAllTasks(string reason = "无")
    {
        LogWarning($"放弃了所有剩余任务 (原因: {reason})");
        Abort();
    }

    private void HandleError(string name, Exception e)
    {
        Error($"执行任务 {name} 过程中出现错误", e);
        CurrentTask = null;
    }

    private void HandleTimeout(string reason)
    {
        LogWarning($"放弃了当前任务 (原因: {reason})");
        CurrentTask = null;
    }

    #region 内部日志

    private void Log(string message)
    {
        if (ShowDebug) 
            Debug(message);
        else 
            Verbose(message);
    }

    private void LogWarning(string message)
    {
        if (ShowDebug)
            Warning(message);
        else
            Verbose(message);
    }

    #endregion
    
    #region 队列与任务

    public void Abort()
    {
        foreach (var queue in Queues)
            queue.Tasks.Clear();
        
        foreach (var kvp in RunningAsyncTasks)
            kvp.Key.CancellationTokenSource?.Cancel();
        
        RunningAsyncTasks.Clear();
        
        CurrentTask = null;
    }
    
    public bool AddQueue(int weight)
    {
        if (Queues.Any(q => q.Weight == weight)) return false;
        Queues.Add(new(weight));
        return true;
    }

    public bool RemoveQueue(int weight)
    {
        LogWarning($"移除了权重 {weight} 队列");
        return Queues.RemoveWhere(q => q.Weight == weight) > 0;
    }

    public void RemoveQueueTasks(int weight)
    {
        LogWarning($"清除了权重 {weight} 队列中的所有任务");
        (Queues.FirstOrDefault(q => q.Weight == weight)?.Tasks ?? []).Clear();
    }

    public bool RemoveQueueFirstTask(int weight)
    {
        LogWarning($"移除了权重 {weight} 队列中的第一个任务");
        return (Queues.FirstOrDefault(q => q.Weight == weight)?.Tasks ?? []).TryDequeue(out _);
    }

    public bool RemoveQueueLastTask(int weight)
    {
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);
        if (!((queue?.Tasks ?? []).Count > 0)) return false;
        
        LogWarning($"清除了权重 {weight} 队列中的最后一个任务");
        queue.Tasks.RemoveAt(queue.Tasks.Count - 1);
        return true;
    }

    public bool RemoveQueueFirstNTasks(int weight, int count)
    {
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);
        if ((queue?.Tasks ?? []).Count > 0)
        {
            LogWarning($"清除了权重 {weight} 队列中的起始 {count} 个任务");
            
            var actualCountToRemove = Math.Min(count, queue.Tasks.Count);
            queue.Tasks.RemoveRange(0, actualCountToRemove);
            return true;
        }

        return false;
    }

    public int GetQueueTaskCount(int weight) => 
        Queues.Count(x => x.Weight == weight);

    #endregion
    
    public static void DisposeAll()
    {
        var disposedCount = 0;
        foreach (var instance in Instances)
        {
            DService.Framework.Update -= instance.Tick;
            disposedCount++;
        }

        if (disposedCount > 0)
            Debug($"已自动清理了 {disposedCount} 个队列管理器");

        Instances.Clear();
    }
}
