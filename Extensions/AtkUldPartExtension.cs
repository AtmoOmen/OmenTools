using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface.Textures.TextureWraps;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InteropGenerator.Runtime;
using OmenTools.Dalamud;

namespace OmenTools.Extensions;

public static unsafe class AtkUldPartExtension
{
    internal static readonly Dictionary<string, bool> FileExistsCache   = [];
    internal static readonly Dictionary<nint, string> ThemeTexturePaths = [];

    extension
    (
        scoped ref AtkUldPart part
    )
    {
        /// <summary>
        ///     获取加载时登记的主题无关纹理路径；未启用主题解析时返回空值。
        /// </summary>
        public string? ThemeTexturePath => 
            ThemeTexturePaths.GetValueOrDefault((nint)part.UldAsset);

        /// <summary>
        ///     清除当前纹理资产的主题路径登记，应在资产释放前调用。
        /// </summary>
        public void ClearThemeBinding() => 
            ThemeTexturePaths.Remove((nint)part.UldAsset);

        /// <summary>
        ///     获取当前部件的纹理是否已加载并可用。
        /// </summary>
        public bool IsTextureReady => 
            part.UldAsset is not null && part.UldAsset->AtkTexture.IsTextureReady();

        /// <summary>
        ///     获取已加载纹理的实际尺寸；纹理未就绪时返回零向量。
        /// </summary>
        public Vector2 LoadedTextureSize => 
            part.GetActualTextureSize();

        /// <summary>
        ///     获取已加载资源纹理的路径；没有资源纹理时返回空字符串。
        /// </summary>
        public string LoadedPath => 
            part.GetLoadedPath();

        /// <summary>
        ///     从游戏资源路径加载纹理，空路径用于卸载当前纹理。
        /// </summary>
        /// <remarks>
        ///     自动移除路径中的 _hr1，由游戏按照当前设置选择纹理分辨率。
        ///     应在登录后加载主题纹理，避免共享纹理资源被固定为默认主题。
        /// </remarks>
        /// <param name="path">游戏资源中的纹理路径。</param>
        /// <param name="resolveTheme">是否解析当前界面主题并登记原始路径。</param>
        public void LoadTexture
        (
            string path,
            bool   resolveTheme = true
        )
        {
            try
            {
                if (part.UldAsset is null)
                    return;

                ThemeTexturePaths.Remove((nint)part.UldAsset);
                part.TryUnloadTexture();

                if (path is "")
                    return;

                var texturePath = path.Replace("_hr1", string.Empty);

                if (resolveTheme                                                             &&
                    texturePath.StartsWith("ui/uld/img", StringComparison.OrdinalIgnoreCase) &&
                    texturePath.Length > 13                                                  &&
                    texturePath[10] is >= '0' and <= '9'                                     &&
                    texturePath[11] is >= '0' and <= '9'                                     &&
                    texturePath[12] == '/')
                    texturePath = texturePath.Remove(7, 6);

                var resolvedPath = ResolveTexturePath(texturePath, resolveTheme);

                if (IsFileExists(resolvedPath))
                {
                    part.UldAsset->AtkTexture.LoadTextureWithDefaultVersion(resolvedPath);

                    if (resolveTheme && texturePath.StartsWith("ui/uld/", StringComparison.OrdinalIgnoreCase))
                        ThemeTexturePaths[(nint)part.UldAsset] = texturePath;
                }
            }
            catch (Exception e)
            {
                DLog.Error("加载原生界面纹理部件失败。", e);
            }
        }

        /// <summary>
        ///     按图标 ID 加载纹理，并使用可用的当前语言图标目录。
        /// </summary>
        /// <remarks>
        ///     当前语言目录中没有图标资源时使用默认目录。
        /// </remarks>
        public void LoadIcon
        (
            uint iconID
        )
        {
            ThemeTexturePaths.Remove((nint)part.UldAsset);
            part.UldAsset->AtkTexture.LoadIconTexture(iconID, GetIconSubFolder(iconID));
        }

        /// <summary>
        ///     使用内核纹理指针设置当前部件的纹理。
        /// </summary>
        /// <remarks>
        ///     重复设置或卸载外部持有的纹理可能释放其他对象仍在使用的资源，调用方须管理纹理所有权。
        /// </remarks>
        /// <param name="texture">需要设置的内核纹理指针。</param>
        public void LoadTexture
        (
            Texture* texture
        )
        {
            if (part.UldAsset is null)
                return;

            ThemeTexturePaths.Remove((nint)part.UldAsset);
            part.TryUnloadTexture();
            part.UldAsset->AtkTexture.KernelTexture = texture;
            part.UldAsset->AtkTexture.TextureType   = TextureType.KernelTexture;
        }

        /// <summary>
        ///     将 Dalamud 纹理包装转换为内核纹理并加载到当前部件。
        /// </summary>
        /// <remarks>
        ///     纹理包装必须在使用该部件的节点存续期间保持有效。
        /// </remarks>
        /// <param name="textureWrap">需要加载的 Dalamud 纹理包装。</param>
        public void LoadTexture
        (
            IDalamudTextureWrap textureWrap
        )
        {
            var texturePointer = (Texture*)ITextureProvider.Instance().ConvertToKernelTexture(textureWrap, true);
            if (texturePointer is null)
                return;

            part.LoadTexture(texturePointer);
        }

        private string GetLoadedPath()
        {
            if (part.UldAsset is null)
                return string.Empty;
            if (part.UldAsset->AtkTexture.Resource is null)
                return string.Empty;
            return part.UldAsset->AtkTexture.Resource->TexFileResourceHandle is null ?
                       string.Empty :
                       part.UldAsset->AtkTexture.Resource->TexFileResourceHandle->FileName.ToString();
        }

        private void TryUnloadTexture()
        {
            if (part.UldAsset is null)
                return;
            if (!part.UldAsset->AtkTexture.IsTextureReady())
                return;
            if (part.UldAsset->AtkTexture.TextureType is 0)
                return;
            if (part.UldAsset->AtkTexture.KernelTexture is null)
                return;

            part.UldAsset->AtkTexture.ReleaseTexture();
            part.UldAsset->AtkTexture.KernelTexture = null;
            part.UldAsset->AtkTexture.TextureType   = 0;
        }

        private Vector2 GetActualTextureSize()
        {
            if (part.UldAsset is null)
                return Vector2.Zero;
            if (!part.UldAsset->AtkTexture.IsTextureReady())
                return Vector2.Zero;
            if (part.UldAsset->AtkTexture.TextureType is 0)
                return Vector2.Zero;
            if (part.UldAsset->AtkTexture.KernelTexture is null)
                return Vector2.Zero;

            var width  = part.UldAsset->AtkTexture.GetTextureWidth();
            var height = part.UldAsset->AtkTexture.GetTextureHeight();
            return new Vector2(width, height);
        }
    }

    private static IconSubFolder GetIconSubFolder
    (
        uint iconID
    )
    {
        var        textureManager = AtkStage.Instance()->AtkTextureResourceManager;
        Span<byte> buffer         = stackalloc byte[0x100];
        buffer.Clear();
        var bytePointer = (byte*)Unsafe.AsPointer(ref buffer[0]);

        var textureScale = textureManager->DefaultTextureScale;
        var targetFolder = (IconSubFolder)textureManager->IconLanguage;

        AtkTexture.GetIconPath(bytePointer, iconID, textureScale, targetFolder);
        var pathResult = new CStringPointer(bytePointer).ToString();

        return IsFileExists(pathResult) ?
                   targetFolder :
                   IconSubFolder.None;
    }

    /// <summary>
    ///     根据当前界面主题解析纹理路径，主题纹理不存在时使用原始路径。
    /// </summary>
    /// <param name="path">不含主题目录的游戏纹理路径。</param>
    /// <param name="resolveTheme">是否解析当前主题对应的纹理目录。</param>
    /// <returns>可用于原生纹理加载的路径。</returns>
    public static string ResolveTexturePath
    (
        string path,
        bool   resolveTheme
    )
    {
        var theme = AtkStage.Instance()->AtkUIColorHolder->ActiveColorThemeType;
        if (!resolveTheme ||
            theme == 0    ||
            !path.StartsWith("ui/uld/", StringComparison.OrdinalIgnoreCase))
            return path;

        var themedPath = path.Insert(7, $"img{theme:00}/");
        return IsFileExists(themedPath) ?
                   themedPath :
                   path;
    }

    private static bool IsFileExists
    (
        string path
    )
    {
        if (FileExistsCache.TryGetValue(path, out var result))
            return result;

        var fileExists = IDataManager.Instance().FileExists(path);
        FileExistsCache.TryAdd(path, fileExists);

        return fileExists;
    }
}
