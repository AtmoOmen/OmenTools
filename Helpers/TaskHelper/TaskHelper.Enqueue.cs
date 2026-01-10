using System.Diagnostics;

namespace OmenTools.Helpers;

public partial class TaskHelper
{
    #region 同步

    /// <summary>
    ///     将任务封装对象加入队列
    /// </summary>
    /// <param name="task">任务对象</param>
    /// <param name="weight">队列权重 (默认为 0)</param>
    public void Enqueue(TaskHelperTask task, int weight = 0)
    {
        if (task is null)
            throw new ArgumentNullException(nameof(task));
        if (task.Action == null && task.AsyncAction == null)
            throw new ArgumentException($"任务 {task.GetName()} 的执行逻辑为空 (Action 和 AsyncAction 均为 null)");
        if (task is { Action: not null, AsyncAction: not null })
            throw new ArgumentException($"任务 {task.GetName()} 的执行逻辑不明确 (Action 和 AsyncAction 均不为 null)");
        
        TaskChannel.Writer.TryWrite((task, weight));
        Interlocked.Increment(ref pendingTaskCount);
        TryRegisterTick();
    }

    /// <summary>
    ///     将同步任务加入队列
    /// </summary>
    /// <param name="task">
    ///     任务执行逻辑 <br />
    ///     返回 <c>true</c> 表示任务完成 <br />
    ///     返回 <c>false</c> 表示任务未完成，将在下一帧继续执行
    /// </param>
    /// <param name="name">任务名称</param>
    /// <param name="timeoutMS">
    ///     超时时间 (毫秒) <br />
    ///     默认为 0; 设置为 ≤ 0 以默认不超时
    /// </param>
    /// <param name="timeoutBehaviour">超时控制行为</param>
    /// <param name="exceptionBehaviour">异常控制行为</param>
    /// <param name="weight">队列权重</param>
    public void Enqueue(
        Func<bool>          task,
        string?             name               = null,
        int                 timeoutMS          = 0,
        TaskAbortBehaviour? timeoutBehaviour   = null,
        TaskAbortBehaviour? exceptionBehaviour = null,
        int                 weight             = 0)
    {
        TaskChannel.Writer.TryWrite((new(task, name, timeoutMS, timeoutBehaviour, exceptionBehaviour), weight));
        Interlocked.Increment(ref pendingTaskCount);
        TryRegisterTick();
    }

    /// <summary>
    ///     将一次性同步操作加入队列
    /// </summary>
    /// <param name="task">要执行的操作</param>
    /// <param name="name">任务名称</param>
    /// <param name="timeoutMS">
    ///     超时时间 (毫秒) <br />
    ///     默认为 0; 设置为 ≤ 0 以默认不超时
    /// </param>
    /// <param name="timeoutBehaviour">超时控制行为</param>
    /// <param name="exceptionBehaviour">异常控制行为</param>
    /// <param name="weight">队列权重</param>
    public void Enqueue(
        Action              task,
        string?             name               = null,
        int                 timeoutMS          = 0,
        TaskAbortBehaviour? timeoutBehaviour   = null,
        TaskAbortBehaviour? exceptionBehaviour = null,
        int                 weight             = 0) =>
        Enqueue(() =>
        {
            task();
            return true;
        }, name, timeoutMS, timeoutBehaviour, exceptionBehaviour, weight);

    /// <summary>
    ///     将可能返回 null 的同步任务加入队列
    /// </summary>
    /// <param name="task">
    ///     任务执行逻辑 <br />
    ///     返回 <c>true</c> 或 <c>null</c> 表示任务完成 <br />
    ///     返回 <c>false</c> 表示任务未完成，将在下一帧继续执行
    /// </param>
    /// <param name="name">任务名称</param>
    /// <param name="timeoutMS">
    ///     超时时间 (毫秒) <br />
    ///     默认为 0; 设置为 ≤ 0 以默认不超时
    /// </param>
    /// <param name="timeoutBehaviour">超时控制行为</param>
    /// <param name="exceptionBehaviour">异常控制行为</param>
    /// <param name="weight">队列权重</param>
    public void Enqueue(
        Func<bool?>         task,
        string?             name               = null,
        int                 timeoutMS          = 0,
        TaskAbortBehaviour? timeoutBehaviour   = null,
        TaskAbortBehaviour? exceptionBehaviour = null,
        int                 weight             = 0) =>
        Enqueue(() => task() ?? false, name, timeoutMS, timeoutBehaviour, exceptionBehaviour, weight);

    #endregion

    #region 异步

    /// <summary>
    ///     将异步任务加入队列
    /// </summary>
    /// <param name="asyncTask">
    ///     异步任务执行逻辑 <br />
    ///     接受 <see cref="CancellationToken" /> <br />
    ///     返回 <c>true</c> 表示任务完成 <br />
    ///     返回 <c>false</c> 表示任务未完成，将在下一帧继续执行
    /// </param>
    /// <param name="name">任务名称</param>
    /// <param name="timeoutMS">
    ///     超时时间 (毫秒) <br />
    ///     默认为 0; 设置为 ≤ 0 以默认不超时
    /// </param>
    /// <param name="timeoutBehaviour">超时控制行为</param>
    /// <param name="exceptionBehaviour">异常控制行为</param>
    /// <param name="weight">队列权重</param>
    public void EnqueueAsync(
        Func<CancellationToken, Task<bool>> asyncTask,
        string?                             name               = null,
        int                                 timeoutMS          = 0,
        TaskAbortBehaviour?                 timeoutBehaviour   = null,
        TaskAbortBehaviour?                 exceptionBehaviour = null,
        int                                 weight             = 0)
    {
        TaskChannel.Writer.TryWrite((new(asyncTask, name, timeoutMS, timeoutBehaviour, exceptionBehaviour), weight));
        Interlocked.Increment(ref pendingTaskCount);
        TryRegisterTick();
    }

    /// <summary>
    ///     将不返回结果的异步任务加入队列
    /// </summary>
    /// <param name="asyncTask">
    ///     异步任务执行逻辑 <br />
    ///     接受 <see cref="CancellationToken" />
    /// </param>
    /// <param name="name">任务名称</param>
    /// <param name="timeoutMS">
    ///     超时时间 (毫秒) <br />
    ///     默认为 0; 设置为 ≤ 0 以默认不超时
    /// </param>
    /// <param name="timeoutBehaviour">超时控制行为</param>
    /// <param name="exceptionBehaviour">异常控制行为</param>
    /// <param name="weight">队列权重</param>
    public void EnqueueAsync(
        Func<CancellationToken, Task> asyncTask,
        string?                       name               = null,
        int                           timeoutMS          = 0,
        TaskAbortBehaviour?           timeoutBehaviour   = null,
        TaskAbortBehaviour?           exceptionBehaviour = null,
        int                           weight             = 0) =>
        EnqueueAsync(async ct =>
        {
            await asyncTask(ct);
            return true;
        }, name, timeoutMS, timeoutBehaviour, exceptionBehaviour, weight);

    /// <summary>
    ///     将可能返回 null 的异步任务加入队列
    /// </summary>
    /// <param name="asyncTask">
    ///     异步任务执行逻辑 <br />
    ///     接受 <see cref="CancellationToken" /> <br />
    ///     返回 <c>true</c> 或 <c>null</c> 表示任务完成 <br />
    ///     返回 <c>false</c> 表示任务未完成，将在下一帧继续执行
    /// </param>
    /// <param name="name">任务名称</param>
    /// <param name="timeoutMS">
    ///     超时时间 (毫秒) <br />
    ///     默认为 0; 设置为 ≤ 0 以默认不超时
    /// </param>
    /// <param name="timeoutBehaviour">超时控制行为</param>
    /// <param name="exceptionBehaviour">异常控制行为</param>
    /// <param name="weight">队列权重</param>
    public void EnqueueAsync(
        Func<CancellationToken, Task<bool?>> asyncTask,
        string?                              name               = null,
        int                                  timeoutMS          = 0,
        TaskAbortBehaviour?                  timeoutBehaviour   = null,
        TaskAbortBehaviour?                  exceptionBehaviour = null,
        int                                  weight             = 0) =>
        EnqueueAsync(async ct => (await asyncTask(ct)) ?? false, name, timeoutMS, timeoutBehaviour, exceptionBehaviour, weight);

    /// <summary>
    ///     将不接受 CancellationToken 的异步任务加入队列
    /// </summary>
    /// <param name="asyncTask">
    ///     异步任务执行逻辑 <br />
    ///     返回 <c>true</c> 表示任务完成 <br />
    ///     返回 <c>false</c> 表示任务未完成，将在下一帧继续执行
    /// </param>
    /// <param name="name">任务名称</param>
    /// <param name="timeoutMS">
    ///     超时时间 (毫秒) <br />
    ///     默认为 0; 设置为 ≤ 0 以默认不超时
    /// </param>
    /// <param name="timeoutBehaviour">超时控制行为</param>
    /// <param name="exceptionBehaviour">异常控制行为</param>
    /// <param name="weight">队列权重</param>
    public void EnqueueAsync(
        Func<Task<bool>>    asyncTask,
        string?             name               = null,
        int                 timeoutMS          = 0,
        TaskAbortBehaviour? timeoutBehaviour   = null,
        TaskAbortBehaviour? exceptionBehaviour = null,
        int                 weight             = 0) =>
        EnqueueAsync(_ => asyncTask(), name, timeoutMS, timeoutBehaviour, exceptionBehaviour, weight);

    /// <summary>
    ///     将不接受 CancellationToken 且不返回结果的异步任务加入队列
    /// </summary>
    /// <param name="asyncTask">异步任务执行逻辑</param>
    /// <param name="name">任务名称</param>
    /// <param name="timeoutMS">
    ///     超时时间 (毫秒) <br />
    ///     默认为 0; 设置为 ≤ 0 以默认不超时
    /// </param>
    /// <param name="timeoutBehaviour">超时控制行为</param>
    /// <param name="exceptionBehaviour">异常控制行为</param>
    /// <param name="weight">队列权重</param>
    public void EnqueueAsync(
        Func<Task>          asyncTask,
        string?             name               = null,
        int                 timeoutMS          = 0,
        TaskAbortBehaviour? timeoutBehaviour   = null,
        TaskAbortBehaviour? exceptionBehaviour = null,
        int                 weight             = 0) =>
        EnqueueAsync(async _ =>
        {
            await asyncTask();
            return true;
        }, name, timeoutMS, timeoutBehaviour, exceptionBehaviour, weight);

    #endregion

    /// <summary>
    ///     延迟执行下一个任务
    /// </summary>
    /// <param name="delayMS">延迟时间 (毫秒)</param>
    /// <param name="uniqueName">任务名称后缀 (将显示为 "uniqueName (延迟 X 毫秒)")</param>
    /// <param name="weight">队列权重</param>
    public void DelayNext(int delayMS, string? uniqueName = null, int weight = 0)
    {
        if (delayMS <= 0) return;

        EnqueueAsync
        (
            async ct => await Task.Delay(delayMS, ct).ConfigureAwait(false),
            $"{uniqueName} (延迟 {delayMS} 毫秒)",
            weight: weight
        );
    }
}
