using FFXIVClientStructs.FFXIV.Client.Game.MJI;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed unsafe class MJICommand : ExecuteCommandBase
{
    /// <summary>
    ///     切换无人岛模式
    /// </summary>
    public static void SetMode(Mode mode) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJISetMode, (uint)mode);

    /// <summary>
    ///     设置无人岛模式参数
    /// </summary>
    public static void SetModeParam(uint param) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJISetModeParam, param);

    /// <summary>
    ///     清除无人岛模式参数
    /// </summary>
    public static void ClearModeParam() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJISetModeParam);

    /// <summary>
    ///     打开无人岛设置面板
    /// </summary>
    public static void OpenSettingPanel() =>
        SetSettingPanel(true);
    
    /// <summary>
    ///     关闭无人岛设置面板
    /// </summary>
    public static void CloseSettingPanel() =>
        SetSettingPanel(false);
    
    /// <summary>
    ///     设置无人岛设置面板开关状态
    /// </summary>
    public static void SetSettingPanel(bool isOpen) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJISettingPanelToggle, isOpen ? 1U : 0U);

    /// <summary>
    ///     请求无人岛工房排班数据
    /// </summary>
    public static void RequestWorkshop(uint cycleDay) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIWorkshopRequest, cycleDay);

    /// <summary>
    ///     添加无人岛工房排班
    /// </summary>
    public static void AddWorkshopSchedule(uint startingHour, uint craftObjectID, uint cycleDay) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIWorkshopAssign, BuildScheduleParam(startingHour, craftObjectID), cycleDay);

    /// <summary>
    ///     删除无人岛工房排班
    /// </summary>
    public static void RemoveWorkshopSchedule(uint startingHour, uint craftObjectID, uint cycleDay) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIWorkshopAssign, BuildScheduleParam(startingHour, craftObjectID), cycleDay, 0, 1);

    /// <summary>
    ///     取消无人岛工坊排班
    /// </summary>
    public static void CancelWorkshopSchedule(uint startingHour, uint craftObjectID, uint cycleDay) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIWorkshopCancel, BuildScheduleParam(startingHour, craftObjectID), cycleDay);

    /// <summary>
    ///     设置无人岛休息周期
    /// </summary>
    public static void SetRestCycles(uint restDay1, uint restDay2, uint restDay3, uint restDay4) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJISetRestCycles, restDay1, restDay2, restDay3, restDay4);

    /// <summary>
    ///     收取无人岛屯货仓库探索结果
    /// </summary>
    public static void CollectGranary(uint granaryIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIGranaryCollect, granaryIndex);

    /// <summary>
    ///     查看无人岛屯货仓库探索目的地
    /// </summary>
    public static void ViewGranaryDestinations(uint granaryIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIGranaryViewDestinations, granaryIndex);

    /// <summary>
    ///     无人岛屯货仓库派遣探险
    /// </summary>
    public static void AssignGranary(uint granaryIndex, uint destinationIndex, uint explorationDays) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIGranaryAssign, granaryIndex, destinationIndex, explorationDays);

    /// <summary>
    ///     在无人岛放养宠物
    /// </summary>
    public static void ReleaseMinion(uint minionID, uint areaIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIReleaseMinion, minionID, areaIndex);

    /// <summary>
    ///     放生无人岛牧场动物
    /// </summary>
    public static void ReleaseAnimal(uint animalIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIReleaseAnimal, animalIndex);

    /// <summary>
    ///     收集无人岛牧场动物产物
    /// </summary>
    public static void CollectAnimalLeavings(uint animalIndex, uint collectFlag) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJICollectAnimalLeavings, animalIndex, collectFlag);

    /// <summary>
    ///     收取无人岛牧场全部动物产物
    /// </summary>
    public static void CollectAllAnimalLeavings()
    {
        foreach (var (_, count) in MJIManager.Instance()->PastureHandler->AvailableMammetLeavings)
            ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJICollectAllAnimalLeavings, (uint)count);
    }

    /// <summary>
    ///     托管无人岛牧场动物
    /// </summary>
    public static void EntrustAnimal(uint animalIndex, uint feedItemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIEntrustAnimal, animalIndex, feedItemID);

    /// <summary>
    ///     召回无人岛放生的宠物
    /// </summary>
    public static void RecallMinion(uint minionIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIRecallMinion, minionIndex);

    /// <summary>
    ///     托管单块无人岛耕地
    /// </summary>
    public static void EntrustFarm(uint farmIndex, uint seedItemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIFarmEntrustSingle, farmIndex, seedItemID);

    /// <summary>
    ///     取消托管单块无人岛耕地
    /// </summary>
    public static void DismissFarm(uint farmIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIFarmDismiss, farmIndex);

    /// <summary>
    ///     收取单块无人岛耕地
    /// </summary>
    public static void CollectFarm(uint farmIndex, bool dismissAfterCollect = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIFarmCollectSingle, farmIndex, dismissAfterCollect ? 1U : 0U);

    /// <summary>
    ///     收取全部无人岛耕地
    /// </summary>
    public static void CollectAllFarms() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIFarmCollectAll, *(uint*)MJIManager.Instance()->GranariesState);

    private static uint BuildScheduleParam(uint startingHour, uint craftObjectID) =>
        8 * (startingHour | 32 * craftObjectID);

    public enum Mode : uint
    {
        Free    = 0,
        Gather  = 1,
        Sow     = 2,
        Water   = 3,
        Remove  = 4,
        Feed    = 6,
        Pet     = 7,
        Beckon  = 8,
        Capture = 9
    }
}
