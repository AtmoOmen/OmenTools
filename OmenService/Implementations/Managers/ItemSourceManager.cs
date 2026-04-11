using System.Collections.Frozen;
using System.Text;
using Lumina.Data;
using Microsoft.Win32.SafeHandles;
using OmenTools.Dalamud;
using OmenTools.Info.Collections;
using OmenTools.Info.Game.ItemSource;
using OmenTools.Info.Game.ItemSource.Enums;
using OmenTools.Info.Game.ItemSource.Models;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public sealed class ItemSourceManager : OmenServiceBase<ItemSourceManager>
{
    private const int  SNAPSHOT_FORMAT_VERSION = 1;
    private const uint SNAPSHOT_MAGIC          = 0x49535243; // ISRC
    private const int  HOT_CACHE_CAPACITY      = 30;

    private static readonly FrozenDictionary<uint, ItemOffset> EmptyOffsets =
        new Dictionary<uint, ItemOffset>().ToFrozenDictionary();

    private readonly Lock                                  snapshotGate = new();
    private readonly LRUCache<uint, ItemSourceQueryResult> hotCache     = new(HOT_CACHE_CAPACITY);

    private FrozenDictionary<uint, ItemOffset> itemOffsets   = EmptyOffsets;
    private string[]                           stringTable   = [];
    private ShopNPCLocation[]                  locationTable = [];
    private FileStream?                        snapshotStream;

    private RepositoryStatus status = RepositoryStatus.Building;
    private int              buildStarted;

    public ItemSourceQueryResult Query(uint itemID)
    {
        if (itemID == 0)
            return ItemSourceQueryResult.NotFound;

        FrozenDictionary<uint, ItemOffset> currentOffsets;
        string[]                           currentStrings;
        ShopNPCLocation[]                  currentLocations;
        SafeFileHandle?                    currentHandle;
        var                                currentStatus = status;

        lock (snapshotGate)
        {
            currentOffsets   = itemOffsets;
            currentStrings   = stringTable;
            currentLocations = locationTable;
            currentHandle    = snapshotStream?.SafeFileHandle;
        }

        if (currentOffsets.TryGetValue(itemID, out var itemOffset) && currentHandle != null)
            return hotCache.GetOrAdd(itemID, _ => DecodeItem(currentHandle, itemOffset, currentStrings, currentLocations));

        return currentStatus switch
        {
            RepositoryStatus.Ready  => ItemSourceQueryResult.NotFound,
            RepositoryStatus.Failed => ItemSourceQueryResult.Failed,
            _                       => ItemSourceQueryResult.Building
        };
    }

    protected override void Init()
    {
        if (TryLoadSnapshot(out _))
        {
            status = RepositoryStatus.Ready;
            return;
        }

        status = RepositoryStatus.Building;
        EnsureBackgroundBuild();
    }

    protected override void Uninit()
    {
        hotCache.Dispose();

        lock (snapshotGate)
        {
            snapshotStream?.Dispose();
            snapshotStream = null;
            itemOffsets    = EmptyOffsets;
            stringTable    = [];
            locationTable  = [];
        }
    }

    private void EnsureBackgroundBuild()
    {
        if (Interlocked.CompareExchange(ref buildStarted, 1, 0) != 0)
            return;

        _ = Task.Run
        (async () =>
            {
                try
                {
                    status = RepositoryStatus.Building;

                    var snapshotPath = GetSnapshotPath();
                    var items        = ItemSourceInfo.BuildAllItems();
                    await WriteSnapshotAsync(snapshotPath, items).ConfigureAwait(false);

                    if (!TryLoadSnapshot(out _))
                        throw new InvalidOperationException("快照已写入, 但重新装载失败");

                    hotCache.ClearAll(true);
                    status = RepositoryStatus.Ready;
                    DLog.Debug($"[ItemSourceRepository] 快照已就绪, 条目数: {itemOffsets.Count}");
                }
                catch (Exception ex)
                {
                    status = RepositoryStatus.Failed;
                    DLog.Error("[ItemSourceRepository] 构建物品来源快照失败", ex);
                }
                finally
                {
                    Interlocked.Exchange(ref buildStarted, 0);
                }
            }
        );
    }

    private bool TryLoadSnapshot(out string? failureReason)
    {
        failureReason = null;

        var path = GetSnapshotPath();

        if (!File.Exists(path))
        {
            failureReason = "快照文件不存在";
            return false;
        }

        try
        {
            var       bytes        = File.ReadAllBytes(path);
            using var memoryStream = new MemoryStream(bytes, false);
            using var reader       = new BinaryReader(memoryStream, Encoding.UTF8, true);

            var header = ReadHeader(reader);
            ValidateHeader(header, bytes.Length);

            if (!string.Equals(header.ClientVersion, GameState.ClientVersion, StringComparison.Ordinal))
                throw new InvalidDataException($"客户端版本不匹配, 当前: {GameState.ClientVersion}, 快照: {header.ClientVersion}");

            if (header.ClientLanguage != GameState.ClientLanguge)
                throw new InvalidDataException($"客户端语言不匹配, 当前: {GameState.ClientLanguge}, 快照: {header.ClientLanguage}");

            ValidateChecksum(bytes, header.StringSection);
            ValidateChecksum(bytes, header.LocationSection);
            ValidateChecksum(bytes, header.IndexSection);
            ValidateChecksum(bytes, header.DataSection);

            var loadedStrings   = ReadStringSection(bytes, header.StringSection);
            var loadedLocations = ReadLocationSection(bytes, header.LocationSection);
            var loadedOffsets   = ReadIndexSection(bytes, header.IndexSection, header.DataSection.Offset);

            var stream = new FileStream
            (
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.RandomAccess | FileOptions.SequentialScan
            );

            lock (snapshotGate)
            {
                snapshotStream?.Dispose();
                snapshotStream = stream;
                stringTable    = loadedStrings;
                locationTable  = loadedLocations;
                itemOffsets    = loadedOffsets;
            }

            hotCache.ClearAll(true);
            return true;
        }
        catch (Exception ex)
        {
            failureReason = ex.Message;
            DLog.Warning($"[ItemSourceRepository] 现有快照不可用, 将后台重建: {failureReason}");
            TryDeleteSnapshot(path);
            return false;
        }
    }

    private static ItemSourceQueryResult DecodeItem
    (
        SafeFileHandle                 handle,
        ItemOffset                     itemOffset,
        IReadOnlyList<string>          strings,
        IReadOnlyList<ShopNPCLocation> locations
    )
    {
        try
        {
            var buffer = GC.AllocateUninitializedArray<byte>(itemOffset.Length);
            var read   = RandomAccess.Read(handle, buffer, itemOffset.Offset);
            if (read != itemOffset.Length)
                throw new EndOfStreamException($"预期读取 {itemOffset.Length} 字节, 实际读取 {read} 字节");

            using var memoryStream = new MemoryStream(buffer, false);
            using var reader       = new BinaryReader(memoryStream, Encoding.UTF8, false);

            var shopType               = (ItemShopType)reader.ReadInt32();
            var achievementDescription = ReadOptionalString(strings, reader.ReadInt32()) ?? string.Empty;
            var npcCount               = reader.ReadInt32();
            var npcInfos               = new List<ShopNPCInfos>(npcCount);

            for (var i = 0; i < npcCount; i++)
            {
                var npcID        = reader.ReadUInt32();
                var npcName      = ReadRequiredString(strings, reader.ReadInt32());
                var shopName     = ReadOptionalString(strings, reader.ReadInt32());
                var locationID   = reader.ReadInt32();
                var costInfoSize = reader.ReadInt32();
                var costInfos    = new List<ShopItemCostInfo>(costInfoSize);

                for (var j = 0; j < costInfoSize; j++)
                {
                    var cost             = reader.ReadUInt32();
                    var costItemID       = reader.ReadUInt32();
                    var hasCollectablity = reader.ReadBoolean();
                    var collectablity    = reader.ReadUInt32();
                    costInfos.Add(new(cost, costItemID, hasCollectablity ? collectablity : null));
                }

                npcInfos.Add
                (
                    new()
                    {
                        ID        = npcID,
                        Name      = npcName,
                        ShopName  = shopName,
                        CostInfos = costInfos,
                        Location  = locationID >= 0 ? locations[locationID] : null
                    }
                );
            }

            return ItemSourceQueryResult.Ready
            (
                new()
                {
                    ItemID                 = itemOffset.ItemID,
                    ShopType               = shopType,
                    AchievementDescription = achievementDescription,
                    NPCInfos               = npcInfos
                }
            );
        }
        catch (Exception ex)
        {
            DLog.Error($"[ItemSourceRepository] 解码物品来源条目失败, ItemID: {itemOffset.ItemID}", ex);
            return ItemSourceQueryResult.Failed;
        }
    }

    private async Task WriteSnapshotAsync(string path, IReadOnlyDictionary<uint, ItemSourceInfo> items)
    {
        var stringIndexBuilder   = new Dictionary<string, int>(StringComparer.Ordinal);
        var stringTableBuilder   = new List<string>();
        var locationIndexBuilder = new Dictionary<LocationKey, int>();
        var locationTableBuilder = new List<ShopNPCLocation>();
        var indexEntries         = new List<IndexEntry>(items.Count);

        var dataStream = new MemoryStream();

        using (var dataWriter = new BinaryWriter(dataStream, Encoding.UTF8, true))
        {
            foreach (var (itemID, info) in items.OrderBy(static x => x.Key))
            {
                var itemOffset = checked((int)dataStream.Position);

                dataWriter.Write((int)info.ShopType);
                dataWriter.Write(GetOptionalStringID(info.AchievementDescription, stringIndexBuilder, stringTableBuilder));
                dataWriter.Write(info.NPCInfos.Count);

                foreach (var npcInfo in info.NPCInfos)
                {
                    dataWriter.Write(npcInfo.ID);
                    dataWriter.Write(GetRequiredStringID(npcInfo.Name, stringIndexBuilder, stringTableBuilder));
                    dataWriter.Write(GetOptionalStringID(npcInfo.ShopName, stringIndexBuilder, stringTableBuilder));
                    dataWriter.Write(GetLocationID(npcInfo.Location, locationIndexBuilder, locationTableBuilder));
                    dataWriter.Write(npcInfo.CostInfos.Count);

                    foreach (var costInfo in npcInfo.CostInfos)
                    {
                        dataWriter.Write(costInfo.Cost);
                        dataWriter.Write(costInfo.ItemID);
                        dataWriter.Write(costInfo.Collectablity.HasValue);
                        dataWriter.Write(costInfo.Collectablity.GetValueOrDefault());
                    }
                }

                var itemLength = checked((int)dataStream.Position - itemOffset);
                indexEntries.Add(new(itemID, itemOffset, itemLength));
            }
        }

        var stringBytes   = BuildStringSection(stringTableBuilder);
        var locationBytes = BuildLocationSection(locationTableBuilder);
        var indexBytes    = BuildIndexSection(indexEntries);
        var dataBytes     = dataStream.ToArray();

        using var outputStream = new MemoryStream();
        using var writer       = new BinaryWriter(outputStream, Encoding.UTF8, true);

        var headerPlaceholderSize = GetHeaderSizeHint();
        writer.Write(new byte[headerPlaceholderSize]);

        var stringSection   = WriteSection(outputStream, stringBytes);
        var locationSection = WriteSection(outputStream, locationBytes);
        var indexSection    = WriteSection(outputStream, indexBytes);
        var dataSection     = WriteSection(outputStream, dataBytes);

        outputStream.Position = 0;
        WriteHeader
        (
            writer,
            new
            (
                SNAPSHOT_MAGIC,
                SNAPSHOT_FORMAT_VERSION,
                GameState.ClientVersion,
                GameState.ClientLanguge,
                stringSection,
                locationSection,
                indexSection,
                dataSection
            )
        );

        await WriteAllBytesAtomicallyAsync(path, outputStream.ToArray()).ConfigureAwait(false);
    }

    private static byte[] BuildStringSection(IReadOnlyList<string> strings)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        writer.Write(strings.Count);
        foreach (var value in strings)
            writer.Write(value);

        return stream.ToArray();
    }

    private static byte[] BuildLocationSection(IReadOnlyList<ShopNPCLocation> locations)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        writer.Write(locations.Count);

        foreach (var location in locations)
        {
            writer.Write(location.X);
            writer.Write(location.Y);
            writer.Write(location.TerritoryID);
            writer.Write(location.MapID);
        }

        return stream.ToArray();
    }

    private static byte[] BuildIndexSection(IReadOnlyList<IndexEntry> entries)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        writer.Write(entries.Count);

        foreach (var entry in entries)
        {
            writer.Write(entry.ItemID);
            writer.Write(entry.Offset);
            writer.Write(entry.Length);
        }

        return stream.ToArray();
    }

    private static SectionInfo WriteSection(Stream stream, byte[] bytes)
    {
        var offset = stream.Position;
        stream.Write(bytes);
        return new(offset, bytes.Length, ComputeChecksum(bytes));
    }

    private static SnapshotHeader ReadHeader(BinaryReader reader)
    {
        var magic           = reader.ReadUInt32();
        var formatVersion   = reader.ReadInt32();
        var clientVersion   = reader.ReadString();
        var clientLanguage  = (Language)reader.ReadInt32();
        var stringSection   = ReadSectionInfo(reader);
        var locationSection = ReadSectionInfo(reader);
        var indexSection    = ReadSectionInfo(reader);
        var dataSection     = ReadSectionInfo(reader);

        return new(magic, formatVersion, clientVersion, clientLanguage, stringSection, locationSection, indexSection, dataSection);
    }

    private static void WriteHeader(BinaryWriter writer, SnapshotHeader header)
    {
        writer.Write(header.Magic);
        writer.Write(header.FormatVersion);
        writer.Write(header.ClientVersion);
        writer.Write((int)header.ClientLanguage);
        WriteSectionInfo(writer, header.StringSection);
        WriteSectionInfo(writer, header.LocationSection);
        WriteSectionInfo(writer, header.IndexSection);
        WriteSectionInfo(writer, header.DataSection);
    }

    private static void ValidateHeader(SnapshotHeader header, int fileLength)
    {
        if (header.Magic != SNAPSHOT_MAGIC)
            throw new InvalidDataException($"快照魔数不匹配: {header.Magic}");

        if (header.FormatVersion != SNAPSHOT_FORMAT_VERSION)
            throw new InvalidDataException($"快照格式版本不匹配: {header.FormatVersion}");

        ValidateSection(header.StringSection,   fileLength);
        ValidateSection(header.LocationSection, fileLength);
        ValidateSection(header.IndexSection,    fileLength);
        ValidateSection(header.DataSection,     fileLength);
    }

    private static void ValidateSection(SectionInfo section, int fileLength)
    {
        if (section.Offset < 0 || section.Length < 0)
            throw new InvalidDataException("快照区段目录存在非法负值");

        if (section.Offset + section.Length > fileLength)
            throw new InvalidDataException("快照区段越界");
    }

    private static void ValidateChecksum(byte[] bytes, SectionInfo section)
    {
        var actual = ComputeChecksum(bytes.AsSpan((int)section.Offset, section.Length));
        if (actual != section.Checksum)
            throw new InvalidDataException($"快照区段校验失败, Offset: {section.Offset}, Length: {section.Length}");
    }

    private static string[] ReadStringSection(byte[] bytes, SectionInfo section)
    {
        using var stream = new MemoryStream(bytes, (int)section.Offset, section.Length, false);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);

        var count  = reader.ReadInt32();
        var result = new string[count];
        for (var i = 0; i < count; i++)
            result[i] = reader.ReadString();

        return result;
    }

    private static ShopNPCLocation[] ReadLocationSection(byte[] bytes, SectionInfo section)
    {
        using var stream = new MemoryStream(bytes, (int)section.Offset, section.Length, false);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);

        var count  = reader.ReadInt32();
        var result = new ShopNPCLocation[count];

        for (var i = 0; i < count; i++)
        {
            var x         = reader.ReadSingle();
            var y         = reader.ReadSingle();
            var territory = reader.ReadUInt32();
            var mapID     = reader.ReadUInt32();
            result[i] = new(x, y, territory, mapID);
        }

        return result;
    }

    private static FrozenDictionary<uint, ItemOffset> ReadIndexSection(byte[] bytes, SectionInfo section, long dataSectionOffset)
    {
        using var stream = new MemoryStream(bytes, (int)section.Offset, section.Length, false);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);

        var count   = reader.ReadInt32();
        var builder = new Dictionary<uint, ItemOffset>(count);

        for (var i = 0; i < count; i++)
        {
            var itemID = reader.ReadUInt32();
            var offset = reader.ReadInt32();
            var length = reader.ReadInt32();
            builder[itemID] = new(itemID, dataSectionOffset + offset, length);
        }

        return builder.ToFrozenDictionary();
    }

    private static int GetHeaderSizeHint() =>
        512;

    private static SectionInfo ReadSectionInfo(BinaryReader reader) =>
        new(reader.ReadInt64(), reader.ReadInt32(), reader.ReadUInt32());

    private static void WriteSectionInfo(BinaryWriter writer, SectionInfo info)
    {
        writer.Write(info.Offset);
        writer.Write(info.Length);
        writer.Write(info.Checksum);
    }

    private static uint ComputeChecksum(ReadOnlySpan<byte> data)
    {
        const uint offsetBasis = 2166136261;
        const uint prime       = 16777619;

        var hash = offsetBasis;

        foreach (var value in data)
        {
            hash ^= value;
            hash *= prime;
        }

        return hash;
    }

    private static uint ComputeChecksum(byte[] bytes) =>
        ComputeChecksum(bytes.AsSpan());

    private static string ReadRequiredString(IReadOnlyList<string> strings, int index)
    {
        if ((uint)index >= strings.Count)
            throw new InvalidDataException($"字符串索引越界: {index}");

        return strings[index];
    }

    private static string? ReadOptionalString(IReadOnlyList<string> strings, int index) =>
        index < 0 ? null : ReadRequiredString(strings, index);

    private static int GetRequiredStringID(string value, IDictionary<string, int> indices, ICollection<string> table)
    {
        if (indices.TryGetValue(value, out var id))
            return id;

        id             = table.Count;
        indices[value] = id;
        table.Add(value);
        return id;
    }

    private static int GetOptionalStringID(string? value, IDictionary<string, int> indices, ICollection<string> table) =>
        string.IsNullOrEmpty(value) ? -1 : GetRequiredStringID(value, indices, table);

    private static int GetLocationID
    (
        ShopNPCLocation?              location,
        IDictionary<LocationKey, int> indices,
        ICollection<ShopNPCLocation>  table
    )
    {
        if (location == null)
            return -1;

        var key = new LocationKey(location.X, location.Y, location.TerritoryID, location.MapID);
        if (indices.TryGetValue(key, out var id))
            return id;

        id           = table.Count;
        indices[key] = id;
        table.Add(location);
        return id;
    }

    private static async Task WriteAllBytesAtomicallyAsync(string path, byte[] bytes)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";

        try
        {
            await File.WriteAllBytesAsync(tempPath, bytes).ConfigureAwait(false);

            if (File.Exists(path))
                File.Replace(tempPath, path, null);
            else
                File.Move(tempPath, path);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private static void TryDeleteSnapshot(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            DLog.Warning($"[ItemSourceRepository] 删除旧快照失败: {path}, {ex.Message}");
        }
    }

    private static string GetSnapshotPath() =>
        Path.Join(DService.Instance().PI.GetPluginConfigDirectory(), "OmenTools", "Cache", "ItemSourceSnapshot.bin");

    private enum RepositoryStatus
    {
        Building,
        Ready,
        Failed
    }

    private readonly record struct ItemOffset
    (
        uint ItemID,
        long Offset,
        int  Length
    );

    private readonly record struct IndexEntry
    (
        uint ItemID,
        int  Offset,
        int  Length
    );

    private readonly record struct LocationKey
    (
        float X,
        float Y,
        uint  TerritoryID,
        uint  MapID
    );

    private readonly record struct SectionInfo
    (
        long Offset,
        int  Length,
        uint Checksum
    );

    private readonly record struct SnapshotHeader
    (
        uint        Magic,
        int         FormatVersion,
        string      ClientVersion,
        Language    ClientLanguage,
        SectionInfo StringSection,
        SectionInfo LocationSection,
        SectionInfo IndexSection,
        SectionInfo DataSection
    );
}
