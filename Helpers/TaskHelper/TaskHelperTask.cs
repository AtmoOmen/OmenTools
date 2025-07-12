namespace OmenTools.Helpers;

public record TaskHelperTask
{
    public Func<bool?>?                           Action         { get; }
    public Func<CancellationToken, Task<bool?>>? AsyncAction    { get; }
    public int                                   TimeLimitMS    { get; }
    public bool                                  AbortOnTimeout { get; }
    public string                                Name           { get; }
    public bool                                  IsAsync        => AsyncAction != null;
    public CancellationTokenSource?             CancellationTokenSource { get; }

    // 同步任务构造函数（保持向后兼容）
    public TaskHelperTask(Func<bool?> action, int timeLimitMS, bool abortOnTimeout, string? name)
    {
        Action         = action;
        AsyncAction    = null;
        TimeLimitMS    = timeLimitMS;
        AbortOnTimeout = abortOnTimeout;
        Name           = name ?? string.Empty;
        CancellationTokenSource = null;
    }

    public TaskHelperTask(Func<CancellationToken, Task<bool?>> asyncAction, int timeLimitMS, bool abortOnTimeout, string? name)
    {
        Action         = null;
        AsyncAction    = asyncAction;
        TimeLimitMS    = timeLimitMS;
        AbortOnTimeout = abortOnTimeout;
        Name           = name ?? string.Empty;
        CancellationTokenSource = new CancellationTokenSource();
    }
}
