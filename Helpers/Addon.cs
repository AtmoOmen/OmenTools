using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public static bool IsScreenReady()
    {
        if (NowLoading != null && NowLoading->IsVisible) return false;
        if (FadeMiddle != null && FadeMiddle->IsVisible) return false;
        if (FadeBack != null && FadeBack->IsVisible) return false;

        return true;
    }

    public static bool TryGetAddonByName<T>(string addonName, out T* addonPtr) where T : unmanaged
    {
        var a = DService.Gui.GetAddonByName(addonName);
        if (a == nint.Zero)
        {
            addonPtr = null;
            return false;
        }

        addonPtr = (T*)a;
        return true;
    }

    public static T* GetAddonByName<T>(string addonName) where T : unmanaged
    {
        var a = DService.Gui.GetAddonByName(addonName);
        if (a == nint.Zero) return null;

        return (T*)a;
    }

    public static AtkUnitBase* GetAddonByName(string name) => GetAddonByName<AtkUnitBase>(name);

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
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[i + 7].String);
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
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[i + 7].String);
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
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[i * 3 + 7].String);
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
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[i * 3 + 7].String);
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
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[i + 7].String);
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
            var currentString = MemoryHelper.ReadStringNullTerminated((nint)addon->AtkValues[i + 7].String);
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
}