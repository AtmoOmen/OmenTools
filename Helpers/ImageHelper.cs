using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Data;
using OmenTools.Abstracts;

namespace OmenTools.Helpers;

public class ImageHelper : OmenServiceBase
{
    private static readonly ConcurrentDictionary<string, ImageLoadingResult>                                CachedTextures      = [];
    private static readonly ConcurrentDictionary<(uint ID, bool HQ, Language Language), ImageLoadingResult> CachedIcons         = [];
    private static readonly List<Func<byte[], byte[]>>                                                      ConversionsToBitmap = [b => b];

    private static HttpClient?              HttpClient;
    private static CancellationTokenSource? CancelSource;

    internal override void Init() => 
        HttpClient ??= new() { Timeout = TimeSpan.FromSeconds(30) };
    
    internal override void Uninit()
    {
        ClearAll();

        CancelSource?.Cancel();
        CancelSource?.Dispose();
        CancelSource = null;
        
        HttpClient?.Dispose();
        HttpClient = null;
    }

    public static IDalamudTextureWrap? GetGameLangIcon(uint iconID, bool isHQ = false) =>
        TryGetGameLangIcon(iconID, out var texture, isHQ) ? texture : null;
    
    public static IDalamudTextureWrap? GetGameIcon(uint iconID, bool isHQ = false) => 
        TryGetGameIcon(iconID, out var texture, isHQ) ? texture : null;

    public static IDalamudTextureWrap? GetImage(string urlOrPath) => 
        TryGetImage(urlOrPath, out var texture) ? texture : null;
    
    public static bool TryGetGameLangIcon(uint icon, [NotNullWhen(true)] out IDalamudTextureWrap? texture, bool isHQ = false)
    {
        var result = CachedIcons.GetOrAdd((icon, isHQ, GameState.ClientLanguge), _ => new ImageLoadingResult
        {
            ImmediateTexture = DService.Texture.GetFromGame(GetIconTexturePath(icon, GameState.ClientLanguge)),
            IsCompleted      = true
        });

        texture = result.Texture;
        return texture != null;
    }
    
    public static bool TryGetGameIcon(uint icon, out IDalamudTextureWrap texture, bool isHQ = false)
    {
        texture = null;
        
        if (DService.Texture.TryGetFromGameIcon(new(icon, isHQ), out var immediateTexture))
        {
            texture = immediateTexture.GetWrapOrEmpty();
            return true;
        }
        
        return false;
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
        while (await LoadNextPendingTextureAsync()) 
        { }
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
                while (HttpClient == null)
                    await Task.Delay(100);
                
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
                value.ImmediateTexture = File.Exists(key) ? DService.Texture.GetFromFile(key) : DService.Texture.GetFromGame(key);
        }
        catch (Exception ex)
        {
            Error("尝试加载图片资源时失败", ex);
        }

        return true;
    }

    private static string GetIconTexturePath(uint iconID, Language language)
    {
        var varient = language switch
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
        
        if (string.IsNullOrEmpty(varient))
            return string.Empty;

        return $"ui/icon/{iconID / 1000 * 1000:D6}/{varient}/{iconID:D6}_hr1.tex";
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
    
    private record ImageLoadingResult
    {
        public ISharedImmediateTexture? ImmediateTexture { get; set; }
        public IDalamudTextureWrap?     TextureWrap      { get; set; }
        public bool                     IsCompleted      { get; set; }

        public IDalamudTextureWrap? Texture =>
            DService.Framework.RunOnFrameworkThread(() => ImmediateTexture?.GetWrapOrEmpty() ?? TextureWrap).Result;
    }
}
