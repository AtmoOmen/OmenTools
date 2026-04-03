using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public unsafe class InputIDManager : OmenServiceBase<InputIDManager>
{
    #region Delegates

    public delegate void PrePressedDelegate(ref bool? overrideResult, ref InputId id);

    public delegate void PostPressedDelegate(bool result, InputId id);

    public delegate void PreHeldDelegate(ref bool? overrideResult, ref InputId id);

    public delegate void PostHeldDelegate(bool result, InputId id);

    public delegate void PreDownDelegate(ref bool? overrideResult, ref InputId id);

    public delegate void PostDownDelegate(bool result, InputId id);

    public delegate void PreReleasedDelegate(ref bool? overrideResult, ref InputId id);

    public delegate void PostReleasedDelegate(bool result, InputId id);

    #endregion

    #region Hooks

    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool IsInputIDDelegate(InputData* data, InputId id);

    private Hook<IsInputIDDelegate>? IsInputIDPressedHook;
    private Hook<IsInputIDDelegate>? IsInputIDHeldHook;
    private Hook<IsInputIDDelegate>? IsInputIDDownHook;
    private Hook<IsInputIDDelegate>? IsInputIDReleasedHook;

    #endregion

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> methodsCollection = [];

    protected override void Init()
    {
        IsInputIDDownHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(InputData.MemberFunctionPointers),
            "IsInputIdDown",
            (IsInputIDDelegate)IsInputIDDownDetour
        );

        IsInputIDHeldHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(InputData.MemberFunctionPointers),
            "IsInputIdHeld",
            (IsInputIDDelegate)IsInputIDHeldDetour
        );

        IsInputIDPressedHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(InputData.MemberFunctionPointers),
            "IsInputIdPressed",
            (IsInputIDDelegate)IsInputIDPressedDetour
        );

        IsInputIDReleasedHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(InputData.MemberFunctionPointers),
            "IsInputIdReleased",
            (IsInputIDDelegate)IsInputIDReleasedDetour
        );

        IsInputIDDownHook?.Enable();
        IsInputIDHeldHook?.Enable();
        IsInputIDPressedHook?.Enable();
        IsInputIDReleasedHook?.Enable();
    }

    protected override void Uninit()
    {
        IsInputIDPressedHook?.Dispose();
        IsInputIDPressedHook = null;

        IsInputIDHeldHook?.Dispose();
        IsInputIDHeldHook = null;

        IsInputIDDownHook?.Dispose();
        IsInputIDDownHook = null;

        IsInputIDReleasedHook?.Dispose();
        IsInputIDReleasedHook = null;

        methodsCollection.Clear();
    }

    #region Register

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

    public bool RegPrePressed(PrePressedDelegate method, params PrePressedDelegate[] methods) =>
        RegisterGeneric(method, methods);
    public bool RegPostPressed(PostPressedDelegate method, params PostPressedDelegate[] methods) =>
        RegisterGeneric(method, methods);
    public bool RegPreHeld(PreHeldDelegate method, params PreHeldDelegate[] methods) =>
        RegisterGeneric(method, methods);
    public bool RegPostHeld(PostHeldDelegate method, params PostHeldDelegate[] methods) =>
        RegisterGeneric(method, methods);
    public bool RegPreDown(PreDownDelegate method, params PreDownDelegate[] methods) =>
        RegisterGeneric(method, methods);
    public bool RegPostDown(PostDownDelegate method, params PostDownDelegate[] methods) =>
        RegisterGeneric(method, methods);
    public bool RegPreReleased(PreReleasedDelegate method, params PreReleasedDelegate[] methods) =>
        RegisterGeneric(method, methods);
    public bool RegPostReleased(PostReleasedDelegate method, params PostReleasedDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool UnregPrePressed(params   PrePressedDelegate[]   methods) => UnregisterGeneric(methods);
    public bool UnregPostPressed(params  PostPressedDelegate[]  methods) => UnregisterGeneric(methods);
    public bool UnregPreHeld(params      PreHeldDelegate[]      methods) => UnregisterGeneric(methods);
    public bool UnregPostHeld(params     PostHeldDelegate[]     methods) => UnregisterGeneric(methods);
    public bool UnregPreDown(params      PreDownDelegate[]      methods) => UnregisterGeneric(methods);
    public bool UnregPostDown(params     PostDownDelegate[]     methods) => UnregisterGeneric(methods);
    public bool UnregPreReleased(params  PreReleasedDelegate[]  methods) => UnregisterGeneric(methods);
    public bool UnregPostReleased(params PostReleasedDelegate[] methods) => UnregisterGeneric(methods);

    #endregion

    #region Hooks

    [return: MarshalAs(UnmanagedType.U1)]
    private bool IsInputIDPressedDetour(InputData* data, InputId id)
    {
        var inputID = id;
        return RunDetour(data, ref inputID, typeof(PrePressedDelegate), typeof(PostPressedDelegate), IsInputIDPressedHook);
    }

    [return: MarshalAs(UnmanagedType.U1)]
    private bool IsInputIDHeldDetour(InputData* data, InputId id)
    {
        var inputID = id;
        return RunDetour(data, ref inputID, typeof(PreHeldDelegate), typeof(PostHeldDelegate), IsInputIDHeldHook);
    }

    [return: MarshalAs(UnmanagedType.U1)]
    private bool IsInputIDDownDetour(InputData* data, InputId id)
    {
        var inputID = id;
        return RunDetour(data, ref inputID, typeof(PreDownDelegate), typeof(PostDownDelegate), IsInputIDDownHook);
    }

    [return: MarshalAs(UnmanagedType.U1)]
    private bool IsInputIDReleasedDetour(InputData* data, InputId id)
    {
        var inputID = id;
        return RunDetour(data, ref inputID, typeof(PreReleasedDelegate), typeof(PostReleasedDelegate), IsInputIDReleasedHook);
    }

    [return: MarshalAs(UnmanagedType.U1)]
    private bool RunDetour
    (
        InputData*               data,
        ref InputId              id,
        Type                     preType,
        Type                     postType,
        Hook<IsInputIDDelegate>? hook
    )
    {
        bool? overrideResult = null;

        if (methodsCollection.TryGetValue(preType, out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                InvokePre(preType, preDelegate, ref overrideResult, ref id);
                if (overrideResult.HasValue)
                    return overrideResult.Value;
            }
        }

        var result = hook!.Original(data, id);

        if (methodsCollection.TryGetValue(postType, out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
                InvokePost(postType, postDelegate, result, id);
        }

        return result;
    }

    private static void InvokePre(Type preType, Delegate method, ref bool? overrideResult, ref InputId id)
    {
        if (preType == typeof(PrePressedDelegate))
            ((PrePressedDelegate)method)(ref overrideResult, ref id);
        else if (preType == typeof(PreHeldDelegate))
            ((PreHeldDelegate)method)(ref overrideResult, ref id);
        else if (preType == typeof(PreDownDelegate))
            ((PreDownDelegate)method)(ref overrideResult, ref id);
        else
            ((PreReleasedDelegate)method)(ref overrideResult, ref id);
    }

    private static void InvokePost(Type postType, Delegate method, bool result, InputId id)
    {
        if (postType == typeof(PostPressedDelegate))
            ((PostPressedDelegate)method)(result, id);
        else if (postType == typeof(PostHeldDelegate))
            ((PostHeldDelegate)method)(result, id);
        else if (postType == typeof(PostDownDelegate))
            ((PostDownDelegate)method)(result, id);
        else
            ((PostReleasedDelegate)method)(result, id);
    }

    #endregion

    #region Invokes

    public bool IsInputIDPressed(InputId id) => IsInputIDPressedHook!.Original(ToStruct(), id);

    public bool IsInputIDHeld(InputId id) => IsInputIDHeldHook!.Original(ToStruct(), id);

    public bool IsInputIDDown(InputId id) => IsInputIDDownHook!.Original(ToStruct(), id);

    public bool IsInputIDReleased(InputId id) => IsInputIDReleasedHook!.Original(ToStruct(), id);

    #endregion

    public static InputData* ToStruct() => &UIInputData.Instance()->InputData;
}
