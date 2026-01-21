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
        
        var entry = pathEntries.GetOrAdd(fullPath, p =>
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelSource.Token);
            var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions 
            { 
                SingleReader = true 
            });

            _ = ProcessFileQueueAsync(p, channel.Reader, cts.Token);
            
            return new FileChannelEntry(channel, cts);
        });

        entry.LastAccessed = DateTime.UtcNow;
        entry.Channel.Writer.TryWrite(content);
    }

    private readonly ConcurrentDictionary<string, FileChannelEntry> pathEntries = [];

    private readonly CancellationTokenSource cancelSource = new();

    public SecureSaveHelper() =>
        _ = CleanIdleChannelsAsync();
    
    internal override void Uninit() =>
        ShutdownAsync().Wait();

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
            if (reader.TryRead(out var finalContent))
                await AtomicWriteAsync(filePath, finalContent);
        }
    }

    private static async Task AtomicWriteAsync(string filePath, string content)
    {
        var tempFile = filePath + ".tmp";
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
            Console.WriteLine($"[Save Error] {filePath}: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    private async Task CleanIdleChannelsAsync()
    {
        while (!cancelSource.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), cancelSource.Token);
            
            var now = DateTime.UtcNow;
            foreach (var (path, entry) in pathEntries)
            {
                if (now - entry.LastAccessed > TimeSpan.FromMinutes(1))
                {
                    if (pathEntries.TryRemove(path, out var removed))
                    {
                        removed.Channel.Writer.Complete();
                        removed.Cts.Cancel();
                    }
                }
            }
        }
    }

    internal async Task ShutdownAsync()
    {
        cancelSource.Cancel();
        
        foreach (var entry in pathEntries.Values)
            entry.Channel.Writer.Complete();

        await Task.Delay(1000); 
        pathEntries.Clear();
    }
    
    private record FileChannelEntry(Channel<string> Channel, CancellationTokenSource Cts)
    {
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    }
}
