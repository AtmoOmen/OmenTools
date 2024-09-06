using System.Collections.Concurrent;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;

namespace OmenTools.Helpers;

public static class ImageHelper
{
    private record ImageLoadingResult
    {
        public ISharedImmediateTexture? ImmediateTexture { get; set; }
        public IDalamudTextureWrap?     TextureWrap      { get; set; }
        public bool                     IsCompleted      { get; set; }

        public IDalamudTextureWrap? Texture =>
            DService.Framework.RunOnFrameworkThread(() => ImmediateTexture?.GetWrapOrEmpty() ?? TextureWrap).Result;
    }

    private static readonly ConcurrentDictionary<string, ImageLoadingResult>             CachedTextures      = new();
    private static readonly ConcurrentDictionary<(uint ID, bool HQ), ImageLoadingResult> CachedIcons         = new();
    private static readonly List<Func<byte[], byte[]>>                                   ConversionsToBitmap = [b => b];

    private static readonly HttpClient               HttpClient       = new() { Timeout = TimeSpan.FromSeconds(10) };
    private static readonly SemaphoreSlim            LoadingSemaphore = new(1, 1);
    private static          CancellationTokenSource? CancelSource;

    public static IDalamudTextureWrap? GetIcon(uint iconID, bool isHQ = false)
        => TryGetIconTextureWrap(iconID, isHQ, out var texture) ? texture : null;

    public static IDalamudTextureWrap? GetImage(string urlOrPath)
        => TryGetTextureWrap(urlOrPath, out var texture) ? texture : null;

    public static bool TryGetIconTextureWrap(uint icon, bool hq, out IDalamudTextureWrap? textureWrap)
    {
        var result = CachedIcons.GetOrAdd((icon, hq), _ => new ImageLoadingResult
        {
            ImmediateTexture = DService.Texture.GetFromGameIcon(new(icon, hq)),
            IsCompleted = true,
        });

        textureWrap = result.Texture;
        return textureWrap != null;
    }

    public static bool TryGetTextureWrap(string url, out IDalamudTextureWrap? textureWrap)
    {
        CancelSource?.Cancel();
        CancelSource = new();
        
        var result = CachedTextures.GetOrAdd(url, _ =>
        {
            Task.Run(LoadPendingTexturesAsync, CancelSource.Token);
            return new ImageLoadingResult();
        });

        textureWrap = result.Texture;
        return textureWrap != null;
    }

    public static async Task<IDalamudTextureWrap?> GetImageAsync(string urlOrPath)
    {
        IDalamudTextureWrap? texture;
        while (!TryGetTextureWrap(urlOrPath, out texture))
            await Task.Delay(100);

        return texture;
    }

    private static async Task LoadPendingTexturesAsync()
    {
        await LoadingSemaphore.WaitAsync();
        try
        {
            while (await LoadNextPendingTextureAsync()) { }
        } finally
        {
            LoadingSemaphore.Release();
        }
    }

    private static async Task<bool> LoadNextPendingTextureAsync()
    {
        if (!CachedTextures.TryGetFirst(x => !x.Value.IsCompleted, out var kvp)) return false;

        var (key, value) = kvp;
        value.IsCompleted = true;

        try
        {
            if (Uri.TryCreate(key, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
            {
                var content = await HttpClient.GetByteArrayAsync(uri);
                foreach (var conversion in ConversionsToBitmap)
                    try
                    {
                        value.TextureWrap = await DService.Texture.CreateFromImageAsync(conversion(content));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        DService.Log.Error(ex, "尝试转换图片资源时失败");
                    }
            }
            else
            {
                value.ImmediateTexture = File.Exists(key)
                                             ? DService.Texture.GetFromFile(key)
                                             : DService.Texture.GetFromGame(key);
            }
        }
        catch (Exception ex)
        {
            DService.Log.Error(ex, "尝试加载图片资源时失败");
        }

        return true;
    }
    
    public static void ClearAll()
    {
        foreach (var (_, value) in CachedTextures)
            try
            {
                value.TextureWrap?.Dispose();
            }
            catch (Exception ex)
            {
                DService.Log.Error(ex, "尝试回收图片资源时失败");
            }

        CachedTextures.Clear();

        foreach (var (_, value) in CachedIcons)
            try
            {
                value.TextureWrap?.Dispose();
            }
            catch (Exception ex)
            {
                DService.Log.Error(ex, "尝试回收图标资源时失败");
            }

        CachedIcons.Clear();
    }

    public static void Uninit()
    {
        ClearAll();
        
        CancelSource?.Cancel();
        CancelSource?.Dispose();
        CancelSource = null;
    }
}
