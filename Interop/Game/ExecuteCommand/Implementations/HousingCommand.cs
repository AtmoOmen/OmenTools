using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed unsafe class HousingCommand : ExecuteCommandBase
{
    /// <summary>
    ///     设置房屋背景音乐
    /// </summary>
    public static void SetBGM(uint orchestrionRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SetHouseBackgroundMusic, orchestrionRowID);
    
    /// <summary>
    ///     查看房屋详情
    /// </summary>
    public static void ViewDetail(uint territoryType, uint wardIndex, uint plotIndex, uint apartmentRoomIndex = 0) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ViewHouseDetail, territoryType, wardIndex * 256 + plotIndex, apartmentRoomIndex);
    
    /// <summary>
    ///     向当前房屋仓库存入指定物品
    /// </summary>
    public static void Store(InventoryType inventoryType, uint inventorySlot)
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID();
        if (houseIDHigh == houseID && houseID == 0) return;

        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.StoreFurniture, houseIDHigh, houseID, (uint)inventoryType, inventorySlot);
    }
    
    /// <summary>
    ///     调整房间环境
    /// </summary>
    /// <param name="light">房间亮度等级</param>
    /// <param name="enableSSAO">是否开启环境光遮蔽 (SSAO)</param>
    public static void SetIndoorEnvironment(BrightnessLevel light, bool enableSSAO) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SetIndoorEnvironment, (uint)light, enableSSAO ? 0U : 1);
    
    /// <summary>
    ///     请求门牌数据
    /// </summary>
    public static void RequestPlacard(HouseTerritory territoryType, uint wardIndex, uint houseIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestPlacardData, (uint)territoryType, wardIndex * 256 + houseIndex);
    
    /// <summary>
    ///     请求抽选数据
    /// </summary>
    public static void RequestLottery(HouseTerritory territoryType, uint wardIndex, uint plotIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestLotteryData, (uint)territoryType, wardIndex * 256 + plotIndex);
    
    /// <summary>
    ///     请求当前房屋名称设置数据
    /// </summary>
    public static void RequestName()
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID();
        if (houseIDHigh == houseID && houseID == 0) return;

        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestHousingName, houseIDHigh, houseID);
    }
    
    /// <summary>
    ///     请求当前房屋访客权限设置数据
    /// </summary>
    public static void RequestGuestAccess()
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID();
        if (houseIDHigh == houseID && houseID == 0) return;

        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestHousingGuestAccess, houseIDHigh, houseID);
    }
    
    /// <summary>
    ///     请求当前房屋问候语设置数据
    /// </summary>
    public static void RequestGreeting()
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID();
        if (houseIDHigh == houseID && houseID == 0) return;

        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestHousingGreeting, houseIDHigh, houseID);
    }
    
    /// <summary>
    ///     保存当前房屋访客权限设置
    /// </summary>
    public static void SaveGreeting(bool allowTeleport, bool allowEnter)
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID();
        if (houseIDHigh == houseID && houseID == 0) return;

        var flags = (allowTeleport ? 1U : 0U) | (allowEnter ? 65536U : 0U);
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SaveHousingGuestAccess, houseIDHigh, houseID, flags);
    }
    
    /// <summary>
    ///     请求当前房屋宣传设置数据
    /// </summary>
    public static void RequestEstateTag()
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID();
        if (houseIDHigh == houseID && houseID == 0) return;

        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestHousingEstateTag, houseIDHigh, houseID);
    }
    
    // TODO: 检查详细的 Flag
    /// <summary>
    ///     保存当前房屋宣传设置
    /// </summary>
    public static void SaveEstateTag(uint tagFlags)
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID();
        if (houseIDHigh == houseID && houseID == 0) return;

        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SaveHousingEstateTag, houseIDHigh, houseID, tagFlags);
    }
    
    /// <summary>
    ///     请求住宅区数据
    /// </summary>
    public static void RequestHousingArea(HouseTerritory territoryType, uint wardIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestHousingAreaData, (uint)territoryType, wardIndex);
    
    /// <summary>
    ///     移动到庭院门前
    /// </summary>
    public static void MoveToFrontGate() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MoveToHouseFrontGate, (uint)HousingManager.Instance()->GetCurrentPlot());
    
    /// <summary>
    ///     从房屋仓库中取出布置指定物品
    /// </summary>
    public static void Place(InventoryType inventoryType, uint inventorySlot)
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID();
        if (houseIDHigh == houseID && houseID == 0) return;

        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.PlaceFurnish, houseIDHigh, houseID, (uint)inventoryType, inventorySlot);
    }
    
    /// <summary>
    ///     从当前房屋中取回指定家具
    /// </summary>
    public static void Restore(InventoryType inventoryType, uint inventorySlot, bool toStoreRoom = false)
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID();
        if (houseIDHigh == houseID && houseID == 0) return;
        
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.RestoreFurniture,
            houseIDHigh,
            houseID,
            (uint)inventoryType,
            toStoreRoom ? inventorySlot + 65536U : inventorySlot
        );
    }

    /// <summary>
    ///     进入布置家具或庭具状态
    /// </summary>
    public static void EnterFurnishState() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FurnishState, 0, (uint)HousingManager.Instance()->GetCurrentPlot());

    /// <summary>
    ///     更改房屋内部装修风格
    /// </summary>
    public static void ChangeInteriorDesign(InteriorDesignStyle style) =>
        ExecuteCommandManager.Instance().ExecuteCommand
            (ExecuteCommandFlag.HouseInteriorDesignChange, (uint)HousingManager.Instance()->GetCurrentPlot(), (uint)style);

    private static (uint High, uint Low) GetCurrentHouseID()
    {
        var manager = HousingManager.Instance();
        if (manager == null)
            return (0, 0);

        if (manager->IndoorTerritory != null)
        {
            var houseID = manager->IndoorTerritory->HouseId;
            return ((uint)(houseID >> 32), (uint)houseID);
        }

        if (manager->OutdoorTerritory != null)
        {
            var houseID = manager->OutdoorTerritory->HouseId;
            return ((uint)(houseID >> 32), (uint)houseID);
        }

        if (manager->WorkshopTerritory != null)
        {
            var houseID = manager->WorkshopTerritory->HouseId;
            return ((uint)(houseID >> 32), (uint)houseID);
        }

        return (0, 0);
    }

    public enum HouseTerritory : uint
    {
        Mist         = 339,
        LavenderBeds = 340,
        Goblet       = 341,
        Shirogane    = 641,
        Empyreum     = 979
    }
    
    public enum InteriorDesignStyle : uint
    {
        /// <summary>
        ///     海雾村风格
        /// </summary>
        Mist = 1 * 3,

        /// <summary>
        ///     薰衣草苗圃风格
        /// </summary>
        LavenderBeds = 2 * 3,

        /// <summary>
        ///     高脚孤丘风格
        /// </summary>
        Goblet = 3 * 3,

        /// <summary>
        ///     白银乡风格
        /// </summary>
        Shirogane = 4 * 3,

        /// <summary>
        ///     穹顶皓天风格
        /// </summary>
        Empyreum = 5 * 3,

        /// <summary>
        ///     简装风格
        /// </summary>
        Simple = 6 * 3,

        /// <summary>
        ///     深色简装风格
        /// </summary>
        DarkSimple = 7 * 3
    }
    
    public enum BrightnessLevel : uint
    {
        Brightest  = 0,
        VeryBright = 1,
        Bright     = 2,
        Medium     = 3,
        Dim        = 4,
        Darkest    = 5
    }
}
