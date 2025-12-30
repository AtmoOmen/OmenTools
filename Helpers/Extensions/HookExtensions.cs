using Dalamud.Hooking;
using Dalamud.Plugin.Services;

namespace OmenTools.Helpers;

public static class HookExtensions
{
    extension(IGameInteropProvider hook)
    {
        public Hook<T> HookFromMemberFunction<T>(Type memberFunctions, string name, T detour) 
            where T : Delegate => 
            hook.HookFromAddress(GetMemberFuncByName(memberFunctions, name), detour);

        public unsafe Hook<T> HookFromVirtualTable<T, TVTable>(TVTable* vTable, string name, T detour) 
            where T : Delegate
            where TVTable : unmanaged => 
            hook.HookFromAddress(GetVFuncByName(vTable, name), detour);
    }
}
