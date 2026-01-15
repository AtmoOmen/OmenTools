using System.Collections.Concurrent;
using System.Diagnostics;
using Dalamud.Plugin.Services;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class FrameworkManager : OmenServiceBase<FrameworkManager>
{
    private static readonly long TicksPerMillisecond = Stopwatch.Frequency / 1000;

    private readonly ConcurrentDictionary<IFramework.OnUpdateDelegate, MethodState> methodsCollection = [];

    internal override void Init() =>
        DService.Instance().Framework.Update += OnUpdate;

    internal override void Uninit()
    {
        DService.Instance().Framework.Update -= OnUpdate;
        methodsCollection.Clear();
    }

    public bool Reg(IFramework.OnUpdateDelegate method, uint throttleMS = 0) =>
        methodsCollection.TryAdd
        (
            method,
            new()
            {
                ThrottleTicks     = throttleMS * TicksPerMillisecond,
                NextExecutionTick = 0
            }
        );

    public bool Unreg(params IFramework.OnUpdateDelegate[] methods)
    {
        var success = true;

        foreach (var method in methods)
        {
            if (!methodsCollection.TryRemove(method, out _))
                success = false;
        }

        return success;
    }

    private void OnUpdate(IFramework framework)
    {
        var currentTick = Stopwatch.GetTimestamp();

        foreach (var (method, state) in methodsCollection)
        {
            if (currentTick < state.NextExecutionTick)
                continue;

            if (state.ThrottleTicks > 0)
                state.NextExecutionTick = currentTick + state.ThrottleTicks;

            try
            {
                method(framework);
            }
            catch (Exception ex)
            {
                Error("在 Framework 更新过程中发生错误", ex);
            }
        }
    }

    private class MethodState
    {
        public long ThrottleTicks;
        public long NextExecutionTick;
    }
}
