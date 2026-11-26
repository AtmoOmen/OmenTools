namespace OmenTools.Extensions;

/// <summary>
///     提供可空异步释放对象的便捷处理。
/// </summary>
public static class AsyncDisposableExtension
{
    extension(IAsyncDisposable? asyncDisposable)
    {
        /// <summary>
        ///     异步释放对象；对象为空时直接完成。
        /// </summary>
        /// <returns>释放操作的完成任务。</returns>
        public Task DisposeAsTaskAsync() =>
            asyncDisposable?.DisposeAsync().AsTask() ?? Task.CompletedTask;
    }
}
