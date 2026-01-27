using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class FontManager : OmenServiceBase<FontManager>
{
    private static IFontAtlas FontAtlasGame => DService.Instance().UIBuilder.FontAtlas;

    private static readonly unsafe ushort[] DefaultFontRange =
        BuildRange
        (
            null,
            ImGui.GetIO().Fonts.GetGlyphRangesDefault(),
            ImGui.GetIO().Fonts.GetGlyphRangesChineseFull(),
            ImGui.GetIO().Fonts.GetGlyphRangesKorean()
        );

    private static unsafe ushort[] BuildRange(ushort[]? extraRanges, params ushort*[] nativeRanges)
    {
        var builder = new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder());

        try
        {
            foreach (var range in nativeRanges)
                builder.AddRanges(range);

            if (extraRanges is { Length: > 0 })
            {
                fixed (ushort* p = extraRanges)
                    builder.AddRanges(p);
            }

            builder.AddText("ΑαΒβΓγΔδΕεΖζΗηΘθΙιΚκΛλΜμΝνΞξΟοΠπΡρΣσΤτΥυΦφΧχΨψΩω");
            builder.AddText("←→↑↓《》■※☀★★☆♥♡ヅツッシ☀☁☂℃℉°♀♂♠♣♦♣♧®©™€$£♯♭♪✓√◎◆◇♦■□〇●△▽▼▲‹›≤≥<«─＼～⅓½¼⅔¾✓✗");
            builder.AddText("ŒœĂăÂâÎîȘșȚț");
            builder.AddChar('⓪');

            Span<ushort> specificRange = [0x2460, 0x24B5, 0];
            fixed (ushort* p = specificRange)
                builder.AddRanges(p);

            return builder.BuildRangesToArray();
        }
        finally
        {
            builder.Destroy();
        }
    }


    public FontManagerConfig Config { get; private set; } = null!;

    public ConcurrentDictionary<string, string> InstalledFonts { get; private set; } = [];

    public IFontHandle AxisFont360 => axisFont360Lazy.Value;
    public IFontHandle AxisFont180 => axisFont180Lazy.Value;
    public IFontHandle AxisFont120 => axisFont120Lazy.Value;
    public IFontHandle AxisFont140 => axisFont140Lazy.Value;
    public IFontHandle AxisFont96  => axisFont96Lazy.Value;

    public IFontHandle JupiterFont160 => jupiterFont160Lazy.Value;
    public IFontHandle JupiterFont200 => jupiterFont200Lazy.Value;
    public IFontHandle JupiterFont230 => jupiterFont230Lazy.Value;
    public IFontHandle JupiterFont450 => jupiterFont450Lazy.Value;
    public IFontHandle JupiterFont460 => jupiterFont460Lazy.Value;
    public IFontHandle JupiterFont900 => jupiterFont900Lazy.Value;

    public IFontHandle MeidingerFont160 => meidingerFont160Lazy.Value;
    public IFontHandle MeidingerFont200 => meidingerFont200Lazy.Value;
    public IFontHandle MeidingerFont400 => meidingerFont400Lazy.Value;

    public IFontHandle MiedingerMidFont100 => miedingerMidFont100Lazy.Value;
    public IFontHandle MiedingerMidFont120 => miedingerMidFont120Lazy.Value;
    public IFontHandle MiedingerMidFont140 => miedingerMidFont140Lazy.Value;
    public IFontHandle MiedingerMidFont180 => miedingerMidFont180Lazy.Value;
    public IFontHandle MiedingerMidFont360 => miedingerMidFont360Lazy.Value;

    public IFontHandle TrumpGothicFont184 => trumpGothicFont184Lazy.Value;
    public IFontHandle TrumpGothicFont230 => trumpGothicFont230Lazy.Value;
    public IFontHandle TrumpGothicFont340 => trumpGothicFont340Lazy.Value;
    public IFontHandle TrumpGothicFont680 => trumpGothicFont680Lazy.Value;

    public IFontHandle UIFont    => GetUIFont(1.0f);
    public IFontHandle UIFont160 => GetUIFont(1.6f);
    public IFontHandle UIFont140 => GetUIFont(1.4f);
    public IFontHandle UIFont120 => GetUIFont(1.2f);
    public IFontHandle UIFont90  => GetUIFont(0.9f);
    public IFontHandle UIFont80  => GetUIFont(0.8f);
    public IFontHandle UIFont60  => GetUIFont(0.6f);

    public float GlobalFontScale => Config.FontSize / 12f * ImGuiHelpers.GlobalScaleSafe;
    public bool  IsFontBuilding  => activeFontBuilds > 0;

    public IFontHandle GetUIFont(float scale) =>
        GetFont(GetActualFontSize(scale));

    public float GetActualFontSize(float scale) =>
        MathF.Round(Config.FontSize * scale * ImGuiHelpers.GlobalScaleSafe, 1);

    public IFontHandle GetFont(float size)
    {
        var task = fontTasks.GetOrAdd(size, CreateFontHandleAsync);

        if (task.IsCompletedSuccessfully) return task.Result;

        if (task.IsFaulted)
        {
            fontTasks.TryRemove(size, out _);
            NotificationError($"字体 (大小: {size}) 构建失败");
            Error($"字体 (大小: {size}) 构建失败", task.Exception);
        }

        return AxisFont180;
    }

    public async Task RebuildUIFontsAsync(bool clearOld = true)
    {
        if (clearOld)
            ClearFontHandles();

        var sizes = PreBuildFontSizes.Select(GetActualFontSize)
                                     .Distinct()
                                     .ToArray();

        await semaphore.WaitAsync();
        Interlocked.Increment(ref activeFontBuilds);

        try
        {
            foreach (var size in sizes)
            {
                if (fontTasks.ContainsKey(size)) continue;

                try
                {
                    var handle = CreateFontHandleDefinition(size);
                    fontTasks.TryAdd(size, Task.FromResult(handle));
                }
                catch (Exception ex)
                {
                    NotificationError($"字体 (大小: {size}) 预构建失败");
                    Error($"字体 (大小: {size}) 预构建失败", ex);
                }
            }

            await FontAtlas.BuildFontsAsync();
        }
        finally
        {
            Interlocked.Decrement(ref activeFontBuilds);
            semaphore.Release();
        }
    }

    public Task RegenerateInstalledFontsAsync() =>
        Task.Run(RegenerateInstalledFonts, cancelSource.Token);

    private IFontAtlas? FontAtlas { get; set; }

    private readonly SemaphoreSlim           semaphore    = new(Environment.ProcessorCount, Environment.ProcessorCount);
    private readonly CancellationTokenSource cancelSource = new();

    private readonly ConcurrentDictionary<float, Task<IFontHandle>> fontTasks = [];

    private volatile int activeFontBuilds;

    internal override void Init()
    {
        Config = LoadConfig<FontManagerConfig>() ?? new();

        Task.Run
        (
            async () => await Task.WhenAll
                        (
                            RebuildUIFontsAsync(),
                            Task.Run(RegenerateInstalledFonts, cancelSource.Token)
                        ),
            cancelSource.Token
        ).ConfigureAwait(false);
    }

    internal override void Uninit()
    {
        cancelSource.Cancel();
        cancelSource.Dispose();

        fontTasks.Clear();
        FontAtlas?.Dispose();
    }

    private IFontHandle CreateFontHandleDefinition(float size)
    {
        var fontPath = Config.FontFileName;

        if (!File.Exists(fontPath))
        {
            Warning("字体获取失败, 已转为 Dalamud 内置字体");

            return FontAtlas.NewDelegateFontHandle
            (e =>
                {
                    e.OnPreBuild
                    (tk =>
                        {
                            var defualtFontPtr = tk.AddDalamudDefaultFont(size, DefaultFontRange);

                            var mixedFontPtr0 = tk.AddGameSymbol
                            (
                                new()
                                {
                                    SizePx     = size,
                                    PixelSnapH = true,
                                    MergeFont  = defualtFontPtr
                                }
                            );

                            tk.AddFontAwesomeIconFont
                            (
                                new()
                                {
                                    SizePx     = size,
                                    PixelSnapH = true,
                                    MergeFont  = mixedFontPtr0
                                }
                            );
                        }
                    );
                }
            );
        }

        return FontAtlas.NewDelegateFontHandle
        (e =>
            {
                e.OnPreBuild
                (tk =>
                    {
                        var fileFontPtr = tk.AddFontFromFile
                        (
                            fontPath,
                            new()
                            {
                                SizePx      = size,
                                PixelSnapH  = true,
                                GlyphRanges = DefaultFontRange,
                                FontNo      = 0
                            }
                        );

                        var mixedFontPtr0 = tk.AddGameSymbol
                        (
                            new()
                            {
                                SizePx     = size,
                                PixelSnapH = true,
                                MergeFont  = fileFontPtr
                            }
                        );

                        tk.AddFontAwesomeIconFont
                        (
                            new()
                            {
                                SizePx     = size,
                                PixelSnapH = true,
                                MergeFont  = mixedFontPtr0
                            }
                        );
                    }
                );
            }
        );
    }

    private async Task<IFontHandle> CreateFontHandleAsync(float size)
    {
        await semaphore.WaitAsync();
        Interlocked.Increment(ref activeFontBuilds);

        try
        {
            var handle = CreateFontHandleDefinition(size);
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
            Interlocked.Decrement(ref activeFontBuilds);
            semaphore.Release();
        }
    }

    private void RegenerateInstalledFonts()
    {
        string[] fontDirectories =
        [
            @"C:\Windows\Fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts")
        ];

        var fontExtensions = FontExtensions.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        var localInstalledFonts = new ConcurrentDictionary<string, string>();
        var errors              = new ConcurrentBag<Exception>();

        var       errorCounter = 0;
        const int MAX_ERRORS   = 100;

        try
        {
            var allFontFiles = fontDirectories
                               .Where(Directory.Exists)
                               .SelectMany
                               (dir => Directory.EnumerateFiles
                                (
                                    dir,
                                    "*.*",
                                    new EnumerationOptions
                                    {
                                        RecurseSubdirectories = true,
                                        IgnoreInaccessible    = true
                                    }
                                )
                               )
                               .Where(f => fontExtensions.Contains(Path.GetExtension(f)));

            Parallel.ForEach
            (
                allFontFiles,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken      = cancelSource.Token
                },
                () => new PrivateFontCollection(),
                (file, loopState, _, threadLocalPfc) =>
                {
                    if (Volatile.Read(ref errorCounter) >= MAX_ERRORS)
                    {
                        loopState.Stop();
                        return threadLocalPfc;
                    }

                    try
                    {
                        threadLocalPfc.AddFontFile(file);

                        if (threadLocalPfc.Families.Length > 0)
                            localInstalledFonts.TryAdd(file, threadLocalPfc.Families[0].Name);

                        threadLocalPfc.Dispose();
                        return new PrivateFontCollection();
                    }
                    catch (Exception ex) when (ex is IOException or SecurityException or UnauthorizedAccessException or ExternalException)
                    {
                        if (Interlocked.Increment(ref errorCounter) <= MAX_ERRORS)
                            errors.Add(ex);

                        threadLocalPfc.Dispose();
                        return new PrivateFontCollection();
                    }
                },
                threadLocalPfc => threadLocalPfc.Dispose()
            );
        }
        catch (OperationCanceledException)
        {
            Warning("已取消获取本机安装字体");
        }
        catch (Exception ex)
        {
            Error("尝试获取本机安装字体时发生错误", ex);
        }
        finally
        {
            InstalledFonts = localInstalledFonts;

            foreach (var error in errors)
                Error("尝试获取本机安装字体时出错", error);
        }
    }

    private void ClearFontHandles()
    {
        fontTasks.Clear();

        FontAtlas?.Dispose();
        FontAtlas = DService.Instance().UIBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Disable);
    }

    public sealed class FontManagerConfig : OmenServiceConfiguration
    {
        public float  FontSize     = 20f;
        public string FontFileName = @"C:\Windows\Fonts\msyh.ttc";

        public void Save() =>
            this.Save(Instance());
    }

    #region Lazy Game Fonts

    private readonly Lazy<IFontHandle> axisFont360Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Axis36)));
    private readonly Lazy<IFontHandle> axisFont180Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Axis18)));
    private readonly Lazy<IFontHandle> axisFont120Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Axis12)));
    private readonly Lazy<IFontHandle> axisFont140Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Axis14)));
    private readonly Lazy<IFontHandle> axisFont96Lazy  = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Axis96)));

    private readonly Lazy<IFontHandle> jupiterFont160Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter16)));
    private readonly Lazy<IFontHandle> jupiterFont200Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter20)));
    private readonly Lazy<IFontHandle> jupiterFont230Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter23)));
    private readonly Lazy<IFontHandle> jupiterFont450Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter45)));
    private readonly Lazy<IFontHandle> jupiterFont460Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter46)));
    private readonly Lazy<IFontHandle> jupiterFont900Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Jupiter90)));

    private readonly Lazy<IFontHandle> meidingerFont160Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Meidinger16)));
    private readonly Lazy<IFontHandle> meidingerFont200Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Meidinger20)));
    private readonly Lazy<IFontHandle> meidingerFont400Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.Meidinger40)));

    private readonly Lazy<IFontHandle> miedingerMidFont100Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.MiedingerMid10)));
    private readonly Lazy<IFontHandle> miedingerMidFont120Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.MiedingerMid12)));
    private readonly Lazy<IFontHandle> miedingerMidFont140Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.MiedingerMid14)));
    private readonly Lazy<IFontHandle> miedingerMidFont180Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.MiedingerMid18)));
    private readonly Lazy<IFontHandle> miedingerMidFont360Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.MiedingerMid36)));

    private readonly Lazy<IFontHandle> trumpGothicFont184Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.TrumpGothic184)));
    private readonly Lazy<IFontHandle> trumpGothicFont230Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.TrumpGothic23)));
    private readonly Lazy<IFontHandle> trumpGothicFont340Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.TrumpGothic34)));
    private readonly Lazy<IFontHandle> trumpGothicFont680Lazy = new(() => FontAtlasGame.NewGameFontHandle(new(GameFontFamilyAndSize.TrumpGothic68)));
    
    private static readonly float[] PreBuildFontSizes = [0.6f, 0.8f, 0.9f, 1.0f, 1.2f, 1.4f, 1.6f];

    private static readonly string[] FontExtensions = [".ttf", ".otf", ".ttc"];

    #endregion
}
