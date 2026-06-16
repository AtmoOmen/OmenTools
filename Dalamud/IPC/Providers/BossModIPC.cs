using System.Numerics;
using OmenTools.Dalamud.Abstractions;
using OmenTools.Dalamud.Attributes;

namespace OmenTools.Dalamud;

// ReSharper disable once InconsistentNaming
public static class BossModIPC
{
    public const string INTERNAL_NAME = "BossMod";
    
    public const string REBORN_INTERNAL_NAME = "BossModReborn";

    public static bool IsPluginEnabled() => 
        DService.Instance().PI.IsPluginEnabled(INTERNAL_NAME) ||
        DService.Instance().PI.IsPluginEnabled(REBORN_INTERNAL_NAME);
    

    [IPCSubscriber("BossMod.HasModuleByDataId", DefaultValue = "false")]
    private static IPCSubscriber<uint, bool>? HasModuleByDataID;

    [IPCSubscriber("BossMod.Configuration", DefaultValue = "false")]
    private static IPCSubscriber<List<string>, bool, bool>? Configuration;

    [IPCSubscriber("BossMod.Configuration.LastModified")]
    private static IPCSubscriber<DateTime>? ConfigurationLastModified;

    [IPCSubscriber("BossMod.Configuration.DisableModule", DefaultValue = "false")]
    private static IPCSubscriber<string, bool, bool>? ConfigurationDisableModule;

    [IPCSubscriber("BossMod.Rotation.ActionQueue.HasEntries", DefaultValue = "false")]
    private static IPCSubscriber<bool>? RotationActionQueueHasEntries;

    [IPCSubscriber("BossMod.Presets.Get")]
    private static IPCSubscriber<string, string?>? PresetsGet;

    [IPCSubscriber("BossMod.Presets.Create", DefaultValue = "false")]
    private static IPCSubscriber<string, bool, bool>? PresetsCreate;

    [IPCSubscriber("BossMod.Presets.Delete", DefaultValue = "false")]
    private static IPCSubscriber<string, bool>? PresetsDelete;

    [IPCSubscriber("BossMod.Presets.GetActive")]
    private static IPCSubscriber<string?>? PresetsGetActive;

    [IPCSubscriber("BossMod.Presets.SetActive", DefaultValue = "false")]
    private static IPCSubscriber<string, bool>? PresetsSetActive;

    [IPCSubscriber("BossMod.Presets.ClearActive", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PresetsClearActive;

    [IPCSubscriber("BossMod.Presets.GetForceDisabled", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PresetsGetForceDisabled;

    [IPCSubscriber("BossMod.Presets.SetForceDisabled", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PresetsSetForceDisabled;

    [IPCSubscriber("BossMod.Presets.Activate", DefaultValue = "false")]
    private static IPCSubscriber<string, bool>? PresetsActivate;

    [IPCSubscriber("BossMod.Presets.Deactivate", DefaultValue = "false")]
    private static IPCSubscriber<string, bool>? PresetsDeactivate;

    [IPCSubscriber("BossMod.Presets.GetActiveList")]
    private static IPCSubscriber<List<string>>? PresetsGetActiveList;

    [IPCSubscriber("BossMod.Presets.SetActiveList", DefaultValue = "false")]
    private static IPCSubscriber<List<string>, bool>? PresetsSetActiveList;

    [IPCSubscriber("BossMod.Presets.AddTransientStrategy", DefaultValue = "false")]
    private static IPCSubscriber<string, string, string, string, bool>? PresetsAddTransientStrategy;

    [IPCSubscriber("BossMod.Presets.AddTransientStrategyTargetEnemyOID", DefaultValue = "false")]
    private static IPCSubscriber<string, string, string, string, int, bool>? PresetsAddTransientStrategyTargetEnemyOID;

    [IPCSubscriber("BossMod.Presets.ClearTransientStrategy", DefaultValue = "false")]
    private static IPCSubscriber<string, string, string, bool>? PresetsClearTransientStrategy;

    [IPCSubscriber("BossMod.Presets.ClearTransientModuleStrategies", DefaultValue = "false")]
    private static IPCSubscriber<string, string, bool>? PresetsClearTransientModuleStrategies;

    [IPCSubscriber("BossMod.Presets.ClearTransientPresetStrategies", DefaultValue = "false")]
    private static IPCSubscriber<string, bool>? PresetsClearTransientPresetStrategies;

    [IPCSubscriber("BossMod.ObstacleMap.Generate", DefaultValue = "false")]
    private static IPCSubscriber<Vector3, float, bool, bool>? ObstacleMapGenerate;

    [IPCSubscriber("BossMod.ObstacleMap.GetGenerationStatus", DefaultValue = "0")]
    private static IPCSubscriber<int>? ObstacleMapGetGenerationStatus;

    [IPCSubscriber("BossMod.ObstacleMap.HasTempMap", DefaultValue = "false")]
    private static IPCSubscriber<bool>? ObstacleMapHasTempMap;

    [IPCSubscriber("BossMod.ObstacleMap.ClearTempMap", DefaultValue = "false")]
    private static IPCSubscriber<bool>? ObstacleMapClearTempMap;

    [IPCSubscriber("BossMod.ObstacleMap.EvaluateTempMapQuality", DefaultValue = "false")]
    private static IPCSubscriber<bool>? ObstacleMapEvaluateTempMapQuality;

    /// <summary>
    ///     检查指定数据 ID 的 Boss 模块是否存在
    /// </summary>
    /// <param name="dataID">数据 ID</param>
    /// <returns></returns>
    public static bool GetHasModuleByDataID(uint dataID) =>
        HasModuleByDataID?.InvokeFunc(dataID) ?? false;

    /// <summary>
    ///     执行配置控制台命令
    /// </summary>
    /// <param name="args">命令参数</param>
    /// <param name="save">是否保存</param>
    /// <returns></returns>
    public static bool InvokeConfiguration(List<string> args, bool save) =>
        Configuration?.InvokeFunc(args, save) ?? false;

    /// <summary>
    ///     获取配置最后修改时间
    /// </summary>
    /// <returns></returns>
    public static DateTime GetConfigurationLastModified() =>
        ConfigurationLastModified?.InvokeFunc() ?? DateTime.MinValue;

    /// <summary>
    ///     禁用或启用指定模块
    /// </summary>
    /// <param name="name">模块名称</param>
    /// <param name="disable">是否禁用</param>
    /// <returns></returns>
    public static bool DisableModule(string name, bool disable) =>
        ConfigurationDisableModule?.InvokeFunc(name, disable) ?? false;

    /// <summary>
    ///     检查旋转操作队列是否有待执行项
    /// </summary>
    /// <returns></returns>
    public static bool GetHasRotationActionQueueEntries() =>
        RotationActionQueueHasEntries ?? false;

    /// <summary>
    ///     获取指定名称的预设
    /// </summary>
    /// <param name="name">预设名称</param>
    /// <returns>预设 JSON 字符串，未找到返回 null</returns>
    public static string? GetPreset(string name) =>
        PresetsGet?.InvokeFunc(name);

    /// <summary>
    ///     创建或覆盖预设
    /// </summary>
    /// <param name="presetSerialized">序列化的预设 JSON</param>
    /// <param name="overwrite">是否覆盖已存在的同名预设</param>
    /// <returns></returns>
    public static bool CreatePreset(string presetSerialized, bool overwrite) =>
        PresetsCreate?.InvokeFunc(presetSerialized, overwrite) ?? false;

    /// <summary>
    ///     删除指定名称的预设
    /// </summary>
    /// <param name="name">预设名称</param>
    /// <returns></returns>
    public static bool DeletePreset(string name) =>
        PresetsDelete?.InvokeFunc(name) ?? false;

    /// <summary>
    ///     获取当前激活的预设名称
    /// </summary>
    /// <returns>激活的预设名称，无激活预设返回 null</returns>
    public static string? GetActivePreset() =>
        PresetsGetActive?.InvokeFunc();

    /// <summary>
    ///     设置当前激活的预设
    /// </summary>
    /// <param name="name">预设名称</param>
    /// <returns></returns>
    public static bool SetActivePreset(string name) =>
        PresetsSetActive?.InvokeFunc(name) ?? false;

    /// <summary>
    ///     清除所有激活的预设
    /// </summary>
    /// <returns></returns>
    public static bool ClearActivePresets() =>
        PresetsClearActive ?? false;

    /// <summary>
    ///     检查自动循环是否被强制禁用
    /// </summary>
    /// <returns></returns>
    public static bool GetIsForceDisabled() =>
        PresetsGetForceDisabled ?? false;

    /// <summary>
    ///     强制禁用自动循环
    /// </summary>
    /// <returns></returns>
    public static bool SetForceDisabled() =>
        PresetsSetForceDisabled ?? false;

    /// <summary>
    ///     激活指定预设（添加到激活列表）
    /// </summary>
    /// <param name="name">预设名称</param>
    /// <returns></returns>
    public static bool ActivatePreset(string name) =>
        PresetsActivate?.InvokeFunc(name) ?? false;

    /// <summary>
    ///     取消激活指定预设（从激活列表移除）
    /// </summary>
    /// <param name="name">预设名称</param>
    /// <returns></returns>
    public static bool DeactivatePreset(string name) =>
        PresetsDeactivate?.InvokeFunc(name) ?? false;

    /// <summary>
    ///     获取当前激活的预设名称列表
    /// </summary>
    /// <returns></returns>
    public static List<string> GetActivePresetList() =>
        PresetsGetActiveList?.InvokeFunc() ?? [];

    /// <summary>
    ///     设置激活的预设列表
    /// </summary>
    /// <param name="names">预设名称列表</param>
    /// <returns></returns>
    public static bool SetActivePresetList(List<string> names) =>
        PresetsSetActiveList?.InvokeFunc(names) ?? false;

    /// <summary>
    ///     为指定预设添加临时策略
    /// </summary>
    /// <param name="presetName">预设名称</param>
    /// <param name="moduleTypeName">模块类型名称</param>
    /// <param name="trackName">轨道名称</param>
    /// <param name="value">策略值</param>
    /// <returns></returns>
    public static bool AddTransientStrategy(string presetName, string moduleTypeName, string trackName, string value) =>
        PresetsAddTransientStrategy?.InvokeFunc(presetName, moduleTypeName, trackName, value) ?? false;

    /// <summary>
    ///     为指定预设添加临时策略（指定目标敌人 OID）
    /// </summary>
    /// <param name="presetName">预设名称</param>
    /// <param name="moduleTypeName">模块类型名称</param>
    /// <param name="trackName">轨道名称</param>
    /// <param name="value">策略值</param>
    /// <param name="oid">目标敌人 OID</param>
    /// <returns></returns>
    public static bool AddTransientStrategyTargetEnemyOID(string presetName, string moduleTypeName, string trackName, string value, int oid) =>
        PresetsAddTransientStrategyTargetEnemyOID?.InvokeFunc(presetName, moduleTypeName, trackName, value, oid) ?? false;

    /// <summary>
    ///     清除指定预设中指定模块的临时策略
    /// </summary>
    /// <param name="presetName">预设名称</param>
    /// <param name="moduleTypeName">模块类型名称</param>
    /// <param name="trackName">轨道名称</param>
    /// <returns></returns>
    public static bool ClearTransientStrategy(string presetName, string moduleTypeName, string trackName) =>
        PresetsClearTransientStrategy?.InvokeFunc(presetName, moduleTypeName, trackName) ?? false;

    /// <summary>
    ///     清除指定预设中指定模块的所有临时策略
    /// </summary>
    /// <param name="presetName">预设名称</param>
    /// <param name="moduleTypeName">模块类型名称</param>
    /// <returns></returns>
    public static bool ClearTransientModuleStrategies(string presetName, string moduleTypeName) =>
        PresetsClearTransientModuleStrategies?.InvokeFunc(presetName, moduleTypeName) ?? false;

    /// <summary>
    ///     清除指定预设的所有临时策略
    /// </summary>
    /// <param name="presetName">预设名称</param>
    /// <returns></returns>
    public static bool ClearTransientPresetStrategies(string presetName) =>
        PresetsClearTransientPresetStrategies?.InvokeFunc(presetName) ?? false;

    /// <summary>
    ///     生成障碍物地图
    /// </summary>
    /// <param name="centerWorld">世界坐标中心</param>
    /// <param name="radius">半径</param>
    /// <param name="writeToFile">是否写入文件</param>
    /// <returns></returns>
    public static bool GenerateObstacleMap(Vector3 centerWorld, float radius, bool writeToFile) =>
        ObstacleMapGenerate?.InvokeFunc(centerWorld, radius, writeToFile) ?? false;

    /// <summary>
    ///     获取障碍物地图生成状态
    /// </summary>
    /// <returns></returns>
    public static int GetObstacleMapGenerationStatus() =>
        ObstacleMapGetGenerationStatus ?? 0;

    /// <summary>
    ///     检查是否存在临时障碍物地图
    /// </summary>
    /// <returns></returns>
    public static bool GetHasTempObstacleMap() =>
        ObstacleMapHasTempMap ?? false;

    /// <summary>
    ///     清除临时障碍物地图
    /// </summary>
    /// <returns></returns>
    public static bool ClearTempObstacleMap() =>
        ObstacleMapClearTempMap ?? false;

    /// <summary>
    ///     评估临时障碍物地图质量
    /// </summary>
    /// <returns></returns>
    public static bool EvaluateTempObstacleMapQuality() =>
        ObstacleMapEvaluateTempMapQuality ?? false;
}
