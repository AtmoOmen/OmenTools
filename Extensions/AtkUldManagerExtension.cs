using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;

namespace OmenTools.Extensions;

public static unsafe class AtkUldManagerExtension
{
    extension
    (
        scoped ref AtkUldManager manager
    )
    {
        private Span<Pointer<AtkResNode>> ObjectNodesSpan =>
            new(manager.Objects->NodeList, manager.Objects->NodeCount);

        /// <summary>
        ///     获取管理器是否已初始化并分配有效的对象节点列表。
        /// </summary>
        public bool HasObjectList
        {
            get
            {
                if ((manager.ResourceFlags & REQUIRED_FLAGS) != REQUIRED_FLAGS)
                    return false;
                if (manager.Objects is null || manager.Objects->NodeCount < 0)
                    return false;

                return manager.Objects->NodeCount is 0 ||
                       manager.Objects->NodeList is not null;
            }
        }

        /// <summary>
        ///     将单个节点加入对象列表，忽略空指针及已登记的节点，不主动刷新绘制列表。
        /// </summary>
        [OverloadResolutionPriority(0)]
        public void AddNodeToObjectList
        (
            AtkResNode* newNode
        )
        {
            if (!manager.HasObjectList)
                return;
            if (newNode is null)
                return;

            if (manager.IsNodeInObjectList(newNode))
                return;

            var oldSize = manager.Objects->NodeCount;
            var newSize = oldSize + 1;

            var newBuffer = (AtkResNode**)IMemorySpace.GetUISpace()->Reallocate<nint>(manager.Objects->NodeList, newSize);
            newBuffer[newSize - 1] = newNode;

            manager.Objects->NodeList  = newBuffer;
            manager.Objects->NodeCount = newSize;
        }

        /// <summary>
        ///     从对象列表移除单个节点并保持其余节点顺序，不主动刷新绘制列表。
        /// </summary>
        public void RemoveNodeFromObjectList
        (
            AtkResNode* node
        )
        {
            if (!manager.HasObjectList)
                return;
            if (node is null)
                return;

            var oldSize = manager.Objects->NodeCount;
            if (oldSize is 0)
                return;

            var nodeList    = manager.Objects->NodeList;
            var removeIndex = -1;

            for (var index = 0; index < oldSize; index++)
                if (nodeList[index] == node)
                {
                    removeIndex = index;
                    break;
                }

            if (removeIndex < 0)
                return;

            var newSize = oldSize - 1;
            for (var index = removeIndex; index < newSize; index++)
                nodeList[index] = nodeList[index + 1];

            var newBuffer = (AtkResNode**)IMemorySpace.GetUISpace()->Reallocate<nint>(nodeList, Math.Max(newSize, 1));
            if (newSize is 0)
                newBuffer[0] = null;

            manager.Objects->NodeList  = newBuffer;
            manager.Objects->NodeCount = newSize;
        }

        /// <summary>
        ///     按节点 ID 遍历节点列表，返回首个匹配节点并转换为指定的非托管类型。
        /// </summary>
        public T* SearchNodeByID<T>
        (
            uint nodeID
        ) where T : unmanaged
        {
            foreach (var node in manager.Nodes)
            {
                if (node.Value is not null)
                {
                    if (node.Value->NodeId == nodeID)
                        return (T*)node.Value;
                }
            }

            return null;
        }

        /// <summary>
        ///     按节点 ID 遍历节点列表，返回首个匹配的原生节点。
        /// </summary>
        public AtkResNode* SearchNodeByID
        (
            uint nodeID
        )
            => manager.SearchNodeByID<AtkResNode>(nodeID);

        private bool IsNodeInObjectList
        (
            AtkResNode* node
        )
        {
            foreach (var objectNode in manager.ObjectNodesSpan)
            {
                if (objectNode.Value == node)
                    return true;
            }

            return false;
        }

        public List<nint> SearchSimpleNodesByType(NodeType type)
        {
            if ((int)type > 1000)
                throw new ArgumentOutOfRangeException(nameof(type), "不支持非 SimpleNode 类型");

            fixed (AtkUldManager* managerPtr = &manager)
            {
                if (managerPtr == null)
                    return [];

                var result = new List<nint>();

                for (var i = 0; i < managerPtr->NodeListCount; i++)
                {
                    var node = managerPtr->NodeList[i];
                    if (node == null || node->Type != type) continue;

                    result.Add((nint)node);
                }

                return result;
            }
        }

        public T* SearchSimpleNodeByType<T>(NodeType type) where T : unmanaged
        {
            if ((int)type > 1000)
                throw new ArgumentOutOfRangeException(nameof(type), "不支持非 SimpleNode 类型");

            fixed (AtkUldManager* managerPtr = &manager)
            {
                if (managerPtr == null)
                    return null;

                for (var i = 0; i < managerPtr->NodeListCount; i++)
                {
                    var node = managerPtr->NodeList[i];
                    if (node == null || node->Type != type) continue;

                    return (T*)node;
                }

                return null;
            }
        }

        public AtkResNode* SearchSimpleNodeByType(NodeType type)
        {
            if ((int)type > 1000)
                throw new ArgumentOutOfRangeException(nameof(type), "不支持非 SimpleNode 类型");

            fixed (AtkUldManager* managerPtr = &manager)
            {
                if (managerPtr == null)
                    return null;

                for (var i = 0; i < managerPtr->NodeListCount; i++)
                {
                    var node = managerPtr->NodeList[i];
                    if (node == null || node->Type != type) continue;

                    return node;
                }

                return null;
            }
        }

        public List<nint> SearchComponentsByType(ComponentType type)
        {
            fixed (AtkUldManager* managerPtr = &manager)
            {
                if (managerPtr == null)
                    return null;

                var result = new List<nint>();

                for (var i = 0; i < managerPtr->NodeListCount; i++)
                {
                    var node = managerPtr->NodeList[i];
                    if (node == null || (uint)node->Type < 1000) continue;

                    var componentNode = (AtkComponentNode*)node;
                    var componentInfo = componentNode->Component->UldManager;
                    var objectInfo    = (AtkUldComponentInfo*)componentInfo.Objects;
                    if (objectInfo == null || objectInfo->ComponentType != type) continue;

                    result.Add((nint)componentNode->Component);
                }

                return result;
            }
        }

        public T* SearchComponentByType<T>(ComponentType type) where T : unmanaged
        {
            fixed (AtkUldManager* managerPtr = &manager)
            {
                if (managerPtr == null)
                    return null;

                for (var i = 0; i < managerPtr->NodeListCount; i++)
                {
                    var node = managerPtr->NodeList[i];
                    if (node == null || (uint)node->Type < 1000) continue;

                    var componentNode = (AtkComponentNode*)node;
                    var componentInfo = componentNode->Component->UldManager;
                    var objectInfo    = (AtkUldComponentInfo*)componentInfo.Objects;
                    if (objectInfo == null || objectInfo->ComponentType != type) continue;

                    return (T*)componentNode->Component;
                }

                return null;
            }
        }

        public AtkComponentBase* SearchComponentByType(ComponentType type)
        {
            fixed (AtkUldManager* managerPtr = &manager)
            {
                if (managerPtr == null)
                    return null;

                for (var i = 0; i < managerPtr->NodeListCount; i++)
                {
                    var node = managerPtr->NodeList[i];
                    if (node == null || (uint)node->Type < 1000) continue;

                    var componentNode = (AtkComponentNode*)node;
                    var componentInfo = componentNode->Component->UldManager;
                    var objectInfo    = (AtkUldComponentInfo*)componentInfo.Objects;
                    if (objectInfo == null || objectInfo->ComponentType != type) continue;

                    return componentNode->Component;
                }

                return null;
            }
        }

        public List<nint> SearchComponentNodesByType(ComponentType type)
        {
            fixed (AtkUldManager* managerPtr = &manager)
            {
                if (managerPtr == null)
                    return null;

                var result = new List<nint>();

                for (var i = 0; i < managerPtr->NodeListCount; i++)
                {
                    var node = managerPtr->NodeList[i];
                    if (node == null || (uint)node->Type < 1000) continue;

                    var componentNode = (AtkComponentNode*)node;
                    var componentInfo = componentNode->Component->UldManager;
                    var objectInfo    = (AtkUldComponentInfo*)componentInfo.Objects;
                    if (objectInfo == null || objectInfo->ComponentType != type) continue;

                    result.Add((nint)componentNode);
                }

                return result;
            }
        }

        public T* SearchComponentNodeByType<T>(ComponentType type) where T : unmanaged
        {
            fixed (AtkUldManager* managerPtr = &manager)
            {
                if (managerPtr == null)
                    return null;

                for (var i = 0; i < managerPtr->NodeListCount; i++)
                {
                    var node = managerPtr->NodeList[i];
                    if (node == null || (uint)node->Type < 1000) continue;

                    var componentNode = (AtkComponentNode*)node;
                    var componentInfo = componentNode->Component->UldManager;
                    var objectInfo    = (AtkUldComponentInfo*)componentInfo.Objects;
                    if (objectInfo == null || objectInfo->ComponentType != type) continue;

                    return (T*)componentNode;
                }

                return null;
            }
        }

        public AtkComponentNode* SearchComponentNodeByType(ComponentType type)
        {
            fixed (AtkUldManager* managerPtr = &manager)
            {
                if (managerPtr == null)
                    return null;

                for (var i = 0; i < managerPtr->NodeListCount; i++)
                {
                    var node = managerPtr->NodeList[i];
                    if (node == null || (uint)node->Type < 1000) continue;

                    var componentNode = (AtkComponentNode*)node;
                    var componentInfo = componentNode->Component->UldManager;
                    var objectInfo    = (AtkUldComponentInfo*)componentInfo.Objects;
                    if (objectInfo == null || objectInfo->ComponentType != type) continue;

                    return componentNode;
                }

                return null;
            }
        }

        public bool IsReady()
        {
            fixed (AtkUldManager* managerPtr = &manager)
            {
                return managerPtr                  != null &&
                       manager.RootNode            != null &&
                       manager.RootNode->ChildNode != null &&
                       manager.NodeList            != null;
            }
        }
    }

    #region 常量

    private const AtkUldManagerResourceFlag REQUIRED_FLAGS =
        AtkUldManagerResourceFlag.Initialized | AtkUldManagerResourceFlag.ArraysAllocated;

    #endregion
}
