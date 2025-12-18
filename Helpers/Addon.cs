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

// TODO: 删除制作 Node 方法
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

    public static bool OpenInventoryItemContext(InventoryItem item) => 
        OpenInventoryItemContext(item.Container, (ushort)item.Slot);

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

    public static bool IsAddonAndNodesReady(AtkUnitBase* ui) =>
        ui != null && ui->IsVisible && ui->UldManager.LoadedState == AtkLoadState.Loaded && ui->RootNode != null &&
        ui->RootNode->ChildNode != null && ui->UldManager.NodeList != null;

    public static bool IsUldManagerReady(AtkUldManager* manager) => 
        manager->RootNode != null && manager->RootNode->ChildNode != null && manager->NodeList != null;

    public static string GetWindowTitle(AddonArgs args, uint windowNodeID, uint[]? textNodeIDs = null) => 
        GetWindowTitle(args.Addon, windowNodeID, textNodeIDs);

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
            var currentString = addon->AtkValues[i + 8].String.ToString();
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
            var currentString = addon->AtkValues[i + 8].String.ToString();
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

    public static void SetSize(AtkResNode* node, int? width, int? height)
    {
        if (width is >= ushort.MinValue and <= ushort.MaxValue) 
            node->Width = (ushort)width.Value;
        if (height is >= ushort.MinValue and <= ushort.MaxValue) 
            node->Height = (ushort)height.Value;
        node->DrawFlags |= 0x1;
    }

    public static void SetPosition(AtkResNode* node, float? x, float? y)
    {
        if (x != null) 
            node->X = x.Value;
        if (y != null) 
            node->Y = y.Value;
        
        node->DrawFlags |= 0x1;
    }
    
    public static void SetPosition(AtkResNode* node, Vector2 position) =>
        SetPosition(node, position.X, position.Y);

    public static void SetPosition(AtkUnitBase* addon, float? x, float? y)
    {
        if (x is >= short.MinValue and <= short.MaxValue)
            addon->X = (short)x.Value;
        if (y is >= short.MinValue and <= short.MaxValue)
            addon->Y = (short)y.Value;
    }

    public static void SetPosition(AtkUnitBase* addon, Vector2 position) =>
        SetPosition(addon, position.X, position.Y);
    
    public static void SetPosition<T>(T* node, float? x, float? y) where T : unmanaged =>
        SetPosition((AtkResNode*)node, x, y);

    public static void SetWindowSize(AtkComponentNode* windowNode, ushort? width, ushort? height)
    {
        if (((AtkUldComponentInfo*)windowNode->Component->UldManager.Objects)->ComponentType !=
            ComponentType.Window) return;

        width ??= windowNode->AtkResNode.Width;
        height ??= windowNode->AtkResNode.Height;

        if (width < 64) 
            width = 64;
        if (height < 16) 
            height = 16;

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

    public static void SetSize<T>(T* node, int? w, int? h) where T : unmanaged => 
        SetSize((AtkResNode*)node, w, h);
}
