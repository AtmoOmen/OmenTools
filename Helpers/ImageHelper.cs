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

public class ImageHelper : OmenServiceBase
{
    private static readonly Channel<string> DownloadChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    private static readonly ConcurrentDictionary<string, ImageLoadingResult>                                CachedTextures = [];
    private static readonly ConcurrentDictionary<(uint ID, bool HQ, Language Language), ImageLoadingResult> CachedIcons    = [];

    private static readonly List<Func<byte[], byte[]>> ConversionsToBitmap = [b => b];

    private static readonly HttpClient HttpClient = new()
    {
        Timeout               = TimeSpan.FromSeconds(30),
        DefaultRequestHeaders = { { "User-Agent", "OmenTools/1.0" } }
    };

    private static readonly CancellationTokenSource GlobalCts = new();

    private static readonly TimeSpan CacheTTL        = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromSeconds(5);

    internal override void Init()
    {
        _ = ProcessDownloadsAsync(GlobalCts.Token);
        _ = CleanupLoopAsync(GlobalCts.Token);
    }

    internal override void Uninit()
    {
        GlobalCts.Cancel();
        ClearAll();
        GlobalCts.Dispose();
        HttpClient.Dispose();
    }

    public static IDalamudTextureWrap? GetGameLangIcon(uint iconID, bool isHQ = false) =>
        TryGetGameLangIcon(iconID, out var texture, isHQ) ? texture : null;

    public static IDalamudTextureWrap? GetGameIcon(uint iconID, bool isHQ = false) =>
        TryGetGameIcon(iconID, out var texture, isHQ) ? texture : null;

    public static IDalamudTextureWrap? GetImage(string urlOrPath) =>
        TryGetImage(urlOrPath, out var texture) ? texture : null;

    public static bool TryGetGameLangIcon(uint icon, [NotNullWhen(true)] out IDalamudTextureWrap? texture, bool isHQ = false)
    {
        var key = (icon, isHQ, GameState.ClientLanguge);

        if (CachedIcons.TryGetValue(key, out var result))
        {
            result.RefreshAccess();
            texture = result.Texture;
            return texture != null;
        }

        result = new ImageLoadingResult
        {
            ImmediateTexture = DService.Texture.GetFromGame(GetIconTexturePath(icon, GameState.ClientLanguge)),
            IsCompleted      = true
        };

        result.CompletionSource.TrySetResult(result.Texture);

        CachedIcons.TryAdd(key, result);
        texture = result.Texture;
        return texture != null;
    }

    public static bool TryGetGameIcon(uint icon, [NotNullWhen(true)] out IDalamudTextureWrap? texture, bool isHQ = false)
    {
        if (DService.Texture.TryGetFromGameIcon(new(icon, isHQ), out var immediateTexture))
        {
            texture = immediateTexture.GetWrapOrEmpty();
            return true;
        }

        texture = null;
        return false;
    }

    public static bool TryGetImage(string url, [NotNullWhen(true)] out IDalamudTextureWrap? texture)
    {
        texture = null;
        if (string.IsNullOrWhiteSpace(url)) return false;

        if (CachedTextures.TryGetValue(url, out var result))
        {
            result.RefreshAccess();
            texture = result.Texture;
            return texture != null;
        }

        var newResult = new ImageLoadingResult();

        if (CachedTextures.TryAdd(url, newResult))
            DownloadChannel.Writer.TryWrite(url);
        else
        {
            if (CachedTextures.TryGetValue(url, out result))
            {
                result.RefreshAccess();
                texture = result.Texture;
                return texture != null;
            }
        }

        return false;
    }

    public static async Task<IDalamudTextureWrap?> GetImageAsync(string urlOrPath)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath)) return null;


        if (!CachedTextures.TryGetValue(urlOrPath, out var result))
        {
            TryGetImage(urlOrPath, out _);

            if (!CachedTextures.TryGetValue(urlOrPath, out result)) return null;
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

    private static async Task ProcessDownloadsAsync(CancellationToken ct)
    {
        var reader = DownloadChannel.Reader;
        
        try 
        {
            await Parallel.ForEachAsync(reader.ReadAllAsync(ct), new ParallelOptions
            {
                MaxDegreeOfParallelism = 8,
                CancellationToken = ct
            }, async (url, token) =>
            {
                if (!CachedTextures.TryGetValue(url, out var result) || result.IsCompleted) 
                    return;

                try
                {
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        var content = await HttpClient.GetByteArrayAsync(uri, token);
                        
                        foreach (var conversion in ConversionsToBitmap)
                        {
                            try
                            {
                                content = conversion(content);
                            }
                            catch (Exception ex)
                            {
                                Error($"[ImageHelper] 图像预处理失败: {url}", ex);
                            }
                        }

                        result.TextureWrap = await DService.Texture.CreateFromImageAsync(content, debugName: url, cancellationToken: token);
                    }
                    else
                    {
                        result.ImmediateTexture = File.Exists(url) 
                            ? DService.Texture.GetFromFile(url) 
                            : DService.Texture.GetFromGame(url);
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
            // 正常退出
        }
    }

    private static async Task CleanupLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(CleanupInterval, ct);

                var now = DateTime.UtcNow;

                foreach (var kvp in CachedTextures)
                {
                    if (now - kvp.Value.LastAccessTime > CacheTTL)
                    {
                        if (CachedTextures.TryRemove(kvp.Key, out var removedItem))
                            removedItem.Dispose();
                    }
                }

                foreach (var kvp in CachedIcons)
                {
                    if (now - kvp.Value.LastAccessTime > CacheTTL)
                    {
                        if (CachedIcons.TryRemove(kvp.Key, out var removedItem))
                            removedItem.Dispose();
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
            }
        }
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

    public static void ClearAll()
    {
        foreach (var value in CachedTextures.Values)
            value.Dispose();
        CachedTextures.Clear();

        foreach (var value in CachedIcons.Values)
            value.Dispose();
        CachedIcons.Clear();
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
