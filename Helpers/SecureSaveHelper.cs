using System.Collections.Concurrent;
using System.IO;
using System.Threading.Channels;
using OmenTools.Abstracts;

namespace OmenTools.Helpers;

public class SecureSaveHelper : OmenServiceBase<SecureSaveHelper>
{
    public void WriteAllText(string path, string content)
    {
        var fullPath = Path.GetFullPath(path);

        while (true)
        {
            var entry = pathEntries.GetOrAdd(fullPath, p => new Lazy<FileChannelEntry>(() =>
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelSource.Token);
                var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions 
                { 
                    SingleReader = true 
                });

                var processingTask = ProcessFileQueueAsync(p, channel.Reader, cts.Token);
                
                return new(channel, cts, processingTask);
            })).Value;

            entry.LastAccessed = DateTime.UtcNow;
            
            if (entry.Channel.Writer.TryWrite(content))
                return;

            Thread.Yield();
        }
    }
    

    private readonly ConcurrentDictionary<string, Lazy<FileChannelEntry>> pathEntries = [];

    private readonly CancellationTokenSource cancelSource = new();

    public SecureSaveHelper() =>
        _ = CleanIdleChannelsAsync();
    
    internal override void Uninit()
    {
        cancelSource.Cancel();
        pathEntries.Clear();
    }

    private static async Task ProcessFileQueueAsync(string filePath, ChannelReader<string> reader, CancellationToken ct)
    {
        try
        {
            while (await reader.WaitToReadAsync(ct))
            {
                await Task.Delay(500, ct);

                string? latestContent = null;
                while (reader.TryRead(out var content))
                {
                    latestContent = content;
                }

                if (latestContent != null)
                {
                    await AtomicWriteAsync(filePath, latestContent);
                }
            }
            
        }
        catch (OperationCanceledException)
        {
            string? latestContent = null;
            while (reader.TryRead(out var content))
                latestContent = content;

            if (latestContent != null)
                await AtomicWriteAsync(filePath, latestContent);
        }
    }

    private static async Task AtomicWriteAsync(string filePath, string content)
    {
        var tempFile = $"{filePath}.{Guid.NewGuid()}.tmp";
        try
        {
            await File.WriteAllTextAsync(tempFile, content);
            if (File.Exists(filePath))
                File.Replace(tempFile, filePath, null);
            else
                File.Move(tempFile, filePath);
        }
        catch (Exception ex)
        {
            Error($"尝试保存内容至 {filePath} 时发生错误", ex);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private async Task CleanIdleChannelsAsync()
    {
        try
        {
            while (!cancelSource.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancelSource.Token);
                
                var now = DateTime.UtcNow;
                foreach (var (path, lazyEntry) in pathEntries)
                {
                    if (!lazyEntry.IsValueCreated) continue;

                    var entry = lazyEntry.Value;
                    if (now - entry.LastAccessed > TimeSpan.FromMinutes(1))
                    {
                        if (pathEntries.TryRemove(path, out var removedLazy))
                        {
                            if (removedLazy.IsValueCreated)
                            {
                                var removed = removedLazy.Value;
                                removed.Channel.Writer.TryComplete();
                            }
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }
    
    private record FileChannelEntry(Channel<string> Channel, CancellationTokenSource Cts, Task ProcessingTask)
    {
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    }
}
