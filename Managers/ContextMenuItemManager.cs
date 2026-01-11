using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using OmenTools.Abstracts;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace OmenTools.Managers;

public unsafe class ContextMenuItemManager : OmenServiceBase<ContextMenuItemManager>
{
    #region 公共接口

    /// <summary>
    ///     当前右键菜单关联的物品ID
    /// </summary>
    public uint CurrentItemID { get; private set; }

    /// <summary>
    ///     当前幻化物品ID（如果有）
    /// </summary>
    public uint CurrentGlamourID { get; private set; }

    /// <summary>
    ///     当前是否有有效的物品ID
    /// </summary>
    public bool IsValidItem => CurrentItemID > 0;

    /// <summary>
    ///     获取当前物品的Item对象
    /// </summary>
    public Item? CurrentItem =>
        CurrentItemID > 0 && LuminaGetter.TryGetRow<Item>(CurrentItemID, out var item) ? item : null;

    /// <summary>
    ///     获取当前幻化物品的Item对象
    /// </summary>
    public Item? CurrentGlamourItem =>
        CurrentGlamourID > 0 && LuminaGetter.TryGetRow<Item>(CurrentGlamourID, out var item) ? item : null;

    /// <summary>
    ///     获取在MiragePrismPrismBoxCrystallize场景下扫描到的特殊物品
    /// </summary>
    public Item? GetPrismBoxItem(IMenuOpenedArgs args)
    {
        if (args.AddonName != "MiragePrismPrismBoxCrystallize") return null;

        if (!ContextMenuAddon->IsAddonAndNodesReady()) return null;

        return TryScanContextMenuText(LuminaWrapper.GetAddonText(11900), out _) ? LastPrismBoxItem : null;
    }

    #endregion

    #region 私有字段

    private static readonly string[] Addons =
    [
        "CabinetWithdraw",
        "Shop",
        "InclusionShop",
        "CollectablesShop",
        "FreeCompanyExchange",
        "FreeCompanyCreditShop",
        "ShopExchangeCurrency",
        "ShopExchangeItem",
        "SkyIslandExchange",
        "TripleTriadCoinExchange",
        "FreeCompanyChest",
        "MJIDisposeShop",
        "GrandCompanyExchange",
        "ReconstructionBuyback",
        "ShopExchangeCoin",
        "MiragePrismPrismBoxCrystallize",
        "ItemSearch",
        "GrandCompanySupplyList"
    ];
    
    private delegate AtkValue* MiragePrismBoxReceiveEventDelegate(
        AgentMiragePrismPrismBox* agent,
        AtkValue*                 returnValue,
        AtkValue*                 values,
        uint                      valueCount,
        ulong                     eventKind);
    private Hook<MiragePrismBoxReceiveEventDelegate>? MiragePrismBoxReceiveEventHook;

    private delegate AtkValue* AchievementReceiveEventDelegate(AgentAchievement* a1, AtkValue* returnValue, AtkValue* values, uint valueCount, ulong eventKind);
    private Hook<AchievementReceiveEventDelegate>? AchievementReceiveEventHook;

    private delegate AtkValue* MateriaAttachReceiveEventDelegate(AgentMateriaAttach* a1, AtkValue* returnValue, AtkValue* values, uint valueCount, ulong eventKind);
    private Hook<MateriaAttachReceiveEventDelegate>? MateriaAttachReceiveEventHook;
    
    private TaskHelper? TaskHelper;
    
    private Item? LastItem;
    private Item? LastPrismBoxItem;
    private Item? LastGlamourItem;
    private uint  LastHoveredItemID;
    private uint  LastDetailItemID;

    private bool IsOnItemHover;
    private bool IsOnItemDetail;

    private readonly HashSet<InventoryItem> CharacterInspectItems = [];

    #endregion

    internal override void Init()
    {
        TaskHelper ??= new() { TimeoutMS = 5_000 };

        DService.Instance().ContextMenu.OnMenuOpened += OnMenuOpened;
        DService.Instance().GameGUI.HoveredItemChanged   += OnHoveredItemChanged;
        FrameworkManager.Instance().Reg(OnUpdate);

        DService.Instance().AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "CharacterInspect", OnAddon);

        DService.Instance().AddonLifecycle.RegisterListener(AddonEvent.PostSetup,   Addons, OnAddon);
        DService.Instance().AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, Addons, OnAddon);

        CharacterInspectItems.Clear();
        
        MiragePrismBoxReceiveEventHook ??= AgentMiragePrismPrismBox.Instance()->VirtualTable->HookVFuncFromName(
            "ReceiveEvent",
            (MiragePrismBoxReceiveEventDelegate)MiragePrismBoxReceiveEventDetour);
        MiragePrismBoxReceiveEventHook.Enable();

        AchievementReceiveEventHook ??= AgentAchievement.Instance()->VirtualTable->HookVFuncFromName(
            "ReceiveEvent",
            (AchievementReceiveEventDelegate)AchievementReceiveEventDetour);
        AchievementReceiveEventHook.Enable();

        MateriaAttachReceiveEventHook ??= AgentMateriaAttach.Instance()->VirtualTable->HookVFuncFromName(
            "ReceiveEvent",
            (MateriaAttachReceiveEventDelegate)AgentMateriaAttachReceiveEventDetour);
        MateriaAttachReceiveEventHook.Enable();
    }

    internal override void Uninit()
    {
        MiragePrismBoxReceiveEventHook?.Dispose();
        MiragePrismBoxReceiveEventHook = null;
        
        AchievementReceiveEventHook?.Dispose();
        AchievementReceiveEventHook = null;
        
        MateriaAttachReceiveEventHook?.Dispose();
        MateriaAttachReceiveEventHook = null;

        DService.Instance().AddonLifecycle.UnregisterListener(OnAddon);
        
        DService.Instance().GameGUI.HoveredItemChanged   -= OnHoveredItemChanged;
        DService.Instance().ContextMenu.OnMenuOpened -= OnMenuOpened;

        TaskHelper?.Abort();
        TaskHelper?.Dispose();
        TaskHelper = null;
        
        CharacterInspectItems.Clear();
        ResetAll();
    }

    #region Hook

    private AtkValue* MiragePrismBoxReceiveEventDetour(AgentMiragePrismPrismBox* agent, AtkValue* returnValue, AtkValue* values, uint valueCount, ulong eventKind)
    {
        if (values == null || returnValue == null)
            return MiragePrismBoxReceiveEventHook.Original(agent, returnValue, values, valueCount, eventKind);
        
        if (values->UInt == 13)
            return MiragePrismBoxReceiveEventHook.Original(agent, returnValue, values, valueCount, eventKind);

        var nextAtkValue = values + 1;

        if (nextAtkValue->Type != ValueType.UInt)
            return MiragePrismBoxReceiveEventHook.Original(agent, returnValue, values, valueCount, eventKind);

        var data = agent->Data;
        switch (nextAtkValue->UInt)
        {
            case 3:
                data->CrystallizeItemIndex = ushort.MaxValue;
                break;
            case 14:
                data->TempContextItemIndex = int.MaxValue;
                break;
        }

        return MiragePrismBoxReceiveEventHook.Original(agent, returnValue, values, valueCount, eventKind);
    }

    private AtkValue* AchievementReceiveEventDetour(AgentAchievement* agent, AtkValue* returnValue, AtkValue* values, uint valueCount, ulong eventKind)
    {
        if (eventKind is 1 or 3 or 4 || values->UInt == 7)
            return AchievementReceiveEventHook.Original(agent, returnValue, values, valueCount, eventKind);

        agent->ContextMenuSelectedItemId = 0;

        return AchievementReceiveEventHook.Original(agent, returnValue, values, valueCount, eventKind);
    }

    private AtkValue* AgentMateriaAttachReceiveEventDetour(AgentMateriaAttach* agent, AtkValue* returnValue, AtkValue* values, uint valueCount, ulong eventKind)
    {
        if (eventKind != 1)
            return MateriaAttachReceiveEventHook.Original(agent, returnValue, values, valueCount, eventKind);
        
        var oldSelectedItemIndex    = agent->SelectedItemIndex;
        var oldSelectedMateriaIndex = agent->SelectedMateriaIndex;

        var value = values->UInt;
        switch (value)
        {
            case 5:
                agent->SelectedItemIndex = -1;
                break;
            case 6:
                agent->SelectedMateriaIndex = -1;
                break;
        }

        var result = MateriaAttachReceiveEventHook.Original(agent, returnValue, values, valueCount, eventKind);
        switch (value)
        {
            case 5:
                agent->SelectedItemIndex = oldSelectedItemIndex;
                break;
            case 6:
                agent->SelectedMateriaIndex = oldSelectedMateriaIndex;
                break;
        }

        return result;
    }

    #endregion

    #region 事件处理

    private void OnAddon(AddonEvent type, AddonArgs? args)
    {
        switch (type)
        {
            case AddonEvent.PostSetup:
                switch (args.AddonName)
                {
                    case "MiragePrismPrismBoxCrystallize":
                        IsOnItemHover = true;
                        break;
                    default:
                        IsOnItemDetail = true;
                        break;
                }

                break;

            case AddonEvent.PostRefresh:
                switch (args.AddonName)
                {
                    case "CharacterInspect":
                        TaskHelper.Enqueue(() =>
                        {
                            if (CharacterInspectItems.Count != 0) return;
                            var container = InventoryManager.Instance()->GetInventoryContainer(InventoryType.Examine);
                            for (var i = 0; i < container->Size; i++)
                            {
                                var item = container->GetInventorySlot(i);
                                if (item == null || item->ItemId == 0) continue;
                                CharacterInspectItems.Add(*item);
                            }

                            IsOnItemHover = true;
                        });

                        break;
                }

                break;

            case AddonEvent.PreFinalize:
                switch (args.AddonName)
                {
                    case "CharacterInspect":
                        TaskHelper.Enqueue(() =>
                        {
                            IsOnItemHover     = false;
                            LastHoveredItemID = 0;
                            CharacterInspectItems.Clear();
                        });
                        break;
                    case "MiragePrismPrismBoxCrystallize":
                        IsOnItemHover = false;
                        break;
                    default:
                        IsOnItemDetail = false;
                        break;
                }

                break;
        }
    }

    private void OnHoveredItemChanged(object? sender, ulong id)
    {
        if (!IsOnItemHover) return;
        if (ContextMenuAddon->IsAddonAndNodesReady()) return;

        var processedID = ProcessRawItemID(id);
        if (processedID != 0 && LastHoveredItemID != processedID)
            LastHoveredItemID = processedID;
    }

    private void OnUpdate(IFramework framework)
    {
        if (!IsOnItemDetail) return;

        var agent = AgentItemDetail.Instance();
        if (agent == null) return;

        var id          = agent->ItemId;
        var processedID = ProcessRawItemID(id);
        if (processedID != 0 && LastDetailItemID != processedID)
            LastDetailItemID = processedID;
    }

    private void OnMenuOpened(IMenuOpenedArgs args)
    {
        ResetLastItems();

        HandleInventoryTarget(args);
        HandleSpecificAddons(args);
        HandleSpecificShops(args);

        UpdateCurrentItemInfo();
    }

    #endregion

    #region 处理逻辑

    private void ResetLastItems()
    {
        LastItem         = null;
        LastGlamourItem  = null;
        LastPrismBoxItem = null;
    }

    private void ResetAll()
    {
        ResetLastItems();
        CurrentItemID     = 0;
        CurrentGlamourID  = 0;
        LastHoveredItemID = 0;
        LastDetailItemID  = 0;
        IsOnItemHover     = false;
        IsOnItemDetail    = false;
    }

    private void UpdateCurrentItemInfo()
    {
        // 重置当前物品信息
        CurrentItemID    = 0;
        CurrentGlamourID = 0;

        if (LastItem != null)
            CurrentItemID = LastItem.Value.RowId;

        if (LastGlamourItem != null)
            CurrentGlamourID = LastGlamourItem.Value.RowId;
    }

    private uint ProcessRawItemID(ulong rawID)
    {
        if (rawID > 1_000_000)
            rawID %= 1_000_000;
        else if (rawID > 500_000)
            rawID %= 500_000;
        return (uint)rawID;
    }

    private bool HandleInventoryTarget(IMenuOpenedArgs args)
    {
        if (args.Target is not MenuTargetInventory { TargetItem: not null } inventoryTarget) return false;

        var item      = inventoryTarget.TargetItem.Value;
        var itemID    = ProcessRawItemID(item.ItemId);
        var glamourID = ProcessRawItemID(item.GlamourId);

        return ProcessItem(itemID, glamourID);
    }

    private bool HandleSpecificAddons(IMenuOpenedArgs args) =>
        args.AddonName switch
        {
            "ChatLog"                        => HandleChatLog(),
            "MiragePrismMiragePlate"         => HandleMiragePrismMiragePlate(),
            "ColorantColoring"               => HandleColorantColoring(args),
            "CabinetWithdraw"                => HandleCabinetWithdraw(),
            "CharacterInspect"               => HandleCharacterInspect(),
            "MiragePrismPrismBoxCrystallize" => HandleMiragePrismPrismBoxCrystallize(),
            "RecipeNote"                     => HandleRecipeNote(),
            "JournalAccept"                  => HandleJournalAccept(),
            "GuildLeve"                      => HandleGuildLeve(),
            "JournalRewardItem"              => HandleJournalRewardItem(),
            "Gathering"                      => HandleGathering(),
            "NeedGreed"                      => HandleNeedGreed(),
            "ContentsFinder"                 => HandleContentsFinder(),
            "Journal"                        => HandleJournal(),
            "Achievement"                    => HandleAchievement(),
            "MateriaAttach"                  => HandleMateriaAttach(),
            _                                => false
        };

    private bool HandleSpecificShops(IMenuOpenedArgs args)
    {
        var specificShops = new[]
        {
            "InclusionShop", "CollectablesShop", "FreeCompanyExchange", "ShopExchangeCurrency",
            "ShopExchangeItem", "FreeCompanyCreditShop", "Shop", "SkyIslandExchange",
            "TripleTriadCoinExchange", "FreeCompanyChest", "MJIDisposeShop", "GrandCompanyExchange",
            "ReconstructionBuyback", "ShopExchangeCoin", "ItemSearch", "GrandCompanySupplyList"
        };

        return specificShops.Contains(args.AddonName) && HandleGenericShopItem();
    }

    private bool HandleGenericShopItem()
    {
        if (LastDetailItemID <= 0) return false;

        var itemID = LastDetailItemID;
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;

        LastItem = item;

        return true;
    }

    private bool ProcessItem(uint itemID, uint glamourID)
    {
        if (itemID == 0) return false;
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;

        LastItem = item;

        if (PresetSheet.Gears.TryGetValue(itemID, out var gearItem))
        {
            LastItem = gearItem;
            if (glamourID > 0 && PresetSheet.Gears.TryGetValue(glamourID, out var glamourItem))
                LastGlamourItem = glamourItem;
        }

        return true;
    }

    private bool HandleChatLog()
    {
        var agent = AgentChatLog.Instance();
        if (agent == null || agent->ContextItemId == 0) return false;

        var itemID = ProcessRawItemID(agent->ContextItemId);
        return ProcessItem(itemID, 0);
    }

    private bool HandleMiragePrismMiragePlate()
    {
        var agent = AgentMiragePrismPrismItemDetail.Instance();
        if (agent == null) return false;

        if (!PresetSheet.Gears.TryGetValue(agent->ItemId, out var item)) return false;
        LastItem = item;

        return true;
    }

    private bool HandleColorantColoring(IMenuOpenedArgs args)
    {
        var agentColoring = AgentColorant.Instance();
        if (agentColoring == null) return false;

        var addon = args.AddonPtr.ToAtkUnitBase();
        if (addon == null) return false;

        var dyeTab = addon->AtkValues[18].Int;
        var dyeIndex = dyeTab switch
        {
            0 => addon->AtkValues[14].UInt,
            1 => addon->AtkValues[15].UInt,
            _ => 0U
        };

        if (!PresetSheet.Dyes.TryGetValue(dyeIndex, out var item)) return false;
        LastItem = item;

        return true;
    }

    private bool HandleCabinetWithdraw()
    {
        if (LastDetailItemID <= 0) return false;

        if (!PresetSheet.Gears.TryGetValue(LastDetailItemID, out var item)) return false;
        LastItem = item;

        return true;
    }

    private bool HandleCharacterInspect()
    {
        if (!PresetSheet.Gears.TryGetValue(LastHoveredItemID, out var inspectItem)) return false;
        var glamourID = CharacterInspectItems.FirstOrDefault(x => x.ItemId == LastHoveredItemID).GlamourId;

        LastItem        = inspectItem;
        LastGlamourItem = PresetSheet.Gears.TryGetValue(glamourID, out var gearItem) ? gearItem : LastItem;

        return true;
    }

    private bool HandleMiragePrismPrismBoxCrystallize()
    {
        var agent = AgentMiragePrismPrismBox.Instance();
        if (agent == null) return false;

        var data = agent->Data;
        if (data == null) return false;

        var crystallizeItemIndex = data->CrystallizeItemIndex;
        var contextItemIndex     = data->TempContextItemIndex;

        uint itemIDToUse = 0;

        if (crystallizeItemIndex != ushort.MaxValue)
            itemIDToUse = data->CrystallizeItems[crystallizeItemIndex].ItemId;
        else if (contextItemIndex != int.MaxValue)
            itemIDToUse = data->PrismBoxItems[contextItemIndex].ItemId;

        if (!LuminaGetter.TryGetRow<Item>(ProcessRawItemID(itemIDToUse), out var item)) return false;

        LastItem         = item;
        LastPrismBoxItem = null;
        LastGlamourItem  = null;

        return true;
    }

    private bool HandleRecipeNote()
    {
        var itemID = AgentRecipeNote.Instance()->ContextMenuResultItemId;

        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;

        LastItem = item;

        return true;
    }

    private bool HandleJournalAccept()
    {
        var agent = AgentJournalAccept.Instance();
        if (agent == null) return false;

        var selectedIndex = agent->ContextMenuSelectedRewardIndex;

        var itemID = ProcessRawItemID(agent->RewardItems[selectedIndex].ItemId);
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;
        LastItem = item;

        return true;
    }

    private bool HandleGuildLeve()
    {
        var agent = AgentLeveQuest.Instance();
        if (agent == null) return false;

        var selectedIndex = agent->ContextMenuSelectedRewardIndex;

        var itemID = ProcessRawItemID(agent->RewardItems[selectedIndex].ItemId);
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;
        LastItem = item;

        return true;
    }

    private bool HandleJournalRewardItem()
    {
        var agent = AgentRecipeItemContext.Instance();
        if (agent == null) return false;

        var itemID = ProcessRawItemID(agent->ResultItemId);
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;
        LastItem = item;

        return true;
    }

    private bool HandleGathering()
    {
        var agent = AgentRecipeItemContext.Instance();
        if (agent == null) return false;

        var itemID = ProcessRawItemID(agent->ResultItemId);
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;
        LastItem = item;

        return true;
    }

    private bool HandleNeedGreed()
    {
        var selectedIndex = AgentLoot.Instance()->SelectedSlotIndex;

        if (selectedIndex >= 16) return false;

        var itemID = ProcessRawItemID(Loot.Instance()->Items[selectedIndex].ItemId);
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;

        LastItem = item;

        return true;
    }

    private bool HandleContentsFinder()
    {
        var agent = AgentContentsFinder.Instance();
        if (agent == null) return false;

        var selectedReward = agent->RewardContextMenuHandler.SelectedReward;
        if (selectedReward == null) return false;

        var itemID = ProcessRawItemID(selectedReward->ItemId);
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;
        LastItem = item;

        return true;
    }

    private bool HandleJournal()
    {
        var agent = AgentQuestJournal.Instance();
        if (agent == null) return false;

        var itemID = ProcessRawItemID(agent->ContextMenuSelectedItemId);
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;
        LastItem = item;

        return true;
    }

    private bool HandleAchievement()
    {
        var agent = AgentAchievement.Instance();
        if (agent == null) return false;

        var itemID = ProcessRawItemID(agent->ContextMenuSelectedItemId);
        if (!LuminaGetter.TryGetRow<Item>(itemID, out var item)) return false;
        LastItem = item;

        return true;
    }

    private bool HandleMateriaAttach()
    {
        var agent = AgentMateriaAttach.Instance();
        if (agent == null) return false;

        var data = agent->Data;
        if (data == null) return false;

        var selectedItemIndex    = agent->SelectedItemIndex;
        var selectedMateriaIndex = agent->SelectedMateriaIndex;

        var itemID = 0ul;

        itemID = selectedItemIndex == -1 ? data->MateriaSorted[selectedMateriaIndex].Value->Item->ItemId : data->ItemsSorted[selectedItemIndex].Value->Item->ItemId;

        if (!LuminaGetter.TryGetRow<Item>(ProcessRawItemID(itemID), out var item)) return false;
        LastItem = item;

        return true;
    }

    #endregion
}
