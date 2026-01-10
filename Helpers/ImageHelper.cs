using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Channels;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Data;
using OmenTools.Abstracts;

namespace OmenTools.Helpers;

public class ImageHelper : OmenServiceBase<ImageHelper>
{
    public static IDalamudTextureWrap? GetGameIcon(uint iconID, bool isHQ = false) =>
        TryGetGameIcon(iconID, out var texture, isHQ) ? texture : null;

    public static bool TryGetGameIcon(uint icon, [NotNullWhen(true)] out IDalamudTextureWrap? texture, bool isHQ = false)
    {
        if (DService.Instance().Texture.TryGetFromGameIcon(new(icon, isHQ), out var immediateTexture))
        {
            texture = immediateTexture.GetWrapOrEmpty();
            return true;
        }

        texture = null;
        return false;
    }
    
    public IDalamudTextureWrap? GetGameLangIcon(uint iconID, bool isHQ = false) =>
        TryGetGameLangIcon(iconID, out var texture, isHQ) ? texture : null;
    
    public IDalamudTextureWrap? GetImage(string urlOrPath) =>
        TryGetImage(urlOrPath, out var texture) ? texture : null;

    public bool TryGetGameLangIcon(uint icon, [NotNullWhen(true)] out IDalamudTextureWrap? texture, bool isHQ = false)
    {
        var key = (icon, isHQ, GameState.ClientLanguge);

        if (cachedIcons.TryGetValue(key, out var result))
        {
            result.RefreshAccess();
            texture = result.Texture;
            return texture != null;
        }

        result = new ImageLoadingResult
        {
            ImmediateTexture = DService.Instance().Texture.GetFromGame(GetIconTexturePath(icon, GameState.ClientLanguge)),
            IsCompleted      = true
        };

        result.CompletionSource.TrySetResult(result.Texture);

        if (cachedIcons.TryAdd(key, result))
            AddToExpirationQueue(key);

        texture = result.Texture;
        return texture != null;
    }
    
    public bool TryGetImage(string url, [NotNullWhen(true)] out IDalamudTextureWrap? texture)
    {
        texture = null;
        if (string.IsNullOrWhiteSpace(url)) return false;

        if (cachedTextures.TryGetValue(url, out var result))
        {
            result.RefreshAccess();
            texture = result.Texture;
            return texture != null;
        }

        var newResult = new ImageLoadingResult();

        if (cachedTextures.TryAdd(url, newResult))
        {
            AddToExpirationQueue(url);
            downloadChannel.Writer.TryWrite(url);
        }
        else
        {
            if (cachedTextures.TryGetValue(url, out result))
            {
                result.RefreshAccess();
                texture = result.Texture;
                return texture != null;
            }
        }

        return false;
    }

    public async Task<IDalamudTextureWrap?> GetImageAsync(string urlOrPath)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath)) return null;
        
        if (!cachedTextures.TryGetValue(urlOrPath, out var result))
        {
            TryGetImage(urlOrPath, out _);

            if (!cachedTextures.TryGetValue(urlOrPath, out result)) return null;
        }

        result.RefreshAccess();
        
        if (result.IsCompleted) return result.Texture;
        
        try
        {
            return await result.CompletionSource.Task;
        }
        catch
        {
            return null;
        }
    }

    public void ClearAll()
    {
        expirationChannel.Writer.TryWrite(new ClearCommand());

        foreach (var value in cachedTextures.Values)
            value.Dispose();
        cachedTextures.Clear();

        foreach (var value in cachedIcons.Values)
            value.Dispose();
        cachedIcons.Clear();
    }


    private static readonly TimeSpan CacheTTL = TimeSpan.FromSeconds(30);

    private readonly Channel<string> downloadChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    private readonly Channel<ExpirationCommand> expirationChannel = Channel.CreateUnbounded<ExpirationCommand>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    private abstract record ExpirationCommand;

    private record AddCommand(object Key, long Ticks) : ExpirationCommand;

    private record ClearCommand : ExpirationCommand;

    private readonly ConcurrentDictionary<string, ImageLoadingResult>                                cachedTextures = [];
    private readonly ConcurrentDictionary<(uint ID, bool HQ, Language Language), ImageLoadingResult> cachedIcons    = [];

    private readonly HttpClient httpClient = new()
    {
        Timeout               = TimeSpan.FromSeconds(30),
        DefaultRequestHeaders = { { "User-Agent", "OmenTools/1.0" } }
    };

    private readonly CancellationTokenSource globalCancelSource = new();

    internal override void Init()
    {
        _ = ProcessDownloadsAsync(globalCancelSource.Token);
        _ = CleanupLoopAsync(globalCancelSource.Token);
    }

    internal override void Uninit()
    {
        globalCancelSource.Cancel();
        ClearAll();
        globalCancelSource.Dispose();
        httpClient.Dispose();
    }

    private void AddToExpirationQueue(object key) =>
        expirationChannel.Writer.TryWrite(new AddCommand(key, DateTime.UtcNow.Ticks + CacheTTL.Ticks));

    private async Task ProcessDownloadsAsync(CancellationToken ct)
    {
        var reader = downloadChannel.Reader;

        try
        {
            await Parallel.ForEachAsync(reader.ReadAllAsync(ct), new ParallelOptions
            {
                MaxDegreeOfParallelism = 8,
                CancellationToken      = ct
            }, async (url, token) =>
            {
                if (!cachedTextures.TryGetValue(url, out var result) || result.IsCompleted)
                    return;

                try
                {
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        var content = await httpClient.GetByteArrayAsync(uri, token);

                        result.TextureWrap = await DService.Instance().Texture.CreateFromImageAsync(content, url, token);
                    }
                    else
                    {
                        result.ImmediateTexture = File.Exists(url)
                                                      ? DService.Instance().Texture.GetFromFile(url)
                                                      : DService.Instance().Texture.GetFromGame(url);
                    }

                    result.CompletionSource.TrySetResult(result.Texture);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Error($"[ImageHelper] 加载资源失败: {url}", ex);
                    result.CompletionSource.TrySetResult(null);
                }
                finally
                {
                    result.IsCompleted = true;
                }
            });
        }
        catch (OperationCanceledException)
        {

        }
    }

    private async Task CleanupLoopAsync(CancellationToken ct)
    {
        var localQueue   = new PriorityQueue<object, long>();
        var reader       = expirationChannel.Reader;
        var readWaitTask = reader.WaitToReadAsync(ct).AsTask();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                long waitTicks;

                if (localQueue.TryPeek(out _, out var ticks))
                {
                    waitTicks = ticks - DateTime.UtcNow.Ticks;
                    if (waitTicks < 0)
                        waitTicks = 0;
                }
                else
                    waitTicks = -1;

                var waitTask = waitTicks == -1
                                   ? Task.Delay(Timeout.Infinite,              ct)
                                   : Task.Delay(TimeSpan.FromTicks(waitTicks), ct);

                var completed = await Task.WhenAny(waitTask, readWaitTask);

                if (completed == readWaitTask)
                {
                    if (await readWaitTask)
                    {
                        while (reader.TryRead(out var cmd))
                        {
                            switch (cmd)
                            {
                                case AddCommand add:
                                    localQueue.Enqueue(add.Key, add.Ticks);
                                    break;
                                case ClearCommand:
                                    localQueue.Clear();
                                    break;
                            }
                        }

                        readWaitTask = reader.WaitToReadAsync(ct).AsTask();
                    }
                    else
                        break;
                }
                else
                {
                    while (localQueue.TryPeek(out var key, out var itemTicks))
                    {
                        if (itemTicks <= DateTime.UtcNow.Ticks)
                        {
                            localQueue.Dequeue();
                            CheckAndProcessItem(key, localQueue);
                        }
                        else
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Error("[ImageHelper] 缓存清理任务异常", ex);
                if (readWaitTask.IsCompleted)
                    readWaitTask = reader.WaitToReadAsync(ct).AsTask();
            }
        }
    }

    private void CheckAndProcessItem(object key, PriorityQueue<object, long> queue)
    {
        ImageLoadingResult? result  = null;
        var                 removed = false;

        if (key is string urlKey)
        {
            if (cachedTextures.TryGetValue(urlKey, out result))
            {
                if (DateTime.UtcNow - result.LastAccessTime > CacheTTL)
                {
                    if (cachedTextures.TryRemove(urlKey, out var removedItem))
                    {
                        removedItem.Dispose();
                        removed = true;
                    }
                }
            }
        }
        else if (key is ValueTuple<uint, bool, Language> iconKey)
        {
            if (cachedIcons.TryGetValue(iconKey, out result))
            {
                if (DateTime.UtcNow - result.LastAccessTime > CacheTTL)
                {
                    if (cachedIcons.TryRemove(iconKey, out var removedItem))
                    {
                        removedItem.Dispose();
                        removed = true;
                    }
                }
            }
        }

        if (!removed && result != null)
            queue.Enqueue(key, result.LastAccessTime.Ticks + CacheTTL.Ticks);
    }

    private static string GetIconTexturePath(uint iconID, Language language)
    {
        var variant = language switch
        {
            Language.Japanese           => "ja",
            Language.English            => "en",
            Language.German             => "de",
            Language.French             => "fr",
            Language.ChineseSimplified  => "chs",
            Language.ChineseTraditional => "cht",
            Language.Korean             => "ko",
            Language.TraditionalChinese => "tc",
            _                           => ""
        };

        if (variant.Length == 0) return string.Empty;

        return $"ui/icon/{iconID / 1000 * 1000:D6}/{variant}/{iconID:D6}_hr1.tex";
    }

    private class ImageLoadingResult : IDisposable
    {
        public          ISharedImmediateTexture? ImmediateTexture { get; set; }
        public          IDalamudTextureWrap?     TextureWrap      { get; set; }
        public volatile bool                     IsCompleted;

        public readonly TaskCompletionSource<IDalamudTextureWrap?> CompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private long     lastAccessTimeTicks = DateTime.UtcNow.Ticks;
        public  DateTime LastAccessTime => new(lastAccessTimeTicks);

        public void RefreshAccess() => Interlocked.Exchange(ref lastAccessTimeTicks, DateTime.UtcNow.Ticks);

        public IDalamudTextureWrap? Texture
        {
            get
            {
                if (TextureWrap != null) return TextureWrap;
                return ImmediateTexture?.GetWrapOrEmpty();
            }
        }

        public void Dispose()
        {
            CompletionSource.TrySetCanceled();

            try
            {
                TextureWrap?.Dispose();
            }
            catch
            {
                // ignored
            }

            ImmediateTexture = null;
            TextureWrap      = null;
        }
    }
}
