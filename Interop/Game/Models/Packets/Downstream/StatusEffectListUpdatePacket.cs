using System.Runtime.InteropServices;
using OmenTools.Interop.Game.Models.Packets.Abstractions;

namespace OmenTools.Interop.Game.Models.Packets.Downstream;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct StatusEffectListUpdatePacket : IDownstreamPacket
{
    [FieldOffset(20)]
    public fixed byte EntryData[240];

    public Span<StatusEffectListEntry> Entries
    {
        get
        {
            fixed (void* ptr = EntryData) return new Span<StatusEffectListEntry>(ptr, 30);
        }
    }

    public string Log() =>
        "Status Effect List Update 包体";
}

[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
public struct StatusEffectListEntry
{
    [FieldOffset(0)]
    public ushort StatusID;

    [FieldOffset(2)]
    public ushort StackCount;

    [FieldOffset(4)]
    public float RemainingTime;

    [FieldOffset(8)]
    public uint SourceID;
}
