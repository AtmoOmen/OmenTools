using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.Security;
using System.Text;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Microsoft.Win32;
using OmenTools.Dalamud;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public class FontManager
(
    NotifyHelper notifyHelper
) : OmenServiceBase<FontManager>
{
    private static IFontAtlas FontAtlasGame => IUiBuilder.Instance().FontAtlas;

    private static readonly unsafe ushort[] DefaultFontRange =
        BuildRange
        (
            null,
            ImGui.GetIO().Fonts.GetGlyphRangesDefault(),
            ImGui.GetIO().Fonts.GetGlyphRangesChineseFull(),
            ImGui.GetIO().Fonts.GetGlyphRangesKorean()
        );

    private static unsafe ushort[] BuildRange
    (
        ushort[]?        extraRanges,
        params ushort*[] nativeRanges
    )
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
            builder.AddText("←→↑↓《》■※☀★★☆♥♡ヅツッシ☀☁☂℃℉°♀♂♠♣♦♣♧®©™€$£♯♭♪✓√◎◆◇♦■□〇●△▽▼▲‹›≤≥≈<«─＼～⅓½¼⅔¾✓✗");
            builder.AddText("ŒœĂăÂâÎîȘșȚț");
            builder.AddChar('⓪');

            Span<ushort> specificRange = [0x2022, 0x2022, 0x2460, 0x24B5, 0];
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

    public float GlobalFontScale => Config.FontSize / 12f * ImGuiHelpers.GlobalScale;
    public bool  IsFontBuilding  => activeFontBuilds > 0;

    public IFontHandle GetUIFont
    (
        float scale
    ) =>
        GetFont(GetActualFontSize(scale));

    public float GetActualFontSize
    (
        float scale
    ) =>
        MathF.Round(Config.FontSize * scale * ImGuiHelpers.GlobalScale, 1);

    public IFontHandle GetFont
    (
        float size
    )
    {
        var task = fontTasks.GetOrAdd(size, CreateFontHandleAsync);

        if (task.IsCompletedSuccessfully) return task.Result;

        if (task.IsFaulted)
        {
            fontTasks.TryRemove(size, out _);
            notifyHelper.NotificationError($"字体 (大小: {size}) 构建失败");
            DLog.Error($"字体 (大小: {size}) 构建失败", task.Exception);
        }

        return AxisFont180;
    }

    public async Task RebuildUIFontsAsync
    (
        bool clearOld = true
    )
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
                    notifyHelper.NotificationError($"字体 (大小: {size}) 预构建失败");
                    DLog.Error($"字体 (大小: {size}) 预构建失败", ex);
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

    protected override void Init()
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

    protected override void Uninit()
    {
        cancelSource.Cancel();
        cancelSource.Dispose();

        fontTasks.Clear();
        FontAtlas?.Dispose();
    }

    private IFontHandle CreateFontHandleDefinition
    (
        float size
    )
    {
        var fontPath = Config.FontFileName;

        if (!File.Exists(fontPath))
        {
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

    private async Task<IFontHandle> CreateFontHandleAsync
    (
        float size
    )
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
            notifyHelper.NotificationError($"字体 (大小: {size}) 构建失败");
            DLog.Error($"构建字体 (大小: {size}) 失败", ex);
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
            Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts")
        ];

        var localInstalledFonts = new ConcurrentDictionary<string, string>();

        try
        {
            var directoryFontFiles = fontDirectories
                                     .Where(Directory.Exists)
                                     .SelectMany
                                     (dir => Directory.EnumerateFiles
                                      (
                                          dir,
                                          "*.*",
                                          new EnumerationOptions
                                          {
                                              RecurseSubdirectories = true,
                                              IgnoreInaccessible    = true,
                                              AttributesToSkip      = FileAttributes.None
                                          }
                                      )
                                     )
                                     .Where(f => FontExtensionSet.Contains(Path.GetExtension(f)));

            var allFontFiles = directoryFontFiles
                               .Concat(GetRegisteredFontFiles().Where(File.Exists))
                               .Distinct(StringComparer.OrdinalIgnoreCase);

            Parallel.ForEach
            (
                allFontFiles,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken      = cancelSource.Token
                },
                file =>
                {
                    var familyName = TryReadFontFamilyName(file);

                    if (familyName is not null)
                        localInstalledFonts.TryAdd(file, familyName);
                }
            );
        }
        catch (OperationCanceledException)
        {
            DLog.Warning("已取消获取本机安装字体");
        }
        catch (Exception ex)
        {
            DLog.Error("尝试获取本机安装字体时发生错误", ex);
        }
        finally
        {
            InstalledFonts = localInstalledFonts;
        }
    }

    private static List<string> GetRegisteredFontFiles()
    {
        var fontsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        var files          = new List<string>();

        try
        {
            RegistryKey[] registryRoots = [Registry.LocalMachine, Registry.CurrentUser];

            foreach (var registryRoot in registryRoots)
            {
                using var fontsKey = registryRoot.OpenSubKey(FONT_REGISTRY_KEY);

                if (fontsKey is null) continue;

                foreach (var valueName in fontsKey.GetValueNames())
                {
                    if (fontsKey.GetValue(valueName) is not string value) continue;

                    var path = Environment.ExpandEnvironmentVariables(value);

                    if (!Path.IsPathRooted(path))
                        path = Path.Combine(fontsDirectory, path);

                    if (FontExtensionSet.Contains(Path.GetExtension(path)))
                        files.Add(path);
                }
            }
        }
        catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException or IOException or PlatformNotSupportedException)
        {
            // ignored
        }

        return files;
    }

    private static string? TryReadFontFamilyName
    (
        string file
    )
    {
        try
        {
            using var stream = new FileStream
            (
                file,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete
            );

            Span<byte> header = stackalloc byte[4];
            stream.ReadExactly(header);

            if (BinaryPrimitives.ReadUInt32BigEndian(header) == FONT_COLLECTION_TAG)
            {
                Span<byte> collectionHeader = stackalloc byte[12];
                stream.ReadExactly(collectionHeader);

                var firstFontTableDirectory = BinaryPrimitives.ReadUInt32BigEndian(collectionHeader[8..]);

                if (firstFontTableDirectory <= 0 || firstFontTableDirectory >= stream.Length)
                    return null;

                stream.Position = firstFontTableDirectory;
                stream.ReadExactly(header);
            }

            if (BinaryPrimitives.ReadUInt32BigEndian(header) is not (SFNT_VERSION_TRUETYPE or SFNT_VERSION_OPENTYPE or SFNT_VERSION_APPLE_TRUETYPE))
                return null;

            Span<byte> tableCountBuffer = stackalloc byte[2];
            stream.ReadExactly(tableCountBuffer);

            var tableCount = BinaryPrimitives.ReadUInt16BigEndian(tableCountBuffer);

            Span<byte> tableRecord = stackalloc byte[16];

            long nameTableOffset = -1;
            long nameTableLength = 0;

            for (var i = 0; i < tableCount; i++)
            {
                stream.ReadExactly(tableRecord);

                if (BinaryPrimitives.ReadUInt32BigEndian(tableRecord) != NAME_TABLE_TAG) continue;

                nameTableOffset = BinaryPrimitives.ReadUInt32BigEndian(tableRecord[8..]);
                nameTableLength = BinaryPrimitives.ReadUInt32BigEndian(tableRecord[12..]);
                break;
            }

            if (nameTableOffset < 0 || nameTableLength < 6 || nameTableOffset + nameTableLength > stream.Length)
                return null;

            var nameTable = new byte[nameTableLength];

            stream.Position = nameTableOffset;
            stream.ReadExactly(nameTable);

            var recordCount         = BinaryPrimitives.ReadUInt16BigEndian(nameTable);
            var stringStorageOffset = BinaryPrimitives.ReadUInt16BigEndian(nameTable.AsSpan(4));

            var currentLanguageID = unchecked((ushort)CultureInfo.CurrentUICulture.LCID);

            string? localizedName = null;
            string? englishName   = null;
            string? windowsName   = null;
            string? macintoshName = null;

            for (var i = 0; i < recordCount; i++)
            {
                var recordStart = 6 + (i * 12);

                if (recordStart + 12 > nameTable.Length) break;

                var record = nameTable.AsSpan(recordStart, 12);

                if (BinaryPrimitives.ReadUInt16BigEndian(record[6..]) != FAMILY_NAME_ID) continue;

                var platformID = BinaryPrimitives.ReadUInt16BigEndian(record);
                var encodingID = BinaryPrimitives.ReadUInt16BigEndian(record[2..]);
                var languageID = BinaryPrimitives.ReadUInt16BigEndian(record[4..]);
                var length     = BinaryPrimitives.ReadUInt16BigEndian(record[8..]);
                var offset     = BinaryPrimitives.ReadUInt16BigEndian(record[10..]);

                var stringStart = stringStorageOffset + offset;

                if (length == 0 || stringStart + length > nameTable.Length) continue;

                var rawName = nameTable.AsSpan(stringStart, length);

                var familyName = (platformID, encodingID) switch
                {
                    (WINDOWS_PLATFORM_ID, WINDOWS_UNICODE_ENCODING_ID) => Encoding.BigEndianUnicode.GetString(rawName).TrimEnd('\0'),
                    (MACINTOSH_PLATFORM_ID, MAC_ROMAN_ENCODING_ID)     => Encoding.Latin1.GetString(rawName).TrimEnd('\0'),
                    _                                                  => null
                };

                if (string.IsNullOrWhiteSpace(familyName)) continue;

                if (platformID == MACINTOSH_PLATFORM_ID)
                {
                    macintoshName ??= familyName;
                    continue;
                }

                if (languageID == currentLanguageID)
                    localizedName ??= familyName;
                else if (languageID == ENGLISH_LANGUAGE_ID)
                    englishName ??= familyName;

                windowsName ??= familyName;
            }

            var result = localizedName ?? englishName ?? windowsName ?? macintoshName;

            return string.IsNullOrWhiteSpace(result) ?
                       null :
                       result.Trim();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or SecurityException)
        {
            // ignored
        }

        return null;
    }

    private void ClearFontHandles()
    {
        fontTasks.Clear();

        FontAtlas?.Dispose();
        FontAtlas = IUiBuilder.Instance().CreateFontAtlas(FontAtlasAutoRebuildMode.Disable);
    }

    public sealed class FontManagerConfig : OmenServiceConfig
    {
        public float  FontSize     = 20f;
        public string FontFileName = string.Empty;

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

    private static readonly FrozenSet<string> FontExtensionSet = FontExtensions.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region 常量

    private const uint   FONT_COLLECTION_TAG         = 0x74746366;
    private const uint   SFNT_VERSION_TRUETYPE       = 0x00010000;
    private const uint   SFNT_VERSION_OPENTYPE       = 0x4F54544F;
    private const uint   SFNT_VERSION_APPLE_TRUETYPE = 0x74727565;
    private const uint   NAME_TABLE_TAG              = 0x6E616D65;
    private const ushort FAMILY_NAME_ID              = 1;
    private const ushort WINDOWS_PLATFORM_ID         = 3;
    private const ushort WINDOWS_UNICODE_ENCODING_ID = 1;
    private const ushort MACINTOSH_PLATFORM_ID       = 1;
    private const ushort MAC_ROMAN_ENCODING_ID       = 0;
    private const ushort ENGLISH_LANGUAGE_ID         = 0x0409;
    private const string FONT_REGISTRY_KEY           = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

    #endregion
}
