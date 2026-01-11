using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dalamud.Plugin.Services;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class FrameworkManager : OmenServiceBase<FrameworkManager>
{
    private readonly CancellationTokenSource cancelSource = new();

    private readonly ConcurrentDictionary<IFramework.OnUpdateDelegate, (uint Throttle, string HashCode)> methodsCollectionSync  = [];
    private readonly ConcurrentDictionary<IFramework.OnUpdateDelegate, (uint Throttle, string HashCode)> methodsCollectionAsync = [];

    private int isLastAsyncUpdating;

    internal override void Init() =>
        DService.Instance().Framework.Update += DailyRoutines_OnUpdate;

    internal override void Uninit()
    {
        DService.Instance().Framework.Update -= DailyRoutines_OnUpdate;

        cancelSource.Cancel();
        cancelSource.Dispose();

        methodsCollectionSync.Clear();
        methodsCollectionAsync.Clear();
    }

    public bool Reg(IFramework.OnUpdateDelegate method, bool isSync = false, uint throttleMS = 0)
    {
        var state = false;
        switch (isSync)
        {
            case false:
                if (methodsCollectionAsync.TryAdd(method, (throttleMS, $"{RuntimeHelpers.GetHashCode(method)}_{method.Method.MethodHandle.Value}")))
                    state = true;
                break;
            case true:
                if (methodsCollectionSync.TryAdd(method, (throttleMS, $"{RuntimeHelpers.GetHashCode(method)}_{method.Method.MethodHandle.Value}")))
                    state = true;
                break;
        }
        
        return state;
    }

    public bool Unreg(params IFramework.OnUpdateDelegate[] methods)
    {
        var state = true;
        foreach (var method in methods)
        {
            if (!methodsCollectionAsync.TryRemove(method, out _) &&
                !methodsCollectionSync.TryRemove(method, out _)) 
                state = false;
        }

        return state;
    }

    private void DailyRoutines_OnUpdate(IFramework framework)
    {
        framework.Run(() =>
        {
            if (Interlocked.Exchange(ref isLastAsyncUpdating, 1) != 0)
                return;

            try
            {
                foreach (var (method, (throttle, key)) in methodsCollectionAsync)
                {
                    if (throttle > 0 && !Throttler.Throttle($"FrameworkManager-OnUpdate-{key}", throttle))
                        continue;

                    try
                    {
                        method(framework);
                    }
                    catch (Exception ex)
                    {
                        Error("在 Framework 异步更新过程中发生错误", ex);
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref isLastAsyncUpdating, 0);
            }
        }, cancellationToken: cancelSource.Token).ConfigureAwait(false);
        
        foreach (var (method, (throttle, hashCode)) in methodsCollectionSync)
        {
            if (throttle > 0 && !Throttler.Throttle($"FrameworkManager-OnUpdate-{hashCode}", throttle))
                continue;
                
            try
            {
                method(framework);
            }
            catch (Exception ex)
            {
                Error("在 Framework 同步更新过程中发生错误", ex);
            }
        }
    }
    
    public void SetCurrentThreadMainDalamud()
    {
        try
        {
            var type = DService.Instance().PI.GetType().Assembly.GetType("Dalamud.Utility.ThreadSafety");
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
}
