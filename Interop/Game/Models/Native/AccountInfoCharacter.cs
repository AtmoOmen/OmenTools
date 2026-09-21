using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.String;

namespace OmenTools.Interop.Game.Models.Native;

[StructLayout(LayoutKind.Explicit, Size = 128)]
public unsafe struct AccountInfoCharacter
{
    [FieldOffset(0)]
    public ulong LastLoginTime;

    [FieldOffset(8)]
    public ulong ContentID;

    [FieldOffset(16)]
    public ushort HomeWorld;

    [FieldOffset(24)]
    public Utf8String Name;
}
