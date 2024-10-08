﻿using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Infos;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public static bool ClickContextMenu(IReadOnlyList<string> text)
    {
        if (!IsAddonAndNodesReady(ContextMenu)) return false;
        if (!TryScanContextMenuText(ContextMenu, text, out var index)) return false;

        return ClickContextMenu(index);
    }

    public static bool ClickContextMenu(string text)
    {
        if (!IsAddonAndNodesReady(ContextMenu)) return false;
        if (!TryScanContextMenuText(ContextMenu, text, out var index)) return false;

        return ClickContextMenu(index);
    }

    public static bool ClickContextMenu(int index)
    {
        if (!IsAddonAndNodesReady(ContextMenu)) return false;

        Callback(ContextMenu, true, 0, index, 0U, 0, 0);
        return true;
    }

    public static bool ClickSelectString(IReadOnlyList<string> text)
    {
        if (!IsAddonAndNodesReady(SelectString)) return false;
        if (!TryScanSelectStringText(SelectString, text, out var index)) return false;

        return ClickSelectString(index);
    }

    public static bool ClickSelectString(string text)
    {
        if (!IsAddonAndNodesReady(SelectString)) return false;
        if (!TryScanSelectStringText(SelectString, text, out var index)) return false;

        return ClickSelectString(index);
    }

    public static bool ClickSelectString(int index)
    {
        if (!IsAddonAndNodesReady(SelectString)) return false;

        Callback(SelectString, true, index);
        return true;
    }

    public static bool ClickSelectIconString(IReadOnlyList<string> text)
    {
        if (!IsAddonAndNodesReady(SelectIconString)) return false;
        if (!TryScanSelectIconStringText(SelectIconString, text, out var index)) return false;

        Callback(SelectIconString, true, index);
        return true;
    }

    public static bool ClickSelectIconString(string text)
    {
        if (!IsAddonAndNodesReady(SelectIconString)) return false;

        if (!TryScanSelectIconStringText(SelectIconString, text, out var index)) return false;

        Callback(SelectIconString, true, index);
        return true;
    }

    public static bool ClickSelectIconString(int index)
    {
        if (!IsAddonAndNodesReady(SelectIconString)) return false;

        Callback(SelectIconString, true, index);
        return true;
    }

    public static void ClickAddonComponent(
        AtkComponentBase* unitbase, AtkComponentNode* target, uint which, EventType type, EventData? eventData = null,
        InputData? inputData = null)
    {
        EventData? newEventData = null;
        InputData? newInputData = null;
        if (eventData == null)
            newEventData = EventData.ForNormalTarget(target, unitbase);

        if (inputData == null)
            newInputData = InputData.Empty();

        InvokeReceiveEvent(&unitbase->AtkEventListener, type, which, eventData ?? newEventData!,
            inputData ?? newInputData!);

        newEventData?.Dispose();
        newInputData?.Dispose();
    }
}