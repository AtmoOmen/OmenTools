using OmenTools.Infos;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public static bool ContextMenu(IReadOnlyList<string> text)
    {
        if (!IsAddonAndNodesReady(InfosOm.ContextMenu)) return false;
        if (!TryScanContextMenuText(InfosOm.ContextMenu, text, out var index)) return false;

        return ContextMenu(index);
    }

    public static bool ContextMenu(string text)
    {
        if (!IsAddonAndNodesReady(InfosOm.ContextMenu)) return false;
        if (!TryScanContextMenuText(InfosOm.ContextMenu, text, out var index)) return false;

        return ContextMenu(index);
    }

    public static bool ContextMenu(int index)
    {
        if (!IsAddonAndNodesReady(InfosOm.ContextMenu)) return false;

        Callback(InfosOm.ContextMenu, true, 0, index, 0U, 0, 0);
        return true;
    }

    public static bool SelectString(IReadOnlyList<string> text)
    {
        if (!IsAddonAndNodesReady(InfosOm.SelectString)) return false;
        if (!TryScanSelectStringText(InfosOm.SelectString, text, out var index)) return false;

        return SelectString(index);
    }

    public static bool SelectString(string text)
    {
        if (!IsAddonAndNodesReady(InfosOm.SelectString)) return false;
        if (!TryScanSelectStringText(InfosOm.SelectString, text, out var index)) return false;

        return SelectString(index);
    }

    public static bool SelectString(int index)
    {
        if (!IsAddonAndNodesReady(InfosOm.SelectString)) return false;

        Callback(InfosOm.SelectString, true, index);
        return true;
    }

    public static bool SelectIconString(IReadOnlyList<string> text)
    {
        if (!IsAddonAndNodesReady(InfosOm.SelectIconString)) return false;

        if (!TryScanSelectIconStringText(InfosOm.SelectIconString, text, out var index)) return false;
        Callback(InfosOm.SelectIconString, true, index);
        return true;
    }

    public static bool SelectIconString(string text)
    {
        if (!IsAddonAndNodesReady(InfosOm.SelectIconString)) return false;

        if (!TryScanSelectIconStringText(InfosOm.SelectIconString, text, out var index)) return false;

        Callback(InfosOm.SelectIconString, true, index);
        return true;
    }

    public static bool SelectIconString(int index)
    {
        if (!IsAddonAndNodesReady(InfosOm.SelectIconString)) return false;

        Callback(InfosOm.SelectIconString, true, index);
        return true;
    }
}