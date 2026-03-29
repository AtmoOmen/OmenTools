using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Interop.Game.Helpers;

public static class AddonHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryGetByName(string addonName, out AtkUnitBase* addonPtr)
    {
        var addon = DService.Instance().GameGUI.GetAddonByName(addonName).Address;

        if (addon == nint.Zero)
        {
            addonPtr = null;
            return false;
        }

        addonPtr = (AtkUnitBase*)addon;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryGetByName<T>(string addonName, out T* addonPtr) where T : unmanaged
    {
        var addon = DService.Instance().GameGUI.GetAddonByName(addonName).Address;

        if (addon == nint.Zero)
        {
            addonPtr = null;
            return false;
        }

        addonPtr = (T*)addon;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* GetByName<T>(string addonName) where T : unmanaged
    {
        var a = DService.Instance().GameGUI.GetAddonByName(addonName).Address;
        if (a == nint.Zero) return null;

        return (T*)a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe AtkUnitBase* GetByName(string name) =>
        GetByName<AtkUnitBase>(name);
}
