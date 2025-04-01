namespace OmenTools.Helpers;

public record TaskHelperTask
{
    public Func<bool?> Action         { get; }
    public int         TimeLimitMS    { get; }
    public bool        AbortOnTimeout { get; }
    public string      Name           { get; }

    public TaskHelperTask(Func<bool?> action, int timeLimitMS, bool abortOnTimeout, string? name)
    {
        Action         = action;
        TimeLimitMS    = timeLimitMS;
        AbortOnTimeout = abortOnTimeout;
        Name           = name ?? string.Empty;
    }
}
