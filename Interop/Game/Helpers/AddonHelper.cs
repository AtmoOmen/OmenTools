using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Interop.Game.Helpers;

public static class AddonHelper
{
    public static bool TryGetPtrByName(string addonName, out nint addonPtr)
    {
        addonPtr = IGameGui.Instance().GetAddonByName(addonName).Address;
        return addonPtr != nint.Zero;
    }
    
    public static unsafe bool TryGetByName(string addonName, out AtkUnitBase* addonPtr)
    {
        var addon = IGameGui.Instance().GetAddonByName(addonName).Address;

        if (addon == nint.Zero)
        {
            addonPtr = null;
            return false;
        }

        addonPtr = (AtkUnitBase*)addon;
        return true;
    }

    public static unsafe bool TryGetByName<T>(string addonName, out T* addonPtr) where T : unmanaged
    {
        var addon = IGameGui.Instance().GetAddonByName(addonName).Address;

        if (addon == nint.Zero)
        {
            addonPtr = null;
            return false;
        }

        addonPtr = (T*)addon;
        return true;
    }

    public static unsafe T* GetByName<T>(string addonName) where T : unmanaged
    {
        var a = IGameGui.Instance().GetAddonByName(addonName).Address;
        if (a == nint.Zero) return null;

        return (T*)a;
    }

    public static unsafe AtkUnitBase* GetByName(string name) =>
        GetByName<AtkUnitBase>(name);
}
