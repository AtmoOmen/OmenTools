using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
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

    private static readonly CompSig InstanceContentDirectorDutyStartedFlagSig =
        new("80 A7 ?? ?? ?? ?? ?? 48 8B CF E8 ?? ?? ?? ?? 0F B7 47");
    private static readonly CompSig InstanceContentDirectorDutyCompletedFlagSig =
        new("80 89 ?? ?? ?? ?? ?? 33 D2 48 8B D9");

    private static nint InstanceContentDirectorDutyStartedOffset;
    private static byte InstanceContentDirectorDutyStartedFlag;
    private static nint InstanceContentDirectorDutyCompletedOffset;
    private static byte InstanceContentDirectorDutyCompletedFlag;

    private static readonly CompSig FateDirectorSetupSig = new("E8 ?? ?? ?? ?? 48 39 37");
    private delegate nint FateDirectorSetupDelegate
    (
        uint rowID,
        nint a2,
        nint a3
    );
    private Hook<FateDirectorSetupDelegate>? FateDirectorSetupHook;

    private Hook<InfoProxyItemSearch.Delegates.ProcessRequestResult>? ProcessRequestResultHook;

    private Hook<WarpInfo.Delegates.CompleteWarp>? CompleteWarpHook;

    private TaskHelper taskHelper = null!;

    private uint worldID;

    protected override void Init()
    {
        var startedFlagAddress   = InstanceContentDirectorDutyStartedFlagSig.ScanText();
        var completedFlagAddress = InstanceContentDirectorDutyCompletedFlagSig.ScanText();

        InstanceContentDirectorDutyStartedOffset   = Marshal.ReadInt32(startedFlagAddress   + 2);
        InstanceContentDirectorDutyStartedFlag     = (byte)~Marshal.ReadByte(startedFlagAddress + 6);
        InstanceContentDirectorDutyCompletedOffset = Marshal.ReadInt32(completedFlagAddress + 2);
        InstanceContentDirectorDutyCompletedFlag   = Marshal.ReadByte(completedFlagAddress  + 6);

        taskHelper = new() { TimeoutMS = int.MaxValue };

        IClientState.Instance().Logout += OnDalamudLogout;

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

        CompleteWarpHook = IGameInteropProvider.Instance().HookFromMemberFunction
        (
            typeof(WarpInfo.MemberFunctionPointers),
            "CompleteWarp",
            (WarpInfo.Delegates.CompleteWarp)CompleteWarpDetour
        );
        CompleteWarpHook.Enable();
    }

    protected override void Uninit()
    {
        FrameworkManager.Instance().Unreg(OnUpdate);

        IClientState.Instance().Logout -= OnDalamudLogout;

        taskHelper.Dispose();
        taskHelper = null;

        FateDirectorSetupHook?.Dispose();
        FateDirectorSetupHook = null;

        ProcessRequestResultHook?.Dispose();
        ProcessRequestResultHook = null;

        CompleteWarpHook?.Dispose();
        CompleteWarpHook = null;
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
            !ICondition.Instance()[ConditionFlag.OnFreeTrial]                  &&
            info->SearchItemId != 0                                            &&
            LuminaGetter.TryGetRow<Item>(info->SearchItemId, out var itemData) &&
            itemData.ItemSearchCategory.RowId > 0)
        {
            DLog.Warning($"[GameState] 市场交易板数据请求被服务器拒绝，错误码：0x{errorCode:X}。");

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

    private void CompleteWarpDetour
    (
        WarpInfo* instance,
        int       eventParam,
        int       eventID
    )
    {
        var warpType = instance->WarpType;

        CompleteWarpHook.Original(instance, eventParam, eventID);

        if (warpType != WarpType.None)
            WarpComplete?.Invoke(instance->WarpType);
        if (warpType == WarpType.Login)
            Login?.Invoke();
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

    private static bool IsInstanceContentDirectorFlagSet
    (
        nint flagByteOffset,
        byte flag
    )
    {
        var framework = EventFramework.Instance();
        if (framework == null) return false;

        var director = framework->GetInstanceContentDirector();

        return director != null && (*((byte*)director + flagByteOffset) & flag) != 0;
    }
}
