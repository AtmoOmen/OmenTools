using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using Lumina.Excel.Sheets;
using DynamicEvent = Lumina.Excel.Sheets.DynamicEvent;

namespace OmenTools.Infos;

/// <summary>
/// 新月岛中的野外事件 (CE / FATE / 魔法罐任务) 数据类
/// </summary>
public class CrescentEvent : IEquatable<CrescentEvent>
{
    /// <summary>
    /// 是否解锁了特殊奖励物品
    /// </summary>
    public static unsafe bool? IsSpecialRewardUnlocked(uint specialReward)
    {
        bool? isObtained = null;

        var itemCount = LocalPlayerState.GetItemCount(specialReward);
        if (itemCount > 0)
            isObtained = true;
        else if (JobItemToJob.TryGetValue(specialReward, out var jobIndex))
            isObtained = PublicContentOccultCrescent.GetState()->SupportJobLevels[jobIndex] > 0;
        else if (LoreItems.Contains(specialReward))
        {
            // 提前请求有几率可以让下面不返回 null
            _ = ExdModule.GetItemRowById(specialReward);
            
            var itemRow = ExdModule.GetItemRowById(specialReward);
            if (itemRow != null)
                isObtained = UIState.Instance()->IsItemActionUnlocked(itemRow) == 1;
        }
        
        return isObtained;
    }

    public static string GetEventTypeName(CrescentEventType type) =>
        type switch
        {
            CrescentEventType.FATE      => LuminaWrapper.GetAddonText(2275),
            CrescentEventType.CE        => LuminaWrapper.GetAddonText(13988),
            CrescentEventType.ForkTower => LuminaWrapper.GetDescriptionString(1205),
            CrescentEventType.MagicPot  => LuminaWrapper.GetENpcName(1005475),
            _                           => string.Empty
        };

    /// <summary>
    /// 数据 ID
    /// </summary>
    public uint DataID { get; }

    /// <summary>
    /// 类型
    /// </summary>
    public CrescentEventType Type { get; }

    /// <summary>
    /// 掉落的 半魂晶 物品 ID (两岐塔 类型无此掉落)
    /// </summary>
    public uint DemiatmaID { get; }

    /// <summary>
    /// 特殊奖励物品 ID (仅 CE 类型有此掉落)
    /// </summary>
    public List<uint> SpecialRewards { get; } = [];
    
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; } = string.Empty;

    /// <summary>
    /// 地图图标 ID
    /// </summary>
    public uint IconID { get; }
    
    
    /// <summary>
    /// 当前位置 (临时数据)
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// 范围半径 (临时数据)
    /// </summary>
    public float Radius { get; set; }
    
    /// <summary>
    /// 用于外部展示的名称 (临时数据)
    /// </summary>
    public string NameDisplay { get; set; } = string.Empty;

    /// <summary>
    /// 当前 FATE 状态
    /// 适用于 FATE 和 魔法罐任务 类型
    /// (临时数据)
    /// </summary>
    public FateState FateState { get; set; } = FateState.Ended;
    
    /// <summary>
    /// 当前 CE 状态
    /// 适用于 CE 和 两岐塔 类型
    /// (临时数据)
    /// </summary>
    public DynamicEventState CEState { get; set; } = DynamicEventState.Inactive;
    
    /// <summary>
    /// 当前 CE 距离正式开始的剩余时间秒数
    /// 适用于 CE 类型
    /// (临时数据)
    /// </summary>
    public long CELeftTimeSecond { get; set; }
    
    /// <summary>
    /// 新月岛中的野外事件 (CE / FATE / 魔法罐任务) 数据类
    /// </summary>
    public CrescentEvent(uint dataID)
    {
        DataID = dataID;

        if (LuminaGetter.TryGetRow<DynamicEvent>(dataID, out _))
        {
            HashSet<uint> forkTowers = [48];
            Type   = forkTowers.Contains(dataID) ? CrescentEventType.ForkTower : CrescentEventType.CE;
            
            if (TryGetCEData(out var ceData))
            {
                Name        = ceData.Name.ExtractText();
                NameDisplay = Name;
                IconID      = ceData.EventType.Value.IconObjective0;
                Radius      = ceData.Unknown4;
            }
        }
        else
        {
            HashSet<uint> magicPots = [1976, 1977];
            Type = magicPots.Contains(dataID) ? CrescentEventType.MagicPot : CrescentEventType.FATE;
            
            if (TryGetFateData(out var fateData))
            {
                Name        = fateData.Name.ExtractText();
                NameDisplay = Name;
                IconID      = fateData.MapIcon;
            }
        }
        
        if (Type != CrescentEventType.ForkTower)
            DemiatmaID = EventToItem.GetValueOrDefault(dataID, 0U);

        if (Type == CrescentEventType.CE)
            SpecialRewards = CEToItems.GetValueOrDefault(dataID, []);
    }

    /// <summary>
    /// 更新坐标和范围半径临时数据
    /// (仅首次更新有效)
    /// </summary>
    public void UpdatePositionAndRadius(Vector3 position, float radius)
    {
        if (Radius == 0)
            Radius = radius;

        if (Position == default)
            Position = position;
    }

    /// <summary>
    /// 更新 FATE 相关临时数据
    /// </summary>
    public void UpdateTempDataFATE(string nameDisplay, FateState fateState)
    {
        if (Type is not (CrescentEventType.FATE or CrescentEventType.MagicPot)) return;
        
        NameDisplay = nameDisplay;
        FateState   = fateState;
    }
    
    /// <summary>
    /// 更新 FATE 相关临时数据
    /// </summary>
    public void UpdateTempDataCE(string nameDisplay, DynamicEventState ceState, long leftTimeSecond = 0)
    {
        if (Type is not (CrescentEventType.CE or CrescentEventType.ForkTower)) return;
        
        NameDisplay      = nameDisplay;
        CEState          = ceState;
        CELeftTimeSecond = leftTimeSecond;
    }

    /// <summary>
    /// 尝试获取对应的 Fate 表数据
    /// 适用于 FATE 和 魔法罐任务 类型
    /// </summary>
    public bool TryGetFateData(out Fate data)
    {
        data = default;
        if (Type is not (CrescentEventType.FATE or CrescentEventType.MagicPot)) return false;

        data = LuminaGetter.GetRow<Fate>(DataID).GetValueOrDefault();
        return true;
    }
    
    /// <summary>
    /// 尝试获取对应的 DynamicEvent 表数据
    /// 适用于 CE 和 两岐塔 类型
    /// </summary>
    public bool TryGetCEData(out DynamicEvent data)
    {
        data = default;
        if (Type is not (CrescentEventType.CE or CrescentEventType.ForkTower)) return false;

        data = LuminaGetter.GetRow<DynamicEvent>(DataID).GetValueOrDefault();
        return true;
    }
    
    /// <summary>
    /// 获取范围内靠近边缘的任一点
    /// </summary>
    public Vector3 GetRandomPointNearEdge()
    {
        var y = Position.Y;

        var randomValue  = Guid.NewGuid().GetHashCode() / (float)int.MaxValue;
        var randomRadius = (Radius == 0 ? 1 : Radius)   * MathF.Sqrt(Math.Abs(randomValue));

        var randomAngle = Guid.NewGuid().GetHashCode() / (float)int.MaxValue * 2 * MathF.PI;

        var x = Position.X + (randomRadius * MathF.Cos(randomAngle));
        var z = Position.Z + (randomRadius * MathF.Sin(randomAngle));

        return new Vector3(x, y, z);
    }

    public bool Equals(CrescentEvent? other) => 
        DataID == other?.DataID && Type == other?.Type;

    public override bool Equals(object? obj) => 
        obj is CrescentEvent other && Equals(other);

    public override int GetHashCode() => 
        HashCode.Combine(DataID, (int)Type);

    public static bool operator ==(CrescentEvent left, CrescentEvent right) => 
        left.Equals(right);

    public static bool operator !=(CrescentEvent left, CrescentEvent right) => 
        !left.Equals(right);

    #region 预置数据

    public static Dictionary<uint, uint> EventToItem { get; } = new()
    {
        // 青色半魂晶 (47744)
        [1962] = 47744,
        [1963] = 47744,
        [1970] = 47744,
        [33]   = 47744,
        [36]   = 47744,
        [35]   = 47744,
    
        // 碧色半魂晶 (47745)
        [1977] = 47745,
        [1968] = 47745,
        [1969] = 47745,
        [37]   = 47745,
    
        // 绿色半魂晶 (47746)
        [1966] = 47746,
        [47]   = 47746,
        [39]   = 47746,
        [38]   = 47746,
    
        // 橙色半魂晶 (47747)
        [1965] = 47747,
        [1967] = 47747,
        [45]   = 47747,
        [41]   = 47747,
        [43]   = 47747,
    
        // 紫色半魂晶 (47748)
        [1972] = 47748,
        [40]   = 47748,
        [42]   = 47748,
        [46]   = 47748,
    
        // 黄色半魂晶 (47749)
        [1976] = 47749,
        [1964] = 47749,
        [1971] = 47749,
        [44]   = 47749,
        [34]   = 47749
    };
    
    public static Dictionary<uint, List<uint>> CEToItems { get; } = new()
    {
        // 愤怒的人造人——新月狂战士
        [35] = [47730, 47751],
        // 挣脱封印的大妖异——回廊恶魔
        [37] = [48008, 47728],
        // 双极的造物——神秘土偶
        [39] = [47729],
        // 贩卖诅咒的商贩——金钱龟
        [45] = [47733],
        // 传说中的鲨鱼——尼姆瓣齿鲨
        [41] = [47731],
        // 双足狮人——跃立狮
        [42] = [47757],
        // 黑色连队
        [34] = [47752, 47732],
    };
    
    public static HashSet<uint> LoreItems { get; } =
    [
        47728,
        // 神秘土偶
        47729,
        // 新月狂战士
        47730,
        // 尼姆瓣齿鲨
        47731,
        // 黑陆行鸟
        47732,
        // 金钱龟
        47733,
        // 恶魔板
        47734,
        // 星头三兄弟
        47735,
        // 大理石龙
        47736,
        // 魔陶洛斯
        47737,
        // 撒娇罐
        47738
    ];
    
    public static Dictionary<uint, byte> JobItemToJob { get; } = new()
    {
        // 狂战士
        [47751] = 2,
        // 猎人
        [47752] = 4,
        // 武士
        [47753] = 5,
        // 风水师
        [47754] = 7,
        // 时魔法师
        [47755] = 8,
        // 炮击士
        [47756] = 9,
        // 预言师
        [47757] = 11,
        // 药剂师
        [47758] = 10,
        // 盗贼
        [47759] = 12,
    };

    #endregion
}

public enum CrescentEventType
{
    /// <summary>
    /// 临危受命
    /// </summary>
    FATE,
        
    /// <summary>
    /// 紧急遭遇战
    /// </summary>
    CE,
        
    /// <summary>
    /// 魔法罐任务
    /// </summary>
    MagicPot,
    
    /// <summary>
    /// 两岐塔
    /// </summary>
    ForkTower
}
