﻿using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using Action = System.Action;
using Map = Lumina.Excel.Sheets.Map;

namespace OmenTools.Infos;

public static unsafe class GameState
{
    private static TaskHelper TaskHelper = null!;
    
    private static readonly CompSig                          FateDirectorSetupSig = new("E8 ?? ?? ?? ?? 48 39 37");
    private delegate        nint                             FateDirectorSetupDelegate(uint rowID, nint a2, nint a3);
    private static          Hook<FateDirectorSetupDelegate>? FateDirectorSetupHook;
    
    internal static void Init()
    {
        TaskHelper ??= new() { TimeLimitMS = int.MaxValue };
        
        DService.ClientState.Login  += OnDalamudLogin;
        DService.ClientState.Logout += OnDalamudLogout;

        FateDirectorSetupHook ??= FateDirectorSetupSig.GetHook<FateDirectorSetupDelegate>(FateDirectorSetupDetour);
        FateDirectorSetupHook.Enable();
    }

    internal static void Uninit()
    {
        DService.ClientState.Login  -= OnDalamudLogin;
        DService.ClientState.Logout -= OnDalamudLogout;
        
        TaskHelper.Abort();
        TaskHelper = null;
        
        FateDirectorSetupHook?.Dispose();
        FateDirectorSetupHook = null;
    }
    
    private static void OnDalamudLogin()
    {
        TaskHelper.Abort();
        
        TaskHelper.Enqueue(() =>
        {
            var agentLobby = AgentLobby.Instance();
            return agentLobby != null && agentLobby->IsLoggedIn && DService.ObjectTable.LocalPlayer != null && IsScreenReady();
        });
        
        TaskHelper.Enqueue(() =>
        {
            Login?.Invoke();
        });
    }
    
    private static void OnDalamudLogout(int type, int code) => 
        Logout?.Invoke();

    private static nint FateDirectorSetupDetour(uint rowID, nint a2, nint a3)
    {
        var original = FateDirectorSetupHook.Original(rowID, a2, a3);
        
        if (rowID == 102401 && FateManager.Instance()->CurrentFate != null)
            EnterFate?.Invoke(FateManager.Instance()->CurrentFate->FateId);

        return original;
    }

    // 因为生命周期里不会变更, 因此只需要懒加载一次即可
    private static readonly Lazy<bool> isCNLazy = 
        new(() => Framework.Instance()->ClientLanguage is 4 or 7);

    /// <summary>
    /// 是否为国服客户端
    /// </summary>
    public static bool IsCN => isCNLazy.Value;

    /// <summary>
    /// 进入临危受命范围时
    /// </summary>
    public static event Action<uint>? EnterFate;
    
    /// <summary>
    /// 登录且玩家可用时
    /// </summary>
    public static event Action? Login;
    
    /// <summary>
    /// 登出时
    /// </summary>
    public static event Action? Logout;

    /// <summary>
    /// 是否已经安全登录且玩家可用
    /// </summary>
    public static bool IsLoggedIn
    {
        get
        {
            var agentLobby = AgentLobby.Instance();
            return agentLobby != null && agentLobby->IsLoggedIn && DService.ObjectTable.LocalPlayer != null && IsScreenReady();
        }
    }

    /// <summary>
    /// 当前 Data Center
    /// </summary>
    public static uint CurrentDataCenter =>
        CurrentWorldData.DataCenter.ValueNullable?.RowId ?? 0;
    
    /// <summary>
    /// 当前 Data Center 表数据
    /// </summary>
    public static WorldDCGroupType CurrentDataCenterData =>
        CurrentWorldData.DataCenter.ValueNullable.GetValueOrDefault();
    
    /// <summary>
    /// 原始 Data Center
    /// </summary>
    public static uint HomeDataCenter =>
        HomeWorldData.DataCenter.ValueNullable?.RowId ?? 0;
    
    /// <summary>
    /// 原始 Data Center 表数据
    /// </summary>
    public static WorldDCGroupType HomeDataCenterData =>
        HomeWorldData.DataCenter.ValueNullable.GetValueOrDefault();
    
    /// <summary>
    /// 原始 World
    /// </summary>
    public static uint HomeWorld =>
        DService.ClientState.IsLoggedIn ? (uint)AgentLobby.Instance()->LobbyData.HomeWorldId : 0;
    
    /// <summary>
    /// 原始 World 表数据
    /// </summary>
    public static World HomeWorldData =>
        LuminaGetter.GetRow<World>(HomeWorld).GetValueOrDefault();
    
    /// <summary>
    /// 当前 World
    /// </summary>
    public static uint CurrentWorld =>
        DService.ClientState.IsLoggedIn ? (uint)AgentLobby.Instance()->LobbyData.CurrentWorldId : 0;
    
    /// <summary>
    /// 当前 World 表数据
    /// </summary>
    public static World CurrentWorldData =>
        LuminaGetter.GetRow<World>(CurrentWorld).GetValueOrDefault();
    
    /// <summary>
    /// 诛灭战限时奖励提升中 (必须在副本外)
    /// </summary>
    public static bool IsChaoticRaidBonusActive =>
        !IsInInstanceArea && UIState.Instance()->InstanceContent.IsLimitedTimeBonusActive == 1;
    
    /// <summary>
    /// 当前帧率
    /// </summary>
    public static float FrameRate => 
        Framework.Instance()->FrameRate;
    
    /// <summary>
    /// 当前服务器时间
    /// </summary>
    public static DateTime ServerTime =>
        DateTimeOffset.FromUnixTimeSeconds(ServerTimeUnix).LocalDateTime;
    
    /// <summary>
    /// 当前服务器时间 Unix 时间戳 (秒级)
    /// </summary>
    public static long ServerTimeUnix => 
        Framework.GetServerTime();
    
    /// <summary>
    /// 是否处于 PVP 副本中
    /// </summary>
    public static bool IsInPVPInstance =>
        GameMain.IsInPvPInstance();
    
    /// <summary>
    /// 是否处于 PVP 区域
    /// </summary>
    public static bool IsInPVPArea =>
        GameMain.IsInPvPArea();
    
    /// <summary>
    /// 是否处于副本区域
    /// </summary>
    public static bool IsInInstanceArea =>
        GameMain.Instance()->IsInInstanceArea();
    
    /// <summary>
    /// 是否处于观景视角中
    /// </summary>
    public static bool IsInIdleCam => 
        GameMain.IsInIdleCam();

    /// <summary>
    /// 当前区域数据是否加载完毕
    /// </summary>
    public static bool IsTerritoryLoaded => 
        TerritoryLoadState == 2;
    
    /// <summary>
    /// 当前 TerritoryLoadState, 2 为 加载完毕
    /// </summary>
    public static uint TerritoryLoadState => 
        GameMain.Instance()->TerritoryLoadState;
    
    /// <summary>
    /// 当前 TerritoryFilterKey
    /// </summary>
    public static uint TerritoryFilterKey => 
        GameMain.Instance()->CurrentTerritoryFilterKey;
    
    /// <summary>
    /// 当前 Map ID
    /// </summary>
    public static uint Map => 
        GameMain.Instance()->CurrentMapId;
    
    /// <summary>
    /// 当前 Map 表数据
    /// </summary>
    public static Map MapData =>
        LuminaGetter.GetRow<Map>(Map).GetValueOrDefault();
    
    /// <summary>
    /// 当前 TerritoryType ID
    /// </summary>
    public static uint TerritoryType => 
        GameMain.Instance()->CurrentTerritoryTypeId;
    
    /// <summary>
    /// 当前 TerritoryType 表数据
    /// </summary>
    public static TerritoryType TerritoryTypeData =>
        LuminaGetter.GetRow<TerritoryType>(TerritoryType).GetValueOrDefault();
    
    /// <summary>
    /// 当前 TerritoryIntendedUse ID
    /// </summary>
    public static uint TerritoryIntendedUse => 
        GameMain.Instance()->CurrentTerritoryIntendedUseId;
    
    /// <summary>
    /// 当前 TerritoryIntendedUse 表数据
    /// </summary>
    public static TerritoryIntendedUse TerritoryIntendedUseData =>
        LuminaGetter.GetRow<TerritoryIntendedUse>(TerritoryIntendedUse).GetValueOrDefault();
    
    /// <summary>
    /// 当前 ContentFinderCondition ID
    /// </summary>
    public static uint ContentFinderCondition => 
        GameMain.Instance()->CurrentContentFinderConditionId;

    /// <summary>
    /// 当前 ContentFinderCondition 表数据
    /// </summary>
    public static ContentFinderCondition ContentFinderConditionData =>
        LuminaGetter.GetRow<ContentFinderCondition>(ContentFinderCondition).GetValueOrDefault();

    /// <summary>
    /// 当前 Weather 表数据
    /// </summary>
    public static Weather WeatherData =>
        LuminaGetter.GetRow<Weather>(Weather).GetValueOrDefault();
    
    /// <summary>
    /// 当前天气 ID
    /// </summary>
    public static uint Weather =>
        WeatherManager.Instance()->GetCurrentWeather();
}
