using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public unsafe class ExecuteCommandManager : OmenServiceBase<ExecuteCommandManager>
{
    public ExecuteCommandManagerConfig Config { get; private set; } = null!;

    #region Hook 定义

    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool ExecuteCommandDelegate(ExecuteCommandFlag command, uint param1, uint param2, uint param3, uint param4);
    private Hook<ExecuteCommandDelegate>? ExecuteCommandHook;

    private static readonly CompSig ExecuteCommandComplexSig = new("E8 ?? ?? ?? ?? 80 7D ?? ?? 74 ?? 41 0F B6 45");
    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool ExecuteCommandComplexDelegate(ExecuteCommandComplexFlag command, long target, uint param1, uint param2, uint param3, uint param4);
    private Hook<ExecuteCommandComplexDelegate>? ExecuteCommandComplexHook;
    
    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool ExecuteCommandComplexLocationDelegate
    (
        ExecuteCommandComplexFlag command,
        Vector3*                  location,
        uint                      param1,
        uint                      param2,
        uint                      param3,
        uint                      param4
    );
    private Hook<ExecuteCommandComplexLocationDelegate>? ExecuteCommandComplexLocationHook;

    #endregion

    #region 事件定义

    public delegate void PreExecuteCommandDelegate
    (
        ref bool               isPrevented,
        ref ExecuteCommandFlag command,
        ref uint               param1,
        ref uint               param2,
        ref uint               param3,
        ref uint               param4
    );

    public delegate void PostExecuteCommandDelegate(ExecuteCommandFlag command, uint param1, uint param2, uint param3, uint param4);

    public delegate void PreExecuteCommandComplexDelegate
    (
        ref bool                      isPrevented,
        ref ExecuteCommandComplexFlag command,
        ref long                      target,
        ref uint                      param1,
        ref uint                      param2,
        ref uint                      param3,
        ref uint                      param4
    );

    public delegate void PostExecuteCommandComplexDelegate
    (
        ExecuteCommandComplexFlag command,
        long                      target,
        uint                      param1,
        uint                      param2,
        uint                      param3,
        uint                      param4
    );

    public delegate void PreExecuteCommandComplexLocationDelegate
    (
        ref bool                      isPrevented,
        ref ExecuteCommandComplexFlag command,
        ref Vector3                   location,
        ref uint                      param1,
        ref uint                      param2,
        ref uint                      param3,
        ref uint                      param4
    );

    public delegate void PostExecuteCommandComplexLocationDelegate
    (
        ExecuteCommandComplexFlag command,
        Vector3                   location,
        uint                      param1,
        uint                      param2,
        uint                      param3,
        uint                      param4
    );

    #endregion

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> methodsCollection = [];

    internal override void Init()
    {
        Config = LoadConfig<ExecuteCommandManagerConfig>() ?? new();

        ExecuteCommandHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(GameMain.MemberFunctionPointers),
            "ExecuteCommand",
            (ExecuteCommandDelegate)ExecuteCommandDetour
        );
        ExecuteCommandHook?.Enable();

        ExecuteCommandComplexHook ??= ExecuteCommandComplexSig.GetHook<ExecuteCommandComplexDelegate>(ExecuteCommandComplexDetour);
        ExecuteCommandComplexHook?.Enable();

        ExecuteCommandComplexLocationHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(GameMain.MemberFunctionPointers),
            "ExecuteLocationCommand",
            (ExecuteCommandComplexLocationDelegate)ExecuteCommandComplexLocationDetour
        );
        ExecuteCommandComplexLocationHook?.Enable();
    }

    internal override void Uninit()
    {
        ExecuteCommandHook?.Dispose();
        ExecuteCommandHook = null;

        ExecuteCommandComplexHook?.Dispose();
        ExecuteCommandComplexHook = null;

        ExecuteCommandComplexLocationHook?.Dispose();
        ExecuteCommandComplexLocationHook = null;

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

    public bool RegPre(PreExecuteCommandDelegate method, params PreExecuteCommandDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool RegPost(PostExecuteCommandDelegate method, params PostExecuteCommandDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool RegPreComplex(PreExecuteCommandComplexDelegate method, params PreExecuteCommandComplexDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool RegPostComplex(PostExecuteCommandComplexDelegate method, params PostExecuteCommandComplexDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool RegPreComplexLocation(PreExecuteCommandComplexLocationDelegate method, params PreExecuteCommandComplexLocationDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool RegPostComplexLocation
        (PostExecuteCommandComplexLocationDelegate method, params PostExecuteCommandComplexLocationDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool Unreg(params PreExecuteCommandDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostExecuteCommandDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PreExecuteCommandComplexDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostExecuteCommandComplexDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PreExecuteCommandComplexLocationDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostExecuteCommandComplexLocationDelegate[] methods) => UnregisterGeneric(methods);

    #endregion

    #region Hooks

    [return: MarshalAs(UnmanagedType.U1)]
    private bool ExecuteCommandDetour(ExecuteCommandFlag command, uint param1, uint param2, uint param3, uint param4)
    {
        if (Config.ShowExecuteCommandLog)
        {
            Debug
            (
                $"[Execute Command Manager] Execute Command\n" +
                $"命令:{command}({(uint)command}) | p1:{param1} | p2:{param2} | p3:{param3} | p4:{param4}"
            );
        }

        var isPrevented = false;

        if (methodsCollection.TryGetValue(typeof(PreExecuteCommandDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PreExecuteCommandDelegate)preDelegate;
                preExecuteCommand(ref isPrevented, ref command, ref param1, ref param2, ref param3, ref param4);
                if (isPrevented)
                    return false;
            }
        }

        var original = ExecuteCommandHook.Original(command, param1, param2, param3, param4);

        if (methodsCollection.TryGetValue(typeof(PostExecuteCommandDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postExecuteCommand = (PostExecuteCommandDelegate)postDelegate;
                postExecuteCommand(command, param1, param2, param3, param4);
            }
        }

        return original;
    }

    [return: MarshalAs(UnmanagedType.U1)]
    private bool ExecuteCommandComplexDetour(ExecuteCommandComplexFlag command, long target, uint param1, uint param2, uint param3, uint param4)
    {
        if (Config.ShowExecuteCommandComplexLog)
        {
            Debug
            (
                $"[Execute Command Manager] Execute Command Complex\n" +
                $"命令:{command}({(uint)command}) | 目标:{target:X} | p1:{param1} | p2:{param2} | p3:{param3} | p4:{param4}"
            );
        }

        var isPrevented = false;

        if (methodsCollection.TryGetValue(typeof(PreExecuteCommandComplexDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PreExecuteCommandComplexDelegate)preDelegate;
                preExecuteCommand
                (
                    ref isPrevented,
                    ref command,
                    ref target,
                    ref param1,
                    ref param2,
                    ref param3,
                    ref param4
                );
                if (isPrevented) return false;
            }
        }

        var original = ExecuteCommandComplexHook.Original(command, target, param1, param2, param3, param4);

        if (methodsCollection.TryGetValue(typeof(PostExecuteCommandComplexDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postExecuteCommand = (PostExecuteCommandComplexDelegate)postDelegate;
                postExecuteCommand(command, target, param1, param2, param3, param4);
            }
        }

        return original;
    }

    [return: MarshalAs(UnmanagedType.U1)]
    private bool ExecuteCommandComplexLocationDetour(ExecuteCommandComplexFlag command, Vector3* location, uint param1, uint param2, uint param3, uint param4)
    {
        var locationModified = *location;

        if (Config.ShowExecuteCommandComplexLog)
        {
            Debug
            (
                $"[Execute Command Manager] Execute Command Complex Location\n" +
                $"命令:{command}({(uint)command}) | 地点:{locationModified} | p1:{param1} | p2:{param2} | p3:{param3} | p4:{param4}"
            );
        }

        var isPrevented = false;

        if (methodsCollection.TryGetValue(typeof(PreExecuteCommandComplexLocationDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PreExecuteCommandComplexLocationDelegate)preDelegate;
                preExecuteCommand
                (
                    ref isPrevented,
                    ref command,
                    ref locationModified,
                    ref param1,
                    ref param2,
                    ref param3,
                    ref param4
                );
                if (isPrevented) return false;
            }
        }

        var original = ExecuteCommandComplexLocationHook.Original(command, &locationModified, param1, param2, param3, param4);

        if (methodsCollection.TryGetValue(typeof(PostExecuteCommandComplexLocationDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postExecuteCommand = (PostExecuteCommandComplexLocationDelegate)postDelegate;
                postExecuteCommand(command, locationModified, param1, param2, param3, param4);
            }
        }

        return original;
    }

    #endregion

    #region Invokes

    public void ExecuteCommand
    (
        ExecuteCommandFlag command,
        uint               param1 = 0,
        uint               param2 = 0,
        uint               param3 = 0,
        uint               param4 = 0
    )
    {
        ExecuteCommandHook.Original(command, param1, param2, param3, param4);

        if (methodsCollection.TryGetValue(typeof(PostExecuteCommandDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postExecuteCommand = (PostExecuteCommandDelegate)postDelegate;
                postExecuteCommand(command, param1, param2, param3, param4);
            }
        }
    }

    public void ExecuteCommandComplex
    (
        ExecuteCommandComplexFlag command,
        long                      target = 0xE0000000,
        uint                      param1 = 0,
        uint                      param2 = 0,
        uint                      param3 = 0,
        uint                      param4 = 0
    )
    {
        ExecuteCommandComplexHook.Original(command, target, param1, param2, param3, param4);

        if (methodsCollection.TryGetValue(typeof(PostExecuteCommandComplexDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postExecuteCommand = (PostExecuteCommandComplexDelegate)postDelegate;
                postExecuteCommand(command, target, param1, param2, param3, param4);
            }
        }
    }

    public void ExecuteCommandComplexLocation
    (
        ExecuteCommandComplexFlag command,
        Vector3                   location = default,
        uint                      param1   = 0,
        uint                      param2   = 0,
        uint                      param3   = 0,
        uint                      param4   = 0
    )
    {
        ExecuteCommandComplexLocationHook.Original(command, &location, param1, param2, param3, param4);

        if (methodsCollection.TryGetValue(typeof(PostExecuteCommandComplexLocationDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postExecuteCommand = (PostExecuteCommandComplexLocationDelegate)postDelegate;
                postExecuteCommand(command, location, param1, param2, param3, param4);
            }
        }
    }

    #endregion

    public class ExecuteCommandManagerConfig : OmenServiceConfiguration
    {
        public bool ShowExecuteCommandLog;
        public bool ShowExecuteCommandComplexLog;
        public bool ShowExecuteCommandComplexLocationLog;

        public void Save() =>
            this.Save(DService.Instance().GetOmenService<ExecuteCommandManager>());
    }
}
