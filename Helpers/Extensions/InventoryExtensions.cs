using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Helpers;

public static unsafe class InventoryExtensions
{
    extension(IEnumerable<InventoryType> inventories)
    {
        public bool TryGetItems(
            Predicate<InventoryItem> predicateFunc,
            out List<InventoryItem>  itemResult)
        {
            itemResult = [];

            var manager = InventoryManager.Instance();
            if (manager == null) return false;

            foreach (var type in inventories)
            {
                var container = manager->GetInventoryContainer(type);
                if (container == null || !container->IsLoaded) return false;
                for (var i = 0; i < container->Size; i++)
                {
                    var slot = container->GetInventorySlot(i);
                    if (slot == null || !predicateFunc(*slot)) continue;

                    itemResult.Add(*slot);
                }
            }

            return itemResult.Count > 0;
        }

        public bool TryGetFirstItem(
            Predicate<InventoryItem> predicateFunc,
            out InventoryItem*       itemResult)
        {
            itemResult = null;

            var manager = InventoryManager.Instance();
            if (manager == null) return false;

            foreach (var type in inventories)
            {
                var container = manager->GetInventoryContainer(type);
                if (container == null || !container->IsLoaded) return false;
                for (var i = 0; i < container->Size; i++)
                {
                    var slot = container->GetInventorySlot(i);
                    if (slot == null || !predicateFunc(*slot)) continue;
                    if (!predicateFunc(*slot)) continue;

                    itemResult = slot;
                    return true;
                }
            }

            return false;
        }
        
        public bool IsFull(uint threshold = 0)
        {
            var manager = InventoryManager.Instance();
            if (manager == null) return true;

            uint emptySlotsCount = 0;

            foreach (var inventoryType in inventories)
            {
                var container = manager->GetInventoryContainer(inventoryType);
                if (container == null || !container->IsLoaded) continue;

                for (var index = 0; index < container->Size; index++)
                {
                    var slot = container->GetInventorySlot(index);
                    if (slot->ItemId == 0)
                    {
                        emptySlotsCount++;

                        if (emptySlotsCount > threshold)
                            return false;
                    }
                }
            }

            return emptySlotsCount <= threshold;
        }
    }

    extension(InventoryType inventoryType)
    {
        public bool OpenSlotContext(ushort slot)
        {
            var agent = AgentInventoryContext.Instance();
            if (agent == null) return false;
            
            agent->OpenForItemSlot(inventoryType, slot, 0, AgentInventory.Instance()->GetActiveAddonID());
            return true;
        }
        
        public bool TryGetItems(
            Predicate<InventoryItem> predicateFunc,
            out List<InventoryItem>  itemResult) => 
            new List<InventoryType> { inventoryType }.TryGetItems(predicateFunc, out itemResult);

        public bool TryGetFirstItem(
            Predicate<InventoryItem> predicateFunc,
            out InventoryItem*       itemResult) => 
            new List<InventoryType> { inventoryType }.TryGetFirstItem(predicateFunc, out itemResult);
    }

    extension(scoped in InventoryItem item)
    {
        public bool OpenContext() => 
            item.Container.OpenSlotContext((ushort)item.Slot);
    }
}
