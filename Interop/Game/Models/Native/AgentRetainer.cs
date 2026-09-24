using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Interop.Game.Models.Native;

// TODO: FFCS
[StructLayout(LayoutKind.Explicit, Size = 0x68D0)]
public unsafe partial struct AgentRetainer {
    [FieldOffset(0x58)] public InventoryType SellItemInventoryType;
    [FieldOffset(0x5C)] public ushort        SellItemInventorySlot;
    
    [FieldOffset(0x4B74)] public int SellItemTotalPrice;
    [FieldOffset(0x4B78)] public int SellItemPriceLimit;
    [FieldOffset(0x4B7C)] public int SellItemUnitPrice;
    [FieldOffset(0x4B80)] public int SellItemQuantity;

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

    private static readonly CompSig OpenRetainerSellSig = 
        new("40 55 53 56 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B 01");
    private delegate void OpenRetainerSellDelegate
    (
        AgentRetainer* agent,
        InventoryType  inventoryType,
        ushort         inventorySlot
    );
    private static readonly OpenRetainerSellDelegate OpenRetainerSellPtr = OpenRetainerSellSig.GetDelegate<OpenRetainerSellDelegate>();

    public void OpenRetainerSell
    (
        InventoryType type,
        ushort        inventorySlot
    )
    {
        fixed (AgentRetainer* ptr = &this)
            OpenRetainerSellPtr(ptr, type, inventorySlot);
    }

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
