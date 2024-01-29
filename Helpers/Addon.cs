namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static unsafe bool IsAddonAndNodesReady(AtkUnitBase* UI)
    {
        return UI != null && UI->IsVisible && UI->UldManager.LoadedState == AtkLoadState.Loaded && UI->RootNode != null && UI->RootNode->ChildNode != null && UI->UldManager.NodeList != null;
    }

    public static unsafe bool IsUldManagerReady(AtkUldManager* Manager)
    {
        return Manager->RootNode != null && Manager->RootNode->ChildNode != null && Manager->NodeList != null;
    }

    public static unsafe string GetWindowTitle(AddonArgs args, uint windowNodeID, uint[]? textNodeIDs = null)
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

    public static unsafe string GetWindowTitle(nint addon, uint windowNodeID, uint[]? textNodeIDs = null)
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
}