using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InteropGenerator.Runtime;
using Lumina.Text.ReadOnly;
using OmenTools.Dalamud;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public unsafe class ContextMenuManager : OmenServiceBase<ContextMenuManager>
{
    public ReadOnlySeString? DefaultPrefix { get; set; }

    #region Hook 定义

    private delegate ushort OpenAddonByAgentDelegate
    (
        AtkModule*      module,
        CStringPointer  addonName,
        int             valueCount,
        AtkValue*       values,
        AgentInterface* agent,
        nint            a7,
        bool            a8
    );
    private Hook<OpenAddonByAgentDelegate>? OpenAddonByAgentHook;

    private delegate bool OnMenuSelectedDelegate
    (
        AddonContextMenu* addon,
        int               selectedIdx,
        byte              a3
    );
    private Hook<OnMenuSelectedDelegate>? OnMenuSelectedHook;

    #endregion

    #region 字段

    private uint? addonContextSubNameID;

    private          AgentInterface*        selectedAgent;
    private          ContextMenuType?       selectedMenuType;
    private          List<ContextMenuItem>? selectedItems;
    private          List<ContextMenuItem>? currentSubmenuItems;
    private          List<ContextMenuItem>  menuItemsInOrder        = [];
    private readonly HashSet<nint>          selectedEventInterfaces = [];
    private readonly List<int>              menuCallbackIds         = [];

    private ContextMenuOpenedArgs? currentArgs;

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
        var atkModuleVTable = (nint*)RaptureAtkModule.StaticVirtualTablePointer;
        OpenAddonByAgentHook ??= IGameInteropProvider.Instance().HookFromAddress<OpenAddonByAgentDelegate>
        (
            atkModuleVTable[22],
            OpenAddonByAgentDetour
        );
        OpenAddonByAgentHook.Enable();

        OnMenuSelectedHook ??= IGameInteropProvider.Instance().HookFromAddress<OnMenuSelectedDelegate>
        (
            (nint)AddonContextMenu.StaticVirtualTablePointer->OnMenuSelected,
            OnMenuSelectedDetour
        );
        OnMenuSelectedHook.Enable();
    }

    protected override void Uninit()
    {
        OpenAddonByAgentHook?.Dispose();
        OpenAddonByAgentHook = null;

        OnMenuSelectedHook?.Dispose();
        OnMenuSelectedHook = null;

        DefaultPrefix = null;

        selectedItems       = null;
        currentSubmenuItems = null;
        currentArgs         = null;
        menuCallbackIds.Clear();
    }

    #region 菜单注入

    private uint GetAddonContextSubNameID()
    {
        if (addonContextSubNameID is { } id)
            return id;

        id = 0;
        var index = 0;

        foreach (var name in RaptureAtkModule.Instance()->AddonNames)
        {
            if (name.EqualToString("AddonContextSub"))
            {
                id = (uint)index;
                break;
            }

            index++;
        }

        addonContextSubNameID = id;
        return id;
    }

    private static AtkValue* ExpandContextMenuArray
    (
        Span<AtkValue> oldValues,
        int            newSize
    )
    {
        if (oldValues.Length >= newSize)
            return (AtkValue*)Unsafe.AsPointer(ref oldValues[0]);

        var size     = (sizeof(AtkValue) * newSize) + 8;
        var newArray = (nint)IMemorySpace.GetUISpace()->Malloc((ulong)size, 0);
        if (newArray == nint.Zero)
            throw new OutOfMemoryException();
        NativeMemory.Fill((void*)newArray, (nuint)size, 0);

        *(ulong*)newArray = (ulong)newSize;

        if (!oldValues.IsEmpty)
            oldValues.CopyTo(new((void*)(newArray + 8), oldValues.Length));

        return (AtkValue*)(newArray + 8);
    }

    private static void FreeExpandedContextMenuArray
    (
        AtkValue* newValues,
        int       newSize
    ) =>
        IMemorySpace.Free((void*)((nint)newValues - 8), (ulong)((newSize * sizeof(AtkValue)) + 8));

    private static AtkValue* CreateEmptySubmenuContextMenuArray
    (
        ReadOnlySeString name,
        int              x,
        int              y,
        out int          valueCount
    )
    {
        valueCount = 8;
        var values = ExpandContextMenuArray([], valueCount);
        values[0].SetUInt(0);
        SetManagedStringValue(&values[1], name);
        values[2].SetInt(x);
        values[3].SetInt(y);
        values[4].SetBool(false);
        values[5].SetUInt(0);
        values[6].SetUInt(0);
        values[7].SetUInt(1);
        return values;
    }

    private void SetupGenericMenu
    (
        int                            headerCount,
        int                            sizeHeaderIdx,
        int                            returnHeaderIdx,
        int                            submenuHeaderIdx,
        IReadOnlyList<ContextMenuItem> items,
        ref int                        valueCount,
        ref AtkValue*                  values
    )
    {
        var prefixItems = items.Select((item, idx) => new { Item = item, Idx = idx }).Where(x => x.Item.Priority < 0).ToArray();
        var suffixItems = items.Select((item, idx) => new { Item = item, Idx = idx }).Where(x => x.Item.Priority >= 0).ToArray();

        var nativeMenuSize = (int)values[sizeHeaderIdx].UInt;
        var prefixMenuSize = prefixItems.Length;
        var suffixMenuSize = suffixItems.Length;

        var hasGameDisabled   = valueCount - headerCount - nativeMenuSize > 0;
        var hasCustomDisabled = items.Any(item => !item.IsEnabled);
        var hasAnyDisabled    = hasGameDisabled || hasCustomDisabled;

        values = ExpandContextMenuArray
        (
            new(values, valueCount),
            valueCount = ((nativeMenuSize + items.Count) *
                          (hasAnyDisabled ?
                               2 :
                               1)) +
                         headerCount
        );
        var offsetData = new Span<AtkValue>(values,               headerCount);
        var nameData   = new Span<AtkValue>(values + headerCount, nativeMenuSize + items.Count);
        var disabledData = hasAnyDisabled ?
                               new Span<AtkValue>(values + headerCount + nativeMenuSize + items.Count, nativeMenuSize + items.Count) :
                               [];

        var returnMask  = offsetData[returnHeaderIdx].UInt;
        var submenuMask = offsetData[submenuHeaderIdx].UInt;

        nameData[..nativeMenuSize].CopyTo(nameData.Slice(prefixMenuSize, nativeMenuSize));

        if (hasAnyDisabled)
        {
            if (hasGameDisabled)
            {
                var oldDisabledData = new Span<AtkValue>(values + headerCount + nativeMenuSize, nativeMenuSize);
                oldDisabledData.CopyTo(disabledData.Slice(prefixMenuSize, nativeMenuSize));
            }
            else
            {
                for (var i = prefixMenuSize; i < prefixMenuSize + nativeMenuSize; ++i)
                    disabledData[i].SetInt(0);
            }
        }

        returnMask  <<= prefixMenuSize;
        submenuMask <<= prefixMenuSize;

        void FillData
        (
            Span<AtkValue>  disabledData,
            Span<AtkValue>  nameData,
            int             i,
            ContextMenuItem item,
            int             idx
        )
        {
            menuCallbackIds.Add(idx);

            if (hasAnyDisabled)
            {
                disabledData[i].SetInt
                (
                    item.IsEnabled ?
                        0 :
                        1
                );
            }

            if (item.IsReturn)
                returnMask |= 1u << i;
            if (item.Submenu is not null)
                submenuMask |= 1u << i;

            SetManagedStringValue((AtkValue*)Unsafe.AsPointer(ref nameData[i]), GetDisplayText(item));
        }

        for (var i = 0; i < prefixMenuSize; ++i)
        {
            var entry = prefixItems[i];
            FillData(disabledData, nameData, i, entry.Item, entry.Idx);
        }

        menuCallbackIds.AddRange(Enumerable.Range(0, nativeMenuSize).Select(i => -i - 1));

        for (var i = prefixMenuSize + nativeMenuSize; i < prefixMenuSize + nativeMenuSize + suffixMenuSize; ++i)
        {
            var entry = suffixItems[i - prefixMenuSize - nativeMenuSize];
            FillData(disabledData, nameData, i, entry.Item, entry.Idx);
        }

        offsetData[returnHeaderIdx].UInt  =  returnMask;
        offsetData[submenuHeaderIdx].UInt =  submenuMask;
        offsetData[sizeHeaderIdx].UInt    += (uint)items.Count;

        menuItemsInOrder = [.. items];
    }

    private void SetupContextMenu
    (
        IReadOnlyList<ContextMenuItem> items,
        ref int                        valueCount,
        ref AtkValue*                  values
    ) =>
        SetupGenericMenu(8, 0, 2, 3, items, ref valueCount, ref values);

    private void SetupContextSubMenu
    (
        IReadOnlyList<ContextMenuItem> items,
        ref int                        valueCount,
        ref AtkValue*                  values
    ) =>
        SetupGenericMenu(8, 0, 6, 5, items, ref valueCount, ref values);

    private ushort OpenAddonByAgentDetour
    (
        AtkModule*      module,
        CStringPointer  addonName,
        int             valueCount,
        AtkValue*       values,
        AgentInterface* agent,
        nint            a7,
        bool            a8
    )
    {
        var oldValues     = values;
        var addonNameSpan = addonName.AsSpan();

        if (addonNameSpan.SequenceEqual("ContextMenu"u8))
        {
            menuCallbackIds.Clear();
            selectedAgent = agent;
            selectedEventInterfaces.Clear();

            if (selectedAgent == (AgentInterface*)AgentInventoryContext.Instance())
                selectedMenuType = ContextMenuType.Inventory;
            else if (selectedAgent == (AgentInterface*)AgentContext.Instance())
            {
                selectedMenuType = ContextMenuType.Default;

                var menu     = AgentContext.Instance()->CurrentContextMenu;
                var handlers = menu->EventHandlers;
                var ids      = menu->EventIds;
                var count    = (int)values[0].UInt;
                handlers = handlers.Slice(7, count);
                ids      = ids.Slice(7, count);

                for (var i = 0; i < count; ++i)
                {
                    if (ids[i] <= 106)
                        continue;
                    selectedEventInterfaces.Add((nint)handlers[i].Value);
                }
            }
            else
                selectedMenuType = null;

            if (selectedMenuType is not null)
            {
                currentArgs = BuildArgs(selectedAgent);
                var createdItems = new List<ContextMenuItem>();

                foreach (var entry in OrderedSnapshot())
                {
                    var item = entry.Create(currentArgs);

                    if (item is not null)
                    {
                        item.Entry = entry;
                        createdItems.Add(item);
                    }
                }

                selectedItems = FixupMenuList(createdItems, (int)values[0].UInt);
                SetupContextMenu(selectedItems, ref valueCount, ref values);
            }
            else
                selectedItems = null;

            currentSubmenuItems = null;
        }
        else if (addonNameSpan.SequenceEqual("AddonContextSub"u8))
        {
            menuCallbackIds.Clear();

            if (currentSubmenuItems is { } submenuItems)
            {
                currentSubmenuItems = FixupMenuList(submenuItems.ToList(), (int)values[0].UInt);
                SetupContextSubMenu(currentSubmenuItems, ref valueCount, ref values);
            }
        }
        else if (addonNameSpan.SequenceEqual("AddonContextMenuTitle"u8))
            menuCallbackIds.Clear();

        var ret = OpenAddonByAgentHook.Original(module, addonName, valueCount, values, agent, a7, a8);
        if (values != oldValues)
            FreeExpandedContextMenuArray(values, valueCount);
        return ret;
    }

    private static List<ContextMenuItem> FixupMenuList
    (
        List<ContextMenuItem> items,
        int                   nativeMenuSize
    )
    {
        const int maxMenuItems = 31;

        if (items.Count + nativeMenuSize > maxMenuItems)
        {
            var orderedItems = items.OrderBy(i => i.Priority).ToArray();
            var newItems     = orderedItems[..(maxMenuItems - nativeMenuSize - 1)];
            var submenuItems = orderedItems[(maxMenuItems   - nativeMenuSize - 1)..];

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
        ReadOnlySeString                name,
        IReadOnlyList<ContextMenuEntry> submenuEntries,
        int                             posX,
        int                             posY
    )
    {
        var submenuItems = new List<ContextMenuItem>();

        foreach (var entry in submenuEntries)
        {
            var item = entry.Create(currentArgs!);

            if (item is not null)
            {
                item.Entry = entry;
                submenuItems.Add(item);
            }
        }

        currentSubmenuItems = submenuItems;

        var module = RaptureAtkModule.Instance();
        var values = CreateEmptySubmenuContextMenuArray(name, posX, posY, out var valueCount);

        switch (selectedMenuType)
        {
            case ContextMenuType.Default:
            {
                var ownerAddonID = ((AgentContext*)selectedAgent)->OwnerAddon;
                module->OpenAddon(GetAddonContextSubNameID(), (uint)valueCount, values, &selectedAgent->AtkEventInterface, 71, checked((ushort)ownerAddonID), 4);
                break;
            }

            case ContextMenuType.Inventory:
            {
                var ownerAddonID = ((AgentInventoryContext*)selectedAgent)->OwnerAddonId;
                module->OpenAddon(GetAddonContextSubNameID(), (uint)valueCount, values, &selectedAgent->AtkEventInterface, 0, checked((ushort)ownerAddonID), 4);
                break;
            }
        }

        FreeExpandedContextMenuArray(values, valueCount);
    }

    private bool OnMenuSelectedDetour
    (
        AddonContextMenu* addon,
        int               selectedIdx,
        byte              a3
    )
    {
        var items = currentSubmenuItems ?? selectedItems;
        if (items == null)
            goto original;
        if (menuCallbackIds.Count == 0)
            goto original;
        if (selectedIdx < 0)
            goto original;
        if (selectedIdx >= menuCallbackIds.Count)
            goto original;

        var callbackID = menuCallbackIds[selectedIdx];

        if (callbackID < 0)
            selectedIdx = -callbackID - 1;
        else
        {
            var item          = menuItemsInOrder[callbackID];
            var openedSubmenu = false;

            try
            {
                short x, y;
                addon->AtkUnitBase.GetPosition(&x, &y);
                var posX = x;
                var posY = y;

                if (item.Submenu is { } submenu)
                {
                    OpenSubmenu(submenu.Title, submenu.Entries, posX, posY);
                    openedSubmenu = true;
                }
                else
                {
                    item.OnClicked?.Invoke
                    (
                        new ContextMenuItemClickedArgs(currentArgs!.Clone(), clickedSubmenu => OpenSubmenu(clickedSubmenu.Title, clickedSubmenu.Entries, posX, posY))
                    );
                }
            }
            catch (Exception ex)
            {
                DLog.Error("处理 ContextMenu 点击时发生错误", ex);
            }

            if (!openedSubmenu)
                addon->AtkUnitBase.FireCallbackInt(-2);
            return false;
        }

        original:
        return OnMenuSelectedHook.Original(addon, selectedIdx, a3);
    }

    private ReadOnlySeString GetDisplayText
    (
        ContextMenuItem item
    )
    {
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
            args.OwnerAddonID      = inventoryAgent->OwnerAddonId;
            args.Addon             = GetAddonByID(inventoryAgent->OwnerAddonId);
            args.TargetInventoryID = inventoryAgent->TargetInventoryId;
            args.TargetSlot        = inventoryAgent->TargetInventorySlotId;
            if (inventoryAgent->TargetInventorySlot is not null)
                args.TargetItem = *inventoryAgent->TargetInventorySlot;
        }
        else if (agent == (AgentInterface*)AgentContext.Instance())
        {
            var contextAgent = AgentContext.Instance();
            args.OwnerAddonID      = contextAgent->OwnerAddon;
            args.Addon             = GetAddonByID(contextAgent->OwnerAddon);
            args.TargetObjectID    = contextAgent->TargetObjectId;
            args.TargetContentID   = contextAgent->TargetContentId;
            args.TargetHomeWorldID = contextAgent->TargetHomeWorldId;
            args.TargetName        = contextAgent->TargetName.ToString();
            args.TargetCharacter   = contextAgent->CurrentContextMenuTarget;
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
        Default,
        Inventory
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
}
