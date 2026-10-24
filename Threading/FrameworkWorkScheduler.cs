using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace OmenTools.Threading;

/// <summary>
///     将生命周期操作按时间预算分配到后续游戏帧中执行。
/// </summary>
public static class FrameworkWorkScheduler
{
    /// <summary>
    ///     依次执行操作，在每个时间片结束后将剩余工作调度到后续游戏帧。
    /// </summary>
    /// <param name="items">按照执行顺序排列的工作项。</param>
    /// <param name="operation">必须在游戏线程同步执行的单项操作。</param>
    /// <param name="cancellationToken">用于取消尚未执行的操作。</param>
    /// <typeparam name="T">工作项类型。</typeparam>
    public static async Task RunAsync<T>
    (
        IReadOnlyList<T>  items,
        Action<T>         operation,
        CancellationToken cancellationToken = default
    )
    {
        if (items.Count == 0)
            return;

        var framework = IFramework.Instance();
        var index     = 0;

        while (index < items.Count)
        {
            await framework.Run
            (
                () =>
                {
                    var startedAt = Stopwatch.GetTimestamp();

                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        operation(items[index++]);
                    }
                    while (index < items.Count && Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds < 1);
                },
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }
    }
}
