using System.Runtime.InteropServices;

namespace OmenTools.Interop.Game.Models.Native;

[StructLayout(LayoutKind.Explicit, Size = 15712)]
public struct GCShopEventHandler
{
    [FieldOffset(448)]
    public nint PurchaseInterface;

    [FieldOffset(456)]
    public GCShopItemSlot Slots;

    [FieldOffset(15656)]
    public byte GrandCompany;

    [FieldOffset(15657)]
    public byte SubCategory;

    [FieldOffset(15658)]
    public byte Tier;

    [FieldOffset(15700)]
    public uint SelectedDisplayIndex;

    [FieldOffset(15704)]
    public uint ExchangeCount;
}
