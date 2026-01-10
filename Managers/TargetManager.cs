using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using OmenTools.Abstracts;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace OmenTools.Managers;

#pragma warning disable CS8629

public sealed unsafe class TargetManager : OmenServiceBase<TargetManager>
{
    public TargetManagerConfig Config { get; private set; } = null!;

    #region Delegates

    public delegate void PreSetHardTargetDelegate
    (
        ref bool         isPrevented,
        ref IGameObject? target,
        ref bool         ignoreTargetModes,
        ref bool         a4,
        ref int          a5
    );

    public delegate void PostSetHardTargetDelegate
    (
        bool         result,
        IGameObject? target,
        bool         ignoreTargetModes,
        bool         a4,
        int          a5
    );

    public delegate void PreSetSoftTargetDelegate
    (
        ref bool         isPrevented,
        ref IGameObject? target
    );

    public delegate void PostSetSoftTargetDelegate
    (
        bool         result,
        IGameObject? target
    );

    public delegate void PreSetFocusTargetDelegate
    (
        ref bool         isPrevented,
        ref GameObjectId gameObjectID
    );

    public delegate void PostSetFocusTargetDelegate
    (
        GameObjectId gameObjectID
    );

    public delegate void PreInteractWithObjectDelegate
    (
        ref bool         isPrevented,
        ref IGameObject? target,
        ref bool         checkLoS
    );

    public delegate void PostInteractWithObjectDelegate
    (
        ulong        result,
        IGameObject? target,
        bool         checkLoS
    );

    public delegate void PreOpenObjectInteractionDelegate
    (
        ref bool         isPrevented,
        ref IGameObject? target
    );

    public delegate void PostOpenObjectInteractionDelegate
    (
        ulong        result,
        IGameObject? target
    );

    #endregion

    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool SetHardTargetDelegate(TargetSystem* system, CSGameObject* target, bool ignoreTargetModes, bool a4, int a5);
    private Hook<SetHardTargetDelegate>? SetHardTargetHook;

    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool SetSoftTargetDelegate(TargetSystem* system, CSGameObject* target);
    private Hook<SetSoftTargetDelegate>? SetSoftTargetHook;

    private delegate void SetFocusTargetDelegate(TargetSystem* system, GameObjectId gameObjectID);
    private Hook<SetFocusTargetDelegate>? SetFocusTargetHook;

    private delegate ulong InteractWithObjectDelegate(TargetSystem* system, CSGameObject* target, bool checkLoS);
    private Hook<InteractWithObjectDelegate>? InteractWithObjectHook;

    private delegate ulong OpenObjectInteractionDelegate(TargetSystem* system, CSGameObject* target);
    private Hook<OpenObjectInteractionDelegate>? OpenObjectInteractionHook;

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> methodsCollection = [];

    internal override void Init()
    {
        Config = LoadConfig<TargetManagerConfig>() ?? new();

        SetHardTargetHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(TargetSystem.MemberFunctionPointers),
            "SetHardTarget",
            (SetHardTargetDelegate)SetHardTargetDetour
        );
        SetHardTargetHook.Enable();

        SetSoftTargetHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(TargetSystem.MemberFunctionPointers),
            "SetSoftTarget",
            (SetSoftTargetDelegate)SetSoftTargetDetour
        );
        SetSoftTargetHook.Enable();

        SetFocusTargetHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(TargetSystem.MemberFunctionPointers),
            "SetFocusTargetByObjectId",
            (SetFocusTargetDelegate)SetFocusTargetDetour
        );
        SetFocusTargetHook.Enable();

        InteractWithObjectHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(TargetSystem.MemberFunctionPointers),
            "InteractWithObject",
            (InteractWithObjectDelegate)InteractWithObjectDetour
        );
        InteractWithObjectHook.Enable();

        OpenObjectInteractionHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(TargetSystem.MemberFunctionPointers),
            "OpenObjectInteraction",
            (OpenObjectInteractionDelegate)OpenObjectInteractionDetour
        );
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

        methodsCollection.Clear();
    }

    private bool RegisterGeneric<T>(T method, params T[] methods) where T : Delegate
    {
        var type = typeof(T);

        methodsCollection.AddOrUpdate
        (
            type,
            _ =>
            {
                var list = ImmutableList.Create<Delegate>(method);
                return methods.Length > 0 ? list.AddRange(methods) : list;
            },
            (_, currentList) =>
            {
                var newList = currentList.Add(method);
                return methods.Length > 0 ? newList.AddRange(methods) : newList;
            }
        );

        return true;
    }

    private bool UnregisterGeneric<T>(params T[] methods) where T : Delegate
    {
        if (methods is not { Length: > 0 }) return false;

        var type = typeof(T);

        while (methodsCollection.TryGetValue(type, out var currentList))
        {
            var newList = currentList.RemoveRange(methods);

            if (newList == currentList)
                return false;

            if (newList.IsEmpty)
            {
                var kvp = new KeyValuePair<Type, ImmutableList<Delegate>>(type, currentList);
                if (((ICollection<KeyValuePair<Type, ImmutableList<Delegate>>>)methodsCollection).Remove(kvp))
                    return true;
            }
            else
            {
                if (methodsCollection.TryUpdate(type, newList, currentList))
                    return true;
            }
        }

        return false;
    }

    #region Register

    public bool RegPreSetHardTarget(PreSetHardTargetDelegate method, params PreSetHardTargetDelegate[] methods) => RegisterGeneric(method, methods);

    public bool RegPostSetHardTarget(PostSetHardTargetDelegate method, params PostSetHardTargetDelegate[] methods) => RegisterGeneric(method, methods);

    public bool RegPreSetSoftTarget(PreSetSoftTargetDelegate method, params PreSetSoftTargetDelegate[] methods) => RegisterGeneric(method, methods);

    public bool RegPostSetSoftTarget(PostSetSoftTargetDelegate method, params PostSetSoftTargetDelegate[] methods) => RegisterGeneric(method, methods);

    public bool RegPreSetFocusTarget(PreSetFocusTargetDelegate method, params PreSetFocusTargetDelegate[] methods) => RegisterGeneric(method, methods);

    public bool RegPostSetFocusTarget(PostSetFocusTargetDelegate method, params PostSetFocusTargetDelegate[] methods) => RegisterGeneric(method, methods);

    public bool RegPreInteractWithObject(PreInteractWithObjectDelegate method, params PreInteractWithObjectDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool RegPostInteractWithObject(PostInteractWithObjectDelegate method, params PostInteractWithObjectDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool RegPreOpenObjectInteraction(PreOpenObjectInteractionDelegate method, params PreOpenObjectInteractionDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool RegPostOpenObjectInteraction(PostOpenObjectInteractionDelegate method, params PostOpenObjectInteractionDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool Unreg(params PreSetHardTargetDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostSetHardTargetDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PreSetSoftTargetDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostSetSoftTargetDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PreSetFocusTargetDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostSetFocusTargetDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PreInteractWithObjectDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostInteractWithObjectDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PreOpenObjectInteractionDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostOpenObjectInteractionDelegate[] methods) => UnregisterGeneric(methods);

    #endregion

    #region Hooks

    private bool SetHardTargetDetour(TargetSystem* system, CSGameObject* target, bool ignoreTargetModes, bool a4, int a5)
    {
        var gameObj = DService.Instance().ObjectTable.CreateObjectReference((nint)target);

        if (Config.ShowSetHardTargetLog)
            Debug
            (
                $"[Target Manager] Set Hard Target\n"                                                                          +
                $"目标: {gameObj?.Name ?? "[空]"} ({gameObj?.Address:X}) | 忽略目标选择模式: {ignoreTargetModes} | a4: {a4} | a5: {a5}\n" +
                $"GameObjectID: {gameObj?.GameObjectID} | EntityID: {gameObj?.EntityID}"
            );

        var isPrevented          = false;
        var targetRef            = gameObj;
        var ignoreTargetModesRef = ignoreTargetModes;
        var a4Ref                = a4;
        var a5Ref                = a5;

        if (methodsCollection.TryGetValue(typeof(PreSetHardTargetDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreSetHardTargetDelegate)preDelegate)(ref isPrevented, ref targetRef, ref ignoreTargetModesRef, ref a4Ref, ref a5Ref);
                if (isPrevented) return false;
            }
        }

        var targetPtr = targetRef == null ? null : (CSGameObject*)targetRef.Address;
        var original  = SetHardTargetHook.Original(system, targetPtr, ignoreTargetModesRef, a4Ref, a5Ref);

        if (methodsCollection.TryGetValue(typeof(PostSetHardTargetDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                ((PostSetHardTargetDelegate)postDelegate)(original, targetRef, ignoreTargetModesRef, a4Ref, a5Ref);
        }

        return original;
    }

    private bool SetSoftTargetDetour(TargetSystem* system, CSGameObject* target)
    {
        var gameObj = DService.Instance().ObjectTable.CreateObjectReference((nint)target);

        if (Config.ShowSetSoftTargetLog)
            Debug
            (
                $"[Target Manager] Set Soft Target\n"                    +
                $"目标: {gameObj?.Name ?? "[空]"} ({gameObj?.Address:X})\n" +
                $"GameObjectID: {gameObj?.GameObjectID} | EntityID: {gameObj?.EntityID}"
            );

        var isPrevented = false;
        var targetRef   = gameObj;

        if (methodsCollection.TryGetValue(typeof(PreSetSoftTargetDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreSetSoftTargetDelegate)preDelegate)(ref isPrevented, ref targetRef);
                if (isPrevented) return false;
            }
        }

        var targetPtr = targetRef == null ? null : (CSGameObject*)targetRef.Address;
        var original  = SetSoftTargetHook.Original(system, targetPtr);

        if (methodsCollection.TryGetValue(typeof(PostSetSoftTargetDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                ((PostSetSoftTargetDelegate)postDelegate)(original, targetRef);
        }

        return original;
    }

    private void SetFocusTargetDetour(TargetSystem* system, GameObjectId gameObjectID)
    {
        if (Config.ShowSetFocusTargetLog)
        {
            var gameObj = DService.Instance().ObjectTable.SearchByID(gameObjectID);
            Debug
            (
                $"[Target Manager] Set Focus Target\n"                                                           +
                $"目标: {gameObj?.Name ?? "[空]"} ({gameObj?.Address:X}) | GameObjectID: {(ulong)gameObjectID:X}\n" +
                $"GameObjectID: {gameObj?.GameObjectID} | EntityID: {gameObj?.EntityID}"
            );
        }

        var isPrevented     = false;
        var gameObjectIDRef = gameObjectID;

        if (methodsCollection.TryGetValue(typeof(PreSetFocusTargetDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreSetFocusTargetDelegate)preDelegate)(ref isPrevented, ref gameObjectIDRef);
                if (isPrevented) return;
            }
        }

        SetFocusTargetHook.Original(system, gameObjectIDRef);

        if (methodsCollection.TryGetValue(typeof(PostSetFocusTargetDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                ((PostSetFocusTargetDelegate)postDelegate)(gameObjectIDRef);
        }
    }

    private ulong InteractWithObjectDetour(TargetSystem* system, CSGameObject* target, bool checkLoS)
    {
        var gameObj = DService.Instance().ObjectTable.CreateObjectReference((nint)target);

        if (Config.ShowInteractWithObjectLog)
            Debug
            (
                $"[Target Manager] Interact With Object\n"                                    +
                $"目标: {gameObj?.Name ?? "[空]"} ({gameObj?.Address:X}) | 检查视线范围: {checkLoS}\n" +
                $"GameObjectID: {gameObj?.GameObjectID} | EntityID: {gameObj?.EntityID}"
            );

        var isPrevented = false;
        var targetRef   = gameObj;
        var checkLoSRef = checkLoS;

        if (methodsCollection.TryGetValue(typeof(PreInteractWithObjectDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreInteractWithObjectDelegate)preDelegate)(ref isPrevented, ref targetRef, ref checkLoSRef);
                if (isPrevented) return 0;
            }
        }

        var targetPtr = targetRef == null ? null : (CSGameObject*)targetRef.Address;
        var original  = InteractWithObjectHook.Original(system, targetPtr, checkLoSRef);

        if (methodsCollection.TryGetValue(typeof(PostInteractWithObjectDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                ((PostInteractWithObjectDelegate)postDelegate)(original, targetRef, checkLoSRef);
        }

        return original;
    }

    private ulong OpenObjectInteractionDetour(TargetSystem* system, CSGameObject* target)
    {
        var gameObj = DService.Instance().ObjectTable.CreateObjectReference((nint)target);

        if (Config.ShowOpenObjectInteractionLog)
            Debug
            (
                $"[Target Manager] Open Object Interaction\n"            +
                $"目标: {gameObj?.Name ?? "[空]"} ({gameObj?.Address:X})\n" +
                $"GameObjectID: {gameObj?.GameObjectID} | EntityID: {gameObj?.EntityID}"
            );

        var isPrevented = false;
        var targetRef   = gameObj;

        if (methodsCollection.TryGetValue(typeof(PreOpenObjectInteractionDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreOpenObjectInteractionDelegate)preDelegate)(ref isPrevented, ref targetRef);
                if (isPrevented) return 0;
            }
        }

        var targetPtr = targetRef == null ? null : (CSGameObject*)targetRef.Address;
        var original  = OpenObjectInteractionHook.Original(system, targetPtr);

        if (methodsCollection.TryGetValue(typeof(PostOpenObjectInteractionDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                ((PostOpenObjectInteractionDelegate)postDelegate)(original, targetRef);
        }

        return original;
    }

    #endregion

    #region Invokes

    public bool SetHardTarget(IGameObject? target, bool ignoreTargetModes = false, bool a4 = false, int a5 = 0)
        => SetHardTargetHook.Original(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address, ignoreTargetModes, a4, a5);

    public bool SetSoftTarget(IGameObject? target)
        => SetSoftTargetHook.Original(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address);

    public void SetFocusTarget(GameObjectId gameObjectID)
        => SetFocusTargetHook.Original(TargetSystem.Instance(), gameObjectID);

    public ulong InteractWithObject(IGameObject? target, bool checkLoS = true)
        => InteractWithObjectHook.Original(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address, checkLoS);

    public ulong OpenObjectInteraction(IGameObject? target)
        => OpenObjectInteractionHook.Original(TargetSystem.Instance(), target == null ? null : (CSGameObject*)target.Address);

    #endregion

    public static IGameObject? Target
    {
        get => DService.Instance().ObjectTable.CreateObjectReference((nint)ToStruct()->GetHardTarget());
        set => ToStruct()->SetHardTarget((CSGameObject*)value?.Address);
    }

    public static IGameObject? MouseOverTarget
    {
        get => DService.Instance().ObjectTable.CreateObjectReference((nint)ToStruct()->MouseOverTarget);
        set => ToStruct()->MouseOverTarget = (CSGameObject*)value?.Address;
    }

    public static IGameObject? FocusTarget
    {
        get => DService.Instance().ObjectTable.CreateObjectReference((nint)ToStruct()->FocusTarget);
        set => ToStruct()->FocusTarget = (CSGameObject*)value?.Address;
    }

    public static IGameObject? PreviousTarget
    {
        get => DService.Instance().ObjectTable.CreateObjectReference((nint)ToStruct()->PreviousTarget);
        set => ToStruct()->PreviousTarget = (CSGameObject*)value?.Address;
    }

    public static IGameObject? SoftTarget
    {
        get => DService.Instance().ObjectTable.CreateObjectReference((nint)ToStruct()->GetSoftTarget());
        set => ToStruct()->SetSoftTarget((CSGameObject*)value?.Address);
    }

    public static IGameObject? GPoseTarget
    {
        get => DService.Instance().ObjectTable.CreateObjectReference((nint)ToStruct()->GPoseTarget);
        set => ToStruct()->GPoseTarget = (CSGameObject*)value?.Address;
    }

    public static IGameObject? MouseOverNameplateTarget
    {
        get => DService.Instance().ObjectTable.CreateObjectReference((nint)ToStruct()->MouseOverNameplateTarget);
        set => ToStruct()->MouseOverNameplateTarget = (CSGameObject*)value?.Address;
    }

    public static TargetSystem* ToStruct() => TargetSystem.Instance();

    public class TargetManagerConfig : OmenServiceConfiguration
    {
        public bool ShowSetHardTargetLog;
        public bool ShowSetSoftTargetLog;
        public bool ShowSetFocusTargetLog;
        public bool ShowInteractWithObjectLog;
        public bool ShowOpenObjectInteractionLog;

        public void Save() =>
            this.Save(DService.Instance().GetOmenService<TargetManager>());
    }
}
