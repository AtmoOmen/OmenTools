namespace OmenTools.Extensions;

public static class LogExtensions
{
    extension(Exception ex)
    {
        public void LogWarning(string? message = null)
            => Warning(message ?? string.Empty, ex);

        public void LogError(string? message = null)
            => Error(message ?? string.Empty, ex);
    }
}
