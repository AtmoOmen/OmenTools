using System.Diagnostics.CodeAnalysis;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Math;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService;
using TerritoryIntendedUse = FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse;

namespace OmenTools.Info.Game;

/// <summary>
///     新月岛中的 辅助职业 数据类
/// </summary>
public class CrescentSupportJob : IEquatable<CrescentSupportJob>
{
    /// <summary>
    ///     辅助自由人
    /// </summary>
    public static CrescentSupportJob Freelancer { get; } = new(0, CrescentSupportJobType.Freelancer);

    /// <summary>
    ///     辅助骑士
    /// </summary>
    public static CrescentSupportJob Knight { get; } = new(1, CrescentSupportJobType.Knight, CrescentSupportJobUnlockType.None, 0, 41589, 4233);

    /// <summary>
    ///     辅助狂战士
    /// </summary>
    public static CrescentSupportJob Berserker { get; } = new(2, CrescentSupportJobType.Berserker, CrescentSupportJobUnlockType.CriticalEncounter, 35);

    /// <summary>
    ///     辅助武僧
    /// </summary>
    public static CrescentSupportJob Monk { get; } = new(3, CrescentSupportJobType.Monk, CrescentSupportJobUnlockType.None, 0, 41597, 4239);

    /// <summary>
    ///     辅助猎人
    /// </summary>
    public static CrescentSupportJob Ranger { get; } = new(4, CrescentSupportJobType.Ranger, CrescentSupportJobUnlockType.CriticalEncounter, 34);

    /// <summary>
    ///     辅助武士
    /// </summary>
    public static CrescentSupportJob Samurai { get; } = new(5, CrescentSupportJobType.Samurai, CrescentSupportJobUnlockType.GoldPiece, 45044);

    /// <summary>
    ///     辅助吟游诗人
    /// </summary>
    public static CrescentSupportJob Bard { get; } = new(6, CrescentSupportJobType.Bard, CrescentSupportJobUnlockType.None, 0, 41609, 4244);

    /// <summary>
    ///     辅助风水师
    /// </summary>
    public static CrescentSupportJob Geomancer { get; } = new(7, CrescentSupportJobType.Geomancer, CrescentSupportJobUnlockType.GoldPiece, 45044);

    /// <summary>
    ///     辅助时魔法师
    /// </summary>
    public static CrescentSupportJob TimeMage { get; } = new(8, CrescentSupportJobType.TimeMage, CrescentSupportJobUnlockType.SilverPiece, 45043);

    /// <summary>
    ///     辅助炮击士
    /// </summary>
    public static CrescentSupportJob Cannoneer { get; } = new(9, CrescentSupportJobType.Cannoneer, CrescentSupportJobUnlockType.SilverPiece, 45043);

    /// <summary>
    ///     辅助药剂师
    /// </summary>
    public static CrescentSupportJob Chemist { get; } = new(10, CrescentSupportJobType.Chemist, CrescentSupportJobUnlockType.SilverPiece, 45043);

    /// <summary>
    ///     辅助预言师
    /// </summary>
    public static CrescentSupportJob Oracle { get; } = new(11, CrescentSupportJobType.Oracle, CrescentSupportJobUnlockType.CriticalEncounter, 42);

    /// <summary>
    ///     辅助盗贼
    /// </summary>
    public static CrescentSupportJob Thief { get; } = new(12, CrescentSupportJobType.Thief, CrescentSupportJobUnlockType.GoldPiece, 45044);

    /// <summary>
    ///     辅助魔法剑士
    /// </summary>
    public static CrescentSupportJob MysticKnight { get; } = new(13, CrescentSupportJobType.MysticKnight, CrescentSupportJobUnlockType.SilverPiece, 45043);

    /// <summary>
    ///     辅助剑斗士
    /// </summary>
    public static CrescentSupportJob Gladiator { get; } = new(14, CrescentSupportJobType.Gladiator, CrescentSupportJobUnlockType.GoldPiece, 45044);

    /// <summary>
    ///     辅助舞者
    /// </summary>
    public static CrescentSupportJob Dancer { get; } = new(15, CrescentSupportJobType.Dancer, CrescentSupportJobUnlockType.SilverPiece, 45043, 46603, 4798);

    /// <summary>
    ///     辅助忍者
    /// </summary>
    public static CrescentSupportJob Ninja { get; } = new(16, CrescentSupportJobType.Ninja, CrescentSupportJobUnlockType.SilverObol, 51975);

    /// <summary>
    ///     辅助白魔法师
    /// </summary>
    public static CrescentSupportJob WhiteMage { get; } = new(17, CrescentSupportJobType.WhiteMage, CrescentSupportJobUnlockType.SilverObol, 51975);

    /// <summary>
    ///     辅助黑魔法师
    /// </summary>
    public static CrescentSupportJob BlackMage { get; } = new(18, CrescentSupportJobType.BlackMage, CrescentSupportJobUnlockType.SilverObol, 51975);

    /// <summary>
    ///     辅助龙骑士
    /// </summary>
    public static CrescentSupportJob Dragoon { get; } = new(19, CrescentSupportJobType.Dragoon, CrescentSupportJobUnlockType.GoldObol, 51976);

    /// <summary>
    ///     辅助召唤师
    /// </summary>
    public static CrescentSupportJob Summoner { get; } = new(20, CrescentSupportJobType.Summoner, CrescentSupportJobUnlockType.GoldObol, 51976);

    /// <summary>
    ///     辅助青魔法师
    /// </summary>
    public static CrescentSupportJob BlueMage { get; } = new(21, CrescentSupportJobType.BlueMage, CrescentSupportJobUnlockType.CriticalEncounter, 63);

    /// <summary>
    ///     辅助赤魔法师
    /// </summary>
    public static CrescentSupportJob RedMage { get; } = new(22, CrescentSupportJobType.RedMage, CrescentSupportJobUnlockType.SilverObol, 51975);

    /// <summary>
    ///     辅助亡灵法师
    /// </summary>
    public static CrescentSupportJob Necromancer { get; } = new(23, CrescentSupportJobType.Necromancer, CrescentSupportJobUnlockType.CriticalEncounter, 57);

    /// <summary>
    ///     全部辅助职业
    /// </summary>
    public static List<CrescentSupportJob> AllJobs { get; } =
    [
        Freelancer,
        Knight,
        Berserker,
        Monk,
        Ranger,
        Samurai,
        Bard,
        Geomancer,
        TimeMage,
        Cannoneer,
        Chemist,
        Oracle,
        Thief,
        MysticKnight,
        Gladiator,
        Dancer,
        Ninja,
        WhiteMage,
        BlackMage,
        Dragoon,
        Summoner,
        BlueMage,
        RedMage,
        Necromancer
    ];

    /// <summary>
    ///     获取当前的辅助职业
    /// </summary>
    /// <returns>新月岛副本区域外调用返回 null</returns>
    public static unsafe CrescentSupportJob? GetCurrentSupportJob()
    {
        var state = PublicContentOccultCrescent.GetState();
        if (state == null) return null;

        return AllJobs[state->CurrentSupportJob];
    }

    /// <summary>
    ///     尝试获取周围是否有可用的知见水晶物体
    /// </summary>
    public static unsafe bool TryFindKnowledgeCrystal([NotNullWhen(true)] out IGameObject? knowledgeCrystal)
    {
        knowledgeCrystal = null;
        if (GameState.TerritoryIntendedUse != TerritoryIntendedUse.OccultCrescent) return false;

        knowledgeCrystal = DService.Instance().ObjectTable.SearchObject
        (
            x => x is IEventObj { ObjectKind: ObjectKind.EventObj, DataID: 2007457 } obj &&
                 string.IsNullOrEmpty(obj.Name)                                          &&
                 obj.ToStruct()->EventState   == 0                                       &&
                 obj.ToStruct()->CameraOffset != Vector3.Zero,
            IObjectTable.EventRange
        );

        return knowledgeCrystal != null;
    }


    /// <summary>
    ///     新月岛中的 辅助职业 数据类
    /// </summary>
    public CrescentSupportJob
    (
        byte                         dataID,
        CrescentSupportJobType       jobType,
        CrescentSupportJobUnlockType unlockType             = CrescentSupportJobUnlockType.None,
        uint                         unlockLink             = 0,
        uint                         longTimeStatusActionID = 0,
        uint                         longTimeStatusID       = 0
    )
    {
        DataID                 = dataID;
        JobType                = jobType;
        UnlockType             = unlockType;
        UnlockLink             = unlockLink;
        LongTimeStatusActionID = longTimeStatusActionID;
        LongTimeStatusID       = longTimeStatusID;

        var data = GetData();

        foreach (var actionData in data.Actions)
        {
            var action = actionData.Action.RowId;
            var level  = actionData.LevelUnlock;
            if (action == 0 || level == 0) continue;

            Actions[action] = level;
        }

        foreach (var trait in LuminaGetter.Get<MKDTrait>())
        {
            if (trait.Icon == -1) continue;

            var supportJob = trait.MKDSupportJob.RowId;
            if (supportJob != DataID) continue;

            var unlockLevel = trait.LevelUnlock;
            Traits[trait.RowId] = unlockLevel;
        }

        Traits = Traits.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    ///     MKDSupportJob ID
    /// </summary>
    public byte DataID { get; }

    /// <summary>
    ///     类型
    /// </summary>
    public CrescentSupportJobType JobType { get; }

    /// <summary>
    ///     解锁类型
    /// </summary>
    public CrescentSupportJobUnlockType UnlockType { get; }

    /// <summary>
    ///     解锁用物品/紧急遭遇战 ID
    /// </summary>
    public uint UnlockLink { get; }

    /// <summary>
    ///     长效增益状态技能 ID (仅 辅助骑士 / 辅助武僧 / 辅助吟游诗人 / 辅助舞者)
    /// </summary>
    public uint LongTimeStatusActionID { get; init; }

    /// <summary>
    ///     长效增益状态 ID (仅 辅助骑士 / 辅助武僧 / 辅助吟游诗人)
    /// </summary>
    public uint LongTimeStatusID { get; init; }

    /// <summary>
    ///     辅助技能一览
    ///     键: Action ID; 值: 解锁等级
    /// </summary>
    public Dictionary<uint, byte> Actions { get; init; } = [];

    /// <summary>
    ///     辅助技能一览
    ///     键: MKDTrait ID; 值: 解锁等级
    /// </summary>
    public Dictionary<uint, byte> Traits { get; init; } = [];

    /// <summary>
    ///     辅助职业名称
    /// </summary>
    public string Name =>
        GetData().Name.ToString();

    public string UnlockTypeName =>
        UnlockType switch
        {
            CrescentSupportJobUnlockType.CriticalEncounter => LuminaWrapper.GetAddonText(13988),
            CrescentSupportJobUnlockType.SilverPiece       => $"{LuminaWrapper.GetENPCName(1053614)} ({LuminaWrapper.GetENPCTitle(1053614)})",
            CrescentSupportJobUnlockType.GoldPiece         => $"{LuminaWrapper.GetENPCName(1053614)} ({LuminaWrapper.GetENPCTitle(1053614)})",
            CrescentSupportJobUnlockType.SilverObol        => $"{LuminaWrapper.GetENPCName(1059485)} ({LuminaWrapper.GetENPCTitle(1059485)})",
            CrescentSupportJobUnlockType.GoldObol          => $"{LuminaWrapper.GetENPCName(1059485)} ({LuminaWrapper.GetENPCTitle(1059485)})",
            _                                              => string.Empty
        };

    public string UnlockLinkName =>
        UnlockType switch
        {
            CrescentSupportJobUnlockType.CriticalEncounter => LuminaWrapper.GetDynamicEventName(UnlockLink),
            CrescentSupportJobUnlockType.SilverPiece       => $"{LuminaWrapper.GetItemName(UnlockLink)} x1000",
            CrescentSupportJobUnlockType.GoldPiece         => $"{LuminaWrapper.GetItemName(UnlockLink)} x1600",
            CrescentSupportJobUnlockType.SilverObol        => $"{LuminaWrapper.GetItemName(UnlockLink)} x1000",
            CrescentSupportJobUnlockType.GoldObol          => $"{LuminaWrapper.GetItemName(UnlockLink)} x1600",
            _                                              => string.Empty
        };

    /// <summary>
    ///     辅助职业最大能达到的等级
    /// </summary>
    public byte MaxLevel =>
        GetData().LevelMax;

    /// <summary>
    ///     辅助职业当前等级, 新月岛副本区域外调用返回 0
    /// </summary>
    public unsafe byte CurrentLevel
    {
        get
        {
            var state = PublicContentOccultCrescent.GetState();
            if (state == null) return 0;

            return state->SupportJobLevels[DataID];
        }
    }

    /// <summary>
    ///     是否已经解锁该辅助职业
    /// </summary>
    /// <returns>在新月岛副本区域外调用返回 false</returns>
    public unsafe bool IsUnlocked()
    {
        var state = PublicContentOccultCrescent.GetState();
        if (state == null) return false;

        return CurrentLevel > 0;
    }

    /// <summary>
    ///     是否已经解锁指定辅助技能
    /// </summary>
    /// <returns>辅助技能不存在或在新月岛副本区域外调用返回 false</returns>
    public bool IsActionUnlocked(uint actionID) =>
        Actions.TryGetValue(actionID, out var levelRequired) &&
        levelRequired <= CurrentLevel                       &&
        ActionManager.IsActionUnlocked(actionID);

    /// <summary>
    ///     是否该辅助职业的长效增益效果技能已解锁
    /// </summary>
    /// <returns>在新月岛副本区域外调用返回 false</returns>
    public bool IsLongTimeStatusUnlocked()
    {
        if (LongTimeStatusActionID == 0 || LongTimeStatusID == 0)
            return false;

        return IsActionUnlocked(LongTimeStatusActionID);
    }

    /// <summary>
    ///     是否该辅助职业的长效增益效果已拥有
    /// </summary>
    /// <returns>本地玩家为 null 时调用返回 false</returns>
    public bool IsWithLongTimeStatus() =>
        LongTimeStatusID != 0 && LocalPlayerState.HasStatus(LongTimeStatusID, out _);

    /// <summary>
    ///     更换至该辅助职业
    /// </summary>
    /// <returns>false - 当前就为该职业 / 不在新月岛副本区域内; true - 发送请求成功</returns>
    public unsafe bool ChangeTo()
    {
        var state = PublicContentOccultCrescent.GetState();
        if (state == null) return false;

        if (state->CurrentSupportJob == DataID) return false;

        AgentMKDSupportJobList.Instance()->ChangeSupportJob(DataID);
        return true;
    }

    /// <summary>
    ///     当前是否为该辅助职业
    /// </summary>
    /// <returns>不在新月岛副本区域内调用返回 false</returns>
    public unsafe bool IsThisJob()
    {
        var state = PublicContentOccultCrescent.GetState();
        if (state == null) return false;

        return state->CurrentSupportJob == DataID;
    }

    /// <summary>
    ///     获取辅助职业表格数据
    /// </summary>
    /// <returns></returns>
    public MKDSupportJob GetData() =>
        LuminaGetter.GetRow<MKDSupportJob>(DataID).GetValueOrDefault();

    public bool Equals(CrescentSupportJob? other) =>
        DataID == other?.DataID;

    public override bool Equals(object? obj) =>
        obj is CrescentSupportJob other && Equals(other);

    public override int GetHashCode() =>
        DataID;

    public static bool operator ==(CrescentSupportJob left, CrescentSupportJob right) =>
        left.Equals(right);

    public static bool operator !=(CrescentSupportJob left, CrescentSupportJob right) =>
        !left.Equals(right);
}

public enum CrescentSupportJobType : byte
{
    /// <summary>
    ///     辅助自由人
    /// </summary>
    Freelancer = 0,

    /// <summary>
    ///     辅助骑士
    /// </summary>
    Knight = 1,

    /// <summary>
    ///     辅助狂战士
    /// </summary>
    Berserker = 2,

    /// <summary>
    ///     辅助武僧
    /// </summary>
    Monk = 3,

    /// <summary>
    ///     辅助猎人
    /// </summary>
    Ranger = 4,

    /// <summary>
    ///     辅助武士
    /// </summary>
    Samurai = 5,

    /// <summary>
    ///     辅助吟游诗人
    /// </summary>
    Bard = 6,

    /// <summary>
    ///     辅助风水师
    /// </summary>
    Geomancer = 7,

    /// <summary>
    ///     辅助时魔法师
    /// </summary>
    TimeMage = 8,

    /// <summary>
    ///     辅助炮击士
    /// </summary>
    Cannoneer = 9,

    /// <summary>
    ///     辅助药剂师
    /// </summary>
    Chemist = 10,

    /// <summary>
    ///     辅助预言师
    /// </summary>
    Oracle = 11,

    /// <summary>
    ///     辅助盗贼
    /// </summary>
    Thief = 12,

    /// <summary>
    ///     辅助魔法剑士
    /// </summary>
    MysticKnight = 13,

    /// <summary>
    ///     辅助剑斗士
    /// </summary>
    Gladiator = 14,

    /// <summary>
    ///     辅助舞者
    /// </summary>
    Dancer = 15,

    /// <summary>
    ///     辅助忍者
    /// </summary>
    Ninja = 16,

    /// <summary>
    ///     辅助白魔法师
    /// </summary>
    WhiteMage = 17,

    /// <summary>
    ///     辅助黑魔法师
    /// </summary>
    BlackMage = 18,

    /// <summary>
    ///     辅助龙骑士
    /// </summary>
    Dragoon = 19,

    /// <summary>
    ///     辅助召唤师
    /// </summary>
    Summoner = 20,

    /// <summary>
    ///     辅助青魔法师
    /// </summary>
    BlueMage = 21,

    /// <summary>
    ///     辅助赤魔法师
    /// </summary>
    RedMage = 22,

    /// <summary>
    ///     辅助亡灵法师
    /// </summary>
    Necromancer = 23
}

public enum CrescentSupportJobUnlockType
{
    /// <summary>
    ///     无需解锁
    /// </summary>
    None,

    /// <summary>
    ///     十二城邦银币 (固定为 1000)
    /// </summary>
    SilverPiece,

    /// <summary>
    ///     十二城邦金币 (固定为 1600)
    /// </summary>
    GoldPiece,

    /// <summary>
    ///     十二城邦白银币
    /// </summary>
    SilverObol,

    /// <summary>
    ///     十二城邦白金币
    /// </summary>
    GoldObol,

    /// <summary>
    ///     紧急遭遇战
    /// </summary>
    CriticalEncounter
}
