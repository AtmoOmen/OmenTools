namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    /// <summary>
    /// Check whether an addon is ready for reading.
    /// </summary>
    /// <param name="UI"></param>
    /// <returns></returns>
    public static bool IsAddonAndNodesReady(AtkUnitBase* UI)
    {
        return UI != null && UI->IsVisible && UI->UldManager.LoadedState == AtkLoadState.Loaded && UI->RootNode != null && UI->RootNode->ChildNode != null && UI->UldManager.NodeList != null;
    }

    /// <summary>
    /// Check whether the UldManager is ready.
    /// </summary>
    /// <param name="Manager"></param>
    /// <returns></returns>
    public static bool IsUldManagerReady(AtkUldManager* Manager)
    {
        return Manager->RootNode != null && Manager->RootNode->ChildNode != null && Manager->NodeList != null;
    }

    /// <summary>
    /// Get the window title of an addon, will prioritize returning the small title.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="windowNodeID"></param>
    /// <param name="textNodeIDs"></param>
    /// <returns></returns>
    public static string GetWindowTitle(AddonArgs args, uint windowNodeID, uint[]? textNodeIDs = null)
    {
        textNodeIDs ??= new uint[] { 3, 4 };

        var UI = (AtkUnitBase*)args.Addon;

        if (!IsAddonAndNodesReady(UI)) return string.Empty;

        var windowNode = (AtkComponentBase*)UI->GetComponentNodeById(windowNodeID);
        if (windowNode == null)
            return string.Empty;

        var bigTitle = windowNode->GetTextNodeById(textNodeIDs[0])->GetAsAtkTextNode()->NodeText.ToString();
        var smallTitle = windowNode->GetTextNodeById(textNodeIDs[1])->GetAsAtkTextNode()->NodeText.ToString();

        var windowTitle = !smallTitle.IsNullOrEmpty() ? smallTitle : bigTitle;

        return windowTitle;
    }

    /// <summary>
    /// Get the window title of an addon, will prioritize returning the small title.
    /// </summary>
    /// <param name="addon"></param>
    /// <param name="windowNodeID"></param>
    /// <param name="textNodeIDs"></param>
    /// <returns></returns>
    public static string GetWindowTitle(nint addon, uint windowNodeID, uint[]? textNodeIDs = null)
    {
        textNodeIDs ??= new uint[] { 3, 4 };

        var UI = (AtkUnitBase*)addon;

        if (!IsAddonAndNodesReady(UI)) return string.Empty;

        var windowNode = (AtkComponentBase*)UI->GetComponentNodeById(windowNodeID);
        if (windowNode == null)
            return string.Empty;

        var bigTitle = windowNode->GetTextNodeById(textNodeIDs[0])->GetAsAtkTextNode()->NodeText.ToString();
        var smallTitle = windowNode->GetTextNodeById(textNodeIDs[1])->GetAsAtkTextNode()->NodeText.ToString();

        var windowTitle = !smallTitle.IsNullOrEmpty() ? smallTitle : bigTitle;

        return windowTitle;
    }

    /// <summary>
    /// Try finding the index of specific SelectString addon entry by the text given.
    /// </summary>
    /// <param name="addon"></param>
    /// <param name="text"></param>
    /// <param name="index"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Try finding the index of specific SelectString addon entry by the texts given.
    /// As long as one text in the list is found, it will return the index.
    /// </summary>
    /// <param name="addon"></param>
    /// <param name="texts"></param>
    /// <param name="index"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Try finding the index of specific SelectIconString addon entry by the text given.
    /// </summary>
    /// <param name="addon"></param>
    /// <param name="text"></param>
    /// <param name="index"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Try finding the index of specific SelectIconString addon entry by the texts given.
    /// As long as one text in the list is found, it will return the index.
    /// </summary>
    /// <param name="addon"></param>
    /// <param name="texts"></param>
    /// <param name="index"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Try finding the index of specific ContextMenu addon entry by the text given.
    /// </summary>
    /// <param name="addon"></param>
    /// <param name="text"></param>
    /// <param name="index"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Try finding the index of specific ContextMenu addon entry by the text given.
    /// As long as one text in the list is found, it will return the index.
    /// </summary>
    /// <param name="addon"></param>
    /// <param name="texts"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool TryScanContextMenuText(AtkUnitBase* addon,IReadOnlyList<string> texts, out int index)
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

    /// <summary>
    /// Try Detecting Whether The Item To Select Is Enabled.
    /// </summary>
    /// <param name="textNodePtr"></param>
    /// <returns></returns>
    public static bool IsSelectItemEnabled(AtkTextNode* textNodePtr)
    {
        var col = textNodePtr->TextColor;
        return col is { A: 0xFF, R: 0xEE, G: 0xE1, B: 0xC5 }
            or { A: 0xFF, R: 0x7D, G: 0x52, B: 0x3B }
            or { A: 0xFF, R: 0xFF, G: 0xFF, B: 0xFF }
            or { A: 0xFF, R: 0xEE, G: 0xE1, B: 0xC5 };
    }
}