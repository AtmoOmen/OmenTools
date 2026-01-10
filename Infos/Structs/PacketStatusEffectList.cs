using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct PacketStatusEffectList
{
    [FieldOffset(20)] public fixed byte EntryData[240];
    
    public Span<StatusEffectListEntry> Entries
    {
        get
        {
            fixed (void* ptr = EntryData)
            {
                return new Span<StatusEffectListEntry>(ptr, 30);
            }
        }
    }
}

[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
public struct StatusEffectListEntry
{
    [FieldOffset(0)] public ushort StatusID;
    [FieldOffset(2)] public ushort StackCount;
    [FieldOffset(4)] public float  RemainingTime;
    [FieldOffset(8)] public uint   SourceID;
}
