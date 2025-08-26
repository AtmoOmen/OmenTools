using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct AgentFreeCompanyChest
{
    [FieldOffset(0)]
    public AgentInterface AgentInterface;

    [FieldOffset(6956)]
    public InventoryType ContextInventoryType;

    [FieldOffset(6960)]
    public short ContextInventorySlot;
    
    public static AgentFreeCompanyChest* Instance() =>
        (AgentFreeCompanyChest*)AgentModule.Instance()->GetAgentByInternalId(AgentId.FreeCompanyChest);

    public InventoryItem* GetContextInventoryItem()
    {
        var manager = InventoryManager.Instance();
        if (manager == null) return null;
        
        if (ContextInventoryType == InventoryType.Invalid) return null;

        return manager->GetInventorySlot(ContextInventoryType, ContextInventorySlot);
    }
}
