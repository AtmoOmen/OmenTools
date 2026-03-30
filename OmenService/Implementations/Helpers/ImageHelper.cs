using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Data;
using OmenTools.Dalamud;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

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
            ImmediateTexture = DService.Instance().Texture.GetFromGame(GetIconTexturePath(icon, GameState.ClientLanguge))
        };

        result.TryCompleteByTexture(StandardTimeManager.Instance().UTCNow, FailedCacheTTL);

        if (cachedIcons.TryAdd(key, result))
            AddToExpirationQueue(key);

        texture = result.Texture;
        return texture != null;
    }

    public bool TryGetImage(string url, [NotNullWhen(true)] out IDalamudTextureWrap? texture)
    {
        texture = null;
        if (string.IsNullOrWhiteSpace(url)) return false;

        while (true)
        {
            var now = StandardTimeManager.Instance().UTCNow;

            if (cachedTextures.TryGetValue(url, out var result))
            {
                result.RefreshAccess();

                if (result.State == ImageLoadState.Failed && !result.IsFailureCooldownActive(now))
                {
                    var retryResult = new ImageLoadingResult();
                    retryResult.RefreshAccess();

                    if (cachedTextures.TryUpdate(url, retryResult, result))
                    {
                        AddToExpirationQueue(url);
                        QueueDownload(url, retryResult);
                        return false;
                    }

                    continue;
                }

                texture = result.Texture;
                return texture != null;
            }

            var created = new ImageLoadingResult();

            if (cachedTextures.TryAdd(url, created))
            {
                AddToExpirationQueue(url);
                QueueDownload(url, created);
                return false;
            }
        }
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

    #region 私有

    private static readonly TimeSpan CacheTTL       = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan FailedCacheTTL = TimeSpan.FromSeconds(5);
    
        private readonly Channel<string> downloadChannel = Channel.CreateUnbounded<string>
        (
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            }
        );
    
        private readonly Channel<ExpirationCommand> expirationChannel = Channel.CreateUnbounded<ExpirationCommand>
        (
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            }
        );
        
        private readonly ConcurrentDictionary<string, ImageLoadingResult>                                cachedTextures = [];
        private readonly ConcurrentDictionary<(uint ID, bool HQ, Language Language), ImageLoadingResult> cachedIcons    = [];
    
        private HttpClient HTTPClient {
            get
            {
                if (field != null)
                    return field;
                
                return field = HTTPClientHelper.Instance().Get
                       (
                           "OmenTools.ImageHelper",
                           client => client.Timeout = TimeSpan.FromSeconds(30)
                       );
            }
            
        }
    
        private readonly CancellationTokenSource globalCancelSource = new();

    #endregion
    
    #region 继承

    protected override void Init()
    {
        _ = ProcessDownloadsAsync(globalCancelSource.Token);
        _ = CleanupLoopAsync(globalCancelSource.Token);
    }

    protected override void Uninit()
    {
        globalCancelSource.Cancel();
        ClearAll();
        globalCancelSource.Dispose();
        HTTPClient.Dispose();
    }

    #endregion

    #region 辅助方法

    private void ClearAll()
    {
        expirationChannel.Writer.TryWrite(new ClearCommand());

        foreach (var value in cachedTextures.Values)
            value.Dispose();
        cachedTextures.Clear();

        foreach (var value in cachedIcons.Values)
            value.Dispose();
        cachedIcons.Clear();
    }

    private void AddToExpirationQueue(string textureURL) =>
        expirationChannel.Writer.TryWrite(new AddCommand(ExpirationKey.FromTexture(textureURL), StandardTimeManager.Instance().UTCNow.Ticks + CacheTTL.Ticks));

    private void AddToExpirationQueue((uint ID, bool HQ, Language Language) iconKey) =>
        expirationChannel.Writer.TryWrite(new AddCommand(ExpirationKey.FromIcon(iconKey), StandardTimeManager.Instance().UTCNow.Ticks + CacheTTL.Ticks));

    private void QueueDownload(string url, ImageLoadingResult result)
    {
        if (!result.TryQueueDownload()) return;
        downloadChannel.Writer.TryWrite(url);
    }

    private async Task ProcessDownloadsAsync(CancellationToken ct)
    {
        var reader = downloadChannel.Reader;

        try
        {
            await Parallel.ForEachAsync
            (
                reader.ReadAllAsync(ct),
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 8,
                    CancellationToken      = ct
                },
                async (url, token) =>
                {
                    if (!cachedTextures.TryGetValue(url, out var result))
                        return;

                    if (!result.TryMarkLoading())
                        return;

                    try
                    {
                        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                        {
                            var content = await HTTPClient.GetByteArrayAsync(uri, token);

                            result.TextureWrap = await DService.Instance().Texture.CreateFromImageAsync(content, url, token);
                        }
                        else
                        {
                            result.ImmediateTexture = File.Exists(url)
                                                          ? DService.Instance().Texture.GetFromFile(url)
                                                          : DService.Instance().Texture.GetFromGame(url);
                        }

                        result.TryCompleteByTexture(StandardTimeManager.Instance().UTCNow, FailedCacheTTL);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        DLog.Error($"[ImageHelper] 加载资源失败: {url}", ex);
                        result.TryCompleteFailure(StandardTimeManager.Instance().UTCNow, FailedCacheTTL);
                    }
                }
            );
        }
        catch (OperationCanceledException)
        {

        }
    }

    private async Task CleanupLoopAsync(CancellationToken ct)
    {
        var localQueue   = new PriorityQueue<ExpirationKey, long>();
        var reader       = expirationChannel.Reader;
        var readWaitTask = reader.WaitToReadAsync(ct).AsTask();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                long waitTicks;

                if (localQueue.TryPeek(out _, out var ticks))
                {
                    waitTicks = ticks - StandardTimeManager.Instance().UTCNow.Ticks;
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
                        if (itemTicks <= StandardTimeManager.Instance().UTCNow.Ticks)
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
                DLog.Error("[ImageHelper] 缓存清理任务异常", ex);
                if (readWaitTask.IsCompleted)
                    readWaitTask = reader.WaitToReadAsync(ct).AsTask();
            }
        }
    }

    private void CheckAndProcessItem(ExpirationKey key, PriorityQueue<ExpirationKey, long> queue)
    {
        ImageLoadingResult? result  = null;
        var                 removed = false;

        if (key is { IsTexture: true, TextureURL: not null })
        {
            if (cachedTextures.TryGetValue(key.TextureURL, out result))
            {
                if (StandardTimeManager.Instance().UTCNow - result.LastAccessTime > CacheTTL)
                {
                    if (cachedTextures.TryRemove(key.TextureURL, out var removedItem))
                    {
                        removedItem.Dispose();
                        removed = true;
                    }
                }
            }
        }
        else if (key.IconKey.HasValue)
        {
            if (cachedIcons.TryGetValue(key.IconKey.Value, out result))
            {
                if (StandardTimeManager.Instance().UTCNow - result.LastAccessTime > CacheTTL)
                {
                    if (cachedIcons.TryRemove(key.IconKey.Value, out var removedItem))
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
            _                           => string.Empty
        };

        if (variant.Length == 0)
            return string.Empty;

        return $"ui/icon/{iconID / 1000 * 1000:D6}/{variant}/{iconID:D6}_hr1.tex";
    }

    #endregion

    #region 辅助类

    private class ImageLoadingResult : IDisposable
    {
        private long lastAccessTimeTicks = StandardTimeManager.Instance().UTCNow.Ticks;
        private int  stateValue          = (int)ImageLoadState.Pending;
        private int  downloadQueued;
        private long failedUntilTicks;

        public ISharedImmediateTexture? ImmediateTexture { get; set; }
        public IDalamudTextureWrap?     TextureWrap      { get; set; }

        public readonly TaskCompletionSource<IDalamudTextureWrap?> CompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public DateTime       LastAccessTime => new(lastAccessTimeTicks);
        public ImageLoadState State          => (ImageLoadState)Volatile.Read(ref stateValue);
        public bool           IsCompleted    => State is ImageLoadState.Ready or ImageLoadState.Failed or ImageLoadState.Disposed;

        public void RefreshAccess() =>
            Interlocked.Exchange(ref lastAccessTimeTicks, StandardTimeManager.Instance().UTCNow.Ticks);

        public bool TryQueueDownload() =>
            Interlocked.CompareExchange(ref downloadQueued, 1, 0) == 0;

        public bool TryMarkLoading() =>
            Interlocked.CompareExchange(ref stateValue, (int)ImageLoadState.Loading, (int)ImageLoadState.Pending) == (int)ImageLoadState.Pending;

        public bool IsFailureCooldownActive(DateTime nowUTC) =>
            State == ImageLoadState.Failed && nowUTC.Ticks < Volatile.Read(ref failedUntilTicks);

        public void TryCompleteByTexture(DateTime nowUTC, TimeSpan failedTtl)
        {
            if (Texture != null)
            {
                Volatile.Write(ref stateValue, (int)ImageLoadState.Ready);
                CompletionSource.TrySetResult(Texture);
                return;
            }

            TryCompleteFailure(nowUTC, failedTtl);
        }

        public void TryCompleteFailure(DateTime nowUTC, TimeSpan failedTtl)
        {
            Volatile.Write(ref failedUntilTicks, nowUTC.Ticks + failedTtl.Ticks);
            Volatile.Write(ref stateValue,       (int)ImageLoadState.Failed);
            CompletionSource.TrySetResult(null);
        }

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
            Volatile.Write(ref stateValue, (int)ImageLoadState.Disposed);
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

    private readonly record struct ExpirationKey
    {
        public string?                                TextureURL { get; init; }
        public (uint ID, bool HQ, Language Language)? IconKey    { get; init; }
        public bool                                   IsTexture  => TextureURL != null;

        public static ExpirationKey FromTexture(string textureURL) =>
            new()
            {
                TextureURL = textureURL
            };

        public static ExpirationKey FromIcon((uint ID, bool HQ, Language Language) iconKey) =>
            new()
            {
                IconKey = iconKey
            };
    }

    private record AddCommand
    (
        ExpirationKey Key,
        long          Ticks
    ) : ExpirationCommand;

    private record ClearCommand : ExpirationCommand;

    private abstract record ExpirationCommand;
    
    private enum ImageLoadState : byte
    {
        Pending  = 0,
        Loading  = 1,
        Ready    = 2,
        Failed   = 3,
        Disposed = 4
    }

    #endregion
}
