using Dalamud.Hooking;
using Dalamud.Plugin.Services;

namespace OmenTools.Extensions;

public static class HookExtension
{
    extension(IGameInteropProvider hook)
    {
        public Hook<T> HookFromMemberFunction<T>(Type memberFunctions, string name, T detour)
            where T : Delegate =>
            hook.HookFromAddress(GetMemberFuncByName(memberFunctions, name), detour);

        public unsafe Hook<T> HookFromVirtualTable<T, TVTable>(TVTable* vTable, string name, T detour)
            where T : Delegate
            where TVTable : unmanaged =>
            hook.HookFromAddress(vTable->GetVFuncByName(name), detour);
    }

    extension<T>(Hook<T>? hook) where T : Delegate
    {
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
