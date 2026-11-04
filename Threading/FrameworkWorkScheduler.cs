using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace OmenTools.Threading;

/// <summary>
///     将生命周期操作按实际帧间隔的比例分配到后续游戏帧中执行，并限制单帧时间预算。
/// </summary>
/// <remarks>
///     在使用之前请先考虑：相关工作是不是真的一定要在游戏主线程上完成，如果“不是”的话，请不要使用本类。
/// </remarks>
public static class FrameworkWorkScheduler
{
    /// <summary>
    ///     依次执行操作，每帧使用实际帧间隔的 10%，最多使用 10 毫秒，尚有剩余工作时等待下一游戏帧。
    /// </summary>
    /// <remarks>
    ///     单项操作同步执行且不可中断，因此单项操作本身可能使当前时间片超出预算。
    /// </remarks>
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
        await framework.Run
        (
            async () =>
            {
                var index = 0;

                while (index < items.Count)
                {
                    var budgetTicks = Math.Min(framework.UpdateDelta.TotalSeconds * FRAME_BUDGET_RATIO, MAX_FRAME_BUDGET_SECONDS) * Stopwatch.Frequency;
                    var startedAt   = Stopwatch.GetTimestamp();

                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        operation(items[index++]);
                    }
                    while (index                                < items.Count &&
                           Stopwatch.GetTimestamp() - startedAt < budgetTicks);

                    if (index < items.Count)
                        await framework.DelayTicks(1, cancellationToken);
                }
            },
            cancellationToken
        ).ConfigureAwait(false);
    }

    #region 常量

    private const double FRAME_BUDGET_RATIO       = 0.1;
    private const double MAX_FRAME_BUDGET_SECONDS = 0.01;

    #endregion
}
