using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using Dalamud.Plugin.Services;

namespace OmenTools.Helpers;

public partial class TaskHelper : IDisposable
{
    private static readonly ConcurrentDictionary<TaskHelper, byte> Instances = [];

    internal static void DisposeAll()
    {
        var disposedCount = 0;

        foreach (var instance in Instances.Keys)
        {
            if (instance is not { IsDisposed: false }) continue;
            
            instance.Dispose();
            disposedCount++;
        }

        if (disposedCount > 0)
            Debug($"[TaskHelper] 已自动清理了 {disposedCount} 个队列管理器");

        Instances.Clear();
    }

    private static Channel<(TaskHelperTask Task, int Weight)> CreateTaskChannel() =>
        Channel.CreateUnbounded<(TaskHelperTask, int)>(new() { SingleReader = true });

    public TaskHelper()
    {
        Instances.TryAdd(this, 0);
        _ = ProcessChannelAsync(CancelSource.Token);
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        
        Instances.TryRemove(this, out _);
        
        CancelSource.Cancel();
        CancelSource.Dispose();
        
        TaskChannel.Writer.TryComplete();

        CurrentTask?.CancelToken?.Cancel();
        CurrentTask?.CancelToken?.Dispose();

        RunningSystemTask = null;

        foreach (var queue in Queues)
        {
            foreach (var task in queue.Tasks)
            {
                task.CancelToken?.Cancel();
                task.CancelToken?.Dispose();
            }
        }
        Queues.Clear();
    }

    /// <summary>
    ///     当前正在进行的任务名称
    /// </summary>
    public string CurrentTaskName => CurrentTask?.GetName() ?? string.Empty;

    /// <summary>
    ///     当前所有正在处理任务数量 (队列中 + 正在执行)
    /// </summary>
    public int CurrentTasksCount => queueTaskCount + (CurrentTask == null ? 0 : 1) + pendingTaskCount;

    /// <summary>
    ///     当前是否正在处理任一任务
    /// </summary>
    public bool IsBusy => CurrentTasksCount > 0;

    /// <summary>
    ///     当单一任务超时时的全局控制逻辑
    ///     若任务本身设置了控制逻辑 (<see cref="TaskHelperTask.TimeoutBehaviour" />) 则以任务的为判断标准
    /// </summary>
    public TaskAbortBehaviour TimeoutBehaviour { get; set; } = TaskAbortBehaviour.AbortAll;

    /// <summary>
    ///     当单一任务执行出现异常时的全局控制逻辑
    ///     若任务本身设置了控制逻辑 (<see cref="TaskHelperTask.ExceptionBehaviour" />) 则以任务的为判断标准 <br />
    ///     注: 异步任务被取消将被视为发生异常
    /// </summary>
    public TaskAbortBehaviour ExceptionBehaviour { get; set; } = TaskAbortBehaviour.AbortAll;

    /// <summary>
    ///     是否显示调试信息
    /// </summary>
    public bool ShowDebug { get; set; }

    /// <summary>
    ///     全局任务超时时间 <br />
    ///     默认为 10 秒; 设置为 ≤ 0 以默认不超时 <br />
    ///     若任务本身设置了 ＞ 0 的过期时间 (<see cref="TaskHelperTask.TimeoutMS" />) 则以任务的为判断标准 <br />
    ///     <seealso cref="TimeoutBehaviour" />
    /// </summary>
    public int TimeoutMS { get; set; } = 10_000;

    /// <summary>
    ///     是否已被销毁
    /// </summary>
    public bool IsDisposed { get; private set; }

    private TaskHelperTask?                            CurrentTask       { get; set; }
    private Task<bool>?                                RunningSystemTask { get; set; }
    private SortedSet<TaskHelperQueue>                 Queues            { get; } = [new(1), new(0)];
    private Channel<(TaskHelperTask Task, int Weight)> TaskChannel       { get; } = CreateTaskChannel();
    private CancellationTokenSource                    CancelSource      { get; } = new();

    private volatile int pendingTaskCount;
    private volatile int queueTaskCount;

    private async Task ProcessChannelAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (CurrentTask == null && queueTaskCount == 0 && pendingTaskCount == 0)
                    await TaskChannel.Reader.WaitToReadAsync(ct).ConfigureAwait(false);

                var isBusy = false;
                await DService.Instance().Framework.Run(() =>
                {
                    SyncPendingTasks();

                    if (CurrentTask == null)
                        ProcessNextTask();
                    
                    if (CurrentTask != null)
                    {
                        ExecuteCurrentTask();
                        isBusy = true;
                    }
                    else
                        isBusy = queueTaskCount > 0 || pendingTaskCount > 0;
                }, cancellationToken: ct).ConfigureAwait(false);

                if (isBusy)
                    await Task.Delay(1, ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (Exception ex)
        {
            LogWarning($"[TaskHelper] 异步轮询出现异常: {ex}");
        }
    }

    private void SyncPendingTasks()
    {
        while (TaskChannel.Reader.TryRead(out var item))
        {
            Interlocked.Decrement(ref pendingTaskCount);
            var lookup = new TaskHelperQueue(item.Weight);

            if (!Queues.TryGetValue(lookup, out var queue))
            {
                queue = lookup;
                Queues.Add(queue);
            }

            queue.Tasks.Add(item.Task);
            Interlocked.Increment(ref queueTaskCount);
        }
    }

    private void ProcessNextTask()
    {
        foreach (var queue in Queues)
        {
            if (!queue.Tasks.TryDequeue(out var task)) continue;
            Interlocked.Decrement(ref queueTaskCount);

            CurrentTask           = task;
            CurrentTask.StartTime = Stopwatch.GetTimestamp();

            Log($"开始执行任务 {CurrentTask.GetName()}");
            break;
        }
    }

    private void ExecuteCurrentTask()
    {
        try
        {
            if (CurrentTask == null) return;

            if (RunningSystemTask == null)
            {
                Task<bool> newTask = null;
                if (CurrentTask.IsAsync)
                {
                    newTask = DService.Instance().Framework.Run
                    (
                        async () => await CurrentTask.AsyncAction(CurrentTask.CancelToken.Token).ConfigureAwait(false),
                        cancellationToken: CurrentTask.CancelToken.Token
                    );
                }
                else
                    newTask = DService.Instance().Framework.Run(() => CurrentTask.Action(), cancellationToken: CurrentTask.CancelToken.Token);
                
                RunningSystemTask = newTask;
                RunningSystemTask.ConfigureAwait(false);

                Log($"启动{(CurrentTask.IsAsync ? "异步" : "同步")}任务 {CurrentTask.GetName()}");
                return;
            }

            var task = RunningSystemTask;
            if (task.IsCompletedSuccessfully)
            {
                if (task.Result)
                    HandleTaskCompletion();
                else
                {
                    Task<bool> newTask = null;
                    if (CurrentTask.IsAsync)
                    {
                        newTask = DService.Instance().Framework.RunOnTick
                        (
                            async () => await CurrentTask.AsyncAction(CurrentTask.CancelToken.Token).ConfigureAwait(false),
                            cancellationToken: CurrentTask.CancelToken.Token
                        );
                    }
                    else
                        newTask = DService.Instance().Framework.RunOnTick(() => CurrentTask.Action(), cancellationToken: CurrentTask.CancelToken.Token);
                    
                    RunningSystemTask = newTask;
                    RunningSystemTask.ConfigureAwait(false);
                }
            }
            else if (task.IsCanceled)
                throw new OperationCanceledException($"异步任务 {CurrentTask.GetName()} 已取消");
            else if (task.IsFaulted)
                throw task.Exception?.GetBaseException() ?? new Exception($"异步任务 {CurrentTask.GetName()} 已失败");
            else
                HandleTaskTimeout();
        }
        catch (Exception ex)
        {
            HandleTaskError(ex);
        }
    }

    private void HandleTaskCompletion()
    {
        if (CurrentTask == null) return;

        Log($"已完成任务: {CurrentTask.GetName()}");
        CurrentTask = null;
        RunningSystemTask = null;
    }

    private void HandleTaskTimeout()
    {
        if (CurrentTask == null) return;

        var timeoutMS = CurrentTask.TimeoutMS <= 0 ? TimeoutMS : CurrentTask.TimeoutMS;
        if (timeoutMS <= 0) return;

        if (Stopwatch.GetElapsedTime(CurrentTask.StartTime).TotalMilliseconds <= timeoutMS) return;

        var timeoutBehaviour = CurrentTask.TimeoutBehaviour ?? TimeoutBehaviour;
        var reason           = $"任务 {CurrentTask.GetName()} 执行时间过长";

        ExecuteTaskAbortBehaviour(timeoutBehaviour, reason, CurrentTask);
    }

    private void HandleTaskError(Exception? ex = null)
    {
        if (CurrentTask == null) return;

        var exceptionBehaviour = CurrentTask.ExceptionBehaviour ?? ExceptionBehaviour;
        var reason             = $"执行任务 {CurrentTask.GetName()} 过程中出现错误";

        if (ex != null)
            Error(reason, ex);
        else
            Error(reason);

        ExecuteTaskAbortBehaviour(exceptionBehaviour, reason, CurrentTask);
    }

    private void ExecuteTaskAbortBehaviour(TaskAbortBehaviour behaviour, string reason, TaskHelperTask task)
    {
        if (task == null) return;

        switch (behaviour)
        {
            case TaskAbortBehaviour.AbortAll:
                LogWarning($"放弃了所有任务 (原因: {reason})");

                Abort();
                break;
            case TaskAbortBehaviour.AbortCurrent:
                LogWarning($"放弃了当前任务 (原因: {reason})");

                CurrentTask?.CancelToken?.Cancel();
                CurrentTask?.CancelToken?.Dispose();

                CurrentTask = null;
                RunningSystemTask = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(behaviour));
        }
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
        SyncPendingTasks();
        
        foreach (var queue in Queues)
        {
            foreach (var task in queue.Tasks)
            {
                task.CancelToken?.Cancel();
                task.CancelToken?.Dispose();
            }
            
            queue.Tasks.Clear();
        }

        queueTaskCount = 0;

        CurrentTask?.CancelToken?.Cancel();
        CurrentTask?.CancelToken?.Dispose();

        RunningSystemTask = null;
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
        SyncPendingTasks();
        LogWarning($"移除了权重 {weight} 队列");
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);
        if (queue == null) return false;

        Interlocked.Add(ref queueTaskCount, -queue.Tasks.Count);
        return Queues.Remove(queue);
    }

    public void RemoveQueueTasks(int weight)
    {
        SyncPendingTasks();
        LogWarning($"清除了权重 {weight} 队列中的所有任务");
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);
        if (queue != null)
        {
            Interlocked.Add(ref queueTaskCount, -queue.Tasks.Count);
            queue.Tasks.Clear();
        }
    }

    public bool RemoveQueueFirstTask(int weight)
    {
        SyncPendingTasks();
        LogWarning($"移除了权重 {weight} 队列中的第一个任务");
        if ((Queues.FirstOrDefault(q => q.Weight == weight)?.Tasks ?? []).TryDequeue(out _))
        {
            Interlocked.Decrement(ref queueTaskCount);
            return true;
        }

        return false;
    }

    public bool RemoveQueueLastTask(int weight)
    {
        SyncPendingTasks();
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);
        if (!((queue?.Tasks ?? []).Count > 0)) return false;

        LogWarning($"清除了权重 {weight} 队列中的最后一个任务");
        queue.Tasks.RemoveAt(queue.Tasks.Count - 1);
        Interlocked.Decrement(ref queueTaskCount);
        return true;
    }

    public bool RemoveQueueFirstNTasks(int weight, int count)
    {
        SyncPendingTasks();
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);

        if ((queue?.Tasks ?? []).Count > 0)
        {
            LogWarning($"清除了权重 {weight} 队列中的起始 {count} 个任务");

            var actualCountToRemove = Math.Min(count, queue.Tasks.Count);
            queue.Tasks.RemoveRange(0, actualCountToRemove);
            Interlocked.Add(ref queueTaskCount, -actualCountToRemove);
            return true;
        }

        return false;
    }

    public int GetQueueTaskCount(int weight)
    {
        SyncPendingTasks();
        return Queues.FirstOrDefault(x => x.Weight == weight)?.Tasks.Count ?? 0;
    }

    #endregion
}
