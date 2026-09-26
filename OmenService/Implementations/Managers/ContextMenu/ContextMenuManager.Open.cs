using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Data;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Interop.Game.Models;

namespace OmenTools.OmenService;

public unsafe partial class ContextMenuManager
{
    // TODO: FFCS
    private static readonly CompSig FindItemSig =
        new("40 55 56 41 54 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B F1");

    private delegate bool FindItemDelegate
    (
        ItemFinderModule* module,
        uint              itemID
    );

    private FindItemDelegate? FindItem;

    public void Open
    (
        ContextMenuOpenedArgs            args,
        IReadOnlyList<ContextMenuEntry>? entries = null
    )
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Agent is null)
            throw new InvalidOperationException("主动打开 ContextMenu 需要指定 Agent");

        CloseMenu();

        var values = stackalloc AtkValue[8];
        new Span<AtkValue>(values, 8).Clear();

        ContextMenuFrame frame;

        try
        {
            currentArgs = args;

            var position  = args.Position ?? ImGui.GetMousePos();
            var positionX = (short)position.X;
            var positionY = (short)position.Y;

            var agentContext = args.DefaultAgentContext;
            var depthLayer = agentContext is null ?
                                 0 :
                                 agentContext->ContextMenuDepthLayer;
            List<ContextMenuEntry> menuEntries = [];
            if (entries is not null)
                menuEntries.AddRange(entries);
            menuEntries.AddRange(OrderedSnapshot());

            var items = FixupMenuList(CreateItems(args, menuEntries), 0);

            frame = CreateMenuFrame
            (
                GetAddonNameID("ContextMenu"),
                new(values, 8),
                args.Agent,
                0, // 与客户端自身打开 ContextMenu 时的 eventKind 一致
                (ushort)args.OwnerAddonID,
                depthLayer,
                items,
                false
            );

            frame.X = positionX;
            frame.Y = positionY;
        }
        catch
        {
            ClearMenus();
            throw;
        }
        finally
        {
            for (var i = 0; i < 8; i++)
                values[i].Dtor();
        }

        try
        {
            if (!ShowMenu(frame, null))
            {
                frame.Dispose();
                ClearMenus();
                return;
            }
        }
        catch
        {
            frame.Dispose();
            ClearMenus();
            throw;
        }

        currentMenu = frame;
    }

    public void OpenItem
    (
        uint itemID,
        uint? parentID = null
    )
    {
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var row))
            return;

        var parentAddonID = parentID ?? 0;

        FindItem ??= FindItemSig.GetDelegate<FindItemDelegate>();

        var args = new ContextMenuOpenedArgs
        {
            InventoryAgentContext = AgentInventoryContext.Instance(),
            TargetItemID          = itemID
        };

        List<ContextMenuItem> menus = [];

        if (row.EquipSlotCategory.RowId > 0)
            menus.AddRange
            (
                [
                    // 装备属性对比
                    new()
                    {
                        Name      = LuminaWrapper.GetAddonText(2150),
                        OnClicked = _ => AgentItemComp.Instance()->CompareItem((ushort)parentAddonID, itemID, 0, 0)
                    },
                    // 试穿
                    new()
                    {
                        Name      = LuminaWrapper.GetAddonText(2426),
                        OnClicked = _ => AgentTryon.TryOn(parentAddonID, itemID)
                    }
                ]
            );

        menus.AddRange
        (
            [
                // 查看持有情况
                new()
                {
                    Name      = LuminaWrapper.GetAddonText(4379),
                    OnClicked = _ => FindItem(ItemFinderModule.Instance(), itemID)
                },
                // 展示道具属性
                new()
                {
                    Name      = LuminaWrapper.GetAddonText(4697),
                    OnClicked = _ => AgentChatLog.Instance()->LinkItem(itemID)
                },
                // 查看它能用来制作什么道具
                new()
                {
                    Name      = LuminaWrapper.GetAddonText(13439),
                    OnClicked = _ => AgentRecipeProductList.Instance()->SearchForRecipesUsingItem(itemID),
                }
            ]
        );
        
        Open
        (
            args,
            [
                new ContextMenuEntryInfo
                (
                    nameof(ContextMenuManager),
                    _ => menus,
                    omitPrefix: true
                )
            ]
        );
    }
}
