namespace OmenTools.Extensions;

public static class TaskExtension
{
    extension(Task task)
    {
        public static async ValueTask WaitForConditionAsync(Func<bool> condition, TimeSpan? timeout = null, CancellationToken? token = null)
        {
            if (condition()) return;
            
            token ??= CancellationToken.None;

            long deadlineTick = 0;
            if (timeout.HasValue)
                deadlineTick = Environment.TickCount64 + (long)timeout.Value.TotalMilliseconds;

            var interval = TimeSpan.FromMilliseconds(100); 

            while (!token.Value.IsCancellationRequested)
            {
                await Task.Delay(interval, token.Value).ConfigureAwait(false);

                if (condition()) return;

                if (timeout.HasValue && Environment.TickCount64 >= deadlineTick)
                    throw new TimeoutException();
            }
        }
    }
}
