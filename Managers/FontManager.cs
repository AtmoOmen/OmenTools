using System.Collections.Concurrent;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class FontManager : OmenServiceBase
{
    public static FontManagerConfig Config { get; private set; } = null!;
    
    public static IFontAtlas FontAtlas     { get; } = DService.UIBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Disable);
    public static IFontAtlas FontAtlasGame { get; } = DService.UIBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.OnNewFrame);

    public static ConcurrentDictionary<string, string> InstalledFonts { get; private set; } = [];
    
    public static IFontHandle AxisFont360 => axisFont360Lazy.Value;
    public static IFontHandle AxisFont180 => axisFont180Lazy.Value;
    public static IFontHandle AxisFont120 => axisFont120Lazy.Value;
    public static IFontHandle AxisFont140 => axisFont140Lazy.Value;
    public static IFontHandle AxisFont96 => axisFont96Lazy.Value;
    
    public static IFontHandle JupiterFont160 => jupiterFont160Lazy.Value;
    public static IFontHandle JupiterFont200 => jupiterFont200Lazy.Value;
    public static IFontHandle JupiterFont230 => jupiterFont230Lazy.Value;
    public static IFontHandle JupiterFont450 => jupiterFont450Lazy.Value;
    public static IFontHandle JupiterFont460 => jupiterFont460Lazy.Value;
    public static IFontHandle JupiterFont900 => jupiterFont900Lazy.Value;
    
    public static IFontHandle MeidingerFont160 => meidingerFont160Lazy.Value;
    public static IFontHandle MeidingerFont200 => meidingerFont200Lazy.Value;
    public static IFontHandle MeidingerFont400 => meidingerFont400Lazy.Value;
    
    public static IFontHandle MiedingerMidFont100 => miedingerMidFont100Lazy.Value;
    public static IFontHandle MiedingerMidFont120 => miedingerMidFont120Lazy.Value;
    public static IFontHandle MiedingerMidFont140 => miedingerMidFont140Lazy.Value;
    public static IFontHandle MiedingerMidFont180 => miedingerMidFont180Lazy.Value;
    public static IFontHandle MiedingerMidFont360 => miedingerMidFont360Lazy.Value;
    
    public static IFontHandle TrumpGothicFont184 => trumpGothicFont184Lazy.Value;
    public static IFontHandle TrumpGothicFont230 => trumpGothicFont230Lazy.Value;
    public static IFontHandle TrumpGothicFont340 => trumpGothicFont340Lazy.Value;
    public static IFontHandle TrumpGothicFont680 => trumpGothicFont680Lazy.Value;
    
    public static IFontHandle UIFont    => GetUIFont(1.0f);
    public static IFontHandle UIFont160 => GetUIFont(1.6f);
    public static IFontHandle UIFont140 => GetUIFont(1.4f);
    public static IFontHandle UIFont120 => GetUIFont(1.2f);
    public static IFontHandle UIFont90  => GetUIFont(0.9f);
    public static IFontHandle UIFont80  => GetUIFont(0.8f);
    public static IFontHandle UIFont60  => GetUIFont(0.6f);

    public static float GlobalFontScale => Config.FontSize / 14f * (ImGui.GetIO().DisplaySize.Y / 1440f);

    public static bool IsFontBuilding => ActiveFontBuilds > 0;
    
    private static readonly unsafe ushort[] DefaultFontRange =
        BuildRange(null, 
                   ImGui.GetIO().Fonts.GetGlyphRangesChineseFull(),
                   ImGui.GetIO().Fonts.GetGlyphRangesJapanese(),
                   ImGui.GetIO().Fonts.GetGlyphRangesKorean(),
                   ImGui.GetIO().Fonts.GetGlyphRangesDefault());

    private static readonly ConcurrentDictionary<float, Task<IFontHandle>> FontTasks = [];

    private static readonly SemaphoreSlim FontCreationSemaphore = new(Environment.ProcessorCount, Environment.ProcessorCount);
    
    private static CancellationTokenSource? CancelSource;
    private static int                      ActiveFontBuilds;

    private static string DefaultFontPath => 
        Path.Join(DService.PI.DalamudAssetDirectory.FullName, "UIRes", GetDefaultDalamudFontFileName());

    internal override void Init()
    {
        Config = LoadConfig<FontManagerConfig>() ?? new();
        
        CancelSource ??= new();
        DService.Framework.RunOnTick(async () =>
                     await Task.WhenAll
                     (
                         RebuildInterfaceFontsAsync(),
                         DService.Framework.RunOnTick(GetInstalledFonts, cancellationToken: CancelSource.Token)
                     ), cancellationToken: CancelSource.Token).ConfigureAwait(false);
    }

    public static IFontHandle GetUIFont(float scale) => 
        GetFont(GetActualFontSize(scale));

    public static float GetActualFontSize(float scale) => 
        MathF.Round(Config.FontSize * scale, 1);

    public static IFontHandle GetFont(float size)
    {
        var task = FontTasks.GetOrAdd(size, CreateFontHandleAsync);

        if (task.IsCompletedSuccessfully) return task.Result;

        if (task.IsFaulted)
        {
            FontTasks.TryRemove(size, out _);
            NotificationError($"字体 (大小: {size}) 构建失败");
            Error($"字体 (大小: {size}) 构建失败", task.Exception);
        }

        return AxisFont180;
    }

    public static async Task RebuildInterfaceFontsAsync(bool clearOld = true)
    {
        if (clearOld) 
            ClearFontHandles();

        float[] sizes = [0.6f, 0.8f, 0.9f, 1.0f, 1.2f, 1.4f, 1.6f];
        var     tasks = new ConcurrentBag<Task>();

        await DService.Framework.RunOnTick(
            () => Parallel.ForEach(sizes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                                   size =>
                                       tasks.Add(DService.Framework.RunOnTick(() => GetUIFont(size), cancellationToken: CancelSource.Token))),
            cancellationToken: CancelSource.Token).ConfigureAwait(false);

        await Task.WhenAll(tasks);
    }

    public static void ClearFontHandles() => FontTasks.Clear();

    public static Task RefreshInstalledFontsAsync() => 
        DService.Framework.RunOnTick(GetInstalledFonts, cancellationToken: CancelSource.Token);

    private static async Task<IFontHandle> CreateFontHandleAsync(float size)
    {
        await FontCreationSemaphore.WaitAsync();
        Interlocked.Increment(ref ActiveFontBuilds);
        
        try
        {
            var fontPath = Config.FontFileName;
            if (!File.Exists(fontPath))
            {
                Warning("字体获取失败, 已转为默认字体");
                fontPath = DefaultFontPath;
            }

            IFontHandle? handle;

            if (!File.Exists(fontPath))
            {
                Warning("默认字体获取失败, 已转为 Dalamud 内置字体");

                handle = FontAtlas.NewDelegateFontHandle(e =>
                {
                    e.OnPreBuild(tk =>
                    {
                        var fileFontPtr = tk.AddDalamudDefaultFont(size, DefaultFontRange);

                        var mixedFontPtr0 = tk.AddGameSymbol(new()
                        {
                            SizePx = size,
                            PixelSnapH = true,
                            MergeFont = fileFontPtr,
                        });

                        tk.AddFontAwesomeIconFont(new()
                        {
                            SizePx = size,
                            PixelSnapH = true,
                            MergeFont = mixedFontPtr0,
                        });
                    });
                });
            }
            else
            {
                handle = FontAtlas.NewDelegateFontHandle(e =>
                {
                    e.OnPreBuild(tk =>
                    {
                        var fileFontPtr = tk.AddFontFromFile(fontPath, new()
                        {
                            SizePx      = size,
                            PixelSnapH  = true,
                            GlyphRanges = DefaultFontRange,
                            FontNo      = 0,
                        });

                        var mixedFontPtr0 = tk.AddGameSymbol(new()
                        {
                            SizePx     = size,
                            PixelSnapH = true,
                            MergeFont  = fileFontPtr,
                        });

                        tk.AddFontAwesomeIconFont(new()
                        {
                            SizePx = size,
                            PixelSnapH = true,
                            MergeFont = mixedFontPtr0,
                        });
                    });
                });
            }

            await FontAtlas.BuildFontsAsync();
            return handle;
        }
        catch (Exception ex)
        {
            NotificationError($"字体 (大小: {size}) 构建失败");
            Error($"构建字体 (大小: {size}) 失败", ex);
            throw;
        }
        finally
        {
            Interlocked.Decrement(ref ActiveFontBuilds);
            FontCreationSemaphore.Release();
        }
    }

    public static unsafe ushort[] BuildRange(IReadOnlyList<ushort>? chars, params ushort*[] ranges)
    {
        var builder = new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder());
        foreach (var range in ranges)
            builder.AddRanges(range);

        if (chars != null)
        {
            for (var i = 0; i < chars.Count; i += 2)
            {
                if (chars[i] == 0)
                    break;

                for (var j = (uint)chars[i]; j <= chars[i + 1]; j++)
                    builder.AddChar((ushort)j);
            }
        }

        builder.AddText("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
        builder.AddText("ΑαΒβΓγΔδΕεΖζΗηΘθΙιΚκΛλΜμΝνΞξΟοΠπΡρΣσΤτΥυΦφΧχΨψΩω←→↑↓《》■※☀★★☆♥♡ヅツッシ☀☁☂℃℉°♀♂♠♣♦♣♧®©™€$£♯♭♪✓√◎◆◇♦■□〇●△▽▼▲‹›≤≥<«─＼～⅓½¼⅔¾✓✗");
        builder.AddText("ŒœĂăÂâÎîȘșȚț");

        for (var i = 0x2460; i <= 0x24B5; i++)
            builder.AddChar((char)i);

        builder.AddChar('⓪');
        return builder.BuildRangesToArray();
    }

    public static void GetInstalledFonts()
    {
        string[] fontDirectories = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                       ?
                                       [
                                           @"C:\Windows\Fonts",
                                           Path.Combine(
                                               Environment.GetFolderPath(
                                                   Environment.SpecialFolder.LocalApplicationData),
                                               @"Microsoft\Windows\Fonts")
                                       ]
                                       : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                                           ?
                                           [
                                               "/Library/Fonts", "/System/Library/Fonts",
                                               Path.Combine(
                                                   Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                                   "Library/Fonts")
                                           ]
                                           :
                                           [
                                               "/usr/share/fonts", "/usr/local/share/fonts",
                                               Path.Combine(
                                                   Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                                   ".fonts")
                                           ];

        string[] fontExtensions = [".ttf", ".otf", ".ttc"];

        var       localInstalledFonts = new ConcurrentDictionary<string, string>();
        var       errors              = new ConcurrentBag<Exception>();
        const int maxErrors           = 100;

        try
        {
            var allFontFiles = fontDirectories
                               .AsParallel()
                               .WithDegreeOfParallelism(Environment.ProcessorCount)
                               .WithMergeOptions(ParallelMergeOptions.FullyBuffered)
                               .Where(Directory.Exists)
                               .SelectMany(dir =>
                                               Directory.EnumerateFiles(dir, "*.*", new EnumerationOptions
                                                        {
                                                            RecurseSubdirectories = true,
                                                            IgnoreInaccessible    = true
                                                        })
                                                        .Where(f => fontExtensions.Contains(
                                                                   Path.GetExtension(f).ToLowerInvariant()))
                               )
                               .Distinct()
                               .ToList();

            Parallel.ForEach(allFontFiles,
                             new ParallelOptions
                             {
                                 MaxDegreeOfParallelism = Environment.ProcessorCount,
                                 CancellationToken      = CancelSource.Token
                             },
                             () => new PrivateFontCollection(),
                             (file, loopState, _, threadLocalPfc) =>
                             {
                                 if (errors.Count >= maxErrors)
                                 {
                                     loopState.Stop();
                                     return threadLocalPfc;
                                 }

                                 try
                                 {
                                     threadLocalPfc.AddFontFile(file);
                                     foreach (var family in threadLocalPfc.Families)
                                         localInstalledFonts.TryAdd(file, family.GetName(0));

                                     threadLocalPfc.Dispose();
                                     return new PrivateFontCollection();
                                 }
                                 catch (Exception ex) when (ex is IOException or SecurityException
                                                                or UnauthorizedAccessException)
                                 {
                                     if (errors.Count < maxErrors)
                                         errors.Add(ex);

                                     return threadLocalPfc;
                                 }
                             },
                             threadLocalPfc => threadLocalPfc.Dispose()
            );
        }
        catch (OperationCanceledException)
        {
            Warning("字体加载操作已取消。");
        }
        catch (Exception ex)
        {
            Error("字体加载过程中发生严重错误", ex);
        } finally
        {
            foreach (var font in localInstalledFonts)
                InstalledFonts.TryAdd(font.Key, font.Value);

            foreach (var error in errors.Take(maxErrors))
                Error("处理字体文件时出错", error);
        }
    }

    public static string GetDefaultDalamudFontFileName()
    {
        if (GameState.IsCN)
            return "NotoSansCJKsc-Medium.otf";
        if (GameState.IsTC)
            return "NotoSansCJKsc-Medium.otf";
        if (GameState.IsGL)
            return "NotoSansCJKjp-Medium.otf";
        if (GameState.IsKR)
            return "NotoSansCJKkr-Medium.otf";

        return "NotoSansCJKsc-Medium.otf";
    }

    internal override void Uninit()
    {
        CancelSource?.Cancel();
        CancelSource?.Dispose();
        CancelSource = null;
        
        FontTasks.Clear();
    }
    
    public sealed class FontManagerConfig : OmenServiceConfiguration
    {
        public float  FontSize     = 20f;
        public string FontFileName = @"C:\Windows\Fonts\msyh.ttc";
        
        public void Save() =>
            this.Save(DService.GetOmenService<FontManager>());
    }

    #region Lazy Game Fonts

    private static readonly Lazy<IFontHandle> axisFont360Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Axis36)));
    private static readonly Lazy<IFontHandle> axisFont180Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Axis18)));
    private static readonly Lazy<IFontHandle> axisFont120Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Axis12)));
    private static readonly Lazy<IFontHandle> axisFont140Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Axis14)));
    private static readonly Lazy<IFontHandle> axisFont96Lazy  = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Axis96)));
    
    private static readonly Lazy<IFontHandle> jupiterFont160Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter16)));
    private static readonly Lazy<IFontHandle> jupiterFont200Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter20)));
    private static readonly Lazy<IFontHandle> jupiterFont230Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter23)));
    private static readonly Lazy<IFontHandle> jupiterFont450Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter45)));
    private static readonly Lazy<IFontHandle> jupiterFont460Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter46)));
    private static readonly Lazy<IFontHandle> jupiterFont900Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter90)));
    
    private static readonly Lazy<IFontHandle> meidingerFont160Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Meidinger16)));
    private static readonly Lazy<IFontHandle> meidingerFont200Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Meidinger20)));
    private static readonly Lazy<IFontHandle> meidingerFont400Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Meidinger40)));
    
    private static readonly Lazy<IFontHandle> miedingerMidFont100Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.MiedingerMid10)));
    private static readonly Lazy<IFontHandle> miedingerMidFont120Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.MiedingerMid12)));
    private static readonly Lazy<IFontHandle> miedingerMidFont140Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.MiedingerMid14)));
    private static readonly Lazy<IFontHandle> miedingerMidFont180Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.MiedingerMid18)));
    private static readonly Lazy<IFontHandle> miedingerMidFont360Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.MiedingerMid36)));
    
    private static readonly Lazy<IFontHandle> trumpGothicFont184Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.TrumpGothic184)));
    private static readonly Lazy<IFontHandle> trumpGothicFont230Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.TrumpGothic23)));
    private static readonly Lazy<IFontHandle> trumpGothicFont340Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.TrumpGothic34)));
    private static readonly Lazy<IFontHandle> trumpGothicFont680Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.TrumpGothic68)));

    #endregion
}
