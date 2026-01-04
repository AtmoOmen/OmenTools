using System.Collections;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 176)]
public unsafe struct AgentTradeMultiple : IEnumerable<SelectedMateria>
{
    [FieldOffset(0)]  public AgentInterface AgentInterface;
    [FieldOffset(40)] public TradeMultiple  TradeMultiple;

    public SelectedMateria this[int index] => 
        SelectedMateria[index];
    public Span<SelectedMateria> SelectedMateria => 
        TradeMultiple.SelectedMateria;

    public IEnumerator<SelectedMateria> GetEnumerator()
    {
        for (var i = 0; i < 5; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// 开始合成
    /// </summary>
    public void StartTransmutation()
    {
        var outValue = new AtkValue();
        var inValue = new AtkValue();
        inValue.SetInt(0);
        AgentInterface.ReceiveEvent(&outValue, &inValue, 1, 2);
    }

    /// <summary>
    /// 添加魔晶石
    /// </summary>
    public void AddMateria(InventoryType inventoryType, ushort slot, uint count)
    {
        var item = InventoryManager.Instance()->GetInventorySlot(inventoryType, slot);

        TradeMultiple.CurrentSelectMateria = new SelectedMateria
        {
            Type        = inventoryType,
            Slot        = slot,
            ItemID      = item->ItemId,
            SelectCount = count
        };

        AgentId.TradeMultiple.SendEvent(1, (int)count);
    }

    public bool IsAllMateriaSelected() => GetCurrentSelectedMateriaCount() == 5;

    public uint GetCurrentSelectedMateriaCount() =>
        (uint)SelectedMateria
              .ToArray()
              .Sum(x => x.SelectCount);

    public bool IsMateriaSelected(InventoryItem* item) =>
        SelectedMateria
            .ToArray()
            .Any(x => x.Type == item->Container && x.Slot == item->Slot);

    public static AgentTradeMultiple* Instance() => 
        (AgentTradeMultiple*)AgentModule.Instance()->GetAgentByInternalId(AgentId.TradeMultiple);
}

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public struct SelectedMateria
{
    [FieldOffset(0)]  public InventoryType Type;
    [FieldOffset(4)]  public ushort        Slot;
    [FieldOffset(8)]  public uint          ItemID;
    [FieldOffset(12)] public uint          SelectCount;
}

[StructLayout(LayoutKind.Explicit, Size = 136)]
public unsafe struct TradeMultiple
{
    [FieldOffset(0)]  public       nint* VirtualTable;
    [FieldOffset(8)]  public       nint  UnkInstane;
    [FieldOffset(32)] public       int   MaxCount;
    [FieldOffset(40)] public fixed byte  SelectedMateriaData[96];

    public Span<SelectedMateria> SelectedMateria
    {
        get
        {
            fixed (void* ptr = SelectedMateriaData)
            {
                return new Span<SelectedMateria>(ptr, 6);
            }
        }
    }

    public SelectedMateria CurrentSelectMateria
    {
        get => SelectedMateria[5];
        set => SelectedMateria[5] = value;
    }

    public void AddMateria(InventoryType inventoryType, ushort slot)
    {
        var slotData = InventoryManager.Instance()->GetInventorySlot(inventoryType, slot);
        if (slotData == null || slotData->ItemId == 0) return;

        var itemData = LuminaGetter.GetRow<Item>(slotData->ItemId);
        if (itemData is not { FilterGroup: 13 }) return;

        fixed (void* ptr = &this)
        {
            ((delegate* unmanaged[Stdcall]<nint, uint, InventoryType, void>)*VirtualTable)((nint)ptr, slot, inventoryType);
        }
    }
}
