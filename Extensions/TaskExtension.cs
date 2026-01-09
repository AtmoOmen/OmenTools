namespace OmenTools.Extensions;

public static class TaskExtension
{
    extension(Task task)
    {
        public static async ValueTask WaitForConditionAsync(Func<bool> condition, TimeSpan? timeout = null)
        {
            if (condition()) return;

            using var cts   = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
            var       token = cts?.Token ?? CancellationToken.None;

            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

            try
            {
                while (await timer.WaitForNextTickAsync(token))
                    if (condition())
                        return;
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException();
            }
        }
    }
}
