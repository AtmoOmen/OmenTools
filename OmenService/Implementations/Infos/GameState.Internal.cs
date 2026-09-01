using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;
using OmenTools.Dalamud;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Interop.Game.Models;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.OmenService;

public unsafe partial class GameState
{
    private static readonly CompSig ContentReplyManagerSig =
        new("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 45 33 C0 48 8D 57 ?? 41 8B CE E8 ?? ?? ?? ?? 48 8D 8F");
    private static readonly CompSig ZoneServerIDOffsetSig =
        new
        (
            "0F 11 83 ?? ?? ?? ?? 0F 10 4F ?? 0F 11 8B ?? ?? ?? ?? 0F 10 47 ?? 0F 11 83 ?? ?? ?? ?? 0F 10 4F ?? 0F 11 8B ?? ?? ?? ?? 0F 10 47 ?? 0F 11 83 ?? ?? ?? ?? 0F 10 4F ?? 0F 11 8B ?? ?? ?? ?? 0F 10 47 ?? 0F 11 83 ?? ?? ?? ?? 0F 10 4F"
        );
    private static readonly nint ContentReplyManagerPtr = ContentReplyManagerSig.GetStatic();
    private static readonly nint ZoneServerIDOffset     = ZoneServerIDOffsetSig.GetStatic();

    private static readonly CompSig                          FateDirectorSetupSig = new("E8 ?? ?? ?? ?? 48 39 37");
    private delegate        nint                             FateDirectorSetupDelegate(uint rowID, nint a2, nint a3);
    private                 Hook<FateDirectorSetupDelegate>? FateDirectorSetupHook;
    
    private Hook<InfoProxyItemSearch.Delegates.ProcessRequestResult>? ProcessRequestResultHook;

    private TaskHelper taskHelper = null!;

    private uint worldID;

    protected override void Init()
    {
        taskHelper = new() { TimeoutMS = int.MaxValue };

        DService.Instance().ClientState.Login  += OnDalamudLogin;
        DService.Instance().ClientState.Logout += OnDalamudLogout;

        if (IsLoggedIn)
            worldID = CurrentWorld;
        FrameworkManager.Instance().Reg(OnUpdate);

        FateDirectorSetupHook = FateDirectorSetupSig.GetHook<FateDirectorSetupDelegate>(FateDirectorSetupDetour);
        FateDirectorSetupHook.Enable();
        
        ProcessRequestResultHook = IGameInteropProvider.Instance().HookFromMemberFunction
        (
            typeof(InfoProxyItemSearch.MemberFunctionPointers),
            "ProcessRequestResult",
            (InfoProxyItemSearch.Delegates.ProcessRequestResult)ProcessRequestResultDetour
        );
        ProcessRequestResultHook.Enable();
    }

    protected override void Uninit()
    {
        FrameworkManager.Instance().Unreg(OnUpdate);
        
        DService.Instance().ClientState.Login  -= OnDalamudLogin;
        DService.Instance().ClientState.Logout -= OnDalamudLogout;

        taskHelper.Dispose();
        taskHelper = null;

        FateDirectorSetupHook?.Dispose();
        FateDirectorSetupHook = null;
        
        ProcessRequestResultHook?.Dispose();
        ProcessRequestResultHook = null;
    }

    private void ProcessRequestResultDetour
    (
        InfoProxyItemSearch* info,
        byte                 resultCount,
        int                  errorCode
    )
    {
        ProcessRequestResultHook.Original(info, resultCount, errorCode);
        
        if (resultCount            == 0                                        &&
            errorCode              > 0                                         &&
            ContentFinderCondition == 0                                        &&
            info->SearchItemId     != 0                                        &&
            LuminaGetter.TryGetRow<Item>(info->SearchItemId, out var itemData) &&
            itemData.ItemSearchCategory.RowId > 0)
        {
            DLog.Warning($"[GameState] 市场交易板数据请求被服务器拒绝，错误码：{errorCode}。");
            
            MarketListingsStuck?.Invoke(errorCode);
            IsMarketListingsStuck = true;
            return;
        }

        IsMarketListingsStuck = false;
    }
    
    private void OnUpdate
    (
        IFramework framework
    )
    {
        if (!IsLoggedIn) return;

        if (CurrentWorld != 0 && CurrentWorld != worldID)
        {
            worldID = CurrentWorld;
            WorldChanged?.Invoke(CurrentWorld);
        }
    }

    private void OnDalamudLogin()
    {
        taskHelper.Abort();

        taskHelper.Enqueue(() => IsLoggedIn);
        taskHelper.Enqueue(() => Login?.Invoke());
    }

    private void OnDalamudLogout
    (
        int type,
        int code
    ) =>
        Logout?.Invoke();

    private nint FateDirectorSetupDetour
    (
        uint rowID,
        nint a2,
        nint a3
    )
    {
        var original = FateDirectorSetupHook.Original(rowID, a2, a3);

        if (rowID == 102401 && FateManager.Instance()->CurrentFate != null)
            EnterFate?.Invoke(FateManager.Instance()->CurrentFate->FateId);

        return original;
    }
}
