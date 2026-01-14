using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Data;
using Lumina.Excel.Sheets;
using OmenTools.Abstracts;
using Action = System.Action;
using Map = Lumina.Excel.Sheets.Map;
using TerritoryIntendedUse = FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse;

namespace OmenTools.Infos;

public unsafe class GameState : OmenServiceBase<GameState>
{
    private static readonly CompSig                          FateDirectorSetupSig = new("E8 ?? ?? ?? ?? 48 39 37");
    private delegate        nint                             FateDirectorSetupDelegate(uint rowID, nint a2, nint a3);
    private                 Hook<FateDirectorSetupDelegate>? FateDirectorSetupHook;

    private TaskHelper taskHelper = null!;

    internal override void Init()
    {
        taskHelper = new() { TimeoutMS = int.MaxValue };

        DService.Instance().ClientState.Login  += OnDalamudLogin;
        DService.Instance().ClientState.Logout += OnDalamudLogout;

        FateDirectorSetupHook ??= FateDirectorSetupSig.GetHook<FateDirectorSetupDelegate>(FateDirectorSetupDetour);
        FateDirectorSetupHook.Enable();
    }

    internal override void Uninit()
    {
        DService.Instance().ClientState.Login  -= OnDalamudLogin;
        DService.Instance().ClientState.Logout -= OnDalamudLogout;

        taskHelper.Abort();
        taskHelper = null;

        FateDirectorSetupHook?.Dispose();
        FateDirectorSetupHook = null;
    }

    private void OnDalamudLogin()
    {
        taskHelper.Abort();

        taskHelper.Enqueue(() => IsLoggedIn);
        taskHelper.Enqueue(() => Login?.Invoke());
    }

    private void OnDalamudLogout(int type, int code) =>
        Logout?.Invoke();

    private nint FateDirectorSetupDetour(uint rowID, nint a2, nint a3)
    {
        var original = FateDirectorSetupHook.Original(rowID, a2, a3);

        if (rowID == 102401 && FateManager.Instance()->CurrentFate != null)
            EnterFate?.Invoke(FateManager.Instance()->CurrentFate->FateId);

        return original;
    }

    /// <summary>
    ///     当前窗口是否位于前台
    /// </summary>
    public static bool IsForeground =>
        !Framework.Instance()->WindowInactive;

    /// <summary>
    ///     当前游戏客户端语言
    /// </summary>
    public static Language ClientLanguge =>
        ClientLangaugeLazy.Value;

    // 因为生命周期里不会变更, 因此只需要懒加载一次即可
    // 因为枚举前面多定义了一个 None, 所以要 + 1
    private static readonly Lazy<Language> ClientLangaugeLazy =
        new(() => (Language)(Framework.Instance()->ClientLanguage + 1));

    // 因为生命周期里不会变更, 因此只需要懒加载一次即可
    private static readonly Lazy<bool> IsGLLazy =
        new(() => Framework.Instance()->ClientLanguage < 4);

    /// <summary>
    ///     是否为国际服客户端
    /// </summary>
    public static bool IsGL => IsGLLazy.Value;

    // 因为生命周期里不会变更, 因此只需要懒加载一次即可
    // 4 - 国服简中 (CHS); 5 - 国服繁中 (CHT, 很神奇, 但确实是有)
    private static readonly Lazy<bool> IsCNLazy =
        new(() => Framework.Instance()->ClientLanguage is 4 or 5);

    /// <summary>
    ///     是否为国服客户端
    /// </summary>
    public static bool IsCN => IsCNLazy.Value;

    // 因为生命周期里不会变更, 因此只需要懒加载一次即可
    private static readonly Lazy<bool> IsTCLazy =
        new(() => Framework.Instance()->ClientLanguage == 7);

    /// <summary>
    ///     是否为繁中客户端
    /// </summary>
    public static bool IsTC => IsTCLazy.Value;

    // 因为生命周期里不会变更, 因此只需要懒加载一次即可
    private static readonly Lazy<bool> IsKRLazy =
        new(() => Framework.Instance()->ClientLanguage == 6);

    /// <summary>
    ///     是否为韩服客户端
    /// </summary>
    public static bool IsKR => IsKRLazy.Value;

    /// <summary>
    ///     进入临危受命范围时
    /// </summary>
    public event Action<uint>? EnterFate;

    /// <summary>
    ///     登录且玩家可用时
    /// </summary>
    public event Action? Login;

    /// <summary>
    ///     登出时
    /// </summary>
    public event Action? Logout;

    /// <summary>
    ///     是否已经安全登录且玩家可用
    /// </summary>
    public static bool IsLoggedIn
    {
        get
        {
            var agent = AgentLobby.Instance();
            return agent != null                &&
                   agent->IsLoggedIn            &&
                   agent->IsLoggedIntoZone      &&
                   agent->LobbyUpdateStage == 1 &&
                   agent->LobbyUIStage     == 1;
        }
    }

    /// <summary>
    ///     地图标点是否已经设置
    /// </summary>
    public static bool IsFlagMarkerSet =>
        AgentMap.Instance()->FlagMarkerCount > 0;

    /// <summary>
    ///     地图标点位置, 若未设置则返回 default(Vector2)
    /// </summary>
    public static Vector2 FlagMarkerPosition =>
        IsFlagMarkerSet ? new(AgentMap.Instance()->FlagMapMarkers[0].XFloat, AgentMap.Instance()->FlagMapMarkers[0].YFloat) : default;

    /// <summary>
    ///     地图标点, 若未设置则返回 default(FlagMapMarker)
    /// </summary>
    public static FlagMapMarker FlagMarker =>
        IsFlagMarkerSet ? AgentMap.Instance()->FlagMapMarkers[0] : default;

    /// <summary>
    ///     当前游戏 Delta Time
    /// </summary>
    public static float DeltaTime =>
        Framework.Instance()->FrameDeltaTime;

    /// <summary>
    ///     当前 Data Center
    /// </summary>
    public static uint CurrentDataCenter =>
        CurrentWorldData.DataCenter.ValueNullable?.RowId ?? 0;

    /// <summary>
    ///     当前 Data Center 表数据
    /// </summary>
    public static WorldDCGroupType CurrentDataCenterData =>
        CurrentWorldData.DataCenter.ValueNullable.GetValueOrDefault();

    /// <summary>
    ///     原始 Data Center
    /// </summary>
    public static uint HomeDataCenter =>
        HomeWorldData.DataCenter.ValueNullable?.RowId ?? 0;

    /// <summary>
    ///     原始 Data Center 表数据
    /// </summary>
    public static WorldDCGroupType HomeDataCenterData =>
        HomeWorldData.DataCenter.ValueNullable.GetValueOrDefault();

    /// <summary>
    ///     原始 World
    /// </summary>
    public static uint HomeWorld =>
        DService.Instance().ClientState.IsLoggedIn ? (uint)AgentLobby.Instance()->LobbyData.HomeWorldId : 0;

    /// <summary>
    ///     原始 World 表数据
    /// </summary>
    public static World HomeWorldData =>
        LuminaGetter.GetRow<World>(HomeWorld).GetValueOrDefault();

    /// <summary>
    ///     当前 World
    /// </summary>
    public static uint CurrentWorld =>
        DService.Instance().ClientState.IsLoggedIn ? (uint)AgentLobby.Instance()->LobbyData.CurrentWorldId : 0;

    /// <summary>
    ///     当前 World 表数据
    /// </summary>
    public static World CurrentWorldData =>
        LuminaGetter.GetRow<World>(CurrentWorld).GetValueOrDefault();

    /// <summary>
    ///     诛灭战限时奖励提升中 (必须在副本外)
    /// </summary>
    public static bool IsChaoticRaidBonusActive =>
        !IsInInstanceArea && UIState.Instance()->InstanceContent.IsLimitedTimeBonusActive;

    /// <summary>
    ///     当前帧率
    /// </summary>
    public static float FrameRate =>
        Framework.Instance()->FrameRate;

    /// <summary>
    ///     当前服务器时间
    /// </summary>
    public static DateTime ServerTime =>
        DateTimeOffset.FromUnixTimeSeconds(ServerTimeUnix).LocalDateTime;

    /// <summary>
    ///     当前服务器时间 Unix 时间戳 (秒级)
    /// </summary>
    public static long ServerTimeUnix =>
        Framework.GetServerTime();

    /// <summary>
    ///     是否处于 PVP 副本中
    /// </summary>
    public static bool IsInPVPInstance =>
        GameMain.IsInPvPInstance();

    /// <summary>
    ///     是否处于 PVP 区域
    /// </summary>
    public static bool IsInPVPArea =>
        GameMain.IsInPvPArea();

    /// <summary>
    ///     是否处于副本区域
    /// </summary>
    public static bool IsInInstanceArea =>
        GameMain.Instance()->IsInInstanceArea();

    /// <summary>
    ///     是否处于观景视角中
    /// </summary>
    public static bool IsInIdleCam =>
        GameMain.IsInIdleCam();

    /// <summary>
    ///     当前区域数据是否加载完毕
    /// </summary>
    public static bool IsTerritoryLoaded =>
        TerritoryLoadState == 2;

    /// <summary>
    ///     当前 TerritoryLoadState, 2 为 加载完毕
    /// </summary>
    public static uint TerritoryLoadState =>
        GameMain.Instance()->TerritoryLoadState;

    /// <summary>
    ///     当前 TerritoryFilterKey
    /// </summary>
    public static uint TerritoryFilterKey =>
        GameMain.Instance()->CurrentTerritoryFilterKey;

    /// <summary>
    ///     当前 Map ID
    /// </summary>
    public static uint Map
    {
        get
        {
            var mapID = AgentMap.Instance()->CurrentMapId;
            if (mapID == 0)
                mapID = GameMain.Instance()->CurrentMapId;

            return mapID;
        }
    }

    /// <summary>
    ///     当前 Map 表数据
    /// </summary>
    public static Map MapData =>
        LuminaGetter.GetRow<Map>(Map).GetValueOrDefault();

    /// <summary>
    ///     当前 TerritoryType ID
    /// </summary>
    public static uint TerritoryType =>
        AgentMap.Instance()->CurrentTerritoryId;

    /// <summary>
    ///     当前 TerritoryType 表数据
    /// </summary>
    public static TerritoryType TerritoryTypeData =>
        LuminaGetter.GetRow<TerritoryType>(TerritoryType).GetValueOrDefault();

    /// <summary>
    ///     当前 TerritoryIntendedUse ID
    /// </summary>
    public static TerritoryIntendedUse TerritoryIntendedUse =>
        GameMain.Instance()->CurrentTerritoryIntendedUseId;

    /// <summary>
    ///     当前 TerritoryIntendedUse 表数据
    /// </summary>
    public static Lumina.Excel.Sheets.TerritoryIntendedUse TerritoryIntendedUseData =>
        LuminaGetter.GetRow<Lumina.Excel.Sheets.TerritoryIntendedUse>((uint)TerritoryIntendedUse).GetValueOrDefault();

    /// <summary>
    ///     当前 ContentFinderCondition ID
    /// </summary>
    public static uint ContentFinderCondition =>
        GameMain.Instance()->CurrentContentFinderConditionId;

    /// <summary>
    ///     当前 ContentFinderCondition 表数据
    /// </summary>
    public static ContentFinderCondition ContentFinderConditionData =>
        LuminaGetter.GetRow<ContentFinderCondition>(ContentFinderCondition).GetValueOrDefault();

    /// <summary>
    ///     当前 Weather 表数据
    /// </summary>
    public static Weather WeatherData =>
        LuminaGetter.GetRow<Weather>(Weather).GetValueOrDefault();

    /// <summary>
    ///     当前天气 ID
    /// </summary>
    public static uint Weather =>
        WeatherManager.Instance()->GetCurrentWeather();
}
