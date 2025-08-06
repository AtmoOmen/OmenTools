using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Utility;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public record PartInfo(ushort U, ushort V, ushort Width, ushort Height);

    public static bool TryGetInventoryItems(IEnumerable<InventoryType> targetTypes,
        Func<InventoryItem, bool> predicateFunc, out List<InventoryItem> itemResult)
    {
        itemResult = [];

        var manager = InventoryManager.Instance();
        if(manager == null) return false;

        foreach (var type in targetTypes)
        {
            var container = manager->GetInventoryContainer(type);
            if(container == null || !container->IsLoaded) return false;
            for (var i = 0; i < container->Size; i++)
            {
                var slot = container->GetInventorySlot(i);
                if(slot == null || !predicateFunc(*slot)) continue;

                itemResult.Add(*slot);
            }
        }

        return itemResult.Count > 0;
    }

    public static bool TryGetFirstInventoryItem(IEnumerable<InventoryType> targetTypes, Func<InventoryItem, bool> predicateFunc, out InventoryItem* itemResult)
    {
        itemResult = null;

        var manager = InventoryManager.Instance();
        if (manager == null) return false;

        foreach (var type in targetTypes)
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

    public static bool OpenInventoryItemContext(InventoryItem item)
        => OpenInventoryItemContext(item.Container, (ushort)item.Slot);

    public static bool OpenInventoryItemContext(InventoryType type, ushort slot)
    {
        var agent = AgentInventoryContext.Instance();
        if (agent == null) return false;

        agent->OpenForItemSlot(type, slot, 0, GetActiveInventoryAddonID());
        return true;
    }

    public static uint GetActiveInventoryAddonID()
    {
        if (Inventory == null) return 0;
        if (InventoryLarge == null) return 0;
        if (InventoryExpansion == null) return 0;

        if (IsAddonAndNodesReady(Inventory)) return Inventory->Id;
        if (IsAddonAndNodesReady(InventoryLarge)) return InventoryLarge->Id;
        if (IsAddonAndNodesReady(InventoryExpansion)) return InventoryExpansion->Id;

        return 0;
    }

    public static bool IsInventoryFull(IEnumerable<InventoryType> inventoryTypes)
    {
        var manager = InventoryManager.Instance();
        if (manager == null) return true;

        foreach (var inventoryType in inventoryTypes)
        {
            var container = manager->GetInventoryContainer(inventoryType);
            if (container == null || !container->IsLoaded) continue;

            for (var index = 0; index < container->Size; index++)
            {
                var slot = container->GetInventorySlot(index);
                if (slot->ItemId == 0) return false;
            }
        }

        return true;
    }

    public static void OutlineNode(AtkResNode* node)
    {
        var position = GetNodePosition(node);
        var scale = GetNodeScale(node);
        var size = new Vector2(node->Width, node->Height) * scale;

        var nodeVisible = GetNodeVisible(node);
        position += ImGui.GetMainViewport().Pos;
        ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddRect(position, position + size, nodeVisible ? 0xFF00FF00 : 0xFF0000FF);
    }

    public static Vector2 GetNodePosition(AtkResNode* node)
    {
        var pos = new Vector2(node->X, node->Y);
        pos -= new Vector2(node->OriginX * (node->ScaleX - 1), node->OriginY * (node->ScaleY - 1));
        var par = node->ParentNode;
        while (par != null)
        {
            pos *= new Vector2(par->ScaleX, par->ScaleY);
            pos += new Vector2(par->X, par->Y);
            pos -= new Vector2(par->OriginX * (par->ScaleX - 1), par->OriginY * (par->ScaleY - 1));
            par = par->ParentNode;
        }

        return pos;
    }

    public static Vector2 GetNodeScale(AtkResNode* node)
    {
        if (node == null) return new Vector2(1, 1);
        var scale = new Vector2(node->ScaleX, node->ScaleY);
        while (node->ParentNode != null)
        {
            node = node->ParentNode;
            scale *= new Vector2(node->ScaleX, node->ScaleY);
        }

        return scale;
    }

    public static bool GetNodeVisible(AtkResNode* node)
    {
        if (node == null) return false;
        while (node != null)
        {
            if (!node->IsVisible()) return false;
            node = node->ParentNode;
        }

        return true;
    }

    public static void Callback(nint unitBasePtr, bool updateState, params object[] args)
    {
        var unitBase = unitBasePtr.ToAtkUnitBase();
        if (unitBase == null) return;

        Callback(unitBase, updateState, args);
    }

    public static void Callback(AtkUnitBase* unitBase, bool updateState, params object[] args)
    {
        if (unitBase == null) return;

        using var atkValues = new AtkValueArray(args);
        FireCallback(unitBase, (uint)atkValues.Length, atkValues.Pointer, (byte)(updateState ? 1 : 0));
    }

    public static bool IsScreenReady()
    {
        if (NowLoading != null && NowLoading->IsVisible) return false;
        if (FadeMiddle != null && FadeMiddle->IsVisible) return false;
        if (FadeBack != null && FadeBack->IsVisible) return false;

        return true;
    }

    public static bool TryGetAddonByName(string addonName, out AtkUnitBase* addonPtr)
    {
        var addon = DService.Gui.GetAddonByName(addonName).Address;
        if (addon == nint.Zero)
        {
            addonPtr = null;
            return false;
        }

        addonPtr = (AtkUnitBase*)addon;
        return true;
    }
    
    public static bool TryGetAddonByName<T>(string addonName, out T* addonPtr) where T : unmanaged
    {
        var addon = DService.Gui.GetAddonByName(addonName).Address;
        if (addon == nint.Zero)
        {
            addonPtr = null;
            return false;
        }

        addonPtr = (T*)addon;
        return true;
    }

    public static T* GetAddonByName<T>(string addonName) where T : unmanaged
    {
        var a = DService.Gui.GetAddonByName(addonName).Address;
        if (a == nint.Zero) return null;

        return (T*)a;
    }

    public static AtkUnitBase* GetAddonByName(string name) => 
        GetAddonByName<AtkUnitBase>(name);

    public static bool IsAddonAndNodesReady(AtkUnitBase* UI) =>
        UI != null && UI->IsVisible && UI->UldManager.LoadedState == AtkLoadState.Loaded && UI->RootNode != null &&
        UI->RootNode->ChildNode != null && UI->UldManager.NodeList != null;

    public static bool IsUldManagerReady(AtkUldManager* Manager) 
        => Manager->RootNode != null && Manager->RootNode->ChildNode != null && Manager->NodeList != null;

    public static string GetWindowTitle(AddonArgs args, uint windowNodeID, uint[]? textNodeIDs = null)
        => GetWindowTitle(args.Addon, windowNodeID, textNodeIDs);

    public static string GetWindowTitle(nint addon, uint windowNodeID, uint[]? textNodeIDs = null)
    {
        textNodeIDs ??= [3, 4];

        var UI = (AtkUnitBase*)addon;
        if (UI == null || !IsAddonAndNodesReady(UI)) return string.Empty;

        var windowNode = (AtkComponentNode*)UI->GetNodeById(windowNodeID);
        if (windowNode == null) return string.Empty;

        var bigTitle = windowNode->Component->UldManager.SearchNodeById(textNodeIDs[0])->GetAsAtkTextNode()->NodeText.ExtractText();
        var smallTitle = windowNode->Component->UldManager.SearchNodeById(textNodeIDs[1])->GetAsAtkTextNode()->NodeText.ExtractText();

        var windowTitle = !string.IsNullOrWhiteSpace(smallTitle) ? smallTitle : bigTitle;

        return windowTitle;
    }

    public static bool TryScanSelectStringText(AtkUnitBase* addon, string text, out int index)
    {
        index = -1;
        if (addon == null) return false;

        var entryCount = ((AddonSelectString*)addon)->PopupMenu.PopupMenu.EntryCount;
        for (var i = 0; i < entryCount; i++)
        {
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[i + 7].String.Value);
            if (!currentString.Contains(text, StringComparison.OrdinalIgnoreCase)) continue;

            index = i;
            return true;
        }

        return false;
    }

    public static bool TryScanSelectStringText(AtkUnitBase* addon, IReadOnlyList<string> texts, out int index)
    {
        index = -1;
        if (addon == null) return false;

        var entryCount = ((AddonSelectString*)addon)->PopupMenu.PopupMenu.EntryCount;
        for (var i = 0; i < entryCount; i++)
        {
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[i + 7].String.Value);
            if (!texts.Any(x => currentString.Contains(x, StringComparison.OrdinalIgnoreCase))) continue;

            index = i;
            return true;
        }

        return false;
    }

    public static bool TryScanSelectIconStringText(AtkUnitBase* addon, string text, out int index)
    {
        index = -1;
        if (addon == null) return false;

        var entryCount = ((AddonSelectIconString*)addon)->PopupMenu.PopupMenu.EntryCount;
        for (var i = 0; i < entryCount; i++)
        {
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[(i * 3) + 7].String.Value);
            if (!currentString.Contains(text, StringComparison.OrdinalIgnoreCase)) continue;

            index = i;
            return true;
        }

        return false;
    }

    public static bool TryScanSelectIconStringText(AtkUnitBase* addon, IReadOnlyList<string> texts, out int index)
    {
        index = -1;
        if (addon == null) return false;

        var entryCount = ((AddonSelectIconString*)addon)->PopupMenu.PopupMenu.EntryCount;
        for (var i = 0; i < entryCount; i++)
        {
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[(i * 3) + 7].String.Value);
            if (!texts.Any(x => currentString.Contains(x, StringComparison.OrdinalIgnoreCase))) continue;

            index = i;
            return true;
        }

        return false;
    }

    public static bool TryScanContextMenuText(AtkUnitBase* addon, string text, out int index)
    {
        index = -1;
        if (addon == null) return false;

        var entryCount = addon->AtkValues[0].UInt;
        if (entryCount == 0) return false;

        for (var i = 0; i < entryCount; i++)
        {
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[i + 7].String.Value);
            if (!currentString.Contains(text, StringComparison.OrdinalIgnoreCase)) continue;

            index = i;
            return true;
        }

        return false;
    }

    public static bool TryScanContextMenuText(AtkUnitBase* addon, IReadOnlyList<string> texts, out int index)
    {
        index = -1;
        if (addon == null) return false;

        var entryCount = addon->AtkValues[0].UInt;
        if (entryCount == 0) return false;

        for (var i = 0; i < entryCount; i++)
        {
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[i + 7].String.Value);
            if (!texts.Any(x => currentString.Contains(x, StringComparison.OrdinalIgnoreCase))) continue;

            index = i;
            return true;
        }

        return false;
    }

    public static bool IsSelectItemEnabled(AtkTextNode* textNodePtr)
    {
        var col = textNodePtr->TextColor;
        return col is { A: 0xFF, R: 0xEE, G: 0xE1, B: 0xC5 }
            or { A: 0xFF, R: 0x7D, G: 0x52, B: 0x3B }
            or { A: 0xFF, R: 0xFF, G: 0xFF, B: 0xFF }
            or { A: 0xFF, R: 0xEE, G: 0xE1, B: 0xC5 };
    }

    public static nint Alloc(ulong size) => new(IMemorySpace.GetUISpace()->Malloc(size, 8UL));

    public static nint Alloc(int size)
    {
        if (size <= 0) throw new ArgumentException("待分配内存的大小必须为正数");
        return Alloc((ulong)size);
    }

    public static void SetSize(AtkResNode* node, int? width, int? height)
    {
        if (width is >= ushort.MinValue and <= ushort.MaxValue) node->Width = (ushort)width.Value;
        if (height is >= ushort.MinValue and <= ushort.MaxValue) node->Height = (ushort)height.Value;
        node->DrawFlags |= 0x1;
    }

    public static void SetPosition(AtkResNode* node, float? x, float? y)
    {
        if (x != null) node->X = x.Value;
        if (y != null) node->Y = y.Value;
        node->DrawFlags |= 0x1;
    }

    public static void SetPosition(AtkUnitBase* atkUnitBase, float? x, float? y)
    {
        if (x is >= short.MinValue and <= short.MaxValue) atkUnitBase->X = (short)x.Value;
        if (y >= short.MinValue && x <= short.MaxValue) atkUnitBase->Y = (short)y.Value;
    }

    public static void SetWindowSize(AtkComponentNode* windowNode, ushort? width, ushort? height)
    {
        if (((AtkUldComponentInfo*)windowNode->Component->UldManager.Objects)->ComponentType !=
            ComponentType.Window) return;

        width ??= windowNode->AtkResNode.Width;
        height ??= windowNode->AtkResNode.Height;

        if (width < 64) width = 64;
        if (height < 16) height = 16;

        SetSize(windowNode, width, height);
        var n = windowNode->Component->UldManager.RootNode;
        SetSize(n, width, height);
        n = n->PrevSiblingNode;
        SetSize(n, (ushort)(width - 14), null);
        n = n->PrevSiblingNode;
        SetSize(n, width, height);
        n = n->PrevSiblingNode;
        SetSize(n, width, height);
        n = n->PrevSiblingNode;
        if (DService.GameConfig.System.GetUInt("ColorThemeType") == 3)
            SetSize(n, width - 8, height - 16);
        else
            SetSize(n, width, height);

        n = n->PrevSiblingNode;
        SetSize(n, (ushort)(width - 5), null);
        n = n->ChildNode;
        SetSize(n, (ushort)(width - 20), null);
        n = n->PrevSiblingNode;
        SetPosition(n, width - 33, 6);
        n = n->PrevSiblingNode;
        SetPosition(n, width - 47, 8);
        n = n->PrevSiblingNode;
        SetPosition(n, width - 61, 8);

        windowNode->AtkResNode.DrawFlags |= 0x1;
    }

    public static void SetSize<T>(T* node, int? w, int? h) where T : unmanaged => SetSize((AtkResNode*)node, w, h);

    public static void SetPosition<T>(T* node, float? x, float? y) where T : unmanaged =>
        SetPosition((AtkResNode*)node, x, y);

    public static T* CloneNode<T>(T* original) where T : unmanaged => (T*)CloneNode((AtkResNode*)original);

    public static void ExpandNodeList(AtkComponentNode* componentNode, ushort addSize)
    {
        var newNodeList = ExpandNodeList(componentNode->Component->UldManager.NodeList,
                                         componentNode->Component->UldManager.NodeListCount,
                                         (ushort)(componentNode->Component->UldManager.NodeListCount + addSize));

        componentNode->Component->UldManager.NodeList = newNodeList;
    }

    public static void ExpandNodeList(AtkUnitBase* atkUnitBase, ushort addSize)
    {
        var newNodeList = ExpandNodeList(atkUnitBase->UldManager.NodeList, atkUnitBase->UldManager.NodeListCount,
                                         (ushort)(atkUnitBase->UldManager.NodeListCount + addSize));

        atkUnitBase->UldManager.NodeList = newNodeList;
    }

    private static AtkResNode** ExpandNodeList(AtkResNode** originalList, ushort originalSize, ushort newSize = 0)
    {
        if (newSize <= originalSize) newSize = (ushort)(originalSize + 1);
        var oldListPtr = new nint(originalList);
        var newListPtr = Alloc((ulong)((newSize + 1) * 8));
        var clone = new nint[originalSize];
        Marshal.Copy(oldListPtr, clone, 0, originalSize);
        Marshal.Copy(clone, 0, newListPtr, originalSize);
        return (AtkResNode**)newListPtr;
    }

    public static AtkResNode* CloneNode(AtkResNode* original)
    {
        var size = original->Type switch
        {
            NodeType.Res => sizeof(AtkResNode),
            NodeType.Image => sizeof(AtkImageNode),
            NodeType.Text => sizeof(AtkTextNode),
            NodeType.NineGrid => sizeof(AtkNineGridNode),
            NodeType.Counter => sizeof(AtkCounterNode),
            NodeType.Collision => sizeof(AtkCollisionNode),
            _ => throw new Exception($"不支持的节点类型: {original->Type}"),
        };

        var allocation = Alloc((ulong)size);
        var bytes = new byte[size];
        Marshal.Copy(new nint(original), bytes, 0, bytes.Length);
        Marshal.Copy(bytes, 0, allocation, bytes.Length);

        var newNode = (AtkResNode*)allocation;
        newNode->ParentNode = null;
        newNode->ChildNode = null;
        newNode->ChildCount = 0;
        newNode->PrevSiblingNode = null;
        newNode->NextSiblingNode = null;
        return newNode;
    }

    public static AtkTextNode* MakeTextNode(uint id) => !TryMakeTextNode(id, out var textNode) ? null : textNode;

    public static AtkImageNode* MakeImageNode(uint id, PartInfo partInfo)
    {
        if (!TryMakeImageNode(id, 0, 0, 0, 0, out var imageNode))
        {
            DService.Log.Error("为 AtkImageNode 分配内存时失败");
            return null;
        }

        if (!TryMakePartsList(0, out var partsList))
        {
            DService.Log.Error("为 AtkUldPartsList 分配内存时失败");
            FreeImageNode(imageNode);
            return null;
        }

        if (!TryMakePart(partInfo.U, partInfo.V, partInfo.Width, partInfo.Height, out var part))
        {
            DService.Log.Error("为 AtkUldPart 分配内存时失败");
            FreePartsList(partsList);
            FreeImageNode(imageNode);
            return null;
        }

        if (!TryMakeAsset(0, out var asset))
        {
            DService.Log.Error("为 AtkUldAsset 分配内存时失败");
            FreePart(part);
            FreePartsList(partsList);
            FreeImageNode(imageNode);
        }

        AddAsset(part, asset);
        AddPart(partsList, part);
        AddPartsList(imageNode, partsList);

        return imageNode;
    }

    public static bool TryMakeTextNode(uint id, [NotNullWhen(true)] out AtkTextNode* textNode)
    {
        textNode = IMemorySpace.GetUISpace()->Create<AtkTextNode>();

        if (textNode is null) return false;

        textNode->AtkResNode.Type = NodeType.Text;
        textNode->AtkResNode.NodeId = id;
        return true;
    }


    public static bool TryMakeImageNode(
        uint id, NodeFlags resNodeFlags, uint resNodeDrawFlags, byte wrapMode, byte imageNodeFlags,
        [NotNullWhen(true)] out AtkImageNode* imageNode)
    {
        imageNode = IMemorySpace.GetUISpace()->Create<AtkImageNode>();

        if (imageNode is null) return false;
        imageNode->AtkResNode.Type = NodeType.Image;
        imageNode->AtkResNode.NodeId = id;
        imageNode->AtkResNode.NodeFlags = resNodeFlags;
        imageNode->AtkResNode.DrawFlags = resNodeDrawFlags;
        imageNode->WrapMode = wrapMode;
        imageNode->Flags = imageNodeFlags;

        return true;
    }

    public static bool TryMakePartsList(uint id, [NotNullWhen(true)] out AtkUldPartsList* partsList)
    {
        partsList = (AtkUldPartsList*)IMemorySpace.GetUISpace()->Malloc((ulong)sizeof(AtkUldPartsList), 8);

        if (partsList is null) return false;

        partsList->Id = id;
        partsList->PartCount = 0;
        partsList->Parts = null;
        return true;
    }

    public static bool TryMakePart(
        ushort u, ushort v, ushort width, ushort height, [NotNullWhen(true)] out AtkUldPart* part)
    {
        part = (AtkUldPart*)IMemorySpace.GetUISpace()->Malloc((ulong)sizeof(AtkUldPart), 8);

        if(part is null) return false;

        part->U = u;
        part->V = v;
        part->Width = width;
        part->Height = height;
        return true;
    }

    public static bool TryMakeAsset(uint id, [NotNullWhen(true)] out AtkUldAsset* asset)
    {
        asset = (AtkUldAsset*)IMemorySpace.GetUISpace()->Malloc((ulong)sizeof(AtkUldAsset), 8);

        if(asset is null) return false;

        asset->Id = id;
        asset->AtkTexture.Ctor();
        return true;
    }

    public static void AddPartsList(AtkImageNode* imageNode, AtkUldPartsList* partsList) => imageNode->PartsList = partsList;

    public static void AddPartsList(AtkCounterNode* counterNode, AtkUldPartsList* partsList) => counterNode->PartsList = partsList;

    public static void AddPart(AtkUldPartsList* partsList, AtkUldPart* part)
    {
        var oldPartArray = partsList->Parts;

        var newSize = partsList->PartCount + 1;
        var newArray = (AtkUldPart*)IMemorySpace.GetUISpace()->Malloc((ulong)sizeof(AtkUldPart) * newSize, 8);

        if (oldPartArray is not null)
        {
            foreach (var index in Enumerable.Range(0, (int)partsList->PartCount))
                Buffer.MemoryCopy(oldPartArray + index, newArray + index, sizeof(AtkUldPart), sizeof(AtkUldPart));

            IMemorySpace.Free(oldPartArray, (ulong)sizeof(AtkUldPart) * partsList->PartCount);
        }

        Buffer.MemoryCopy(part, newArray + (newSize - 1), sizeof(AtkUldPart), sizeof(AtkUldPart));
        partsList->Parts = newArray;
        partsList->PartCount = newSize;
    }

    public static void AddAsset(AtkUldPart* part, AtkUldAsset* asset) => part->UldAsset = asset;

    public static void FreeImageNode(AtkImageNode* node)
    {
        node->AtkResNode.Destroy(false);
        IMemorySpace.Free(node, (ulong)sizeof(AtkImageNode));
    }

    public static void FreeTextNode(AtkTextNode* node)
    {
        node->AtkResNode.Destroy(false);
        IMemorySpace.Free(node, (ulong)sizeof(AtkTextNode));
    }

    public static void FreePartsList(AtkUldPartsList* partsList)
    {
        foreach (var index in Enumerable.Range(0, (int)partsList->PartCount))
        {
            var part = &partsList->Parts[index];

            FreeAsset(part->UldAsset);
            FreePart(part);
        }

        IMemorySpace.Free(partsList, (ulong)sizeof(AtkUldPartsList));
    }

    public static void FreePart(AtkUldPart* part) => IMemorySpace.Free(part, (ulong)sizeof(AtkUldPart));

    public static void FreeAsset(AtkUldAsset* asset) => IMemorySpace.Free(asset, (ulong)sizeof(AtkUldAsset));

    public static void LinkNodeAtEnd(AtkResNode* imageNode, AtkUnitBase* parent)
    {
        var node = parent->RootNode->ChildNode;
        while (node->PrevSiblingNode != null) node = node->PrevSiblingNode;

        node->PrevSiblingNode = imageNode;
        imageNode->NextSiblingNode = node;
        imageNode->ParentNode = node->ParentNode;

        parent->UldManager.UpdateDrawNodeList();
    }
    
    public static void LinkNodeAtEnd(AtkResNode* imageNode, AtkComponentBase* parent) 
    {
        var node                                   = parent->UldManager.RootNode;
        while (node->PrevSiblingNode != null) node = node->PrevSiblingNode;

        node->PrevSiblingNode      = imageNode;
        imageNode->NextSiblingNode = node;
        imageNode->ParentNode      = node->ParentNode;
        
        parent->UldManager.UpdateDrawNodeList();
    }

    public static void UnlinkNode<T>(T* atkNode, AtkComponentNode* componentNode) where T : unmanaged
    {
        var node = (AtkResNode*)atkNode;
        if (node == null) return;

        if (node->ParentNode->ChildNode == node) node->ParentNode->ChildNode = node->NextSiblingNode;

        if (node->NextSiblingNode != null && node->NextSiblingNode->PrevSiblingNode == node)
            node->NextSiblingNode->PrevSiblingNode = node->PrevSiblingNode;

        if (node->PrevSiblingNode != null && node->PrevSiblingNode->NextSiblingNode == node)
            node->PrevSiblingNode->NextSiblingNode = node->NextSiblingNode;

        componentNode->Component->UldManager.UpdateDrawNodeList();
    }

    public static void UnlinkAndFreeImageNode(AtkImageNode* node, AtkUnitBase* parent)
    {
        if (node->AtkResNode.PrevSiblingNode is not null)
            node->AtkResNode.PrevSiblingNode->NextSiblingNode = node->AtkResNode.NextSiblingNode;

        if (node->AtkResNode.NextSiblingNode is not null)
            node->AtkResNode.NextSiblingNode->PrevSiblingNode = node->AtkResNode.PrevSiblingNode;

        parent->UldManager.UpdateDrawNodeList();

        FreePartsList(node->PartsList);
        FreeImageNode(node);
    }
    
    public static void UnlinkAndFreeImageNode(AtkImageNode* node, AtkComponentNode* parent)
    {
        if (node->AtkResNode.PrevSiblingNode is not null)
            node->AtkResNode.PrevSiblingNode->NextSiblingNode = node->AtkResNode.NextSiblingNode;

        if (node->AtkResNode.NextSiblingNode is not null)
            node->AtkResNode.NextSiblingNode->PrevSiblingNode = node->AtkResNode.PrevSiblingNode;

        parent->Component->UldManager.UpdateDrawNodeList();

        FreePartsList(node->PartsList);
        FreeImageNode(node);
    }

    public static void UnlinkAndFreeTextNode(AtkTextNode* node, AtkUnitBase* parent)
    {
        if (node->AtkResNode.PrevSiblingNode is not null)
            node->AtkResNode.PrevSiblingNode->NextSiblingNode = node->AtkResNode.NextSiblingNode;

        if (node->AtkResNode.NextSiblingNode is not null)
            node->AtkResNode.NextSiblingNode->PrevSiblingNode = node->AtkResNode.PrevSiblingNode;

        parent->UldManager.UpdateDrawNodeList();
        FreeTextNode(node);
    }

    public static void UnlinkAndFreeTextNode(AtkTextNode* node, AtkComponentNode* parent)
    {
        if(node->AtkResNode.PrevSiblingNode is not null)
            node->AtkResNode.PrevSiblingNode->NextSiblingNode = node->AtkResNode.NextSiblingNode;

        if(node->AtkResNode.NextSiblingNode is not null)
            node->AtkResNode.NextSiblingNode->PrevSiblingNode = node->AtkResNode.PrevSiblingNode;

        parent->Component->UldManager.UpdateDrawNodeList();
        FreeTextNode(node);
    }
}
