using System.Collections.Concurrent;
using System.Numerics;
using Dalamud.Hooking;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class ExecuteCommandManager : OmenServiceBase
{
    public static ExecuteCommandManagerConfig Config { get; private set; } = null!;
    
    #region Hook 定义

    private static readonly CompSig ExecuteCommandSig = new("E8 ?? ?? ?? ?? 48 8B 06 48 8B CE FF 50 ?? E9 ?? ?? ?? ?? 49 8B CC");
    private delegate nint ExecuteCommandDelegate(
        ExecuteCommandFlag command, uint param1, uint param2, uint param3, uint param4);
    private static Hook<ExecuteCommandDelegate>? ExecuteCommandHook;

    private static readonly CompSig ExecuteCommandComplexSig = new("E8 ?? ?? ?? ?? 80 7D ?? ?? 74 ?? 41 0F B6 45");
    private delegate nint ExecuteCommandComplexDelegate(
        ExecuteCommandComplexFlag command, long target, uint param1, uint param2, uint param3, uint param4);
    private static Hook<ExecuteCommandComplexDelegate>? ExecuteCommandComplexHook;

    private static readonly CompSig ExecuteCommandComplexLocationSig = new("E8 ?? ?? ?? ?? EB ?? 48 8B 54 24 ?? 45 33 C9");
    private unsafe delegate nint ExecuteCommandComplexLocationDelegate(
        ExecuteCommandComplexFlag command, Vector3* location, uint param1, uint param2, uint param3, uint param4);
    private static Hook<ExecuteCommandComplexLocationDelegate>? ExecuteCommandComplexLocationHook;

    #endregion

    #region 事件定义

    public delegate void PreExecuteCommandDelegate(
        ref bool isPrevented, ref ExecuteCommandFlag command, ref uint param1, ref uint param2, ref uint param3, ref uint param4);
    public delegate void PostExecuteCommandDelegate(ExecuteCommandFlag command, uint param1, uint param2, uint param3, uint param4);

    public delegate void PreExecuteCommandComplexDelegate(
        ref bool isPrevented, ref ExecuteCommandComplexFlag command, ref long target, ref uint param1, ref uint param2,
        ref uint param3, ref uint param4);
    public delegate void PostExecuteCommandComplexDelegate(
        ExecuteCommandComplexFlag command, long target, uint param1, uint param2, uint param3, uint param4);

    public delegate void PreExecuteCommandComplexLocationDelegate(
        ref bool isPrevented, ref ExecuteCommandComplexFlag command, ref Vector3 location, ref uint param1,
        ref uint param2, ref uint param3, ref uint param4);
    public delegate void PostExecuteCommandComplexLocationDelegate(
        ExecuteCommandComplexFlag command, Vector3 location, uint param1, uint param2, uint param3, uint param4);

    #endregion

    private static readonly ConcurrentDictionary<Type, ConcurrentBag<Delegate>> methodsCollection = [];

    private static readonly SemaphoreSlim Semaphore = new(3);

    internal override unsafe void Init()
    {
        Config = LoadConfig<ExecuteCommandManagerConfig>() ?? new();
        
        ExecuteCommandHook                ??= ExecuteCommandSig.GetHook<ExecuteCommandDelegate>(ExecuteCommandDetour);
        ExecuteCommandComplexHook         ??= ExecuteCommandComplexSig.GetHook<ExecuteCommandComplexDelegate>(ExecuteCommandComplexDetour);
        ExecuteCommandComplexLocationHook ??= ExecuteCommandComplexLocationSig.GetHook<ExecuteCommandComplexLocationDelegate>(ExecuteCommandComplexLocationDetour);

        EnableHooks();
    }

    #region Register

    private static bool RegisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        var bag = methodsCollection.GetOrAdd(type, _ => []);
        foreach (var method in methods)
            bag.Add(method);

        EnableHooks();
        return true;
    }

    private static bool UnregisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        if (methodsCollection.TryGetValue(type, out var bag))
        {
            foreach (var method in methods)
            {
                var newBag = new ConcurrentBag<Delegate>(bag.Where(d => d != method));
                methodsCollection[type] = newBag;
            }
            return true;
        }

        return false;
    }

    public static bool Register(params PreExecuteCommandDelegate[]                 methods) => RegisterGeneric(methods);
    public static bool Register(params PostExecuteCommandDelegate[]                methods) => RegisterGeneric(methods);
    public static bool Register(params PreExecuteCommandComplexDelegate[]          methods) => RegisterGeneric(methods);
    public static bool Register(params PostExecuteCommandComplexDelegate[]         methods) => RegisterGeneric(methods);
    public static bool Register(params PreExecuteCommandComplexLocationDelegate[]  methods) => RegisterGeneric(methods);
    public static bool Register(params PostExecuteCommandComplexLocationDelegate[] methods) => RegisterGeneric(methods);

    public static bool Unregister(params PreExecuteCommandDelegate[]                 methods) => UnregisterGeneric(methods);
    public static bool Unregister(params PostExecuteCommandDelegate[]                methods) => UnregisterGeneric(methods);
    public static bool Unregister(params PreExecuteCommandComplexDelegate[]          methods) => UnregisterGeneric(methods);
    public static bool Unregister(params PostExecuteCommandComplexDelegate[]         methods) => UnregisterGeneric(methods);
    public static bool Unregister(params PreExecuteCommandComplexLocationDelegate[]  methods) => UnregisterGeneric(methods);
    public static bool Unregister(params PostExecuteCommandComplexLocationDelegate[] methods) => UnregisterGeneric(methods);

    #endregion

    #region Hooks

    public static void EnableHooks()
    {
        ExecuteCommandHook?.Enable();
        ExecuteCommandComplexHook?.Enable();
        ExecuteCommandComplexLocationHook?.Enable();
    }

    public static void DisableHooks()
    {
        ExecuteCommandHook?.Disable();
        ExecuteCommandComplexHook?.Disable();
        ExecuteCommandComplexLocationHook?.Disable();
    }

    private static nint ExecuteCommandDetour(ExecuteCommandFlag command, uint param1, uint param2, uint param3, uint param4)
    {
        if (Config.ShowExecuteCommandLog)
            Debug(
                $"[Execute Command Manager] Execute Command\n" +
                $"命令:{command}({(uint)command}) | p1:{param1} | p2:{param2} | p3:{param3} | p4:{param4}");

        var isPrevented = false;
        if (methodsCollection.TryGetValue(typeof(PreExecuteCommandDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PreExecuteCommandDelegate)preDelegate;
                preExecuteCommand(ref isPrevented, ref command, ref param1, ref param2, ref param3, ref param4);
                if (isPrevented) return nint.Zero;
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

    private static nint ExecuteCommandComplexDetour(
        ExecuteCommandComplexFlag command, long target, uint param1, uint param2, uint param3, uint param4)
    {
        if (Config.ShowExecuteCommandComplexLog)
            Debug(
                $"[Execute Command Manager] Execute Command Complex\n" +
                $"命令:{command}({(uint)command}) | p1:{target} | p2:{param1} | p3:{param2} | p4:{param3} | p5:{param4}");

        var isPrevented = false;
        if (methodsCollection.TryGetValue(typeof(PreExecuteCommandComplexDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PreExecuteCommandComplexDelegate)preDelegate;
                preExecuteCommand(ref isPrevented, ref command, ref target, ref param1, ref param2, ref param3,
                                  ref param4);
                if (isPrevented) return nint.Zero;
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

    private static unsafe nint ExecuteCommandComplexLocationDetour(
        ExecuteCommandComplexFlag command, Vector3* location, uint param1, uint param2, uint param3, uint param4)
    {
        var locationModified = *location;

        if (Config.ShowExecuteCommandComplexLog)
            Debug(
                $"[Execute Command Manager] Execute Command Complex Location\n" +
                $"命令:{command}({(uint)command}) | 地点:{locationModified} | p1:{param1} | p2:{param2} | p3:{param3} | p4:{param4}");

        var isPrevented = false;
        if (methodsCollection.TryGetValue(typeof(PreExecuteCommandComplexLocationDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PreExecuteCommandComplexLocationDelegate)preDelegate;
                preExecuteCommand(ref isPrevented, ref command, ref locationModified, ref param1, ref param2,
                                  ref param3, ref param4);
                if (isPrevented) return nint.Zero;
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

    public static void ExecuteCommand(
        ExecuteCommandFlag command, uint param1 = 0, uint param2 = 0, uint param3 = 0, uint param4 = 0)
    {
        Semaphore.Wait();

        try
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
        finally
        {
            Semaphore.Release();
        }
    }

    public static void ExecuteCommandComplex(ExecuteCommandComplexFlag command, long target = 0xE0000000, uint param1 = 0, 
                                      uint param2 = 0, uint param3 = 0, uint param4 = 0)
    {
        Semaphore.Wait();

        try
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
        finally
        {
            Semaphore.Release(); 
        }
    }

    public static unsafe void ExecuteCommandComplexLocation(ExecuteCommandComplexFlag command, Vector3 location = default, 
                                                     uint param1 = 0, uint param2 = 0, uint param3 = 0, uint param4 = 0)
    {
        Semaphore.Wait();

        try
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
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    internal override void Uninit()
    {
        DisableHooks();

        ExecuteCommandHook?.Dispose();
        ExecuteCommandHook = null;

        ExecuteCommandComplexHook?.Dispose();
        ExecuteCommandComplexHook = null;

        ExecuteCommandComplexLocationHook?.Dispose();
        ExecuteCommandComplexLocationHook = null;

        methodsCollection.Clear();
    }
    
    public class ExecuteCommandManagerConfig : OmenServiceConfiguration
    {
        public bool ShowExecuteCommandLog;
        public bool ShowExecuteCommandComplexLog;
        public bool ShowExecuteCommandComplexLocationLog;

        public void Save() => 
            this.Save(DService.GetOmenService<ExecuteCommandManager>());
    }
}
