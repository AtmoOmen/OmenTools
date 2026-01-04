using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Helpers;

public static unsafe class AgentInventoryExtensions
{
    extension(ref AgentInventory agent)
    {
        public static uint GetActiveAddonID()
        {
            if (Inventory          == null) return 0;
            if (InventoryLarge     == null) return 0;
            if (InventoryExpansion == null) return 0;

            if (IsAddonAndNodesReady(Inventory)) 
                return Inventory->Id;
            if (IsAddonAndNodesReady(InventoryLarge)) 
                return InventoryLarge->Id;
            if (IsAddonAndNodesReady(InventoryExpansion)) 
                return InventoryExpansion->Id;

            return 0;
        }
    }
}
