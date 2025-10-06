using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace DailyRoutines.Helpers;

public static class FieldMarkerHelper
{
    /// <summary>
    /// 获取场地标点本地位置
    /// </summary>
    public static Vector3 GetLocalPosition(FieldMarkerPoint point)
    {
        var markAddress = GetMarkAddress(point);
        if (MemoryHelper.Read<byte>(markAddress + 28) == 0)
            return default;

        return MemoryHelper.Read<Vector3>(markAddress);
    }
    
    /// <summary>
    /// 放置指定的场地标点至指定地点 (在线)
    /// </summary>
    public static void PlaceOnline(FieldMarkerPoint point, Vector3 pos) => 
        ExecuteCommandManager.ExecuteCommand(ExecuteCommandFlag.PlaceFieldMarker, (uint)point, (uint)pos.X * 1000, (uint)pos.Y * 1000, (uint)pos.Z * 1000);

    /// <summary>
    /// 放置指定的场地标点至指定地点 (在线)
    /// </summary>
    public static void PlaceOnline(uint point, Vector3 pos) => 
        ExecuteCommandManager.ExecuteCommand(ExecuteCommandFlag.PlaceFieldMarker, point, (uint)pos.X * 1000, (uint)pos.Y * 1000, (uint)pos.Z * 1000);

    /// <summary>
    /// 放置指定的场地标点至指定地点 (本地)
    /// </summary>
    public static void PlaceLocal(FieldMarkerPoint index, Vector3 pos, bool isActive)
    {
        var markAddress = GetMarkAddress(index);

        MemoryHelper.Write(markAddress, pos.X);
        MemoryHelper.Write(markAddress + 4, pos.Y);
        MemoryHelper.Write(markAddress + 8, pos.Z);

        MemoryHelper.Write(markAddress + 16, (int)(pos.X * 1000));
        MemoryHelper.Write(markAddress + 20, (int)(pos.Y * 1000));
        MemoryHelper.Write(markAddress + 24, (int)(pos.Z * 1000));

        MemoryHelper.Write(markAddress + 28, (byte)(isActive ? 1 : 0));
    }

    /// <summary>
    /// 获取指定标点在内存中的地址
    /// </summary>
    public static unsafe nint GetMarkAddress(FieldMarkerPoint index)
    {
        if ((uint)index > 7) return nint.Zero;
        
        return (nint)Unsafe.AsPointer(ref MarkingController.Instance()->FieldMarkers[(int)index]);
    }

    /// <summary>
    /// 放置指定的场地标点至指定地点 (本地)
    /// </summary>
    public static unsafe void PlaceLocal(uint index, Vector3 pos, bool isActive)
    {
        if (index > 7) return;

        var markAddress = (nint)Unsafe.AsPointer(ref MarkingController.Instance()->FieldMarkers[(int)index]);

        MemoryHelper.Write(markAddress, pos.X);
        MemoryHelper.Write(markAddress + 0x4, pos.Y);
        MemoryHelper.Write(markAddress + 0x8, pos.Z);

        MemoryHelper.Write(markAddress + 0x10, (int)(pos.X * 1000));
        MemoryHelper.Write(markAddress + 0x14, (int)(pos.Y * 1000));
        MemoryHelper.Write(markAddress + 0x18, (int)(pos.Z * 1000));

        MemoryHelper.Write(markAddress + 0x1C, (byte)(isActive ? 1 : 0));
    }

    /// <summary>
    /// 移除指定的场地标点 (在线)
    /// </summary>
    public static void RemoveOnline(FieldMarkerPoint point) => 
        ExecuteCommandManager.ExecuteCommand(ExecuteCommandFlag.RemoveFieldMarker, (uint)point);

    /// <summary>
    /// 移除指定的场地标点 (在线)
    /// </summary>
    public static void RemoveOnline(uint point) => 
        ExecuteCommandManager.ExecuteCommand(ExecuteCommandFlag.RemoveFieldMarker, point);

    /// <summary>
    /// 移除指定的场地标点 (本地)
    /// </summary>
    public static unsafe void RemoveLocal(FieldMarkerPoint index) => 
        MarkingController.Instance()->ClearFieldMarker((uint)index);

    /// <summary>
    /// 移除指定的场地标点 (本地)
    /// </summary>
    public static unsafe void RemoveLocal(uint index) => 
        MarkingController.Instance()->ClearFieldMarker(index);

    /// <summary>
    /// 清除所有的场地标点 (在线)
    /// </summary>
    public static unsafe byte ClearOnline() => 
        MarkingController.Instance()->ClearFieldMarkers();
}

public enum FieldMarkerPoint : uint
{
    A, 
    B, 
    C, 
    D, 
    One, 
    Two, 
    Three, 
    Four
}
