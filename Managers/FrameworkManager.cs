using System.Collections.Concurrent;
using System.Reflection;
using Dalamud.Plugin.Services;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class FrameworkManager : OmenServiceBase
{
    private static CancellationTokenSource? CancelSource = new();

    private static readonly ConcurrentDictionary<string, (uint Throttle, IFramework.OnUpdateDelegate Method)> methodsInfoSync  = [];
    private static readonly ConcurrentDictionary<string, (uint Throttle, IFramework.OnUpdateDelegate Method)> methodsInfoAsync = [];

    internal override void Init()
    {
        CancelSource?.Cancel();
        CancelSource?.Dispose();
        CancelSource = new();
        
        DService.Framework.Update +=  DailyRoutines_OnUpdate;
    }

    public static bool Reg(IFramework.OnUpdateDelegate method, bool isSync = false, uint throttleMS = 0)
    {
        var state      = true;
        var uniqueName = GetUniqueName(method);
        switch (isSync)
        {
            case false:
                if (!methodsInfoAsync.TryAdd(uniqueName, (throttleMS, method))) 
                    state = false;
                break;
            case true:
                if (!methodsInfoSync.TryAdd(uniqueName, (throttleMS, method))) 
                    state = false;
                break;
        }

        return state;
    }

    public static bool Unreg(params IFramework.OnUpdateDelegate[] methods)
    {
        var state = true;
        foreach (var method in methods)
        {
            var uniqueName = GetUniqueName(method);
            if (!methodsInfoAsync.TryRemove(uniqueName, out _) &&
                !methodsInfoSync.TryRemove(uniqueName, out _)) 
                state = false;
        }

        return state;
    }

    public static bool Unreg(params MethodInfo[] methods)
    {
        var state = true;
        foreach (var method in methods)
        {
            var uniqueName = GetUniqueName(method);
            if (!methodsInfoAsync.TryRemove(uniqueName, out _) &&
                !methodsInfoSync.TryRemove(uniqueName, out _))
                state = false;
        }

        return state;
    }

    private static string GetUniqueName(IFramework.OnUpdateDelegate method)
    {
        var methodInfo = method.Method;
        var target = method.Target;
        return $"{methodInfo.DeclaringType.FullName}_{methodInfo.Name}_{target?.GetHashCode() ?? 0}";
    }

    private static string GetUniqueName(MemberInfo methodInfo) => 
        $"{methodInfo.DeclaringType.FullName}_{methodInfo.Name}_{methodInfo.GetHashCode()}";

    private static void DailyRoutines_OnUpdate(IFramework framework)
    {
        var copiedAsync = methodsInfoAsync;
        framework.RunOnTick(() =>
        {
            foreach (var (name, methodInfo) in copiedAsync)
            {
                if (methodInfo.Throttle > 0 && !Throttler.Throttle($"FrameworkManager-OnUpdate-{name}", methodInfo.Throttle))
                    continue;
                
                try
                {
                    methodInfo.Method(framework);
                }
                catch (Exception ex)
                {
                    Error("在 Framework 异步更新过程中发生错误", ex);
                }
            }
        }, TimeSpan.Zero, 0, CancelSource.Token).ConfigureAwait(false);
        
        var copiedSync = methodsInfoSync;
        foreach (var (name, methodInfo) in copiedSync)
        {
            if (methodInfo.Throttle > 0 && !Throttler.Throttle($"FrameworkManager-OnUpdate-{name}", methodInfo.Throttle))
                continue;
                
            try
            {
                methodInfo.Method(framework);
            }
            catch (Exception ex)
            {
                Error("在 Framework 同步更新过程中发生错误", ex);
            }
        }
    }
    
    public static void SetCurrentThreadMainDalamud()
    {
        try
        {
            var type = DService.PI.GetType().Assembly.GetType("Dalamud.Utility.ThreadSafety");
            if (type == null) return;

            var field = type.GetField("threadStaticIsMainThread", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null) return;

            field.SetValue(null, true);
        }
        catch
        {
            // ignored
        }
    }

    internal override void Uninit()
    {
        DService.Framework.Update -= DailyRoutines_OnUpdate;

        CancelSource?.Cancel();
        CancelSource?.Dispose();
        CancelSource = null;

        methodsInfoAsync.Clear();
        methodsInfoSync.Clear();
    }
}
