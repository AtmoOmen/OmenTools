using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Extensions;

public static unsafe class InventoryExtension
{
    extension(IEnumerable<InventoryType> inventories)
    {
        /// <summary>
        /// 获取指定库存集合内所有已加载容器的空位，按容器枚举顺序和格位顺序返回。
        /// </summary>
        /// <returns>包含容器类型和格位编号的空位列表；库存管理器不可用时返回空列表。</returns>
        public List<(InventoryType Container, ushort Slot)> GetEmptySlots()
        {
            List<(InventoryType Container, ushort Slot)> emptySlots = [];

            foreach (var type in inventories)
                emptySlots.AddRange(type.GetEmptySlots());

            return emptySlots;
        }

        /// <summary>
        /// 统计库存集合的可用空位总数，达到指定上限后停止遍历。
        /// </summary>
        /// <param name="limit">最多统计的空位总数；默认统计全部空位，零表示不进行统计。</param>
        /// <returns>不超过上限的空位总数；不可用或未加载的容器不提供空位。</returns>
        public uint GetEmptySlotCount(uint limit = uint.MaxValue)
        {
            uint count = 0;

            foreach (var type in inventories)
            {
                if (count >= limit)
                    break;

                count += type.GetEmptySlotCount(limit - count);
            }

            return count;
        }

        /// <summary>
        /// 按容器枚举顺序收集所有已加载库存中满足条件的物品副本。
        /// </summary>
        /// <param name="predicateFunc">对每个有效格位执行一次的筛选条件。</param>
        /// <param name="itemResult">匹配物品的副本列表；无匹配物品时为空。</param>
        /// <returns>至少找到一个匹配物品时返回真。</returns>
        public bool TryGetItems
        (
            Predicate<InventoryItem> predicateFunc,
            out List<InventoryItem>  itemResult
        )
        {
            itemResult = [];

            foreach (var type in inventories)
            {
                if (type.TryGetItems(predicateFunc, out var items))
                    itemResult.AddRange(items);
            }

            return itemResult.Count > 0;
        }

        /// <summary>
        /// 按容器枚举顺序和格位顺序查找已加载库存中首个满足条件的物品。
        /// </summary>
        /// <param name="predicateFunc">对每个有效格位执行一次的筛选条件。</param>
        /// <param name="itemResult">首个匹配物品的指针；无匹配物品时为空，不应保存该指针。</param>
        /// <returns>找到匹配物品时返回真。</returns>
        public bool TryGetFirstItem
        (
            Predicate<InventoryItem> predicateFunc,
            out InventoryItem*       itemResult
        )
        {
            itemResult = null;

            foreach (var type in inventories)
            {
                if (type.TryGetFirstItem(predicateFunc, out itemResult))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断库存集合的可用空位总数是否不超过指定阈值。
        /// </summary>
        /// <param name="threshold">允许判定为已满的最大空位数，默认为零。</param>
        /// <returns>空位总数不超过阈值时返回真；不可用或未加载的容器不提供空位。</returns>
        public bool IsFull(uint threshold = 0) =>
            inventories.GetEmptySlotCount
            (
                threshold == uint.MaxValue ?
                    threshold :
                    threshold + 1
            ) <=
            threshold;
    }

    extension(InventoryType inventoryType)
    {
        /// <summary>
        /// 打开指定库存格位的物品右键菜单。
        /// </summary>
        /// <param name="slot">目标格位的零起始编号。</param>
        /// <returns>物品菜单处理系统可用且已提交打开请求时返回真。</returns>
        public bool OpenSlotContext(ushort slot)
        {
            var agent = AgentInventoryContext.Instance();
            if (agent == null) return false;

            agent->OpenForItemSlot(inventoryType, slot, 0, AgentInventory.Instance()->GetActiveAddonID());
            return true;
        }

        /// <summary>
        /// 获取当前库存内的所有空位，按格位顺序返回。
        /// </summary>
        /// <returns>包含容器类型和格位编号的空位列表；库存不可用或未加载时为空。</returns>
        public List<(InventoryType Container, ushort Slot)> GetEmptySlots()
        {
            List<(InventoryType Container, ushort Slot)> emptySlots = [];

            var manager = InventoryManager.Instance();
            if (manager == null)
                return emptySlots;

            var container = manager->GetInventoryContainer(inventoryType);
            if (container == null || !container->IsLoaded)
                return emptySlots;

            for (var slotIndex = 0; slotIndex < container->Size; slotIndex++)
            {
                var slot = container->GetInventorySlot(slotIndex);
                if (slot == null || slot->ItemId != 0)
                    continue;

                emptySlots.Add((inventoryType, (ushort)slotIndex));
            }

            return emptySlots;
        }

        /// <summary>
        /// 统计当前库存的可用空位数，达到指定上限后停止遍历。
        /// </summary>
        /// <param name="limit">最多统计的空位数；默认统计全部空位，零表示不进行统计。</param>
        /// <returns>不超过上限的空位数；库存不可用或未加载时返回零。</returns>
        public uint GetEmptySlotCount(uint limit = uint.MaxValue)
        {
            if (limit == 0)
                return 0;

            var manager = InventoryManager.Instance();
            if (manager == null)
                return 0;

            var container = manager->GetInventoryContainer(inventoryType);
            if (container == null || !container->IsLoaded)
                return 0;

            uint count = 0;

            for (var slotIndex = 0; slotIndex < container->Size; slotIndex++)
            {
                var slot = container->GetInventorySlot(slotIndex);
                if (slot == null || slot->ItemId != 0)
                    continue;

                count++;
                if (count >= limit)
                    break;
            }

            return count;
        }

        /// <summary>
        /// 判断当前库存的可用空位数是否不超过指定阈值。
        /// </summary>
        /// <param name="threshold">允许判定为已满的最大空位数，默认为零。</param>
        /// <returns>空位数不超过阈值时返回真；库存不可用或未加载时视为已满。</returns>
        public bool IsFull(uint threshold = 0) =>
            inventoryType.GetEmptySlotCount
            (
                threshold == uint.MaxValue ?
                    threshold :
                    threshold + 1
            ) <=
            threshold;

        /// <summary>
        /// 收集当前库存中满足条件的物品副本。
        /// </summary>
        /// <param name="predicateFunc">对每个有效格位执行一次的筛选条件。</param>
        /// <param name="itemResult">匹配物品的副本列表；库存不可用、未加载或无匹配物品时为空。</param>
        /// <returns>至少找到一个匹配物品时返回真。</returns>
        public bool TryGetItems
        (
            Predicate<InventoryItem> predicateFunc,
            out List<InventoryItem>  itemResult
        )
        {
            itemResult = [];

            var manager = InventoryManager.Instance();
            if (manager == null)
                return false;

            var container = manager->GetInventoryContainer(inventoryType);
            if (container == null || !container->IsLoaded)
                return false;

            for (var slotIndex = 0; slotIndex < container->Size; slotIndex++)
            {
                var slot = container->GetInventorySlot(slotIndex);
                if (slot == null || !predicateFunc(*slot))
                    continue;

                itemResult.Add(*slot);
            }

            return itemResult.Count > 0;
        }

        /// <summary>
        /// 按格位顺序查找当前库存中首个满足条件的物品。
        /// </summary>
        /// <param name="predicateFunc">对每个有效格位执行一次的筛选条件。</param>
        /// <param name="itemResult">首个匹配物品的指针；未找到时为空，不应保存该指针。</param>
        /// <returns>库存已加载且找到匹配物品时返回真。</returns>
        public bool TryGetFirstItem
        (
            Predicate<InventoryItem> predicateFunc,
            out InventoryItem*       itemResult
        )
        {
            itemResult = null;

            var manager = InventoryManager.Instance();
            if (manager == null)
                return false;

            var container = manager->GetInventoryContainer(inventoryType);
            if (container == null || !container->IsLoaded)
                return false;

            for (var slotIndex = 0; slotIndex < container->Size; slotIndex++)
            {
                var slot = container->GetInventorySlot(slotIndex);
                if (slot == null || !predicateFunc(*slot))
                    continue;

                itemResult = slot;
                return true;
            }

            return false;
        }
    }

    extension(scoped in InventoryItem item)
    {
        /// <summary>
        /// 根据物品所属容器和格位打开其右键菜单。
        /// </summary>
        /// <returns>物品菜单处理系统可用且已提交打开请求时返回真。</returns>
        public bool OpenContext() =>
            item.Container.OpenSlotContext((ushort)item.Slot);
    }
}
