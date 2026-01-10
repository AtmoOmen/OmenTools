namespace OmenTools.Helpers;

/// <summary>
///     任务助手任务封装类
/// </summary>
public record TaskHelperTask
{
    /// <summary>
    ///     同步任务执行逻辑 <br />
    ///     返回 <c>true</c> 表示任务完成 <br />
    ///     返回 <c>false</c> 表示任务未完成，将在下一帧继续执行
    /// </summary>
    public Func<bool>? Action { get; }

    /// <summary>
    ///     异步任务执行逻辑 <br />
    ///     返回 <c>true</c> 表示任务完成 <br />
    ///     返回 <c>false</c> 表示任务未完成，将在下一帧继续执行
    /// </summary>
    public Func<CancellationToken, Task<bool>>? AsyncAction { get; }

    /// <summary>
    ///     任务超时时间 (毫秒) <br />
    ///     默认为 0; 设置为 ≤ 0 以默认不超时 (或使用全局配置) <br />
    ///     此处优先级大于 <see cref="TaskHelper.TimeoutMS" />
    /// </summary>
    public int TimeoutMS { get; }
    
    /// <summary>
    ///     当任务超时时的控制逻辑 <br />
    ///     若为 null 则使用全局配置 <see cref="TaskHelper.TimeoutBehaviour" />
    /// </summary>
    public TaskAbortBehaviour? TimeoutBehaviour   { get; }
    
    /// <summary>
    ///     当任务执行出现异常时的控制逻辑 <br />
    ///     若为 null 则使用全局配置 <see cref="TaskHelper.ExceptionBehaviour" />
    /// </summary>
    public TaskAbortBehaviour? ExceptionBehaviour { get; }
    
    /// <summary>
    ///     任务取消令牌源 <br />
    /// </summary>
    public CancellationTokenSource? CancelToken { get; }
    
    /// <summary>
    ///     任务名称
    /// </summary>
    private string? Name { get; }

    /// <summary>
    ///     任务开始时间 (时间戳)
    /// </summary>
    public long StartTime { get; set; }

    /// <summary>
    ///     是否为异步任务
    /// </summary>
    public bool IsAsync => AsyncAction != null;

    /// <summary>
    ///     初始化同步任务
    /// </summary>
    /// <param name="action">
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
    public TaskHelperTask(
        Func<bool>          action, 
        string?             name               = null,
        int                 timeoutMS          = 0,
        TaskAbortBehaviour? timeoutBehaviour   = null,
        TaskAbortBehaviour? exceptionBehaviour = null)
    {
        Action             = action;
        AsyncAction        = null;
        TimeoutMS          = timeoutMS;
        TimeoutBehaviour   = timeoutBehaviour;
        ExceptionBehaviour = exceptionBehaviour;
        Name               = name;
        CancelToken        = new();
    }

    /// <summary>
    ///     初始化异步任务
    /// </summary>
    /// <param name="asyncAction">
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
    public TaskHelperTask(
        Func<CancellationToken, Task<bool>> asyncAction, string? name = null,
        int                                 timeoutMS          = 0,
        TaskAbortBehaviour?                 timeoutBehaviour   = null,
        TaskAbortBehaviour?                 exceptionBehaviour = null)
    {
        Action             = null;
        AsyncAction        = asyncAction;
        TimeoutMS          = timeoutMS;
        TimeoutBehaviour   = timeoutBehaviour;
        ExceptionBehaviour = exceptionBehaviour;
        Name               = name;
        CancelToken        = new();
    }

    /// <summary>
    ///     获取任务名称
    /// </summary>
    /// <returns>任务名称，若未设置则返回 "(无名称)"</returns>
    public string GetName() => 
        Name ?? "(无名称)";
}
