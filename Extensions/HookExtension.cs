using Dalamud.Hooking;
using OmenTools.Dalamud.Helpers;

namespace OmenTools.Extensions;

public static class HookExtension
{
    extension(IGameInteropProvider hook)
    {
        public Hook<T> HookFromMemberFunction<T>(Type memberFunctions, string name, T detour)
            where T : Delegate =>
            hook.HookFromAddress(DalamudReflector.GetMemberFuncByName(memberFunctions, name), detour);

        public unsafe Hook<T> HookFromVirtualTable<T, TVTable>(TVTable* vTable, string name, T detour)
            where T : Delegate
            where TVTable : unmanaged =>
            hook.HookFromAddress(vTable->GetVFuncByName(name), detour);
    }

    extension<T>(Hook<T>? hook) where T : Delegate
    {
        /// <summary>
        ///     在游戏主线程上启用钩子；未创建钩子时直接完成。
        /// </summary>
        /// <returns>启用操作的完成任务。</returns>
        public Task EnableAsync() =>
            hook is null ?
                Task.CompletedTask :
                IFramework.Instance().Run(hook.Enable);

        /// <summary>
        ///     在游戏主线程上禁用钩子；未创建钩子时直接完成。
        /// </summary>
        /// <returns>禁用操作的完成任务。</returns>
        public Task DisableAsync() =>
            hook is null ?
                Task.CompletedTask :
                IFramework.Instance().Run(hook.Disable);

        /// <summary>
        ///     在游戏主线程上释放钩子；未创建钩子时直接完成。
        /// </summary>
        /// <returns>释放操作的完成任务。</returns>
        public Task DisposeAsync() =>
            hook is null ?
                Task.CompletedTask :
                IFramework.Instance().Run(hook.Dispose);

        public void Toggle(bool? isEnabled = null)
        {
            if (hook == null || hook.IsDisposed) return;

            if (isEnabled == null)
            {
                if (hook.IsEnabled)
                    hook.Disable();
                else
                    hook.Enable();
            }
            else
            {
                if (isEnabled.Value)
                    hook.Enable();
                else
                    hook.Disable();
            }
        }
    }
}
