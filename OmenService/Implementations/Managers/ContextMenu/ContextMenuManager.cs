using System.Collections.Concurrent;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Hooking;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InteropGenerator.Runtime;
using Lumina.Text.ReadOnly;
using OmenTools.Dalamud;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public unsafe class ContextMenuManager : OmenServiceBase<ContextMenuManager>
{
    public ReadOnlySeString? DefaultPrefix { get; set; }

    public ContextMenuManagerConfig Config { get; private set; } = null!;

    #region Hook 定义

    private Hook<RaptureAtkModule.Delegates.OpenAddon>? OpenAddonHook;

    private Hook<AtkUnitBase.Delegates.FireCallback>? FireCallbackHook;

    #endregion

    #region 字段

    private uint? addonContextSubNameID;

    private          ContextMenuFrame?       currentMenu;
    private readonly Stack<ContextMenuFrame> parentMenus = [];
    private          bool                    isNavigating;

    private          ContextMenuOpenedArgs?  currentArgs;
    private readonly ContextMenuItemResolver itemResolver = new();

    private readonly ConcurrentDictionary<ContextMenuEntry, byte> entries = [];

    #endregion

    #region 注册

    public void Reg
    (
        ContextMenuEntry entry
    )
    {
        ArgumentNullException.ThrowIfNull(entry);
        entries.TryAdd(entry, 0);
    }

    public void Unreg
    (
        ContextMenuEntry entry
    ) =>
        entries.TryRemove(entry, out _);

    #endregion

    protected override void Init()
    {
        Config = LoadConfig<ContextMenuManagerConfig>() ?? new();
        itemResolver.Init();

        OpenAddonHook ??= IGameInteropProvider.Instance().HookFromAddress<RaptureAtkModule.Delegates.OpenAddon>
        (
            (nint)RaptureAtkModule.MemberFunctionPointers.OpenAddon,
            OpenAddonDetour
        );
        OpenAddonHook.Enable();

        FireCallbackHook ??= IGameInteropProvider.Instance().HookFromAddress<AtkUnitBase.Delegates.FireCallback>
        (
            (nint)AtkUnitBase.MemberFunctionPointers.FireCallback,
            FireCallbackDetour
        );
        FireCallbackHook.Enable();

        foreach (var name in new[] { "ContextMenu", "AddonContextSub", "AddonContextMenuTitle" })
        {
            IAddonLifecycle.Instance().RegisterListener(AddonEvent.PostHide,    name, OnMenuClosed);
            IAddonLifecycle.Instance().RegisterListener(AddonEvent.PreFinalize, name, OnMenuClosed);
        }
    }

    protected override void Uninit()
    {
        itemResolver.Dispose();
        IAddonLifecycle.Instance().UnregisterListener(OnMenuClosed);
        CloseMenu();

        OpenAddonHook?.Dispose();
        OpenAddonHook = null;

        FireCallbackHook?.Dispose();
        FireCallbackHook = null;

        DefaultPrefix = null;

        ClearMenus();
    }

    #region 菜单注入

    private uint GetAddonContextSubNameID()
    {
        if (addonContextSubNameID is { } id)
            return id;

        var index = 0;

        foreach (var name in RaptureAtkModule.Instance()->AddonNames)
        {
            if (name.EqualToString("AddonContextSub"))
            {
                addonContextSubNameID = (uint)index;
                return addonContextSubNameID.Value;
            }

            index++;
        }

        throw new InvalidOperationException("找不到 AddonContextSub 的名称 ID");
    }

    private ContextMenuFrame CreateMenuFrame
    (
        uint                   addonNameID,
        ReadOnlySpan<AtkValue> nativeValues,
        AgentInterface*        agent,
        ulong                  eventKind,
        ushort                 ownerAddonID,
        int                    depthLayer,
        List<ContextMenuItem>  items,
        bool                   isSubmenu
    )
    {
        const int HEADER_COUNT      = 8;
        var       nativeCount       = (int)nativeValues[0].UInt;
        var       count             = nativeCount + items.Count;
        var       hasNativeDisabled = nativeCount > 0 && nativeValues.Length >= HEADER_COUNT + (nativeCount * 2);
        var       hasDisabled       = hasNativeDisabled || items.Any(item => !item.IsEnabled);
        var frame = new ContextMenuFrame
        (
            HEADER_COUNT +
            (count *
             (hasDisabled ?
                  2 :
                  1))
        )
        {
            AddonNameID  = addonNameID,
            Agent        = agent,
            EventKind    = eventKind,
            OwnerAddonID = ownerAddonID,
            DepthLayer   = depthLayer,
            IsSubmenu    = isSubmenu,
            Items        = [.. items],
            CallbackIDs  = new int[count]
        };

        try
        {
            for (var i = 0; i < HEADER_COUNT; i++)
            {
                var value = nativeValues[i];
                frame.CopyValue(i, &value);
            }

            var returnIndex = isSubmenu ?
                                  6 :
                                  2;
            var submenuIndex = isSubmenu ?
                                   5 :
                                   3;
            var returnMask   = 0u;
            var submenuMask  = 0u;
            var displayIndex = 0;

            for (var i = 0; i < items.Count; i++)
                if (items[i].Priority < 0 && !(isSubmenu && items[i].IsReturn))
                    AddItem(i);

            for (var i = 0; i < nativeCount; i++)
            {
                var value = nativeValues[HEADER_COUNT + i];
                frame.CopyValue(HEADER_COUNT + displayIndex, &value);
                frame.CallbackIDs[displayIndex] = -i - 1;

                if (hasDisabled)
                {
                    var disabled = hasNativeDisabled ?
                                       nativeValues[HEADER_COUNT + nativeCount + i].Int :
                                       0;
                    frame.Values[HEADER_COUNT + count + displayIndex].SetInt(disabled);
                }

                if ((nativeValues[returnIndex].UInt & (1u << i)) != 0)
                    returnMask |= 1u << displayIndex;
                if ((nativeValues[submenuIndex].UInt & (1u << i)) != 0)
                    submenuMask |= 1u << displayIndex;
                displayIndex++;
            }

            for (var i = 0; i < items.Count; i++)
                if (items[i].Priority >= 0 && !(isSubmenu && items[i].IsReturn))
                    AddItem(i);

            if (isSubmenu)
            {
                for (var i = 0; i < items.Count; i++)
                    if (items[i].IsReturn)
                        AddItem(i);
            }

            frame.Values[0].SetUInt((uint)count);
            frame.Values[returnIndex].SetUInt(returnMask);
            frame.Values[submenuIndex].SetUInt(submenuMask);

            if (!isSubmenu && nativeValues[1].Type == AtkValueType.UInt)
            {
                var selectedIndex = Array.IndexOf(frame.CallbackIDs, -(int)nativeValues[1].UInt - 1);
                frame.SelectedIndex = Math.Max(0, selectedIndex);
                frame.Values[1].SetUInt((uint)frame.SelectedIndex);
            }

            return frame;

            void AddItem
            (
                int itemIndex
            )
            {
                var item = items[itemIndex];
                frame.CallbackIDs[displayIndex] = itemIndex;
                SetManagedStringValue(frame.Values + HEADER_COUNT + displayIndex, GetDisplayText(item));
                if (hasDisabled)
                    frame.Values[HEADER_COUNT + count + displayIndex].SetInt
                    (
                        item.IsEnabled ?
                            0 :
                            1
                    );
                if (item.IsReturn)
                    returnMask |= 1u << displayIndex;
                else if (item.Submenu is not null)
                    submenuMask |= 1u << displayIndex;
                displayIndex++;
            }
        }
        catch
        {
            frame.Dispose();
            throw;
        }
    }

    private ushort OpenAddonDetour
    (
        RaptureAtkModule*                     module,
        uint                                  addonNameID,
        uint                                  valueCount,
        AtkValue*                             values,
        AtkModuleInterface.AtkEventInterface* eventInterface,
        ulong                                 eventKind,
        ushort                                parentAddonID,
        int                                   depthLayer
    )
    {
        var addonNames = module->AddonNames.AsSpan();
        if (addonNameID >= addonNames.Length)
            return OpenAddonHook.Original(module, addonNameID, valueCount, values, eventInterface, eventKind, parentAddonID, depthLayer);

        var addonName = addonNames[(int)addonNameID].AsSpan();
        if (!addonName.SequenceEqual("ContextMenu"u8)     &&
            !addonName.SequenceEqual("AddonContextSub"u8) &&
            !addonName.SequenceEqual("AddonContextMenuTitle"u8))
            return OpenAddonHook.Original(module, addonNameID, valueCount, values, eventInterface, eventKind, parentAddonID, depthLayer);

        if (currentMenu is { IsSubmenu: true } previous)
        {
            var wasNavigating = isNavigating;
            isNavigating = true;

            try
            {
                var previousAddon = GetAddonByID(previous.AddonID);
                if (previousAddon is not null)
                    previousAddon->Hide(true, false, 0);
            }
            finally
            {
                isNavigating = wasNavigating;
            }
        }

        ClearMenus();
        var agent = (AgentInterface*)eventInterface;
        var menuType = agent == (AgentInterface*)AgentContext.Instance()          ? ContextMenuType.AgentContext :
                       agent == (AgentInterface*)AgentInventoryContext.Instance() ? ContextMenuType.AgentInventoryContext :
                                                                                    (ContextMenuType?)null;
        if (menuType is null                          ||
            !addonName.SequenceEqual("ContextMenu"u8) ||
            valueCount     < 8                        ||
            values[0].UInt > 31                       ||
            valueCount     < 8 + values[0].UInt)
            return OpenAddonHook.Original(module, addonNameID, valueCount, values, eventInterface, eventKind, parentAddonID, depthLayer);

        ContextMenuFrame frame;

        try
        {
            currentArgs = BuildArgs(agent);
            itemResolver.Resolve(currentArgs, (int)values[0].UInt);

            if (Config.ShowOpenMenuLog)
            {
                DLog.Debug
                (
                    $"[Context Menu Manager] 打开菜单\n"                                                    +
                    $"类型：{menuType}\n"                                                                  +
                    $"Addon：{currentArgs.AddonName ?? "[空]"} ({currentArgs.OwnerAddonID})\n"            +
                    $"目标名称：{currentArgs.TargetName ?? "[空]"}\n"                                         +
                    $"目标 Object ID：0x{currentArgs.TargetObjectID:X}\n"                                  +
                    $"目标 Content ID：{currentArgs.TargetContentID}\n"                                    +
                    $"目标 Home World ID：{currentArgs.TargetHomeWorldID}\n"                               +
                    $"目标角色：0x{(nint)currentArgs.TargetCharacter:X}（玩家: {currentArgs.IsTargetPlayer}）\n" +
                    $"物品 ID：{currentArgs.TargetItemID}\n"                                               +
                    $"幻化物品 ID：{currentArgs.TargetGlamourID}\n"                                          +
                    $"Inventory Type：{currentArgs.TargetInventoryType?.ToString() ?? "[空]"}\n"          +
                    $"Inventory Slot：{currentArgs.TargetSlot?.ToString()          ?? "[空]"}\n"          +
                    $"Agent Context：0x{(nint)currentArgs.DefaultAgentContext:X}\n"                      +
                    $"Inventory Agent Context：0x{(nint)currentArgs.InventoryAgentContext:X}"
                );
            }

            var createdItems = new List<ContextMenuItem>();

            foreach (var entry in OrderedSnapshot())
            {
                var item = entry.Create(currentArgs);
                if (item is null)
                    continue;

                item.Entry = entry;
                createdItems.Add(item);
            }

            var items = FixupMenuList(createdItems, (int)values[0].UInt);
            frame = CreateMenuFrame(addonNameID, new(values, (int)valueCount), agent, eventKind, parentAddonID, depthLayer, items, false);
        }
        catch (Exception ex)
        {
            ClearMenus();
            DLog.Error("[ContextMenuManager] 构建 ContextMenu 时发生错误", ex);
            return OpenAddonHook.Original(module, addonNameID, valueCount, values, eventInterface, eventKind, parentAddonID, depthLayer);
        }

        currentMenu = frame;
        frame.Retain();

        try
        {
            frame.AddonID = OpenAddonHook.Original(module, addonNameID, (uint)frame.ValueCount, frame.Values, eventInterface, eventKind, parentAddonID, depthLayer);
            if (frame.AddonID == 0 && currentMenu == frame)
                ClearMenus();
            return frame.AddonID;
        }
        catch
        {
            if (currentMenu == frame)
                ClearMenus();
            throw;
        }
        finally
        {
            frame.Dispose();
        }
    }

    private static List<ContextMenuItem> FixupMenuList
    (
        List<ContextMenuItem> items,
        int                   nativeMenuSize,
        int                   reservedItems = 0
    )
    {
        const int MAX_MENU_ITEMS = 31;
        var       availableItems = MAX_MENU_ITEMS - nativeMenuSize - reservedItems;
        if (availableItems <= 0)
            return [];

        if (items.Count > availableItems)
        {
            var orderedItems = items.OrderBy(i => i.Priority).ToArray();
            var newItems     = orderedItems[..(availableItems - 1)];
            var submenuItems = orderedItems[(availableItems   - 1)..];

            var entries = submenuItems.Select(ContextMenuEntry (item) => new LocalMenuItemEntry(item)).ToArray();
            return
            [
                .. newItems,
                new ContextMenuItem
                {
                    Name     = "更多功能",
                    Prefix   = "Ⓓ",
                    Priority = int.MaxValue,
                    Submenu = new ContextMenuSubmenu
                    {
                        Title   = "更多功能",
                        Entries = entries
                    }
                }
            ];
        }

        return items;
    }

    private void OpenSubmenu
    (
        ContextMenuFrame   parent,
        ContextMenuSubmenu submenu
    )
    {
        if (currentMenu != parent)
            return;

        var addon = GetAddonByID(parent.AddonID);
        if (addon is null || !addon->IsVisible)
            return;

        var              submenuItems = new List<ContextMenuItem>();
        ContextMenuItem? returnItem   = null;

        foreach (var entry in submenu.Entries)
        {
            var item = entry.Create(currentArgs!);
            if (item is null)
                continue;

            if (entry is not LocalMenuItemEntry)
                item.Entry = entry;
            if (item.IsReturn)
                returnItem ??= item;
            else
                submenuItems.Add(item);
        }

        if (currentMenu != parent)
            return;

        addon = GetAddonByID(parent.AddonID);
        if (addon is null || !addon->IsVisible)
            return;

        submenuItems = FixupMenuList(submenuItems, 0, 1);
        submenuItems.Add
        (
            returnItem ??
            new ContextMenuItem
            {
                Name     = LuminaWrapper.GetAddonTextSeString(2440),
                IsReturn = true
            }
        );

        short x, y;
        addon->GetPosition(&x, &y);
        parent.X = x;
        parent.Y = y;
        var values = stackalloc AtkValue[8];
        new Span<AtkValue>(values, 8).Clear();
        ContextMenuFrame frame;

        try
        {
            values[0].SetUInt(0);
            SetManagedStringValue(&values[1], submenu.Title);
            values[2].SetInt(x);
            values[3].SetInt(y);
            values[4].SetBool(false);
            values[5].SetUInt(0);
            values[6].SetUInt(0);
            values[7].SetUInt(1);
            frame = CreateMenuFrame
            (
                GetAddonContextSubNameID(),
                new(values, 8),
                parent.Agent,
                parent.EventKind,
                parent.OwnerAddonID,
                parent.DepthLayer,
                submenuItems,
                true
            );
        }
        finally
        {
            for (var i = 0; i < 8; i++)
                values[i].Dtor();
        }

        frame.X = x;
        frame.Y = y;

        try
        {
            if (!ShowMenu(frame, addon))
            {
                frame.Dispose();
                return;
            }
        }
        catch
        {
            frame.Dispose();
            throw;
        }

        parentMenus.Push(parent);
        currentMenu = frame;
    }

    private bool ShowMenu
    (
        ContextMenuFrame frame,
        AtkUnitBase*     previousAddon
    )
    {
        var previous      = currentMenu;
        var wasNavigating = isNavigating;
        isNavigating = true;
        frame.Retain();

        try
        {
            if (frame.IsSubmenu)
            {
                frame.Values[2].SetInt(frame.X);
                frame.Values[3].SetInt(frame.Y);
            }
            else
            {
                frame.Values[1].SetUInt((uint)frame.SelectedIndex);
                frame.Values[4].SetUInt(frame.Values[4].UInt | 1u);
            }

            var id = OpenAddonHook.Original
            (
                RaptureAtkModule.Instance(),
                frame.AddonNameID,
                (uint)frame.ValueCount,
                frame.Values,
                &frame.Agent->AtkEventInterface,
                frame.EventKind,
                frame.OwnerAddonID,
                frame.DepthLayer
            );
            if (id == 0 || currentMenu != previous)
                return false;

            frame.AddonID        = id;
            frame.Agent->AddonId = id;
            if (previousAddon is not null && previousAddon->Id != id)
                previousAddon->Hide(true, false, 0);

            var addon = GetAddonByID(id);
            if (addon is not null)
            {
                var (x, y) = GetScreenClampedPosition(addon, frame.X, frame.Y);
                addon->SetPosition(x, y);
            }
            return true;
        }
        finally
        {
            frame.Dispose();
            isNavigating = wasNavigating;
        }
    }

    private static unsafe (short X, short Y) GetScreenClampedPosition
    (
        AtkUnitBase* addon,
        short        x,
        short        y
    )
    {
        var stage = AtkStage.Instance();

        var screenWidth  = (float)stage->ScreenSize.Width;
        var screenHeight = (float)stage->ScreenSize.Height;

        if (stage->IsScreenSizeScaled)
        {
            screenWidth  *= stage->ScreenSizeScale;
            screenHeight *= stage->ScreenSizeScale;
        }

        var maxX = Math.Max(0f, screenWidth  - addon->GetScaledWidth(true));
        var maxY = Math.Max(0f, screenHeight - addon->GetScaledHeight(true));

        return ((short)Math.Clamp((float)x, 0f, maxX),
                (short)Math.Clamp((float)y, 0f, maxY));
    }

    private bool ReturnToParent
    (
        AtkUnitBase* addon
    )
    {
        if (!parentMenus.TryPeek(out var parent))
            return CloseMenu();

        if (ShowMenu(parent, addon))
        {
            var previous = currentMenu;
            currentMenu = parentMenus.Pop();
            previous?.Dispose();
        }

        return false;
    }

    private bool FireCallbackDetour
    (
        AtkUnitBase* addon,
        uint         valueCount,
        AtkValue*    values,
        bool         close
    )
    {
        if (isNavigating && addon->NameString is "ContextMenu" or "AddonContextSub" or "AddonContextMenuTitle")
            return false;

        var frame = currentMenu;

        if (frame is null || frame.AddonID != addon->Id)
            return parentMenus.All(parent => parent.AddonID != addon->Id) && 
                   FireCallbackHook.Original(addon, valueCount, values, close);

        if (valueCount < 2 || values[0].Type != AtkValueType.Int || values[0].Int != 0 || values[1].Type != AtkValueType.Int)
            return FireCallbackHook.Original(addon, valueCount, values, close);

        var selectedIndex = values[1].Int;

        if (selectedIndex < 0)
        {
            if (!frame.IsSubmenu)
                return FireCallbackHook.Original(addon, valueCount, values, close);

            if (valueCount > 2 && values[2].Type == AtkValueType.UInt && values[2].UInt != 0)
            {
                try
                {
                    return ReturnToParent(addon);
                }
                catch (Exception ex)
                {
                    DLog.Error("[ContextMenuManager] 返回 ContextMenu 上级菜单时发生错误", ex);
                    return CloseMenu();
                }
            }

            return CloseMenu();
        }

        if (selectedIndex >= frame.CallbackIDs.Length)
            return false;

        var callbackID = frame.CallbackIDs[selectedIndex];

        if (callbackID < 0)
        {
            values[1].Int = -callbackID - 1;

            try
            {
                return FireCallbackHook.Original(addon, valueCount, values, close);
            }
            finally
            {
                values[1].Int = selectedIndex;
            }
        }

        var item = frame.Items[callbackID];
        if (!item.IsEnabled)
            return false;

        frame.SelectedIndex = selectedIndex;

        try
        {
            if (item.IsReturn)
                return ReturnToParent(addon);

            if (item.Submenu is { } submenu)
            {
                OpenSubmenu(frame, submenu);
                return false;
            }

            item.OnClicked?.Invoke(new ContextMenuItemClickedArgs(currentArgs!.Clone(), x => OpenSubmenu(frame, x)));
        }
        catch (Exception ex)
        {
            DLog.Error("[ContextMenuManager] 处理 ContextMenu 点击时发生错误", ex);
        }

        return currentMenu == frame && CloseMenu();
    }

    private bool CloseMenu()
    {
        var frame = currentMenu;
        if (frame is null)
            return false;

        var wasNavigating = isNavigating;
        isNavigating = true;

        try
        {
            var addon = GetAddonByID(frame.AddonID);
            if (addon is null)
                return false;

            var value = new AtkValue { Type = AtkValueType.Int, Int = -2 };
            return FireCallbackHook.Original(addon, 1, &value, true);
        }
        finally
        {
            if (currentMenu == frame)
                ClearMenus();
            
            isNavigating = wasNavigating;
        }
    }

    private void OnMenuClosed
    (
        AddonEvent type,
        AddonArgs  args
    )
    {
        if (!isNavigating && currentMenu is { } frame && ((AtkUnitBase*)args.Addon.Address)->Id == frame.AddonID)
            ClearMenus();
    }

    private void ClearMenus()
    {
        currentMenu?.Dispose();
        currentMenu = null;
        
        while (parentMenus.TryPop(out var parent))
            parent.Dispose();
        
        currentArgs = null;
    }

    private ReadOnlySeString GetDisplayText
    (
        ContextMenuItem item
    )
    {
        if (item is { IsReturn: true, Entry: null, Prefix: null })
            return item.Name;

        var prefix = item.Prefix ?? item.Entry?.Prefix;
        if (prefix is null && item.Entry?.OmitPrefix != true)
            prefix = DefaultPrefix;

        if (prefix is null)
            return item.Name;

        using var rented  = new RentedSeStringBuilder();
        var       builder = rented.Builder;

        return builder
               .Append(prefix.Value)
               .Append(' ')
               .Append(item.Name)
               .ToReadOnlySeString();
    }

    private static void SetManagedStringValue
    (
        AtkValue*        value,
        ReadOnlySeString text
    )
    {
        var raw    = text.Data.ToArray();
        var buffer = new byte[raw.Length + 1];
        raw.CopyTo(buffer, 0);
        fixed (byte* ptr = buffer)
            value->SetManagedString(new CStringPointer(ptr));
    }

    #endregion

    #region 工具

    private List<ContextMenuEntry> OrderedSnapshot() =>
    [
        .. entries.Keys.OrderBy(e => e.Identifier, StringComparer.Ordinal)
                  .ThenBy(e => e.Priority ?? 0)
    ];

    private static ContextMenuOpenedArgs BuildArgs
    (
        AgentInterface* agent
    )
    {
        var args = new ContextMenuOpenedArgs(agent);

        if (agent == (AgentInterface*)AgentInventoryContext.Instance())
        {
            var inventoryAgent = AgentInventoryContext.Instance();
            args.InventoryAgentContext = inventoryAgent;
            args.OwnerAddonID          = inventoryAgent->OwnerAddonId;
            args.Addon                 = GetAddonByID(inventoryAgent->OwnerAddonId);
            args.TargetInventoryType   = inventoryAgent->TargetInventoryId;
            args.TargetSlot            = inventoryAgent->TargetInventorySlotId;

            if (inventoryAgent->TargetInventorySlot is not null)
            {
                args.TargetInventoryItem = *inventoryAgent->TargetInventorySlot;
                args.TargetItemID        = inventoryAgent->TargetInventorySlot->GetBaseItemId();
                args.TargetGlamourID     = inventoryAgent->TargetInventorySlot->GetGlamourId();
            }
        }
        else if (agent == (AgentInterface*)AgentContext.Instance())
        {
            var contextAgent = AgentContext.Instance();
            args.DefaultAgentContext = contextAgent;
            args.OwnerAddonID        = contextAgent->OwnerAddon;
            args.Addon               = GetAddonByID(contextAgent->OwnerAddon);
            args.TargetObjectID      = contextAgent->TargetObjectId;
            args.TargetContentID     = contextAgent->TargetContentId;
            args.TargetHomeWorldID   = contextAgent->TargetHomeWorldId;
            args.TargetName          = contextAgent->TargetName.ToString();
            args.TargetCharacter     = contextAgent->CurrentContextMenuTarget;
        }

        args.AddonName = args.Addon is null ?
                             null :
                             args.Addon->NameString;
        return args;
    }

    private static AtkUnitBase* GetAddonByID
    (
        uint addonID
    ) =>
        addonID is 0 or 0xFFFF ?
            null :
            RaptureAtkUnitManager.Instance()->GetAddonById((ushort)addonID);

    #endregion

    private enum ContextMenuType
    {
        AgentContext,
        AgentInventoryContext
    }

    private sealed class LocalMenuItemEntry
    (
        ContextMenuItem item
    ) : ContextMenuEntry
    {
        public override string Identifier =>
            nameof(ContextMenuManager);

        public override ContextMenuItem Create
        (
            ContextMenuOpenedArgs args
        ) =>
            item;
    }

    public class ContextMenuManagerConfig : OmenServiceConfig
    {
        public bool ShowOpenMenuLog;

        public void Save() =>
            this.Save(DService.Instance().GetOmenService<ContextMenuManager>());
    }
}
