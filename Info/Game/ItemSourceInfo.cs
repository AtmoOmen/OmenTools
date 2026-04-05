using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel.Sheets;
using OmenTools.Dalamud;
using OmenTools.Dalamud.DataShare.Attributes;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService;
using GrandCompany = FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany;

namespace OmenTools.Info.Game;

public sealed class ItemSourceInfo
{
    [DataShareTag]
    private const string DATA_SHARE_TAG = "OmenTools.ItemSourceInfo";
    
    private const string DEFAULT_SHOP_NAME = "アイテムの購入";

    private static bool IsDataInitialized { get; set; }
    private static bool IsDataLoaded      { get; set; }

    private static BuildState? ActiveBuildState { get; set; }

    private static Dictionary<uint, ItemSourceInfo> CachedItemInfos { get; set; } = [];

    public uint               ItemID                 { get; init; }
    public string             Name                   { get; init; }
    public List<ShopNPCInfos> NPCInfos               { get; private set; } = [];
    public ItemShopType       ShopType               { get; private set; }
    public string             AchievementDescription { get; private set; }

    static ItemSourceInfo()
    {
        if (IsDataInitialized || IsDataLoaded) return;
        
        if (DService.Instance().PI.TryGetData(DATA_SHARE_TAG, out Dictionary<uint, ItemSourceInfo> data) &&
            data is { Count: > 0 })
        {
            IsDataInitialized = IsDataLoaded = true;
            CachedItemInfos   = data;
            return;
        }

        CachedItemInfos = DService.Instance().PI.GetOrCreateData(DATA_SHARE_TAG, () => new Dictionary<uint, ItemSourceInfo>());
        
        IsDataInitialized = true;
        IsDataLoaded      = false;

        Task.Run
        (() =>
            {
                var timer = new Stopwatch();
                timer.Start();
                DLog.Debug("[ItemShopInfo] 开始构建数据");

                Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos = [];
                Dictionary<uint, ShopNPCLocation> npcIDToLocations    = [];

                try
                {
                    ActiveBuildState = new(itemToItemShopInfos);

                    BuildNPCLocations(ref npcIDToLocations);
                    CorrectNPCLocations(ref npcIDToLocations);

                    foreach (var npcBase in LuminaGetter.Get<ENpcBase>())
                        BuildNPCInfo(npcBase, ref itemToItemShopInfos, ref npcIDToLocations);

                    AddAchievementItem(ref itemToItemShopInfos, ref npcIDToLocations);
                }
                finally
                {
                    ActiveBuildState = null;
                    timer.Stop();
                    DLog.Debug($"[ItemShopInfo] 构建完毕, 构建时间: {timer.Elapsed}");

                    foreach (var itemToItemShopInfo in itemToItemShopInfos)
                        CachedItemInfos.Add(itemToItemShopInfo.Key, itemToItemShopInfo.Value);

                    IsDataLoaded = true;
                }
            }
        );
    }
    
    public static ItemSourceInfo? GetItemInfo(uint itemID) =>
        !IsDataLoaded
            ? null
            : CachedItemInfos?.GetValueOrDefault(itemID);

    private static BuildContext CurrentBuildContext =>
        ActiveBuildState?.Context ?? throw new InvalidOperationException("ItemSourceInfo 构建上下文尚未初始化");

    private static ItemSourceAccumulator CurrentItemAccumulator =>
        ActiveBuildState?.Accumulator ?? throw new InvalidOperationException("ItemSourceInfo 构建器尚未初始化");

    private sealed class BuildState
    (
        Dictionary<uint, ItemSourceInfo> itemInfos
    )
    {
        public BuildContext Context { get; } = BuildContext.Create();

        public ItemSourceAccumulator Accumulator { get; } = new(itemInfos);
    }

    private sealed class BuildContext
    {
        private BuildContext
        (
            Dictionary<uint, string>                    achievementDescriptions,
            Dictionary<ulong, uint>                     mapRowIds,
            Dictionary<uint, uint>                      tomestoneItemIds,
            Dictionary<uint, List<GCScripShopCategory>> gcCategories,
            bool                                        shouldHideDefaultShopName
        )
        {
            AchievementDescriptions   = achievementDescriptions;
            MapRowIds                 = mapRowIds;
            TomestoneItemIds          = tomestoneItemIds;
            GcCategories              = gcCategories;
            ShouldHideDefaultShopName = shouldHideDefaultShopName;
        }

        public Dictionary<uint, string> AchievementDescriptions { get; }

        public Dictionary<ulong, uint> MapRowIds { get; }

        public Dictionary<uint, uint> TomestoneItemIds { get; }

        public Dictionary<uint, List<GCScripShopCategory>> GcCategories { get; }

        public bool ShouldHideDefaultShopName { get; }

        public static BuildContext Create()
        {
            Dictionary<uint, string>                    achievementDescriptions = [];
            Dictionary<ulong, uint>                     mapRowIds               = [];
            Dictionary<uint, uint>                      tomestoneItemIds        = [];
            Dictionary<uint, List<GCScripShopCategory>> gcCategories            = [];

            foreach (var achievement in LuminaGetter.Get<Achievement>())
            {
                var itemID = achievement.Item.RowId;
                if (itemID == 0 || achievementDescriptions.ContainsKey(itemID))
                    continue;

                achievementDescriptions.Add(itemID, achievement.Description.ToString());
            }

            foreach (var map in LuminaGetter.Get<Map>())
                mapRowIds.TryAdd(GetMapKey(map.TerritoryType.RowId, unchecked((uint)map.MapIndex)), map.RowId);

            foreach (var tomestonesItem in LuminaGetter.Get<TomestonesItem>())
            {
                var tomestoneID = tomestonesItem.Tomestones.Value.RowId;
                var itemID      = tomestonesItem.Item.RowId;
                if (tomestoneID == 0 || itemID == 0)
                    continue;

                tomestoneItemIds.TryAdd(tomestoneID, itemID);
            }

            foreach (var category in LuminaGetter.Get<GCScripShopCategory>())
            {
                var gcRowID = category.GrandCompany.RowId;
                if (gcRowID == 0)
                    continue;

                ref var categories = ref CollectionsMarshal.GetValueRefOrAddDefault(gcCategories, gcRowID, out var exists);
                if (!exists || categories == null)
                    categories = [];

                categories.Add(category);
            }

            return new
            (
                achievementDescriptions,
                mapRowIds,
                tomestoneItemIds,
                gcCategories,
                DService.Instance().ClientState.ClientLanguage != ClientLanguage.Japanese
            );
        }

        public bool TryGetMapRowID(uint territoryID, uint mapIndex, out uint mapRowID) =>
            MapRowIds.TryGetValue(GetMapKey(territoryID, mapIndex), out mapRowID);

        private static ulong GetMapKey(uint territoryID, uint mapIndex) =>
            (ulong)territoryID << 32 | mapIndex;
    }

    private sealed class ItemSourceAccumulator
    (
        Dictionary<uint, ItemSourceInfo> itemInfos
    )
    {
        private readonly Dictionary<uint, Dictionary<uint, int>> npcIndexByItemID = [];

        public void AddItem
        (
            uint                   itemID,
            string                 itemName,
            uint                   npcID,
            string                 npcName,
            string?                shopName,
            List<ShopItemCostInfo> cost,
            ShopNPCLocation        npcLocation,
            ItemShopType           shopType,
            string                 achievementDesc
        )
        {
            ref var itemInfo = ref CollectionsMarshal.GetValueRefOrAddDefault(itemInfos, itemID, out var exists);

            if (!exists || itemInfo == null)
            {
                itemInfo = new()
                {
                    ItemID                 = itemID,
                    Name                   = itemName,
                    NPCInfos               = [],
                    ShopType               = shopType,
                    AchievementDescription = achievementDesc
                };
            }

            if (shopType == ItemShopType.Achievement && itemInfo.ShopType != ItemShopType.Achievement)
            {
                itemInfo.ShopType               = ItemShopType.Achievement;
                itemInfo.AchievementDescription = achievementDesc;
            }

            ref var npcIndex = ref CollectionsMarshal.GetValueRefOrAddDefault(npcIndexByItemID, itemID, out var hasNpcIndex);
            if (!hasNpcIndex || npcIndex == null)
                npcIndex = [];

            if (!npcIndex.TryAdd(npcID, itemInfo.NPCInfos.Count))
                return;

            itemInfo.NPCInfos.Add(new() { ID = npcID, Location = npcLocation, Name = npcName, CostInfos = cost, ShopName = shopName });
        }

        public void AddCost(uint itemID, uint npcID, List<ShopItemCostInfo> cost)
        {
            if (!itemInfos.TryGetValue(itemID, out var itemInfo))
                return;

            if (!npcIndexByItemID.TryGetValue(itemID, out var npcIndex))
                return;

            if (!npcIndex.TryGetValue(npcID, out var index))
                return;

            itemInfo.NPCInfos[index].CostInfos.AddRange(cost);
        }
    }

    private static void BuildNPCLocations(ref Dictionary<uint, ShopNPCLocation> npcIDToLocations)
    {
        HashSet<uint> addedAetheryte = [];

        foreach (var aetheryte in LuminaGetter.Get<Aetheryte>())
        {
            if (!aetheryte.Territory.IsValid || aetheryte.Territory.RowId == 1)
                continue;

            var territory = aetheryte.Territory.Value;
            if (addedAetheryte.Contains(territory.RowId)) continue;

            var file = GetLgbFileFromBg(territory.Bg.ToString());
            if (file == null) continue;

            ParseLgbFile(file, territory, ref npcIDToLocations);
            addedAetheryte.Add(territory.RowId);
        }

        foreach (var territory in LuminaGetter.Get<TerritoryType>())
        {
            var condition = territory.ContentFinderCondition.Value;
            if (condition.ContentType.RowId is not (26 or 29 or 16))
                continue;

            var file = GetLgbFileFromBg(territory.Bg.ToString());
            if (file == null) continue;

            ParseLgbFile(file, territory, ref npcIDToLocations);
        }

        foreach (var level in LuminaGetter.Get<Level>())
        {
            if (level.Type != 8 || level.Territory.ValueNullable == null)
                continue;

            if (npcIDToLocations.ContainsKey(level.Object.RowId)) continue;
            if (!LuminaGetter.TryGetRow(level.Object.RowId, out ENpcBase npcBase)) continue;

            if (!HasRelevantEventHandler(npcBase)) continue;

            npcIDToLocations.Add(level.Object.RowId, new(level.X, level.Z, level.Territory.RowId));
        }

        foreach (var npc in LuminaGetter.GetSub<HousingEmploymentNpcList>())
        {
            foreach (var entry in npc)
            {
                var id = entry.MaleENpcBase;
                if (id.RowId == 0)
                    continue;

                npcIDToLocations.Add(id.RowId, new(0, 0, 282));
            }
        }
    }

    private static void CorrectNPCLocations(ref Dictionary<uint, ShopNPCLocation> npcIDToLocations)
    {
        npcIDToLocations[1019100] = new(-85.03851f, 117.05188f, 641);
        npcIDToLocations[1022846] = new(-83.93994f, 115.31238f, 641);
        npcIDToLocations[1019106] = new(-99.22949f, 105.6687f, 641);
        npcIDToLocations[1019107] = new(-100.26703f, 107.43872f, 641);
        npcIDToLocations[1019104] = new(-67.582275f, 59.739014f, 641);
        npcIDToLocations[1019102] = new(-59.617065f, 33.524048f, 641);
        npcIDToLocations[1019103] = new(-52.35376f, 76.58496f, 641);
        npcIDToLocations[1019101] = new(-36.484375f, 49.240845f, 641);
        npcIDToLocations[1004418] = new(-114.0307f, 118.30322f, 131, 73);

        npcIDToLocations.TryAdd(1006004, new(5.355835f, 155.22998f, 128));
        npcIDToLocations.TryAdd(1017613, new(2.822865f, 153.521f, 128));
        npcIDToLocations.TryAdd(1003633, new(-259.32715f, 37.491333f, 129));
        npcIDToLocations.TryAdd(1008145, new(-31.265808f, -245.38031f, 133));
        npcIDToLocations.TryAdd(1006005, new(-61.234497f, -141.31384f, 133));
        npcIDToLocations.TryAdd(1017614, new(-58.79309f, -142.1073f, 133));
        npcIDToLocations.TryAdd(1003077, new(145.83044f, -106.767456f, 133));
        npcIDToLocations.TryAdd(1000215, new(155.35205f, -70.26782f, 133));
        npcIDToLocations.TryAdd(1000996, new(-28.152893f, 196.70398f, 128));
        npcIDToLocations.TryAdd(1000999, new(-29.465149f, 197.92468f, 128));
        npcIDToLocations.TryAdd(1000217, new(170.30591f, -73.16705f, 133));
        npcIDToLocations.TryAdd(1000597, new(-163.07324f, -78.62976f, 153));
        npcIDToLocations.TryAdd(1000185, new(-8.590881f, -2.2125854f, 132));
        npcIDToLocations.TryAdd(1000392, new(-17.746277f, 43.35083f, 132));
        npcIDToLocations.TryAdd(1000391, new(66.819214f, -143.45007f, 133));
        npcIDToLocations.TryAdd(1000232, new(164.72107f, -133.68433f, 133));
        npcIDToLocations.TryAdd(1000301, new(-87.174866f, -173.51044f, 133));
        npcIDToLocations.TryAdd(1000267, new(103.89868f, -213.03125f, 133));
        npcIDToLocations.TryAdd(1003252, new(-139.57434f, 31.967651f, 129));
        npcIDToLocations.TryAdd(1001016, new(-42.679565f, 119.920654f, 128));
        npcIDToLocations.TryAdd(1005422, new(-397.6349f, 80.979614f, 129));
        npcIDToLocations.TryAdd(1000244, new(423.17834f, -119.95117f, 154));
        npcIDToLocations.TryAdd(1000234, new(423.69714f, -122.08746f, 154));
        npcIDToLocations.TryAdd(1000230, new(421.46936f, -125.993774f, 154));
        npcIDToLocations.TryAdd(1000222, new(-213.94684f, 300.4348f, 152));
        npcIDToLocations.TryAdd(1000535, new(-579.4003f, 104.32593f, 152));
        npcIDToLocations.TryAdd(1002371, new(-480.91858f, 201.9226f, 152));
        npcIDToLocations.TryAdd(1000396, new(82.597046f, -103.349365f, 148));
        npcIDToLocations.TryAdd(1000220, new(16.189758f, -15.640564f, 148));
        npcIDToLocations.TryAdd(1000717, new(175.61597f, 319.32544f, 148));
        npcIDToLocations.TryAdd(1000718, new(332.23462f, 332.47876f, 154));
        npcIDToLocations.TryAdd(1002376, new(10.635498f, 220.20288f, 154));
        npcIDToLocations.TryAdd(1002374, new(204.39453f, -65.75122f, 153));
        npcIDToLocations.TryAdd(1000579, new(16.03717f, 220.50806f, 152));
        npcIDToLocations.TryAdd(1002377, new(11.062683f, 221.57617f, 154));
        npcIDToLocations.TryAdd(1002375, new(203.75366f, -64.560974f, 153));
        npcIDToLocations.TryAdd(1002389, new(95.8114f, 67.61267f, 128));
    }

    private static void BuildNPCInfo
    (
        ENpcBase                              npcBase,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations
    )
    {
        var resident = LuminaGetter.GetRowOrDefault<ENpcResident>(npcBase.RowId);
        if (FixNPCInfo(npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations)) return;

        var fateShop = LuminaGetter.GetRow<FateShop>(npcBase.RowId);

        if (fateShop.HasValue)
        {
            foreach (var specialShop in fateShop.Value.SpecialShop)
            {
                var specialShopCustom = LuminaGetter.GetRow<SpecialShop>(specialShop.RowId);

                if (specialShopCustom == null)
                    continue;

                AddSpecialItem(specialShopCustom.Value, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
            }

            return;
        }

        var npcID       = npcBase.RowId;
        var npcName     = resident.Singular.ToString();
        var npcLocation = npcIDToLocations.GetValueOrDefault(npcID);

        foreach (var entry in npcBase.ENpcData)
        {
            var npcData = entry.RowId;
            if (npcData == 0) break;

            if (!TryGetEventHandlerType(npcData, out var handlerType))
                continue;

            if (handlerType == EventHandlerType.CollectablesShop)
            {
                var collectablesShop = LuminaGetter.GetRowOrDefault<CollectablesShop>(npcData);
                AddCollectablesShop(collectablesShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                continue;
            }

            if (handlerType == EventHandlerType.InclusionShop)
            {
                var inclusionShop = LuminaGetter.GetRowOrDefault<InclusionShop>(npcData);
                AddInclusionShop(inclusionShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                continue;
            }

            if (handlerType == EventHandlerType.FcShop)
            {
                var fccShop = LuminaGetter.GetRowOrDefault<FccShop>(npcData);
                AddFccShop(fccShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                continue;
            }

            if (handlerType == EventHandlerType.PreHandler)
            {
                var preHandler = LuminaGetter.GetRowOrDefault<PreHandler>(npcData);
                AddItemsInPrehandler(preHandler, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                continue;
            }

            if (handlerType == EventHandlerType.TopicSelect)
            {
                var topicSelect = LuminaGetter.GetRowOrDefault<TopicSelect>(npcData);
                AddItemsInTopicSelect(topicSelect, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                continue;
            }

            if (handlerType == EventHandlerType.GcShop)
            {
                var gcShop = LuminaGetter.GetRowOrDefault<GCShop>(npcData);
                AddGcShopItem(gcShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                continue;
            }

            if (handlerType == EventHandlerType.SpecialShop)
            {
                var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(npcData);
                AddSpecialItem(specialShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations, shop: specialShop.Name.ToString());
                continue;
            }

            if (handlerType == EventHandlerType.GilShop)
            {
                var gilShop = LuminaGetter.GetRowOrDefault<GilShop>(npcData);
                AddGilShopItem(gilShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                continue;
            }

            if (handlerType == EventHandlerType.CustomTalk)
            {
                if (!LuminaGetter.TryGetRow<CustomTalk>(npcData, out var customTalk)) break;
                var scriptArg2  = 0u;
                var scriptArg4  = 0u;
                var scriptIndex = 0;

                foreach (var script in customTalk.Script)
                {
                    if (scriptIndex == 2)
                        scriptArg2 = script.ScriptArg;
                    else if (scriptIndex == 4)
                        scriptArg4 = script.ScriptArg;

                    scriptIndex++;
                }

                if (npcData == 721068)
                {
                    AddItemGeneral
                    (
                        scriptArg2,
                        LuminaWrapper.GetItemName(scriptArg2),
                        npcID,
                        npcName,
                        customTalk.MainOption.ToString(),
                        [new(scriptArg4, 28)],
                        npcLocation,
                        ItemShopType.SpecialShop,
                        ref itemToItemShopInfos
                    );
                    continue;
                }

                if (customTalk.SpecialLinks.RowId != 0 &&
                    LuminaGetter.TryGetSubRowAll<CustomTalkNestHandlers>(customTalk.SpecialLinks.RowId, out var nestHandlers))
                {
                    foreach (var customTalkNestHandler in nestHandlers)
                    {
                        if (!TryGetEventHandlerType(customTalkNestHandler.NestHandler.RowId, out var nestHandlerType))
                            continue;

                        if (nestHandlerType == EventHandlerType.SpecialShop)
                        {
                            var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(customTalkNestHandler.NestHandler.RowId);
                            AddSpecialItem(specialShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                            continue;
                        }

                        if (nestHandlerType == EventHandlerType.GilShop)
                        {
                            var gilShop = LuminaGetter.GetRowOrDefault<GilShop>(customTalkNestHandler.NestHandler.RowId);
                            AddGilShopItem(gilShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                        }
                    }
                }

                foreach (var script in customTalk.Script)
                {
                    var arg = script.ScriptArg;
                    if (!TryGetEventHandlerType(arg, out var argHandlerType))
                        continue;

                    if (argHandlerType == EventHandlerType.GilShop)
                    {
                        var gilShop = LuminaGetter.GetRowOrDefault<GilShop>(arg);
                        AddGilShopItem(gilShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                        continue;
                    }

                    if (argHandlerType == EventHandlerType.FcShop)
                    {
                        var shop = LuminaGetter.GetRowOrDefault<FccShop>(arg);
                        AddFccShop(shop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                        continue;
                    }

                    if (argHandlerType == EventHandlerType.SpecialShop)
                    {
                        var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(arg);
                        AddSpecialItem(specialShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                    }
                }
            }
        }
    }

    private static bool FixNPCInfo
    (
        ENpcBase                              npcBase,
        ENpcResident                          resident,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations
    )
    {
        switch (npcBase.RowId)
        {
            case 1043463:

                AddSpecialItem
                (
                    LuminaGetter.GetRowOrDefault<SpecialShop>(1770601),
                    npcBase,
                    resident,
                    ref itemToItemShopInfos,
                    ref npcIDToLocations,
                    ItemShopType.SpecialShop,
                    $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000")}\n{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q2_000_000")}"
                );
                AddSpecialItem
                (
                    LuminaGetter.GetRowOrDefault<SpecialShop>(1770659),
                    npcBase,
                    resident,
                    ref itemToItemShopInfos,
                    ref npcIDToLocations,
                    ItemShopType.SpecialShop,
                    $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000")} \n {GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q2_000_005")}"
                );
                AddSpecialItem
                (
                    LuminaGetter.GetRowOrDefault<SpecialShop>(1770660),
                    npcBase,
                    resident,
                    ref itemToItemShopInfos,
                    ref npcIDToLocations,
                    ItemShopType.SpecialShop,
                    $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000")}\n{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q2_000_010")}"
                );
                AddSpecialItem
                (
                    LuminaGetter.GetRowOrDefault<SpecialShop>(1770602),
                    npcBase,
                    resident,
                    ref itemToItemShopInfos,
                    ref npcIDToLocations,
                    ItemShopType.SpecialShop,
                    $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_005")}"
                );
                AddSpecialItem
                (
                    LuminaGetter.GetRowOrDefault<SpecialShop>(1770603),
                    npcBase,
                    resident,
                    ref itemToItemShopInfos,
                    ref npcIDToLocations,
                    ItemShopType.SpecialShop,
                    $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_010")}"
                );
                AddSpecialItem
                (
                    LuminaGetter.GetRowOrDefault<SpecialShop>(1770723),
                    npcBase,
                    resident,
                    ref itemToItemShopInfos,
                    ref npcIDToLocations,
                    ItemShopType.SpecialShop,
                    $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_025")}"
                );
                AddSpecialItem
                (
                    LuminaGetter.GetRowOrDefault<SpecialShop>(1770734),
                    npcBase,
                    resident,
                    ref itemToItemShopInfos,
                    ref npcIDToLocations,
                    ItemShopType.SpecialShop,
                    $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_030")}"
                );
                return true;

                string GetNameFromKey(string key)
                {
                    return MJISpecialShopNames.TryGetValue(key, out var str) ? str : string.Empty;
                }
            case 1018655:
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769743), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769744), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770537), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                return true;
            case 1016289:
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769635), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                return true;
            case 1025047:
                for (uint i = 1769820; i <= 1769834; i++)
                {
                    var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(i);
                    AddSpecialItem(specialShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                }

                return true;
            case 1025763:
                AddGilShopItem(LuminaGetter.GetRowOrDefault<GilShop>(262919), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                return true;
            case 1027123:
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769934), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769935), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                return true;
            case 1027124:
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769937), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                return true;
            case 1033921:
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770282), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                return true;
            case 1035012:
                for (ushort i = 0; i <= 10; i++)
                {
                    var questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(14, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(15, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(19, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                }

                return true;
            case 1016135:

                for (uint i = 3; i <= 10; i++)
                for (ushort j = 0; j <= 12; j++)
                {
                    var questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(i, j);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                    AddQuestRewardCost(questClassJobReward, npcBase, ref itemToItemShopInfos, GetCost(i));
                }

                return true;

                List<ShopItemCostInfo>? GetCost(uint i)
                {
                    return i switch
                    {
                        3 => new()
                        {
                            new(1, 13575),
                            new(1, 13576)
                        },
                        5 => new()
                        {
                            new(1, 13577),
                            new(1, 13578),
                            new(1, 13579),
                            new(1, 13580)
                        },
                        6 => new()
                        {
                            new(5, 14899)
                        },
                        7 => new()
                        {
                            new(60, 15840),
                            new(60, 15841)
                        },
                        8 => new()
                        {
                            new(50, 16064)
                        },
                        9 => new()
                        {
                            new(1, 16932)
                        },
                        10 => new()
                        {
                            new(1, 16934)
                        },
                        _ => null
                    };
                }
            case 1032903:
                for (ushort i = 0; i <= 16; i++)
                {
                    var questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(12, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations, [new(4, 30273)]);
                }

                return true;

            case 1032905:
                for (ushort i = 0; i <= 16; i++)
                {
                    var questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(13, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations, [new(4, 30273)]);
                }

                for (ushort i = 0; i <= 16; i++)
                {
                    var questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(17, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                    AddQuestRewardCost
                    (
                        questClassJobReward,
                        npcBase,
                        ref itemToItemShopInfos,
                        [
                            new(20, 31573),
                            new(20, 31574),
                            new(20, 31575)
                        ]
                    );

                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(18, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                    AddQuestRewardCost(questClassJobReward, npcBase, ref itemToItemShopInfos, [new(6, 31576)]);

                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(20, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                    AddQuestRewardCost(questClassJobReward, npcBase, ref itemToItemShopInfos, [new(15, 32956)]);

                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(21, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                    AddQuestRewardCost(questClassJobReward, npcBase, ref itemToItemShopInfos, [new(15, 32959)]);

                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(22, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                    AddQuestRewardCost(questClassJobReward, npcBase, ref itemToItemShopInfos, [new(15, 33767)]);
                }

                return true;
            default:
                if (!SHBFateShopNPC.TryGetValue(npcBase.RowId, out var value)) return false;

                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(value), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
                return true;
        }
    }

    #region 添加物品

    private static void AddSpecialItem
    (
        SpecialShop                           specialShop,
        ENpcBase                              npcBase,
        ENpcResident                          resident,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations,
        ItemShopType                          shopType = ItemShopType.SpecialShop,
        string?                               shop     = null
    )
    {
        var npcID       = npcBase.RowId;
        var npcName     = resident.Singular.ToString();
        var npcLocation = npcIDToLocations.GetValueOrDefault(npcID);
        var context     = CurrentBuildContext;

        foreach (var entry in specialShop.Item)
        {
            List<ShopItemCostInfo> costs = [];

            foreach (var itemCost in entry.ItemCosts)
            {
                if (!itemCost.ItemCost.IsValid || itemCost.ItemCost.Value.Name == string.Empty)
                    continue;

                var currencyItem = ConvertCurrency(itemCost.ItemCost.Value.RowId, specialShop);
                costs.Add(new(itemCost.CurrencyCost, currencyItem.RowId));
            }

            foreach (var receiveItem in entry.ReceiveItems)
            {
                var item = receiveItem.Item.Value;
                var achievementDescription = shopType == ItemShopType.Achievement &&
                                             context.AchievementDescriptions.TryGetValue(item.RowId, out var description)
                                                 ? description
                                                 : string.Empty;

                AddItemGeneral
                (
                    item.RowId,
                    item.Name.ToString(),
                    npcID,
                    npcName,
                    shop,
                    costs,
                    npcLocation,
                    shopType,
                    ref itemToItemShopInfos,
                    achievementDescription
                );
            }
        }
    }

    private static void AddGilShopItem
    (
        GilShop                               gilShop,
        ENpcBase                              npcBase,
        ENpcResident                          resident,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations,
        string                                shop = null
    )
    {
        if (!LuminaGetter.TryGetSubRowAll<GilShopItem>(gilShop.RowId, out var items))
            return;

        var npcID       = npcBase.RowId;
        var npcName     = resident.Singular.ToString();
        var npcLocation = npcIDToLocations.GetValueOrDefault(npcID);
        var shopName    = shop != null ? $"{shop}\n{gilShop.Name}" : gilShop.Name.ToString();

        foreach (var item in items)
        {
            AddItemGeneral
            (
                item.Item.Value.RowId,
                item.Item.Value.Name.ToString(),
                npcID,
                npcName,
                shopName,
                [new(item.Item.Value.PriceMid, 1)],
                npcLocation,
                ItemShopType.GilShop,
                ref itemToItemShopInfos
            );
        }
    }

    private static void AddGcShopItem
    (
        GCShop                                gcID,
        ENpcBase                              npcBase,
        ENpcResident                          resident,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations
    )
    {
        var seal = GrandCompanySeals.Find(i => i.Description.ToString().Contains($"{gcID.GrandCompany.Value.Name.ToString()}"));
        if (seal.RowId == 0)
            return;

        if (!CurrentBuildContext.GcCategories.TryGetValue(gcID.GrandCompany.RowId, out var categories))
            return;

        var npcID       = npcBase.RowId;
        var npcName     = resident.Singular.ToString();
        var npcLocation = npcIDToLocations.GetValueOrDefault(npcID);

        foreach (var category in categories)
        {
            if (!LuminaGetter.TryGetSubRowAll<GCScripShopItem>(category.RowId, out var items))
                continue;

            foreach (var item in items)
            {
                if (item.SortKey == 0)
                    break;

                AddItemGeneral
                (
                    item.Item.Value.RowId,
                    item.Item.Value.Name.ToString(),
                    npcID,
                    npcName,
                    null,
                    [new(item.CostGCSeals, seal.RowId)],
                    npcLocation,
                    ItemShopType.GcShop,
                    ref itemToItemShopInfos
                );
            }
        }
    }

    private static void AddInclusionShop
    (
        InclusionShop                         inclusionShop,
        ENpcBase                              npcBase,
        ENpcResident                          resident,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations
    )
    {
        var prefix = inclusionShop.Unknown0.ToString();

        foreach (var category in inclusionShop.Category)
        {
            if (category.Value.RowId == 0) continue;

            if (!LuminaGetter.TryGetSubRowAll<InclusionShopSeries>(category.Value.InclusionShopSeries.RowId, out var seriesCollection))
                continue;

            foreach (var series in seriesCollection)
            {
                var specialShop = series.SpecialShop.Value;
                var shop = string.IsNullOrEmpty(prefix)
                               ? $"{category.Value.Name}\n{specialShop.Name}"
                               : $"{prefix}\n{category.Value.Name}\n{specialShop.Name}";
                AddSpecialItem(specialShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations, shop: shop);
            }
        }
    }

    private static void AddFccShop
    (
        FccShop                               shop,
        ENpcBase                              npcBase,
        ENpcResident                          resident,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations
    )
    {
        foreach (var t in shop.ItemData)
        {
            if (!LuminaGetter.TryGetRow(t.Item.RowId, out Item item)) continue;

            if (item.Name == string.Empty) continue;

            var cost = t.Cost;

            AddItemGeneral
            (
                item.RowId,
                item.Name.ToString(),
                npcBase.RowId,
                resident.Singular.ToString(),
                null,
                [new(cost, 102233)],
                npcIDToLocations.GetValueOrDefault(npcBase.RowId),
                ItemShopType.FcShop,
                ref itemToItemShopInfos
            );
        }
    }

    private static void AddItemsInPrehandler
    (
        PreHandler                            preHandler,
        ENpcBase                              npcBase,
        ENpcResident                          resident,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations
    )
    {
        var target = preHandler.Target.RowId;
        if (target == 0)
            return;

        if (!TryGetEventHandlerType(target, out var handlerType))
            return;

        if (handlerType == EventHandlerType.GilShop)
        {
            var gilShop = LuminaGetter.GetRowOrDefault<GilShop>(target);
            AddGilShopItem(gilShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
            return;
        }

        if (handlerType == EventHandlerType.SpecialShop)
        {
            var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(target);
            AddSpecialItem(specialShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
            return;
        }

        if (handlerType == EventHandlerType.InclusionShop)
        {
            var inclusionShop = LuminaGetter.GetRowOrDefault<InclusionShop>(target);
            AddInclusionShop(inclusionShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
        }
    }

    private static void AddItemsInTopicSelect
    (
        TopicSelect                           topicSelect,
        ENpcBase                              npcBase,
        ENpcResident                          resident,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations
    )
    {
        var topicName = topicSelect.Name.ToString();

        foreach (var shop in topicSelect.Shop)
        {
            var data = shop.RowId;
            if (data == 0)
                continue;

            if (!TryGetEventHandlerType(data, out var handlerType))
                continue;

            if (handlerType == EventHandlerType.SpecialShop)
            {
                var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(data);

                AddSpecialItem
                (
                    specialShop,
                    npcBase,
                    resident,
                    ref itemToItemShopInfos,
                    ref npcIDToLocations,
                    shop: $"{topicName}\n{specialShop.Name.ToString()}"
                );

                continue;
            }

            if (handlerType == EventHandlerType.GilShop)
            {
                var gilShop = LuminaGetter.GetRowOrDefault<GilShop>(data);
                AddGilShopItem(gilShop, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations, topicName);
                continue;
            }

            if (handlerType == EventHandlerType.PreHandler)
            {
                var preHandler = LuminaGetter.GetRowOrDefault<PreHandler>(data);
                AddItemsInPrehandler(preHandler, npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations);
            }
        }
    }

    private static void AddCollectablesShop
    (
        CollectablesShop                      shop,
        ENpcBase                              npcBase,
        ENpcResident                          resident,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations
    )
    {
        var shopName = shop.Name.ToString();
        if (shopName == string.Empty)
            return;

        var npcID       = npcBase.RowId;
        var npcName     = resident.Singular.ToString();
        var npcLocation = npcIDToLocations.GetValueOrDefault(npcID);

        foreach (var t in shop.ShopItems)
        {
            var row = t.Value.RowId;

            if (row == 0)
                continue;

            if (!LuminaGetter.TryGetSubRowAll<CollectablesShopItem>(row, out var exchangeItems))
                continue;

            foreach (var exchangeItem in exchangeItems)
            {
                if (exchangeItem.Item.RowId <= 1000)
                    continue;

                var rewardItem = LuminaGetter.GetRowOrDefault<CollectablesShopRewardItem>(exchangeItem.CollectablesShopRewardScrip.RowId);
                var refine     = LuminaGetter.GetRowOrDefault<CollectablesShopRefine>(exchangeItem.CollectablesShopRefine.RowId);

                AddItemGeneral
                (
                    rewardItem.Item.Value.RowId,
                    rewardItem.Item.Value.Name.ToString(),
                    npcID,
                    npcName,
                    $"{shopName}\n{exchangeItem.CollectablesShopItemGroup.Value.Name.ToString()}",
                    [
                        new(rewardItem.RewardLow, exchangeItem.Item.RowId, refine.LowCollectability),
                        new(rewardItem.RewardMid, exchangeItem.Item.RowId, refine.MidCollectability),
                        new(rewardItem.RewardHigh, exchangeItem.Item.RowId, refine.HighCollectability)
                    ],
                    npcLocation,
                    ItemShopType.CollectableExchange,
                    ref itemToItemShopInfos
                );
            }
        }
    }

    private static void AddQuestReward
    (
        QuestClassJobReward                   questReward,
        ENpcBase                              npcBase,
        ENpcResident                          resident,
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations,
        List<ShopItemCostInfo>                cost = null
    )
    {
        if (questReward.ClassJobCategory.RowId == 0)
            return;

        var npcID       = npcBase.RowId;
        var npcName     = resident.Singular.ToString();
        var npcLocation = npcIDToLocations.GetValueOrDefault(npcID);

        if (cost == null)
        {
            cost = [];

            for (var i = 0; i < questReward.RequiredItem.Count; i++)
            {
                var requireItem = questReward.RequiredItem[i];
                if (requireItem.RowId == 0)
                    break;

                cost.Add(new(questReward.RequiredAmount[i], requireItem.Value.RowId));
            }
        }

        foreach (var rewardItem in questReward.RewardItem)
        {
            if (rewardItem.RowId == 0)
                break;

            AddItemGeneral
            (
                rewardItem.RowId,
                rewardItem.Value.Name.ToString(),
                npcID,
                npcName,
                string.Empty,
                cost,
                npcLocation,
                ItemShopType.QuestReward,
                ref itemToItemShopInfos
            );
        }
    }

    private static void AddQuestRewardCost
    (
        QuestClassJobReward                  questReward,
        ENpcBase                             npcBase,
        ref Dictionary<uint, ItemSourceInfo> itemToItemShopInfos,
        List<ShopItemCostInfo>               cost
    )
    {
        if (cost == null || questReward.ClassJobCategory.RowId == 0)
            return;

        foreach (var rewardItem in questReward.RewardItem)
        {
            if (rewardItem.RowId == 0)
                break;

            AddItemCost(rewardItem.RowId, npcBase.RowId, ref itemToItemShopInfos, cost);
        }
    }

    private static void AddAchievementItem
    (
        ref Dictionary<uint, ItemSourceInfo>  itemToItemShopInfos,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations
    )
    {
        for (var i = 1006004u; i <= 1006006; i++)
        {
            var npcBase  = LuminaGetter.GetRowOrDefault<ENpcBase>(i);
            var resident = LuminaGetter.GetRowOrDefault<ENpcResident>(i);

            for (var j = 1769898u; j <= 1769906; j++)
                AddSpecialItem
                    (LuminaGetter.GetRowOrDefault<SpecialShop>(j), npcBase, resident, ref itemToItemShopInfos, ref npcIDToLocations, ItemShopType.Achievement);
        }
    }

    private static void AddItemCost
    (
        uint                                 itemID,
        uint                                 npcID,
        ref Dictionary<uint, ItemSourceInfo> itemToItemShopInfos,
        List<ShopItemCostInfo>               cost
    )
    {
        if (itemID == 0)
            return;

        CurrentItemAccumulator.AddCost(itemID, npcID, cost);
    }

    private static void AddItemGeneral
    (
        uint                                 itemID,
        string                               itemName,
        uint                                 npcID,
        string                               npcName,
        string?                              shopName,
        List<ShopItemCostInfo>               cost,
        ShopNPCLocation                      npcLocation,
        ItemShopType                         shopType,
        ref Dictionary<uint, ItemSourceInfo> itemToItemShopInfos,
        string                               achievementDesc = ""
    )
    {
        if (itemID == 0)
            return;

        if (CurrentBuildContext.ShouldHideDefaultShopName && shopName == DEFAULT_SHOP_NAME)
            shopName = string.Empty;

        CurrentItemAccumulator.AddItem(itemID, itemName, npcID, npcName, shopName, cost, npcLocation, shopType, achievementDesc);
    }

    #endregion

    #region 工具

    private static void ParseLgbFile
    (
        LgbFile                               lgbFile,
        TerritoryType                         sTerritoryType,
        ref Dictionary<uint, ShopNPCLocation> npcIDToLocations
    )
    {
        foreach (var sLgbGroup in lgbFile.Layers)
        {
            foreach (var instanceObject in sLgbGroup.InstanceObjects)
            {
                if (instanceObject.AssetType != LayerEntryType.EventNPC)
                    continue;

                var eventNPC = (LayerCommon.ENPCInstanceObject)instanceObject.Object;
                var npcRowID = eventNPC.ParentData.ParentData.BaseId;
                if (npcRowID == 0) continue;

                if (npcIDToLocations.ContainsKey(npcRowID)) continue;

                if (!LuminaGetter.TryGetRow(npcRowID, out ENpcBase npcBase)) continue;
                if (!LuminaGetter.TryGetRow(npcRowID, out ENpcResident resident)) continue;

                if (!HasRelevantEventHandler(npcBase)) continue;

                var mapID = resident.Map;
                var x     = instanceObject.Transform.Translation.X;
                var y     = instanceObject.Transform.Translation.Z;

                if (CurrentBuildContext.TryGetMapRowID(sTerritoryType.RowId, mapID, out var mapRowID))
                    npcIDToLocations.Add(npcRowID, new(x, y, sTerritoryType.RowId, mapRowID));
                else
                    npcIDToLocations.Add(npcRowID, new(x, y, sTerritoryType.RowId));
            }
        }
    }

    private static LgbFile? GetLgbFileFromBg(string bg) =>
        DService.Instance().Data.GetFile<LgbFile>("bg/" + bg[..(bg.IndexOf("/level/", StringComparison.Ordinal) + 1)] + "level/planevent.lgb");

    private static bool HasRelevantEventHandler(ENpcBase npcBase)
    {
        foreach (var data in npcBase.ENpcData)
        {
            if (data.RowId == 0)
                break;

            if (TryGetEventHandlerType(data.RowId, out _))
                return true;
        }

        return false;
    }

    private static bool TryGetEventHandlerType(uint data, out EventHandlerType type)
    {
        type = (EventHandlerType)(data >> 16);
        return type is EventHandlerType.GilShop
                   or EventHandlerType.CustomTalk
                   or EventHandlerType.GcShop
                   or EventHandlerType.SpecialShop
                   or EventHandlerType.FcShop
                   or EventHandlerType.TopicSelect
                   or EventHandlerType.PreHandler
                   or EventHandlerType.InclusionShop
                   or EventHandlerType.CollectablesShop;
    }

    private static Item ConvertCurrency(uint itemID, SpecialShop specialShop) =>
        itemID is >= 8 or 0
            ? LuminaGetter.GetRowOrDefault<Item>(itemID)
            : specialShop.UseCurrencyType switch
            {
                16 => LuminaGetter.GetRowOrDefault<Item>(Currencies[itemID]),
                8  => LuminaGetter.GetRowOrDefault<Item>(1),
                4 => CurrentBuildContext.TomestoneItemIds.TryGetValue(itemID, out var tomestoneItemID)
                         ? LuminaGetter.GetRowOrDefault<Item>(tomestoneItemID)
                         : LuminaGetter.GetRowOrDefault<Item>(itemID),
                _ => LuminaGetter.GetRowOrDefault<Item>(itemID)
            };

    #endregion

    public bool HasShopNames() =>
        NPCInfos.Any(i => i.ShopName != null);

    public void ApplyFilters()
    {
        FilterDuplicates();
        FilterNoLocationNPCs();
        FilterGCResults();
    }

    public unsafe void FilterGCResults()
    {
        var otherGcVendorIds = GrandCompanyVendors.Values.Where(i => i != GrandCompanyVendors[LocalPlayerState.GrandCompany]);
        if (NPCInfos.Any(i => !otherGcVendorIds.Contains(i.ID)))
            NPCInfos.RemoveAll(i => otherGcVendorIds.Contains(i.ID));

        var freeCompanyFC     = InfoProxyFreeCompany.Instance()->GrandCompany;
        var otherOicVendorIds = OICVendors.Values.Where(i => i != OICVendors[freeCompanyFC]);

        if (otherOicVendorIds != null && NPCInfos.Any(i => !otherOicVendorIds.Contains(i.ID)))
            NPCInfos.RemoveAll(i => otherOicVendorIds.Contains(i.ID));
    }

    public void FilterNoLocationNPCs() =>
        NPCInfos.RemoveAll(i => i.Location == null);

    public void FilterDuplicates() =>
        NPCInfos = NPCInfos.GroupBy(i => new { i.Name, i.Location.TerritoryID, i.Location.X, i.Location.Y })
                           .Select(i => i.First())
                           .ToList();

    public Item GetItem() =>
        LuminaGetter.GetRowOrDefault<Item>(ItemID);

    private enum EventHandlerType : uint
    {
        GilShop          = 0x0004,
        CustomTalk       = 0x000B,
        GcShop           = 0x0016,
        SpecialShop      = 0x001B,
        FcShop           = 0x002A,
        TopicSelect      = 0x0032,
        PreHandler       = 0x0036,
        InclusionShop    = 0x003a,
        CollectablesShop = 0x003B
    }

    private static readonly Dictionary<uint, uint> Currencies = new()
    {
        [1] = 28,
        [2] = 33913,
        [4] = 33914,
        [6] = 41784,
        [7] = 41785
    };

    private static readonly Dictionary<uint, uint> SHBFateShopNPC = new()
    {
        [1027998] = 1769957,
        [1027538] = 1769958,
        [1027385] = 1769959,
        [1027497] = 1769960,
        [1027892] = 1769961,
        [1027665] = 1769962,
        [1027709] = 1769963,
        [1027766] = 1769964
    };

    private static readonly Dictionary<string, string> MJISpecialShopNames = new()
    {
        ["0"]  = "TEXT_CTSMJISPECIALSHOP_00789_TALK_ACTOR",
        ["1"]  = "TEXT_CTSMJISPECIALSHOP_00789_SYSTEM_000_000",
        ["2"]  = "TEXT_CTSMJISPECIALSHOP_00789_SYSTEM_000_005",
        ["3"]  = "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000",
        ["4"]  = "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_005",
        ["5"]  = "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_010",
        ["6"]  = "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_015",
        ["7"]  = "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_020",
        ["8"]  = "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_025",
        ["9"]  = "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_030",
        ["10"] = "TEXT_CTSMJISPECIALSHOP_00789_Q2_000_000",
        ["11"] = "TEXT_CTSMJISPECIALSHOP_00789_Q2_000_005",
        ["12"] = "TEXT_CTSMJISPECIALSHOP_00789_Q2_000_010",
        ["13"] = "TEXT_CTSMJISPECIALSHOP_00789_Q2_000_015",
        ["14"] = "TEXT_CTSMJISPECIALSHOP_00789_OMISE_100_000",
        ["15"] = "TEXT_CTSMJISPECIALSHOP_00789_SYSTEM_100_000",
        ["16"] = "TEXT_CTSMJISPECIALSHOP_00789_OMISE_200_000"
    };

    private static readonly Dictionary<GrandCompany, uint> GrandCompanyVendors = new()
    {
        [GrandCompany.None]           = 0,
        [GrandCompany.Maelstrom]      = 1002387,
        [GrandCompany.TwinAdder]      = 1002393,
        [GrandCompany.ImmortalFlames] = 1002390
    };

    private static readonly Dictionary<GrandCompany, uint> OICVendors = new()
    {
        [GrandCompany.Maelstrom]      = 1002389,
        [GrandCompany.TwinAdder]      = 1000165,
        [GrandCompany.ImmortalFlames] = 1003925,
        [GrandCompany.None]           = 0
    };

    private static readonly List<Item> GrandCompanySeals =
        LuminaGetter.Get<Item>().Where(i => i.RowId is >= 20 and <= 22).Select(i => i).ToList();
}

public class ShopNPCInfos
{
    public uint                   ID        { get; init; }
    public string                 Name      { get; init; }
    public string?                ShopName  { get; init; }
    public List<ShopItemCostInfo> CostInfos { get; init; }
    public ShopNPCLocation        Location  { get; init; }
}

public class ShopNPCLocation
{
    public ShopNPCLocation(float x, float y, uint territoryID, uint? map = null)
    {
        TexturePosition = new Vector2(x, y);
        TerritoryID     = territoryID;
        MapID           = map ?? LuminaGetter.GetRowOrDefault<TerritoryType>(territoryID).Map.RowId;

        var mapInfo = LuminaGetter.GetRowOrDefault<Map>(MapID);
        MapPosition = ToMapPos(TexturePosition, mapInfo.SizeFactor, new(mapInfo.OffsetX, mapInfo.OffsetY));
    }

    public Vector2 TexturePosition { get; }
    public Vector2 MapPosition     { get; }
    public uint    TerritoryID     { get; }
    public uint    MapID           { get; }

    public float X => TexturePosition.X;
    public float Y => TexturePosition.Y;

    public TerritoryType GetTerritory() =>
        LuminaGetter.GetRowOrDefault<TerritoryType>(TerritoryID);

    public Map GetMap() =>
        LuminaGetter.GetRowOrDefault<Map>(MapID);

    private static Vector2 ToMapPos(Vector2 pos, float scale, Vector2 offset)
    {
        var x = ToMapPos(pos.X, scale, (short)offset.X);
        var y = ToMapPos(pos.Y, scale, (short)offset.Y);
        return new(x, y);
    }

    private static float ToMapPos(float val, float scale, short offset)
    {
        var c = scale / 100.0f;

        val = (val + offset) * c;

        return 41.0f / c * ((val + 1024.0f) / 2048.0f) + 1;
    }
}

public record ShopItemCostInfo
(
    uint  Cost,
    uint  ItemID,
    uint? Collectablity = null
)
{
    public string GetItemName() =>
        LuminaWrapper.GetItemName(ItemID);

    public override string ToString()
    {
        if (Collectablity != null)
            return $"{GetItemName()} \ue03d ({Collectablity.Value}~)";

        return $"{GetItemName()} x{Cost}";
    }
}

public enum ItemShopType
{
    GilShop,
    SpecialShop,
    GcShop,
    Achievement,
    FcShop,
    QuestReward,
    CollectableExchange
}
