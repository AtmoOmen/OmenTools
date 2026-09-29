using System.Collections.Concurrent;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Dalamud;
using OmenTools.Dalamud.DataShare.Attributes;
using OmenTools.Interop.Game.Models;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

internal unsafe class AtkEventManager : OmenServiceBase<AtkEventManager>
{
    private readonly ConcurrentDictionary<uint, byte> claimedParamKeys =
        IDalamudPluginInterface.Instance().GetOrCreateData<ConcurrentDictionary<uint, byte>>(PARAM_KEY_TAG, static () => []);
    
    private Hook<AtkUnitBase.Delegates.ReceiveGlobalEvent>? ReceiveGlobalEventHook;
    
    private readonly ConcurrentDictionary<uint, AtkEventWrapper> eventHandlers = [];
    
    protected override void Init()
    {
        ReceiveGlobalEventHook = AtkUnitBase.StaticVirtualTablePointer->HookVFuncFromName
        (
            "ReceiveGlobalEvent",
            (AtkUnitBase.Delegates.ReceiveGlobalEvent)ReceiveGlobalEventDetour
        );
        ReceiveGlobalEventHook.Enable();

        IAddonLifecycle.Instance().RegisterListener(AddonEvent.PreFinalize, OnAddonPreFinalize);
    }

    protected override void Uninit()
    {
        IAddonLifecycle.Instance().UnregisterListener(OnAddonPreFinalize);

        ReceiveGlobalEventHook?.Disable();

        foreach (var (_, atkEvent) in eventHandlers)
            atkEvent.Dispose();

        eventHandlers.Clear();

        ReceiveGlobalEventHook?.Dispose();
    }
    
    internal uint RegisterEvent
    (
        AtkEventWrapper eventWrapper
    )
    {
        var paramKey = ClaimParamKey();

        if (!eventHandlers.TryAdd(paramKey, eventWrapper))
        {
            claimedParamKeys.TryRemove(paramKey, out _);
            throw new Exception($"注册事件失败: {paramKey}");
        }

        return paramKey;
    }

    internal void UnregisterEvent
    (
        uint paramKey
    )
    {
        if (!eventHandlers.TryRemove(paramKey, out _)) return;

        claimedParamKeys.TryRemove(paramKey, out _);
    }

    private uint ClaimParamKey()
    {
        for (var attempt = 0; attempt < PARAM_KEY_MAX_ATTEMPTS; attempt++)
        {
            var paramKey = PARAM_KEY_BASE + (uint)Random.Shared.Next((int)PARAM_KEY_SPACE_SIZE);

            if (claimedParamKeys.TryAdd(paramKey, 0))
                return paramKey;
        }

        throw new Exception($"获取 AtkEvent 参数 Key 失败: {PARAM_KEY_BASE:X} ~ {PARAM_KEY_BASE + PARAM_KEY_SPACE_SIZE:X}");
    }

    private void OnAddonPreFinalize
    (
        AddonEvent type,
        AddonArgs  args
    )
    {
        var addonPtr = args.Addon.Address;

        foreach (var (_, atkEvent) in eventHandlers)
        {
            if (atkEvent.IsBoundToAddon(addonPtr))
                atkEvent.Dispose();
        }
    }

    private void ReceiveGlobalEventDetour
    (
        AtkUnitBase*  addon,
        AtkEventType  eventType,
        int           eventParam,
        AtkEvent*     atkEvent,
        AtkEventData* data
    )
    {
        if (addon    == null ||
            atkEvent == null ||
            data     == null)
            return;

        if (eventHandlers.TryGetValue((uint)eventParam, out var simpleEvent))
        {
            try
            {
                simpleEvent.Action(eventType, addon, atkEvent, data);
                atkEvent->SetEventIsHandled();
                return;
            }
            catch (Exception ex)
            {
                DLog.Error($"尝试触发自定义 AtkEvent 时发生错误, ID: {eventParam}", ex);
            }
        }

        // 因为游戏还会触发回调所以没法
        ReceiveGlobalEventHook?.OriginalDisposeSafe(addon, eventType, eventParam, atkEvent, data);
    }

    #region 常量

    [DataShareTag]
    private const string PARAM_KEY_TAG = "OmenTools.OmenService.AtkEventManager.ParamKey";
    
    private const uint PARAM_KEY_BASE         = 0x38D9B000U;
    private const uint PARAM_KEY_SPACE_SIZE   = 1000000U;
    private const int  PARAM_KEY_MAX_ATTEMPTS = 64;

    #endregion
}
