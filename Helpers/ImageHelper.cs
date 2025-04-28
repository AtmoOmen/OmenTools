using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;

namespace OmenTools.Helpers;

public static class ImageHelper
{
    private static readonly ConcurrentDictionary<string, ImageLoadingResult>             CachedTextures      = new();
    private static readonly ConcurrentDictionary<(uint ID, bool HQ), ImageLoadingResult> CachedIcons         = new();
    private static readonly List<Func<byte[], byte[]>>                                   ConversionsToBitmap = [b => b];

    private static readonly HttpClient               HttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };
    private static          CancellationTokenSource? CancelSource;

    public static IDalamudTextureWrap? GetGameIcon(uint iconID, bool isHQ = false)
        => TryGetGameIcon(iconID, isHQ, out var texture) ? texture : null;

    public static IDalamudTextureWrap? GetImage(string urlOrPath)
        => TryGetImage(urlOrPath, out var texture) ? texture : null;

    public static bool TryGetGameIcon(uint icon, bool isHQ, [NotNullWhen(true)] out IDalamudTextureWrap? texture)
    {
        var result = CachedIcons.GetOrAdd((icon, isHQ), _ => new ImageLoadingResult
        {
            ImmediateTexture = DService.Texture.GetFromGameIcon(new(icon, isHQ)),
            IsCompleted      = true
        });

        texture = result.Texture;
        return texture != null;
    }

    public static bool TryGetImage(string url, [NotNullWhen(true)] out IDalamudTextureWrap? texture)
    {
        texture = null;
        if (string.IsNullOrWhiteSpace(url)) return false;

        var result = CachedTextures.GetOrAdd(url, _ =>
        {
            CancelSource ??= new();
            
            Task.Run(LoadPendingTexturesAsync, CancelSource.Token);
            return new ImageLoadingResult();
        });

        texture = result.Texture;
        return texture != null;
    }

    public static async Task<IDalamudTextureWrap> GetImageAsync(string urlOrPath)
    {
        IDalamudTextureWrap? texture;
        while (!TryGetImage(urlOrPath, out texture))
            await Task.Delay(100);

        return texture;
    }

    private static async Task LoadPendingTexturesAsync()
    {
        while (await LoadNextPendingTextureAsync()) { };
    }

    private static async Task<bool> LoadNextPendingTextureAsync()
    {
        if (!CachedTextures.TryGetFirst(x => !x.Value.IsCompleted, out var kvp)) return false;

        var (key, value)  = kvp;
        value.IsCompleted = true;

        try
        {
            if (Uri.TryCreate(key, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
            {
                var content = await HttpClient.GetByteArrayAsync(uri);
                foreach (var conversion in ConversionsToBitmap)
                {
                    try
                    {
                        value.TextureWrap = await DService.Texture.CreateFromImageAsync(conversion(content));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Error("尝试转换图片资源时失败", ex);
                    }
                }
            }
            else
            {
                value.ImmediateTexture = File.Exists(key) ? DService.Texture.GetFromFile(key) : DService.Texture.GetFromGame(key);
            }
        }
        catch (Exception ex)
        {
            Error("尝试加载图片资源时失败", ex);
        }

        return true;
    }

    public static void ClearAll()
    {
        foreach (var (_, value) in CachedTextures)
        {
            try
            {
                value.TextureWrap?.Dispose();
            }
            catch (Exception ex)
            {
                Error("尝试回收图片资源时失败", ex);
            }
        }

        CachedTextures.Clear();

        foreach (var (_, value) in CachedIcons)
        {
            try
            {
                value.TextureWrap?.Dispose();
            }
            catch (Exception ex)
            {
                Error("尝试回收图标资源时失败", ex);
            }
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
    
    private record ImageLoadingResult
    {
        public ISharedImmediateTexture? ImmediateTexture { get; set; }
        public IDalamudTextureWrap?     TextureWrap      { get; set; }
        public bool                     IsCompleted      { get; set; }

        public IDalamudTextureWrap? Texture =>
            DService.Framework.RunOnFrameworkThread(() => ImmediateTexture?.GetWrapOrEmpty() ?? TextureWrap).Result;
    }
}
