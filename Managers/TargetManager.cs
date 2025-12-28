using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using OmenTools.Abstracts;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace OmenTools.Managers;

#pragma warning disable CS8629

public sealed unsafe class TargetManager : OmenServiceBase
{
    public static TargetManagerConfig Config { get; private set; } = null!;

    #region Delegates

    public delegate void PreSetHardTargetDelegate(
        ref bool         isPrevented,
        ref IGameObject? target,
        ref bool         ignoreTargetModes,
        ref bool         a4,
        ref int          a5);

    public delegate void PostSetHardTargetDelegate(
        bool         result,
        IGameObject? target,
        bool         ignoreTargetModes,
        bool         a4,
        int          a5);

    public delegate void PreSetSoftTargetDelegate(
        ref bool         isPrevented,
        ref IGameObject? target);

    public delegate void PostSetSoftTargetDelegate(
        bool         result,
        IGameObject? target);

    public delegate void PreSetFocusTargetDelegate(
        ref bool         isPrevented,
        ref GameObjectId gameObjectID);

    public delegate void PostSetFocusTargetDelegate(
        GameObjectId gameObjectID);

    public delegate void PreInteractWithObjectDelegate(
        ref bool         isPrevented,
        ref IGameObject? target,
        ref bool         checkLoS);

    public delegate void PostInteractWithObjectDelegate(
        ulong        result,
        IGameObject? target,
        bool         checkLoS);

    public delegate void PreOpenObjectInteractionDelegate(
        ref bool         isPrevented,
        ref IGameObject? target);

    public delegate void PostOpenObjectInteractionDelegate(
        ulong        result,
        IGameObject? target);

    #endregion

    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool SetHardTargetDelegate(TargetSystem* system, CSGameObject* target, bool ignoreTargetModes, bool a4, int a5);
    private static Hook<SetHardTargetDelegate>? SetHardTargetHook;

    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool SetSoftTargetDelegate(TargetSystem* system, CSGameObject* target);
    private static Hook<SetSoftTargetDelegate>? SetSoftTargetHook;

    private delegate void                          SetFocusTargetDelegate(TargetSystem* system, GameObjectId gameObjectID);
    private static   Hook<SetFocusTargetDelegate>? SetFocusTargetHook;

    private delegate ulong                             InteractWithObjectDelegate(TargetSystem* system, CSGameObject* target, bool checkLoS);
    private static   Hook<InteractWithObjectDelegate>? InteractWithObjectHook;

    private delegate ulong                                OpenObjectInteractionDelegate(TargetSystem* system, CSGameObject* target);
    private static   Hook<OpenObjectInteractionDelegate>? OpenObjectInteractionHook;

    private static readonly ConcurrentDictionary<Type, ConcurrentBag<Delegate>> MethodsCollection = [];

    internal override void Init()
    {
        Config = LoadConfig<TargetManagerConfig>() ?? new();

        SetHardTargetHook ??= DService.Hook.HookFromAddress<SetHardTargetDelegate>(
            GetMemberFuncByName(typeof(TargetSystem.MemberFunctionPointers), "SetHardTarget"),
            SetHardTargetDetour);
        SetHardTargetHook.Enable();

        SetSoftTargetHook ??= DService.Hook.HookFromAddress<SetSoftTargetDelegate>(
            GetMemberFuncByName(typeof(TargetSystem.MemberFunctionPointers), "SetSoftTarget"),
            SetSoftTargetDetour);
        SetSoftTargetHook.Enable();

        SetFocusTargetHook ??= DService.Hook.HookFromAddress<SetFocusTargetDelegate>(
            GetMemberFuncByName(typeof(TargetSystem.MemberFunctionPointers), "SetFocusTargetByObjectId"),
            SetFocusTargetDetour);
        SetFocusTargetHook.Enable();

        InteractWithObjectHook ??= DService.Hook.HookFromAddress<InteractWithObjectDelegate>(
            GetMemberFuncByName(typeof(TargetSystem.MemberFunctionPointers), "InteractWithObject"),
            InteractWithObjectDetour);
        InteractWithObjectHook.Enable();

        OpenObjectInteractionHook ??= DService.Hook.HookFromAddress<OpenObjectInteractionDelegate>(
            GetMemberFuncByName(typeof(TargetSystem.MemberFunctionPointers), "OpenObjectInteraction"),
            OpenObjectInteractionDetour);
        OpenObjectInteractionHook.Enable();
    }

    internal override void Uninit()
    {
        SetHardTargetHook?.Dispose();
        SetHardTargetHook = null;

        SetSoftTargetHook?.Dispose();
        SetSoftTargetHook = null;

        SetFocusTargetHook?.Dispose();
        SetFocusTargetHook = null;

        InteractWithObjectHook?.Dispose();
        InteractWithObjectHook = null;

        OpenObjectInteractionHook?.Dispose();
        OpenObjectInteractionHook = null;

        MethodsCollection.Clear();
    }

    private static bool RegisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        var bag  = MethodsCollection.GetOrAdd(type, _ => []);
        foreach (var method in methods)
            bag.Add(method);

        return true;
    }

    private static bool UnregisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        if (MethodsCollection.TryGetValue(type, out var bag))
        {
            foreach (var method in methods)
            {
                var newBag = new ConcurrentBag<Delegate>(bag.Where(d => d != method));
                MethodsCollection[type] = newBag;
            }

            return true;
        }

        return false;
    }

    #region Register

    public static bool RegPreSetHardTarget(params PreSetHardTargetDelegate[] methods) => RegisterGeneric(methods);

    public static bool RegPostSetHardTarget(params PostSetHardTargetDelegate[] methods) => RegisterGeneric(methods);

    public static bool RegPreSetSoftTarget(params PreSetSoftTargetDelegate[] methods) => RegisterGeneric(methods);

    public static bool RegPostSetSoftTarget(params PostSetSoftTargetDelegate[] methods) => RegisterGeneric(methods);

    public static bool RegPreSetFocusTarget(params PreSetFocusTargetDelegate[] methods) => RegisterGeneric(methods);

    public static bool RegPostSetFocusTarget(params PostSetFocusTargetDelegate[] methods) => RegisterGeneric(methods);

    public static bool RegPreInteractWithObject(params PreInteractWithObjectDelegate[] methods) => RegisterGeneric(methods);

    public static bool RegPostInteractWithObject(params PostInteractWithObjectDelegate[] methods) => RegisterGeneric(methods);

    public static bool RegPreOpenObjectInteraction(params PreOpenObjectInteractionDelegate[] methods) => RegisterGeneric(methods);

    public static bool RegPostOpenObjectInteraction(params PostOpenObjectInteractionDelegate[] methods) => RegisterGeneric(methods);

    public static bool Unreg(params PreSetHardTargetDelegate[] methods) => UnregisterGeneric(methods);

    public static bool Unreg(params PostSetHardTargetDelegate[] methods) => UnregisterGeneric(methods);

    public static bool Unreg(params PreSetSoftTargetDelegate[] methods) => UnregisterGeneric(methods);

    public static bool Unreg(params PostSetSoftTargetDelegate[] methods) => UnregisterGeneric(methods);

    public static bool Unreg(params PreSetFocusTargetDelegate[] methods) => UnregisterGeneric(methods);

    public static bool Unreg(params PostSetFocusTargetDelegate[] methods) => UnregisterGeneric(methods);

    public static bool Unreg(params PreInteractWithObjectDelegate[] methods) => UnregisterGeneric(methods);

    public static bool Unreg(params PostInteractWithObjectDelegate[] methods) => UnregisterGeneric(methods);

    public static bool Unreg(params PreOpenObjectInteractionDelegate[] methods) => UnregisterGeneric(methods);

    public static bool Unreg(params PostOpenObjectInteractionDelegate[] methods) => UnregisterGeneric(methods);

    #endregion

    #region Hooks

    private static bool SetHardTargetDetour(TargetSystem* system, CSGameObject* target, bool ignoreTargetModes, bool a4, int a5)
    {
        var gameObj = DService.ObjectTable.CreateObjectReference((nint)target);

        if (Config.ShowSetHardTargetLog)
            Debug($"[Target Manager] Set Hard Target\n"                                                                          +
                  $"目标: {gameObj?.Name ?? "[空]"} ({gameObj?.Address:X}) | 忽略目标选择模式: {ignoreTargetModes} | a4: {a4} | a5: {a5}\n" +
                  $"GameObjectID: {gameObj?.GameObjectID} | EntityID: {gameObj?.EntityID}");

        var isPrevented          = false;
        var targetRef            = gameObj;
        var ignoreTargetModesRef = ignoreTargetModes;
        var a4Ref                = a4;
        var a5Ref                = a5;

        if (MethodsCollection.TryGetValue(typeof(PreSetHardTargetDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreSetHardTargetDelegate)preDelegate)(ref isPrevented, ref targetRef, ref ignoreTargetModesRef, ref a4Ref, ref a5Ref);
                if (isPrevented) return false;
            }
        }

        var targetPtr = targetRef == null ? null : (CSGameObject*)targetRef.Address;
        var original  = SetHardTargetHook.Original(system, targetPtr, ignoreTargetModesRef, a4Ref, a5Ref);

        if (MethodsCollection.TryGetValue(typeof(PostSetHardTargetDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                ((PostSetHardTargetDelegate)postDelegate)(original, targetRef, ignoreTargetModesRef, a4Ref, a5Ref);
        }

        return original;
    }

    private static bool SetSoftTargetDetour(TargetSystem* system, CSGameObject* target)
    {
        var gameObj = DService.ObjectTable.CreateObjectReference((nint)target);

        if (Config.ShowSetSoftTargetLog)
            Debug($"[Target Manager] Set Soft Target\n"                    +
                  $"目标: {gameObj?.Name ?? "[空]"} ({gameObj?.Address:X})\n" +
                  $"GameObjectID: {gameObj?.GameObjectID} | EntityID: {gameObj?.EntityID}");

        var isPrevented = false;
        var targetRef   = gameObj;

        if (MethodsCollection.TryGetValue(typeof(PreSetSoftTargetDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreSetSoftTargetDelegate)preDelegate)(ref isPrevented, ref targetRef);
                if (isPrevented) return false;
            }
        }

        var targetPtr = targetRef == null ? null : (CSGameObject*)targetRef.Address;
        var original  = SetSoftTargetHook.Original(system, targetPtr);

        if (MethodsCollection.TryGetValue(typeof(PostSetSoftTargetDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                ((PostSetSoftTargetDelegate)postDelegate)(original, targetRef);
        }

        return original;
    }

    private static void SetFocusTargetDetour(TargetSystem* system, GameObjectId gameObjectID)
    {
        if (Config.ShowSetFocusTargetLog)
        {
            var gameObj = DService.ObjectTable.SearchByID(gameObjectID);
            Debug($"[Target Manager] Set Focus Target\n"                   +
                  $"目标: {gameObj?.Name ?? "[空]"} ({gameObj?.Address:X})\n" +
                  $"GameObjectID: {gameObj?.GameObjectID} | EntityID: {gameObj?.EntityID}");
        }

        var isPrevented     = false;
        var gameObjectIDRef = gameObjectID;

        if (MethodsCollection.TryGetValue(typeof(PreSetFocusTargetDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreSetFocusTargetDelegate)preDelegate)(ref isPrevented, ref gameObjectIDRef);
                if (isPrevented) return;
            }
        }

        SetFocusTargetHook.Original(system, gameObjectIDRef);

        if (MethodsCollection.TryGetValue(typeof(PostSetFocusTargetDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                ((PostSetFocusTargetDelegate)postDelegate)(gameObjectIDRef);
        }
    }

    private static ulong InteractWithObjectDetour(TargetSystem* system, CSGameObject* target, bool checkLoS)
    {
        var gameObj = DService.ObjectTable.CreateObjectReference((nint)target);

        if (Config.ShowInteractWithObjectLog)
            Debug($"[Target Manager] Interact With Object\n"                                    +
                  $"目标: {gameObj?.Name ?? "[空]"} ({gameObj?.Address:X}) | 检查视线范围: {checkLoS}\n" +
                  $"GameObjectID: {gameObj?.GameObjectID} | EntityID: {gameObj?.EntityID}");

        var isPrevented = false;
        var targetRef   = gameObj;
        var checkLoSRef = checkLoS;

        if (MethodsCollection.TryGetValue(typeof(PreInteractWithObjectDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreInteractWithObjectDelegate)preDelegate)(ref isPrevented, ref targetRef, ref checkLoSRef);
                if (isPrevented) return 0;
            }
        }

        var targetPtr = targetRef == null ? null : (CSGameObject*)targetRef.Address;
        var original  = InteractWithObjectHook.Original(system, targetPtr, checkLoSRef);

        if (MethodsCollection.TryGetValue(typeof(PostInteractWithObjectDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                ((PostInteractWithObjectDelegate)postDelegate)(original, targetRef, checkLoSRef);
        }

        return original;
    }

    private static ulong OpenObjectInteractionDetour(TargetSystem* system, CSGameObject* target)
    {
        var gameObj = DService.ObjectTable.CreateObjectReference((nint)target);

        if (Config.ShowOpenObjectInteractionLog)
            Debug($"[Target Manager] Open Object Interaction\n"            +
                  $"目标: {gameObj?.Name ?? "[空]"} ({gameObj?.Address:X})\n" +
                  $"GameObjectID: {gameObj?.GameObjectID} | EntityID: {gameObj?.EntityID}");

        var isPrevented = false;
        var targetRef   = gameObj;

        if (MethodsCollection.TryGetValue(typeof(PreOpenObjectInteractionDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreOpenObjectInteractionDelegate)preDelegate)(ref isPrevented, ref targetRef);
                if (isPrevented) return 0;
            }
        }

        var targetPtr = targetRef == null ? null : (CSGameObject*)targetRef.Address;
        var original  = OpenObjectInteractionHook.Original(system, targetPtr);

        if (MethodsCollection.TryGetValue(typeof(PostOpenObjectInteractionDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                ((PostOpenObjectInteractionDelegate)postDelegate)(original, targetRef);
        }

        return original;
    }

    #endregion

    #region Invokes

    public static bool SetHardTarget(IGameObject? target, bool ignoreTargetModes = false, bool a4 = false, int a5 = 0)
        => SetHardTargetHook.Original(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address, ignoreTargetModes, a4, a5);

    public static bool SetSoftTarget(IGameObject? target)
        => SetSoftTargetHook.Original(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address);

    public static void SetFocusTarget(GameObjectId gameObjectID)
        => SetFocusTargetHook.Original(TargetSystem.Instance(), gameObjectID);

    public static ulong InteractWithObject(IGameObject? target, bool checkLoS = true)
        => InteractWithObjectHook.Original(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address, checkLoS);

    public static ulong OpenObjectInteraction(IGameObject? target)
        => OpenObjectInteractionHook.Original(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address);

    public static bool SetHardTargetCallDetour(IGameObject? target, bool ignoreTargetModes = false, bool a4 = false, int a5 = 0)
        => SetHardTargetDetour(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address, ignoreTargetModes, a4, a5);

    public static bool SetSoftTargetCallDetour(IGameObject? target)
        => SetSoftTargetDetour(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address);

    public static void SetFocusTargetCallDetour(GameObjectId gameObjectID)
        => SetFocusTargetDetour(TargetSystem.Instance(), gameObjectID);

    public static ulong InteractWithObjectCallDetour(IGameObject? target, bool checkLoS = true)
        => InteractWithObjectDetour(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address, checkLoS);

    public static ulong OpenObjectInteractionCallDetour(IGameObject? target)
        => OpenObjectInteractionDetour(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address);

    #endregion

    public static IGameObject? Target
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->GetHardTarget());
        set => Struct->SetHardTarget((CSGameObject*)value?.Address);
    }

    public static IGameObject? MouseOverTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->MouseOverTarget);
        set => Struct->MouseOverTarget = (CSGameObject*)value?.Address;
    }

    public static IGameObject? FocusTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->FocusTarget);
        set => Struct->FocusTarget = (CSGameObject*)value?.Address;
    }

    public static IGameObject? PreviousTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->PreviousTarget);
        set => Struct->PreviousTarget = (CSGameObject*)value?.Address;
    }

    public static IGameObject? SoftTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->GetSoftTarget());
        set => Struct->SetSoftTarget((CSGameObject*)value?.Address);
    }

    public static IGameObject? GPoseTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->GPoseTarget);
        set => Struct->GPoseTarget = (CSGameObject*)value?.Address;
    }

    public static IGameObject? MouseOverNameplateTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->MouseOverNameplateTarget);
        set => Struct->MouseOverNameplateTarget = (CSGameObject*)value?.Address;
    }

    private static TargetSystem* Struct => TargetSystem.Instance();

    public class TargetManagerConfig : OmenServiceConfiguration
    {
        public bool ShowSetHardTargetLog;
        public bool ShowSetSoftTargetLog;
        public bool ShowSetFocusTargetLog;
        public bool ShowInteractWithObjectLog;
        public bool ShowOpenObjectInteractionLog;

        public void Save() =>
            this.Save(DService.GetOmenService<TargetManager>());
    }
}
