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
            builder.AddText("↔←→↑↓《》■※☀★★☆♥♡ヅツッシ☀☁☂℃℉°♀♂♠♣♦♣♧®©™€$£♯♭♪✓√◎◆◇♦■□〇●△▽▼▲‹›≤≥≈<«─＼～⅓½¼⅔¾✓✗");
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

    public FrozenDictionary<string, FontFace[]> InstalledFontFamilies { get; private set; } =
        FrozenDictionary<string, FontFace[]>.Empty;

    public FontFace? GetSelectedFontFace() =>
        InstalledFontFamilies.Values
                             .SelectMany(faces => faces)
                             .FirstOrDefault
                             (face => face.FaceIndex == Config.FontFaceIndex &&
                                      string.Equals(face.FilePath, Config.FontFileName, StringComparison.OrdinalIgnoreCase)
                             );

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

    /// <summary>
    /// 获取应用配置缩放后的界面字体相对于 16 像素基准字号的显示倍率，包含一次 Dalamud 全局界面缩放。
    /// </summary>
    public float GlobalFontScale => GetActualFontSize(1f) / DEFAULT_FONT_SIZE * ImGuiHelpers.GlobalScale;

    public bool IsFontBuilding => activeFontBuilds > 0;

    public IFontHandle GetUIFont
    (
        float scale
    ) =>
        GetFont(GetActualFontSize(scale));

    /// <summary>
    /// 获取用于字体图集构建的像素字号，不包含由图集负责处理的 Dalamud 全局界面缩放。
    /// </summary>
    /// <param name="scale">相对于应用配置缩放后的 Dalamud 默认字号的倍率。</param>
    /// <returns>Dalamud 当前默认字号乘以配置缩放和指定倍率，保留一位小数。</returns>
    public float GetActualFontSize
    (
        float scale
    ) =>
        MathF.Round(IUiBuilder.Instance().FontDefaultSizePx * Config.FontScale * scale, 1);

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

    /// <summary>
    /// 串行重建界面字体，并预构建常用的相对字号。
    /// </summary>
    /// <param name="clearOld">是否清除已有字体句柄并重新创建图集。</param>
    /// <returns>在本次字体图集构建完成后结束的任务。</returns>
    public async Task RebuildUIFontsAsync
    (
        bool clearOld = true
    )
    {
        await semaphore.WaitAsync();
        Interlocked.Increment(ref activeFontBuilds);

        try
        {
            if (cancelSource.IsCancellationRequested)
                return;

            if (clearOld)
                ClearFontHandles();

            var sizes = PREBUILD_FONT_SCALES.Select(GetActualFontSize)
                                            .Distinct()
                                            .ToArray();

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

    public void ApplyFontSelection
    (
        FontFace face
    )
    {
        Config.FontFileName  = face.FilePath;
        Config.FontFaceIndex = face.FaceIndex;
        Config.Save();

        _ = RebuildUIFontsAsync();
    }

    private IFontAtlas? FontAtlas { get; set; }

    private readonly SemaphoreSlim           semaphore    = new(1, 1);
    private readonly CancellationTokenSource cancelSource = new();

    private readonly ConcurrentDictionary<float, Task<IFontHandle>> fontTasks = [];

    private volatile int activeFontBuilds;

    protected override void Init()
    {
        Config = LoadConfig<FontManagerConfig>() ?? new();
        ClearFontHandles();

        IUiBuilder.Instance().DefaultFontChanged        += OnDalamudFontChanged;
        IUiBuilder.Instance().DefaultGlobalScaleChanged += OnDalamudFontChanged;

        Task.Run
        (
            async () => await Task.WhenAll
                        (
                            RebuildUIFontsAsync(false),
                            Task.Run(RegenerateInstalledFonts, cancelSource.Token)
                        ),
            cancelSource.Token
        ).ConfigureAwait(false);
    }

    protected override void Uninit()
    {
        IUiBuilder.Instance().DefaultFontChanged        -= OnDalamudFontChanged;
        IUiBuilder.Instance().DefaultGlobalScaleChanged -= OnDalamudFontChanged;

        cancelSource.Cancel();

        _ = DisposeFontsAsync();
    }

    private async Task DisposeFontsAsync()
    {
        await semaphore.WaitAsync();

        try
        {
            fontTasks.Clear();
            FontAtlas?.Dispose();
            cancelSource.Dispose();
        }
        finally
        {
            semaphore.Release();
        }
    }

    private void OnDalamudFontChanged()
    {
        if (cancelSource.IsCancellationRequested)
            return;

        _ = RebuildUIFontsAsync(false);
    }

    private IFontHandle CreateFontHandleDefinition
    (
        float size
    )
    {
        var fontPath      = Config.FontFileName;
        var iconFontIndex = 0;

        return FontAtlas.NewDelegateFontHandle
        (e =>
            {
                e.OnPreBuild
                (tk =>
                    {
                        var font = File.Exists(fontPath) ?
                                       tk.AddFontFromFile
                                       (
                                           fontPath,
                                           new()
                                           {
                                               SizePx      = size,
                                               PixelSnapH  = true,
                                               GlyphRanges = DefaultFontRange,
                                               FontNo      = Config.FontFaceIndex
                                           }
                                       ) :
                                       tk.AddDalamudDefaultFont(size, DefaultFontRange);

                        tk.AddGameSymbol
                        (
                            new()
                            {
                                SizePx     = size,
                                PixelSnapH = true,
                                MergeFont  = font
                            }
                        );

                        iconFontIndex = tk.Fonts.Length;
                        tk.AddFontAwesomeIconFont
                        (
                            new()
                            {
                                SizePx     = size,
                                PixelSnapH = true
                            }
                        );
                    }
                );

                e.OnPostBuild
                (tk =>
                    {
                        var iconFont = tk.Fonts[iconFontIndex];
                        tk.FitRatio(iconFont);
                        iconFont.Ascent = tk.Font.Ascent;
                        tk.CopyGlyphsAcrossFonts
                        (
                            iconFont,
                            tk.Font,
                            missingOnly: false,
                            rangeLow: '',
                            rangeHigh: ''
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
            if (cancelSource.IsCancellationRequested)
                throw new OperationCanceledException();

            var handle = CreateFontHandleDefinition(size);
            await FontAtlas.BuildFontsAsync();
            return handle;
        }
        catch (Exception ex) when (!cancelSource.IsCancellationRequested)
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

        var localInstalledFaces = new ConcurrentBag<FontFace>();

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
                    foreach (var face in ReadFontFaces(file))
                        localInstalledFaces.Add(face);
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
            InstalledFontFamilies = localInstalledFaces
                                    .OrderBy(face => face.FamilyName, StringComparer.OrdinalIgnoreCase)
                                    .GroupBy(face => face.FamilyName, StringComparer.OrdinalIgnoreCase)
                                    .ToFrozenDictionary
                                    (
                                        group => group.Key,
                                        group => group.OrderBy(face => face.FilePath, StringComparer.OrdinalIgnoreCase)
                                                      .ThenBy(face => face.FaceIndex)
                                                      .ToArray(),
                                        StringComparer.OrdinalIgnoreCase
                                    );
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

    private static List<FontFace> ReadFontFaces
    (
        string file
    )
    {
        var faces = new List<FontFace>();

        try
        {
            using var stream = new FileStream
            (
                file,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete
            );

            Span<byte> fileHeader = stackalloc byte[12];
            stream.ReadExactly(fileHeader);

            long[] tableDirectories;

            if (BinaryPrimitives.ReadUInt32BigEndian(fileHeader) == FONT_COLLECTION_TAG)
            {
                var faceCount = BinaryPrimitives.ReadUInt32BigEndian(fileHeader[8..]);

                if (faceCount is 0 or > MAX_FONT_COLLECTION_FACES)
                    return faces;

                tableDirectories = new long[faceCount];

                Span<byte> faceOffset = stackalloc byte[4];

                for (var i = 0; i < faceCount; i++)
                {
                    stream.ReadExactly(faceOffset);
                    tableDirectories[i] = BinaryPrimitives.ReadUInt32BigEndian(faceOffset);
                }
            }
            else
                tableDirectories = [0];

            for (var i = 0; i < tableDirectories.Length; i++)
            {
                var face = ReadFontFace(stream, file, i, tableDirectories[i]);

                if (face is not null)
                    faces.Add(face);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or SecurityException)
        {
            // ignored
        }

        return faces;
    }

    private static FontFace? ReadFontFace
    (
        FileStream stream,
        string     file,
        int        faceIndex,
        long       tableDirectory
    )
    {
        if (tableDirectory + 12 > stream.Length)
            return null;

        stream.Position = tableDirectory;

        Span<byte> header = stackalloc byte[4];
        stream.ReadExactly(header);

        if (BinaryPrimitives.ReadUInt32BigEndian(header) is not (SFNT_VERSION_TRUETYPE or SFNT_VERSION_OPENTYPE or SFNT_VERSION_APPLE_TRUETYPE))
            return null;

        Span<byte> sfntHeader = stackalloc byte[8];
        stream.ReadExactly(sfntHeader);

        var tableCount = BinaryPrimitives.ReadUInt16BigEndian(sfntHeader);

        Span<byte> tableRecord = stackalloc byte[16];

        long nameTableOffset = -1;
        long nameTableLength = 0;

        for (var i = 0; i < tableCount; i++)
        {
            if (stream.Position + 16 > stream.Length) break;

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

        var (familyName, subfamilyName) = ReadFontNames(nameTable);

        return string.IsNullOrWhiteSpace(familyName) ?
                   null :
                   new FontFace(file, faceIndex, familyName.Trim(), subfamilyName?.Trim() ?? string.Empty);
    }

    private static (string? FamilyName, string? SubfamilyName) ReadFontNames
    (
        byte[] nameTable
    )
    {
        var recordCount         = BinaryPrimitives.ReadUInt16BigEndian(nameTable.AsSpan(2));
        var stringStorageOffset = BinaryPrimitives.ReadUInt16BigEndian(nameTable.AsSpan(4));

        var currentLanguageID = unchecked((ushort)CultureInfo.CurrentUICulture.LCID);

        var familyNames    = new string?[NAME_PRIORITY_COUNT];
        var subfamilyNames = new string?[NAME_PRIORITY_COUNT];

        for (var i = 0; i < recordCount; i++)
        {
            var recordStart = 6 + (i * 12);

            if (recordStart + 12 > nameTable.Length) break;

            var record = nameTable.AsSpan(recordStart, 12);

            var target = BinaryPrimitives.ReadUInt16BigEndian(record[6..]) switch
            {
                FAMILY_NAME_ID    => familyNames,
                SUBFAMILY_NAME_ID => subfamilyNames,
                _                 => null
            };

            if (target is null) continue;

            var platformID = BinaryPrimitives.ReadUInt16BigEndian(record);
            var encodingID = BinaryPrimitives.ReadUInt16BigEndian(record[2..]);
            var languageID = BinaryPrimitives.ReadUInt16BigEndian(record[4..]);
            var length     = BinaryPrimitives.ReadUInt16BigEndian(record[8..]);
            var offset     = BinaryPrimitives.ReadUInt16BigEndian(record[10..]);

            var stringStart = stringStorageOffset + offset;

            if (length == 0 || stringStart + length > nameTable.Length) continue;

            var rawName = nameTable.AsSpan(stringStart, length);

            var name = (platformID, encodingID) switch
            {
                (WINDOWS_PLATFORM_ID, WINDOWS_UNICODE_ENCODING_ID) => Encoding.BigEndianUnicode.GetString(rawName).TrimEnd('\0'),
                (MACINTOSH_PLATFORM_ID, MAC_ROMAN_ENCODING_ID)     => Encoding.Latin1.GetString(rawName).TrimEnd('\0'),
                _                                                  => null
            };

            if (string.IsNullOrWhiteSpace(name)) continue;

            var priority = platformID switch
            {
                MACINTOSH_PLATFORM_ID                    => MACINTOSH_NAME_PRIORITY,
                _ when languageID == currentLanguageID   => LOCALIZED_NAME_PRIORITY,
                _ when languageID == ENGLISH_LANGUAGE_ID => ENGLISH_NAME_PRIORITY,
                _                                        => WINDOWS_NAME_PRIORITY
            };

            target[priority] ??= name;
        }

        return (familyNames.FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)),
                   subfamilyNames.FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)));
    }

    private void ClearFontHandles()
    {
        fontTasks.Clear();

        FontAtlas?.Dispose();
        FontAtlas = IUiBuilder.Instance().CreateFontAtlas(FontAtlasAutoRebuildMode.Disable);
    }

    public sealed class FontManagerConfig : OmenServiceConfig
    {
        /// <summary>
        /// 相对于 Dalamud 默认字号的缩放倍率，默认为 1，随 Dalamud 的默认字号变化。
        /// </summary>
        public float FontScale = 1f;

        public string FontFileName = string.Empty;
        public int    FontFaceIndex;

        public void Save() =>
            this.Save(Instance());
    }

    public sealed record FontFace
    (
        string FilePath,
        int    FaceIndex,
        string FamilyName,
        string SubfamilyName
    );

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

    private static readonly string[] FontExtensions = [".ttf", ".otf", ".ttc", ".otc"];

    private static readonly FrozenSet<string> FontExtensionSet = FontExtensions.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region 常量

    private static readonly float[] PREBUILD_FONT_SCALES = [0.6f, 0.8f, 0.9f, 1.0f, 1.2f, 1.4f, 1.6f];

    private const float DEFAULT_FONT_SIZE = 16f;

    private const uint   FONT_COLLECTION_TAG         = 0x74746366;
    private const uint   SFNT_VERSION_TRUETYPE       = 0x00010000;
    private const uint   SFNT_VERSION_OPENTYPE       = 0x4F54544F;
    private const uint   SFNT_VERSION_APPLE_TRUETYPE = 0x74727565;
    private const uint   NAME_TABLE_TAG              = 0x6E616D65;
    private const uint   MAX_FONT_COLLECTION_FACES   = 256;
    private const ushort FAMILY_NAME_ID              = 1;
    private const ushort SUBFAMILY_NAME_ID           = 2;
    private const ushort WINDOWS_PLATFORM_ID         = 3;
    private const ushort WINDOWS_UNICODE_ENCODING_ID = 1;
    private const ushort MACINTOSH_PLATFORM_ID       = 1;
    private const ushort MAC_ROMAN_ENCODING_ID       = 0;
    private const ushort ENGLISH_LANGUAGE_ID         = 0x0409;
    private const int    LOCALIZED_NAME_PRIORITY     = 0;
    private const int    ENGLISH_NAME_PRIORITY       = 1;
    private const int    WINDOWS_NAME_PRIORITY       = 2;
    private const int    MACINTOSH_NAME_PRIORITY     = 3;
    private const int    NAME_PRIORITY_COUNT         = 4;
    private const string FONT_REGISTRY_KEY           = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

    #endregion
}
