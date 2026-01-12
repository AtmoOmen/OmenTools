using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Dalamud.Plugin.Services;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class FrameworkManager : OmenServiceBase<FrameworkManager>
{
    private readonly ConcurrentDictionary<IFramework.OnUpdateDelegate, (uint Throttle, string HashCode)> methodsCollection = [];

    internal override void Init() =>
        DService.Instance().Framework.Update += OnUpdate;

    internal override void Uninit()
    {
        DService.Instance().Framework.Update -= OnUpdate;

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

    private void OnUpdate(IFramework framework)
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
}
