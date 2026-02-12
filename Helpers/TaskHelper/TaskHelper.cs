using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

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

    private static Channel<ITaskHelperMessage> CreateTaskChannel() =>
        Channel.CreateUnbounded<ITaskHelperMessage>(new() { SingleReader = true });

    public TaskHelper()
    {
        Instances.TryAdd(this, 0);
        QueueCounts.TryAdd(1, 0);
        QueueCounts.TryAdd(0, 0);
        _ = ProcessChannelAsync(CancelSource.Token);
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;

        Instances.TryRemove(this, out _);

        try
        {
            CancelSource.Cancel();
            CancelSource.Dispose();
        }
        catch
        {
            // ignored
        }


        TaskChannel.Writer.TryComplete();

        try
        {
            CurrentTask?.CancelToken?.Cancel();
            CurrentTask?.CancelToken?.Dispose();
        }
        catch
        {
            // ignored
        }

        foreach (var queue in Queues)
        {
            foreach (var task in queue.Tasks)
            {
                try
                {
                    task.CancelToken?.Cancel();
                    task.CancelToken?.Dispose();
                }
                catch
                {
                    // ignored
                }

            }
        }

        Queues.Clear();
        QueueCounts.Clear();
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
    ///     当单一任务超时时额外执行的全局逻辑
    ///     若任务本身设置了控制逻辑 (<see cref="TaskHelperTask.TimeoutAction" />) 则以任务的为判断标准
    /// </summary>
    public Action? TimeoutAction { get; set; }

    /// <summary>
    ///     当单一任务执行出现异常时额外执行的全局逻辑
    ///     若任务本身设置了控制逻辑 (<see cref="TaskHelperTask.ExceptionAction" />) 则以任务的为判断标准
    /// </summary>
    public Action? ExceptionAction { get; set; }

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

    private TaskHelperTask?                CurrentTask  { get; set; }
    private SortedSet<TaskHelperQueue>     Queues       { get; } = [new(1), new(0)];
    private ConcurrentDictionary<int, int> QueueCounts  { get; } = new();
    private Channel<ITaskHelperMessage>    TaskChannel  { get; } = CreateTaskChannel();
    private CancellationTokenSource        CancelSource { get; } = new();

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
                await DService.Instance().Framework.Run
                (
                    async () =>
                    {
                        SyncPendingTasks();

                        if (CurrentTask == null)
                            ProcessNextTask();

                        if (CurrentTask != null)
                        {
                            if (CurrentTask.IsAsync)
                                await ExecuteCurrentTaskAsync();
                            else
                                ExecuteCurrentTask();

                            isBusy = true;
                        }
                        else
                            isBusy = queueTaskCount > 0 || pendingTaskCount > 0;
                    },
                    ct
                ).ConfigureAwait(false);

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
        while (TaskChannel.Reader.TryRead(out var message))
        {
            switch (message)
            {
                case AddTaskMessage msg:
                    Interlocked.Decrement(ref pendingTaskCount);
                    var lookup = new TaskHelperQueue(msg.Weight);

                    if (!Queues.TryGetValue(lookup, out var queue))
                    {
                        queue = lookup;
                        Queues.Add(queue);
                        QueueCounts.TryAdd(msg.Weight, 0);
                    }

                    queue.Tasks.Add(msg.Task);
                    Interlocked.Increment(ref queueTaskCount);
                    QueueCounts.AddOrUpdate(msg.Weight, 1, (_, c) => c + 1);
                    break;
                case AbortMessage:
                    PerformAbort();
                    break;
                case AddQueueMessage msg:
                    if (Queues.All(q => q.Weight != msg.Weight))
                    {
                        Queues.Add(new(msg.Weight));
                        QueueCounts.TryAdd(msg.Weight, 0);
                    }

                    break;
                case RemoveQueueMessage msg:
                    PerformRemoveQueue(msg.Weight);
                    break;
                case RemoveQueueTasksMessage msg:
                    PerformRemoveQueueTasks(msg.Weight);
                    break;
                case RemoveQueueFirstTaskMessage msg:
                    PerformRemoveQueueFirstTask(msg.Weight);
                    break;
                case RemoveQueueLastTaskMessage msg:
                    PerformRemoveQueueLastTask(msg.Weight);
                    break;
                case RemoveQueueFirstNTasksMessage msg:
                    PerformRemoveQueueFirstNTasks(msg.Weight, msg.Count);
                    break;
            }
        }
    }

    private void ProcessNextTask()
    {
        foreach (var queue in Queues)
        {
            if (!queue.Tasks.TryDequeue(out var task)) continue;
            Interlocked.Decrement(ref queueTaskCount);
            QueueCounts.AddOrUpdate(queue.Weight, 0, (_, c) => Math.Max(0, c - 1));

            CurrentTask           = task;
            CurrentTask.StartTime = Stopwatch.GetTimestamp();

            Log($"开始执行任务: {CurrentTask.GetName()}");
            break;
        }
    }

    private void ExecuteCurrentTask()
    {
        try
        {
            if (CheckAndHandleTimeout()) return;

            if (CurrentTask.Action())
            {
                Log($"已完成任务: {CurrentTask.GetName()}");
                CurrentTask = null;
            }
        }
        catch (Exception ex)
        {
            HandleTaskError(ex);
        }
    }

    private async Task ExecuteCurrentTaskAsync()
    {
        try
        {
            if (CheckAndHandleTimeout()) return;

            if (await CurrentTask.AsyncAction(CurrentTask.CancelToken.Token).ConfigureAwait(false))
            {
                Log($"已完成任务: {CurrentTask.GetName()}");
                CurrentTask = null;
            }
        }
        catch (Exception ex)
        {
            HandleTaskError(ex);
        }
    }

    private bool CheckAndHandleTimeout()
    {
        if (CurrentTask == null) return false;

        var timeoutMS = CurrentTask.TimeoutMS <= 0 ? TimeoutMS : CurrentTask.TimeoutMS;
        if (timeoutMS <= 0) return false;

        if (Stopwatch.GetElapsedTime(CurrentTask.StartTime).TotalMilliseconds <= timeoutMS) return false;

        var timeoutBehaviour = CurrentTask.TimeoutBehaviour ?? TimeoutBehaviour;
        var reason           = $"任务 {CurrentTask.GetName()} 执行时间过长";
        var extraAction      = CurrentTask.TimeoutAction ?? TimeoutAction;

        ExecuteTaskAbortBehaviour(timeoutBehaviour, reason, CurrentTask, extraAction);
        return true;
    }

    private void HandleTaskError(Exception? ex = null)
    {
        if (CurrentTask == null) return;

        var exceptionBehaviour = CurrentTask.ExceptionBehaviour ?? ExceptionBehaviour;
        var reason             = $"执行任务 {CurrentTask.GetName()} 过程中出现错误";
        var extraAction        = CurrentTask.ExceptionAction ?? ExceptionAction;

        if (ex != null)
            Error(reason, ex);
        else
            Error(reason);

        ExecuteTaskAbortBehaviour(exceptionBehaviour, reason, CurrentTask, extraAction);
    }

    private void ExecuteTaskAbortBehaviour(TaskAbortBehaviour behaviour, string reason, TaskHelperTask task, Action? extraAction = null)
    {
        if (task == null) return;

        switch (behaviour)
        {
            case TaskAbortBehaviour.AbortAll:
                LogWarning($"放弃了所有任务 (原因: {reason})" + (extraAction == null ? string.Empty : "\n开始执行该原因出现时的自定义逻辑"));

                PerformAbort();
                break;
            case TaskAbortBehaviour.AbortCurrent:
                LogWarning($"放弃了当前任务 (原因: {reason})" + (extraAction == null ? string.Empty : "\n开始执行该原因出现时的自定义逻辑"));

                try
                {
                    CurrentTask?.CancelToken?.Cancel();
                    CurrentTask?.CancelToken?.Dispose();
                }
                catch
                {
                    // ignored
                }

                CurrentTask = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(behaviour));
        }

        try
        {
            extraAction?.Invoke();
        }
        catch (Exception ex)
        {
            Error($"在执行 {reason} 原因出现时应执行的自定义逻辑时发生错误", ex);
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

    private void PerformAbort()
    {
        foreach (var queue in Queues)
        {
            foreach (var task in queue.Tasks)
            {
                try
                {
                    task.CancelToken?.Cancel();
                    task.CancelToken?.Dispose();
                }
                catch
                {
                    // ignored
                }
            }

            queue.Tasks.Clear();
        }

        queueTaskCount = 0;
        QueueCounts.Clear();

        CurrentTask?.CancelToken?.Cancel();
        CurrentTask?.CancelToken?.Dispose();

        CurrentTask = null;
    }

    public void Abort() => TaskChannel.Writer.TryWrite(new AbortMessage());

    public void AddQueue(int weight) => TaskChannel.Writer.TryWrite(new AddQueueMessage(weight));

    private void PerformRemoveQueue(int weight)
    {
        LogWarning($"移除了权重 {weight} 队列");
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);
        if (queue == null) return;

        Interlocked.Add(ref queueTaskCount, -queue.Tasks.Count);
        QueueCounts.TryRemove(weight, out _);
        Queues.Remove(queue);
    }

    public void RemoveQueue(int weight) => TaskChannel.Writer.TryWrite(new RemoveQueueMessage(weight));

    private void PerformRemoveQueueTasks(int weight)
    {
        LogWarning($"清除了权重 {weight} 队列中的所有任务");
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);

        if (queue != null)
        {
            Interlocked.Add(ref queueTaskCount, -queue.Tasks.Count);
            queue.Tasks.Clear();
            QueueCounts.AddOrUpdate(weight, 0, (_, _) => 0);
        }
    }

    public void RemoveQueueTasks(int weight) => TaskChannel.Writer.TryWrite(new RemoveQueueTasksMessage(weight));

    private void PerformRemoveQueueFirstTask(int weight)
    {
        LogWarning($"移除了权重 {weight} 队列中的第一个任务");

        if ((Queues.FirstOrDefault(q => q.Weight == weight)?.Tasks ?? []).TryDequeue(out _))
        {
            Interlocked.Decrement(ref queueTaskCount);
            QueueCounts.AddOrUpdate(weight, 0, (_, c) => Math.Max(0, c - 1));
        }
    }

    public void RemoveQueueFirstTask(int weight) => TaskChannel.Writer.TryWrite(new RemoveQueueFirstTaskMessage(weight));

    private void PerformRemoveQueueLastTask(int weight)
    {
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);
        if (!((queue?.Tasks ?? []).Count > 0)) return;

        LogWarning($"清除了权重 {weight} 队列中的最后一个任务");
        queue.Tasks.RemoveAt(queue.Tasks.Count - 1);
        Interlocked.Decrement(ref queueTaskCount);
        QueueCounts.AddOrUpdate(weight, 0, (_, c) => Math.Max(0, c - 1));
    }

    public void RemoveQueueLastTask(int weight) => TaskChannel.Writer.TryWrite(new RemoveQueueLastTaskMessage(weight));

    private void PerformRemoveQueueFirstNTasks(int weight, int count)
    {
        var queue = Queues.FirstOrDefault(q => q.Weight == weight);

        if ((queue?.Tasks ?? []).Count > 0)
        {
            LogWarning($"清除了权重 {weight} 队列中的起始 {count} 个任务");

            var actualCountToRemove = Math.Min(count, queue.Tasks.Count);
            queue.Tasks.RemoveRange(0, actualCountToRemove);
            Interlocked.Add(ref queueTaskCount, -actualCountToRemove);
            QueueCounts.AddOrUpdate(weight, 0, (_, c) => Math.Max(0, c - actualCountToRemove));
        }
    }

    public void RemoveQueueFirstNTasks(int weight, int count) => TaskChannel.Writer.TryWrite(new RemoveQueueFirstNTasksMessage(weight, count));

    public int GetQueueTaskCount(int weight) => QueueCounts.GetValueOrDefault(weight, 0);

    #endregion

    private interface ITaskHelperMessage;

    private readonly record struct AddTaskMessage
    (
        TaskHelperTask Task,
        int            Weight
    ) : ITaskHelperMessage;

    private readonly record struct AbortMessage : ITaskHelperMessage;

    private readonly record struct AddQueueMessage
    (
        int Weight
    ) : ITaskHelperMessage;

    private readonly record struct RemoveQueueMessage
    (
        int Weight
    ) : ITaskHelperMessage;

    private readonly record struct RemoveQueueTasksMessage
    (
        int Weight
    ) : ITaskHelperMessage;

    private readonly record struct RemoveQueueFirstTaskMessage
    (
        int Weight
    ) : ITaskHelperMessage;

    private readonly record struct RemoveQueueLastTaskMessage
    (
        int Weight
    ) : ITaskHelperMessage;

    private readonly record struct RemoveQueueFirstNTasksMessage
    (
        int Weight,
        int Count
    ) : ITaskHelperMessage;
}
