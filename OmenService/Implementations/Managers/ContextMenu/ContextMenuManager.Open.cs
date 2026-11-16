using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.OmenService;

public unsafe partial class ContextMenuManager
{
    /// <summary>
    ///     打开自定义右键菜单，并通过原生菜单生命周期阻止所属游戏界面的交互。
    /// </summary>
    /// <param name="args">菜单上下文；所属界面由 <see cref="ContextMenuOpenedArgs.AddonID" /> 指定。</param>
    /// <param name="entries">本次打开时额外添加的菜单条目。</param>
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
                args.Addon->Id,
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

    /// <summary>
    ///     打开物品右键菜单，并在指定父界面时通过原生菜单生命周期阻止其交互。
    /// </summary>
    /// <param name="itemID">菜单目标的物品 ID。</param>
    /// <param name="parentID">所属游戏界面的 ID；未指定时打开不绑定游戏界面的菜单。</param>
    public void OpenItem
    (
        uint         itemID,
        AtkUnitBase* addon
    )
    {
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var row))
            return;

        var args = new ContextMenuOpenedArgs
        {
            InventoryAgentContext = AgentInventoryContext.Instance(),
            Addon                 = addon,
            TargetItemID          = itemID
        };

        List<ContextMenuItem> menus = [];

        if (row.EquipSlotCategory.RowId > 0)
        {
            menus.AddRange
            (
                [
                    // 装备属性对比
                    new()
                    {
                        Name      = LuminaWrapper.GetAddonText(2150),
                        OnClicked = _ => AgentItemComp.Instance()->CompareItem(addon->Id, itemID, 0, 0)
                    },
                    // 试穿
                    new()
                    {
                        Name      = LuminaWrapper.GetAddonText(2426),
                        OnClicked = _ => AgentTryon.TryOn(addon->Id, itemID)
                    }
                ]
            );
        }

        menus.AddRange
        (
            [
                // 查看持有情况
                new()
                {
                    Name      = LuminaWrapper.GetAddonText(4379),
                    OnClicked = _ => ItemFinderModule.Instance()->SearchForItem(itemID)
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
                    OnClicked = _ => AgentRecipeProductList.Instance()->SearchForRecipesUsingItem(itemID)
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
