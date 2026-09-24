using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using InteropGenerator.Runtime.Attributes;

namespace OmenTools.Interop.Game.Models.Native;

// TODO: FFCS
[StructLayout(LayoutKind.Explicit, Size = 0x68D0)]
public unsafe partial struct AgentRetainer {
    [FieldOffset(0x4B74)] public int TotalPrice;
    [FieldOffset(0x4B78)] public int PriceLimit;
    [FieldOffset(0x4B7C)] public int UnitPrice;
    [FieldOffset(0x4B80)] public int Quantity;

    [FieldOffset(0x4B84)] public int ContextMenuIndex;
    [FieldOffset(0x4B88)] public int SellListEntryCount;

    [FieldOffset(0x4B90)] internal fixed byte SellListEntriesData[0x168 * 20];
    
    public Span<SellListEntry> SellListEntries
    {
        get
        {
            fixed (void* ptr = SellListEntriesData) 
                return new Span<SellListEntry>(ptr, 20);
        }
    }

    [FieldOffset(0x688C)] public uint              RetainerSellListAddonID;
    [FieldOffset(0x6890)] public uint              RetainerSellAddonID;
    [FieldOffset(0x68B0)] public ShopEventHandler* ShopEventHandler;

    public static AgentRetainer* Instance() =>
        (AgentRetainer*)AgentModule.Instance()->GetAgentByInternalId(AgentId.Retainer);

    [StructLayout(LayoutKind.Explicit, Size = 0x168)]
    public struct SellListEntry {
        [FieldOffset(0x00)]  public uint       ItemID;
        [FieldOffset(0x08)]  public Utf8String ItemName;
        [FieldOffset(0x70)]  public int        Quantity;
        [FieldOffset(0x78)]  public Utf8String TotalPriceText;
        [FieldOffset(0xE0)]  public Utf8String UnitPriceText;
        [FieldOffset(0x148)] public ushort     InventorySlot;
    }
}
