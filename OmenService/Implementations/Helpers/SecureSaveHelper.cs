using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using OmenTools.Dalamud;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

/// <summary>
/// 按文件路径合并保存请求，并通过临时文件替换目标文件完成写入。
/// </summary>
public class SecureSaveHelper : OmenServiceBase<SecureSaveHelper>
{
    /// <summary>
    /// 将文本编码为无字节序标记的 UTF-8，并加入文件保存队列。
    /// 同一路径的连续请求会合并，仅写入最新内容。
    /// </summary>
    /// <param name="path">需要写入的文件路径。</param>
    /// <param name="content">需要保存的文本内容。</param>
    public void WriteAllText(string path, string content) =>
        EnqueueWrite(path, UTF8.GetBytes(content));

    /// <summary>
    /// 复制二进制内容并加入文件保存队列，调用返回后即可修改或释放原始缓冲区。
    /// 同一路径的连续请求会合并，仅写入最新内容。
    /// </summary>
    /// <param name="path">需要写入的文件路径。</param>
    /// <param name="content">需要保存的二进制内容。</param>
    public void WriteAllBytes(string path, ReadOnlySpan<byte> content) =>
        EnqueueWrite(path, [.. content]);

    private void EnqueueWrite(string path, byte[] content)
    {
        var fullPath = Path.GetFullPath(path);

        while (true)
        {
            var entry = pathEntries.GetOrAdd
            (
                fullPath,
                p => new Lazy<FileChannelEntry>
                (() =>
                    {
                        var channel = Channel.CreateUnbounded<byte[]>
                        (
                            new UnboundedChannelOptions
                            {
                                SingleReader = true
                            }
                        );

                        _ = ProcessFileQueueAsync(p, channel.Reader, cancelSource.Token);

                        return new(channel);
                    }
                )
            ).Value;

            entry.LastAccessed = DateTime.UtcNow;

            if (entry.Channel.Writer.TryWrite(content))
                return;

            Thread.Yield();
        }
    }


    private readonly ConcurrentDictionary<string, Lazy<FileChannelEntry>> pathEntries = [];

    private readonly CancellationTokenSource cancelSource = new();

    /// <summary>
    /// 初始化保存队列，并启动空闲队列清理任务。
    /// </summary>
    public SecureSaveHelper() =>
        _ = CleanIdleChannelsAsync();

    protected override void Uninit()
    {
        cancelSource.Cancel();
        pathEntries.Clear();
    }

    private static async Task ProcessFileQueueAsync(string filePath, ChannelReader<byte[]> reader, CancellationToken ct)
    {
        try
        {
            while (await reader.WaitToReadAsync(ct))
            {
                await Task.Delay(500, ct);

                byte[]? latestContent                                 = null;
                while (reader.TryRead(out var content)) latestContent = content;

                if (latestContent != null) await AtomicWriteAsync(filePath, latestContent);
            }

        }
        catch (OperationCanceledException)
        {
            byte[]? latestContent = null;
            while (reader.TryRead(out var content))
                latestContent = content;

            if (latestContent != null)
                await AtomicWriteAsync(filePath, latestContent);
        }
    }

    private static async Task AtomicWriteAsync(string filePath, byte[] content)
    {
        var tempFile = $"{filePath}.{Guid.NewGuid()}.tmp";

        try
        {
            await File.WriteAllBytesAsync(tempFile, content);
            if (File.Exists(filePath))
                File.Replace(tempFile, filePath, null);
            else
                File.Move(tempFile, filePath);
        }
        catch (Exception ex)
        {
            DLog.Error($"尝试保存内容至 {filePath} 时发生错误", ex);
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

    private record FileChannelEntry
    (
        Channel<byte[]> Channel
    )
    {
        /// <summary>
        /// 获取或设置最近一次提交保存请求的 UTC 时间。
        /// </summary>
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    }

    #region 常量

    private static readonly UTF8Encoding UTF8 = new(false);

    #endregion
}
