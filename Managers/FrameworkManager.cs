using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dalamud.Plugin.Services;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class FrameworkManager : OmenServiceBase<FrameworkManager>
{
    private readonly ConcurrentDictionary<IFramework.OnUpdateDelegate, (uint Throttle, string HashCode)> methodsCollection  = [];


    internal override void Init() =>
        DService.Instance().Framework.Update += DailyRoutines_OnUpdate;

    internal override void Uninit()
    {
        DService.Instance().Framework.Update -= DailyRoutines_OnUpdate;

        methodsCollection.Clear();
    }

    public bool Reg(IFramework.OnUpdateDelegate method, uint throttleMS = 0) =>
        methodsCollection.TryAdd(method, (throttleMS, $"{RuntimeHelpers.GetHashCode(method)}_{method.Method.MethodHandle.Value}"));

    public bool Unreg(params IFramework.OnUpdateDelegate[] methods)
    {
        var state = true;
        foreach (var method in methods)
        {
            if (!methodsCollection.TryRemove(method, out _)) 
                state = false;
        }

        return state;
    }

    private void DailyRoutines_OnUpdate(IFramework framework)
    {
        foreach (var (method, (throttle, hashCode)) in methodsCollection)
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
