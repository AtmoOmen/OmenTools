using Dalamud.Hooking;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Models;

namespace OmenTools.OmenService;

internal sealed unsafe class ContextMenuItemResolver : IDisposable
{
    private static readonly CompSig BuildItemMenuSig = new
        ("40 53 56 41 55 41 56 41 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 45 0F B6 E9");

    private delegate byte BuildItemMenuDelegate
    (
        nint          handler,
        AgentContext* context,
        uint          itemID,
        byte          stain0,
        byte          stain1,
        ushort        ownerAddonID,
        ushort        flags,
        byte          clearMenu,
        ushort        extraFlags
    );

    private Hook<BuildItemMenuDelegate>? buildItemMenuHook;
    
    private (nint Handler, nint Context, uint OwnerAddonID, uint ItemID) pendingItem;

    public void Init()
    {
        buildItemMenuHook = IGameInteropProvider.Instance().HookFromAddress<BuildItemMenuDelegate>
        (
            BuildItemMenuSig.ScanText(),
            BuildItemMenuDetour
        );
        buildItemMenuHook.Enable();
    }

    public void Dispose()
    {
        buildItemMenuHook?.Dispose();
        buildItemMenuHook = null;
        pendingItem       = default;
    }

    private byte BuildItemMenuDetour
    (
        nint          handler,
        AgentContext* context,
        uint          itemID,
        byte          stain0,
        byte          stain1,
        ushort        ownerAddonID,
        ushort        flags,
        byte          clearMenu,
        ushort        extraFlags
    )
    {
        var result = buildItemMenuHook!.Original(handler, context, itemID, stain0, stain1, ownerAddonID, flags, clearMenu, extraFlags);
        pendingItem = result != 0 ?
                          (handler, (nint)context, ownerAddonID, itemID) :
                          default;
        return result;
    }

    public void Resolve
    (
        ContextMenuOpenedArgs args,
        int                   nativeMenuSize
    )
    {
        var captured = pendingItem;
        pendingItem = default;

        if (args.InventoryAgentContext is not null)
            return;

        var context = args.DefaultAgentContext;
        if (context == null || context->CurrentContextMenu == null)
            return;

        if (captured.Context      == (nint)context     &&
            captured.OwnerAddonID == args.OwnerAddonID &&
            HasHandler(context, nativeMenuSize, captured.Handler))
        {
            args.TargetItemID = ItemUtil.GetBaseId(captured.ItemID).ItemId;
            return;
        }

        var recipeContext = AgentRecipeItemContext.Instance();

        if (recipeContext != null && HasHandler(context, nativeMenuSize, (nint)recipeContext))
        {
            args.TargetItemID = ItemUtil.GetBaseId(recipeContext->ResultItemId).ItemId;
            return;
        }

        uint itemID    = 0;
        uint glamourID = 0;

        switch (args.AddonName)
        {
            case "ChatLog":
            {
                var agent = AgentChatLog.Instance();
                if (agent != null)
                    itemID = agent->ContextItemId;
                break;
            }
            case "MiragePrismMiragePlate":
            {
                var agent = AgentMiragePrismPrismItemDetail.Instance();
                if (agent != null)
                    itemID = agent->ItemId;
                break;
            }
            case "ColorantColoring":
            {
                var addon = args.Addon;
                if (addon == null || addon->AtkValuesCount <= 18)
                    break;

                var stainID = addon->AtkValues[18].Int switch
                {
                    0 => addon->AtkValues[14].UInt,
                    1 => addon->AtkValues[15].UInt,
                    _ => 0U
                };
                if (stainID.ToLuminaRowRef<Stain>().TryGetValue(out var stain))
                    itemID = stain.Item[0].RowId;
                break;
            }
            case "CabinetWithdraw":
            {
                var agent = AgentCabinetWithdraw.Instance();
                if (agent != null && agent->Data != null)
                    itemID = agent->Data->ContextMenuSelectedItemId;
                break;
            }
            case "CharacterInspect":
            {
                var agent = AgentInspect.Instance();
                if (agent == null || !HasHandler(context, nativeMenuSize, (nint)agent, 1, 4))
                    break;

                var index = agent->SelectedItemSlot;
                if ((uint)index >= agent->Items.Length)
                    break;

                var item = agent->Items[index];
                itemID    = item.ItemId;
                glamourID = item.GlamourItemId;
                break;
            }
            case "MiragePrismPrismBox":
            case "MiragePrismPrismBoxCrystallize":
            {
                var agent = AgentMiragePrismPrismBox.Instance();
                if (agent == null || agent->Data == null)
                    break;

                var data = agent->Data;

                if (HasHandler(context, nativeMenuSize, (nint)agent, 19, 23))
                {
                    var index = data->CrystallizeItemIndex;
                    if (index < data->CrystallizeItemCount && index < data->CrystallizeItems.Length)
                        itemID = data->CrystallizeItems[index].ItemId;
                }
                else if (HasHandler(context, nativeMenuSize, (nint)agent, 8, 17))
                    itemID = data->TempContextItem.ItemId;

                break;
            }
            case "RecipeNote":
            {
                var agent = AgentRecipeNote.Instance();
                if (agent != null)
                    itemID = agent->ContextMenuResultItemId;
                break;
            }
            case "JournalAccept":
            {
                var agent = AgentJournalAccept.Instance();
                if (agent != null && (uint)agent->ContextMenuSelectedRewardIndex < agent->RewardItems.Count)
                    itemID = agent->RewardItems[agent->ContextMenuSelectedRewardIndex].ItemId;
                break;
            }
            case "GuildLeve":
            {
                var agent = AgentLeveQuest.Instance();
                if (agent != null && (uint)agent->ContextMenuSelectedRewardIndex < agent->RewardItems.Count)
                    itemID = agent->RewardItems[agent->ContextMenuSelectedRewardIndex].ItemId;
                break;
            }
            case "NeedGreed":
            {
                var agent = AgentLoot.Instance();
                var loot  = Loot.Instance();
                if (agent != null && loot != null && (uint)agent->SelectedSlotIndex < loot->Items.Length)
                    itemID = loot->Items[agent->SelectedSlotIndex].ItemId;
                break;
            }
            case "ContentsFinder":
            {
                var agent = AgentContentsFinder.Instance();
                if (agent != null && agent->RewardContextMenuHandler.SelectedReward != null)
                    itemID = agent->RewardContextMenuHandler.SelectedReward->ItemId;
                break;
            }
            case "Journal":
            {
                var agent = AgentQuestJournal.Instance();
                if (agent != null)
                    itemID = agent->ContextMenuSelectedItemId;
                break;
            }
            case "Achievement":
            {
                var agent = AgentAchievement.Instance();
                if (agent != null && HasHandler(context, nativeMenuSize, (nint)agent, 65537, 65540))
                    itemID = agent->ContextMenuSelectedItemId;
                break;
            }
            case "MateriaAttach":
            {
                var agent = AgentMateriaAttach.Instance();
                if (agent == null || agent->Data == null || !HasHandler(context, nativeMenuSize, (nint)agent, 3, 3))
                    break;

                var index = agent->SelectedMateriaIndex;
                if ((uint)index >= agent->MateriaCount || agent->Data->MateriaArraySorted == null)
                    break;

                var entry = agent->Data->MateriaArraySorted[index];
                if (entry != null && entry->Item != null)
                    itemID = entry->Item->GetBaseItemId();
                break;
            }
        }

        args.TargetItemID    = ItemUtil.GetBaseId(itemID).ItemId;
        args.TargetGlamourID = ItemUtil.GetBaseId(glamourID).ItemId;
    }

    private static bool HasHandler
    (
        AgentContext* context,
        int           nativeMenuSize,
        nint          handler,
        long          minimumParam = long.MinValue,
        long          maximumParam = long.MaxValue
    )
    {
        if (handler == 0)
            return false;

        var menu  = context->CurrentContextMenu;
        var count = Math.Min(nativeMenuSize, menu->EventHandlers.Length - 8);
        for (var i = 8; i < 8 + count; i++)
            if (menu->EventIds[i]                  > 106           &&
                (nint)menu->EventHandlers[i].Value == handler      &&
                menu->EventHandlerParams[i]        >= minimumParam &&
                menu->EventHandlerParams[i]        <= maximumParam)
                return true;

        return false;
    }
}
