using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

public static unsafe class AtkUldManagerExtension
{
    extension(scoped ref AtkUldManager manager)
    {
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

        public bool IsUldManagerReady()
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
}
