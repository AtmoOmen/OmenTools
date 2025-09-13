using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel.Sheets;
using GrandCompany = FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany;

namespace OmenTools.Infos;

public class ItemShopInfo
{
    private static bool IsDataInitialized { get; set; }
    
    private static readonly Dictionary<uint, ItemShopInfo> ItemToItemShopInfos = [];
    private static readonly Dictionary<uint, ShopNPCLocation>  NPCIDToLocations    = [];
    
    private static readonly EventHandlerType[] EventHandlerTypes = Enum.GetValues<EventHandlerType>();
    
    public uint               ID                     { get; init; }
    public string             Name                   { get; init; }
    public List<ShopNPCInfos> NPCInfos               { get; private set; }
    public ItemShopType       ShopType               { get; private set; }
    public string             AchievementDescription { get; private set; }

    static ItemShopInfo()
    {
        if (IsDataInitialized) return;
        IsDataInitialized = true;

        Task.Run(() =>
        {
            BuildNPCLocations();
            CorrectNPCLocations();
            
            LuminaGetter.Get<ENpcBase>().ForEach(BuildNPCInfo);
            
            AddAchievementItem();
            
            ItemToItemShopInfos.ForEach(x => x.Value.ApplyFilters());
        });
    }
    
    public static ItemShopInfo? GetItemInfo(uint itemID) => 
        ItemToItemShopInfos.GetValueOrDefault(itemID);

    private static void BuildNPCLocations()
    {
        HashSet<uint> addedAetheryte = [];
        foreach (var territory in LuminaGetter.Get<Aetheryte>()
                                              .Where(i => i.Territory.IsValid && i.Territory.RowId != 1)
                                              .Select(i => i.Territory.Value))
        {
            if (addedAetheryte.Contains(territory.RowId)) continue;

            var file = GetLgbFileFromBg(territory.Bg.ExtractText());
            if (file == null) continue;

            ParseLgbFile(file, territory);
            addedAetheryte.Add(territory.RowId);
        }

        foreach (var territory in LuminaGetter.Get<TerritoryType>()
                                              .Where(type =>
                                              {
                                                  var condition = type.ContentFinderCondition.Value;
                                                  return condition.ContentType.RowId is 26 or 29 or 16;
                                              }))
        {
            var file = GetLgbFileFromBg(territory.Bg.ExtractText());
            if (file == null) continue;

            ParseLgbFile(file, territory);
        }

        foreach (var level in LuminaGetter.Get<Level>().Where(i => i is { Type: 8, Territory.ValueNullable: not null }))
        {
            if (NPCIDToLocations.ContainsKey(level.Object.RowId)) continue;
            if (!LuminaGetter.TryGetRow(level.Object.RowId, out ENpcBase npcBase)) continue;

            var match = npcBase.ENpcData.Any(data => EventHandlerTypes.Any(i => MatchEventHandlerType(data.RowId, i)));
            if (!match) continue;

            NPCIDToLocations.Add(level.Object.RowId, new(level.X, level.Z, level.Territory.RowId));
        }

        foreach (var npc in LuminaGetter.GetSub<HousingEmploymentNpcList>())
        {
            foreach (var id in npc.Select(x => x.MaleENpcBase).Where(i => i.RowId != 0))
                NPCIDToLocations.Add(id.RowId, new(0, 0, 282));
        }
    }

    private static void CorrectNPCLocations()
    {
        NPCIDToLocations[1019100] = new(-85.03851f, 117.05188f, 641);
        NPCIDToLocations[1022846] = new(-83.93994f, 115.31238f, 641);
        NPCIDToLocations[1019106] = new(-99.22949f, 105.6687f, 641);
        NPCIDToLocations[1019107] = new(-100.26703f, 107.43872f, 641);
        NPCIDToLocations[1019104] = new(-67.582275f, 59.739014f, 641);
        NPCIDToLocations[1019102] = new(-59.617065f, 33.524048f, 641);
        NPCIDToLocations[1019103] = new(-52.35376f, 76.58496f, 641);
        NPCIDToLocations[1019101] = new(-36.484375f, 49.240845f, 641);
        NPCIDToLocations[1004418] = new(-114.0307f, 118.30322f, 131, 73);
        
        NPCIDToLocations.TryAdd(1006004, new(5.355835f, 155.22998f, 128));
        NPCIDToLocations.TryAdd(1017613, new(2.822865f, 153.521f, 128));
        NPCIDToLocations.TryAdd(1003633, new(-259.32715f, 37.491333f, 129));
        NPCIDToLocations.TryAdd(1008145, new(-31.265808f, -245.38031f, 133));
        NPCIDToLocations.TryAdd(1006005, new(-61.234497f, -141.31384f, 133));
        NPCIDToLocations.TryAdd(1017614, new(-58.79309f, -142.1073f, 133));
        NPCIDToLocations.TryAdd(1003077, new(145.83044f, -106.767456f, 133));
        NPCIDToLocations.TryAdd(1000215, new(155.35205f, -70.26782f, 133));
        NPCIDToLocations.TryAdd(1000996, new(-28.152893f, 196.70398f, 128));
        NPCIDToLocations.TryAdd(1000999, new(-29.465149f, 197.92468f, 128));
        NPCIDToLocations.TryAdd(1000217, new(170.30591f, -73.16705f, 133));
        NPCIDToLocations.TryAdd(1000597, new(-163.07324f, -78.62976f, 153));
        NPCIDToLocations.TryAdd(1000185, new(-8.590881f, -2.2125854f, 132));
        NPCIDToLocations.TryAdd(1000392, new(-17.746277f, 43.35083f, 132));
        NPCIDToLocations.TryAdd(1000391, new(66.819214f, -143.45007f, 133));
        NPCIDToLocations.TryAdd(1000232, new(164.72107f, -133.68433f, 133));
        NPCIDToLocations.TryAdd(1000301, new(-87.174866f, -173.51044f, 133));
        NPCIDToLocations.TryAdd(1000267, new(103.89868f, -213.03125f, 133));
        NPCIDToLocations.TryAdd(1003252, new(-139.57434f, 31.967651f, 129));
        NPCIDToLocations.TryAdd(1001016, new(-42.679565f, 119.920654f, 128));
        NPCIDToLocations.TryAdd(1005422, new(-397.6349f, 80.979614f, 129));
        NPCIDToLocations.TryAdd(1000244, new(423.17834f, -119.95117f, 154));
        NPCIDToLocations.TryAdd(1000234, new(423.69714f, -122.08746f, 154));
        NPCIDToLocations.TryAdd(1000230, new(421.46936f, -125.993774f, 154));
        NPCIDToLocations.TryAdd(1000222, new(-213.94684f, 300.4348f, 152));
        NPCIDToLocations.TryAdd(1000535, new(-579.4003f, 104.32593f, 152));
        NPCIDToLocations.TryAdd(1002371, new(-480.91858f, 201.9226f, 152));
        NPCIDToLocations.TryAdd(1000396, new(82.597046f, -103.349365f, 148));
        NPCIDToLocations.TryAdd(1000220, new(16.189758f, -15.640564f, 148));
        NPCIDToLocations.TryAdd(1000717, new(175.61597f, 319.32544f, 148));
        NPCIDToLocations.TryAdd(1000718, new(332.23462f, 332.47876f, 154));
        NPCIDToLocations.TryAdd(1002376, new(10.635498f, 220.20288f, 154));
        NPCIDToLocations.TryAdd(1002374, new(204.39453f, -65.75122f, 153));
        NPCIDToLocations.TryAdd(1000579, new(16.03717f, 220.50806f, 152));
        NPCIDToLocations.TryAdd(1002377, new(11.062683f, 221.57617f, 154));
        NPCIDToLocations.TryAdd(1002375, new(203.75366f, -64.560974f, 153));
        NPCIDToLocations.TryAdd(1002389, new(95.8114f, 67.61267f, 128));
    }

    private static void BuildNPCInfo(ENpcBase npcBase)
    {
        var resident = LuminaGetter.GetRowOrDefault<ENpcResident>(npcBase.RowId);
        if (FixNPCInfo(npcBase, resident)) return;

        var fateShop = LuminaGetter.GetRow<FateShop>(npcBase.RowId);
        if (fateShop.HasValue)
        {
            foreach (var specialShop in fateShop.Value.SpecialShop)
            {
                var specialShopCustom = LuminaGetter.GetRow<SpecialShop>(specialShop.RowId);

                if (specialShopCustom == null)
                    continue;

                AddSpecialItem(specialShopCustom.Value, npcBase, resident);
            }

            return;
        }

        foreach (var npcData in npcBase.ENpcData.Select(x => x.RowId))
        {
            if (npcData == 0) break;

            if (MatchEventHandlerType(npcData, EventHandlerType.CollectablesShop))
            {
                var collectablesShop = LuminaGetter.GetRowOrDefault<CollectablesShop>(npcData);
                AddCollectablesShop(collectablesShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.InclusionShop))
            {
                var inclusionShop = LuminaGetter.GetRowOrDefault<InclusionShop>(npcData);
                AddInclusionShop(inclusionShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.FcShop))
            {
                var fccShop = LuminaGetter.GetRowOrDefault<FccShop>(npcData);
                AddFccShop(fccShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.PreHandler))
            {
                var preHandler = LuminaGetter.GetRowOrDefault<PreHandler>(npcData);
                AddItemsInPrehandler(preHandler, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.TopicSelect))
            {
                var topicSelect = LuminaGetter.GetRowOrDefault<TopicSelect>(npcData);
                AddItemsInTopicSelect(topicSelect, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.GcShop))
            {
                var gcShop = LuminaGetter.GetRowOrDefault<GCShop>(npcData);
                AddGcShopItem(gcShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.SpecialShop))
            {
                var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(npcData);
                AddSpecialItem(specialShop, npcBase, resident, shop: specialShop.Name.ExtractText());
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.GilShop))
            {
                var gilShop = LuminaGetter.GetRowOrDefault<GilShop>(npcData);
                AddGilShopItem(gilShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.CustomTalk))
            {
                if (!LuminaGetter.TryGetRow<CustomTalk>(npcData, out var customTalk)) break;

                var scriptArgs = customTalk.Script.Select(x => x.ScriptArg).ToArray();
                if (npcData == 721068)
                {
                    AddItemGeneral(scriptArgs[2],
                                   LuminaWrapper.GetItemName(scriptArgs[2]),
                                   npcBase.RowId,
                                   resident.Singular.ExtractText(),
                                   customTalk.MainOption.ExtractText(),
                                   [new(scriptArgs[4], 28)],
                                   NPCIDToLocations.GetValueOrDefault(npcBase.RowId),
                                   ItemShopType.SpecialShop);
                    continue;
                }

                if (customTalk.SpecialLinks.RowId != 0)
                {
                    try
                    {
                        for (ushort index = 0; index <= 30; index++)
                        {
                            if (!LuminaGetter.TryGetSubRow(customTalk.SpecialLinks.RowId, out CustomTalkNestHandlers customTalkNestHandler, index)) break;

                            if (MatchEventHandlerType(customTalkNestHandler.NestHandler.RowId, EventHandlerType.SpecialShop))
                            {
                                var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(customTalkNestHandler.NestHandler.RowId);
                                AddSpecialItem(specialShop, npcBase, resident);
                                continue;
                            }

                            if (MatchEventHandlerType(customTalkNestHandler.NestHandler.RowId, EventHandlerType.GilShop))
                            {
                                var gilShop = LuminaGetter.GetRowOrDefault<GilShop>(customTalkNestHandler.NestHandler.RowId);
                                AddGilShopItem(gilShop, npcBase, resident);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                foreach (var arg in scriptArgs)
                {
                    if (MatchEventHandlerType(arg, EventHandlerType.GilShop))
                    {
                        var gilShop = LuminaGetter.GetRowOrDefault<GilShop>(arg);
                        AddGilShopItem(gilShop, npcBase, resident);
                        continue;
                    }

                    if (MatchEventHandlerType(arg, EventHandlerType.FcShop))
                    {
                        var shop = LuminaGetter.GetRowOrDefault<FccShop>(arg);
                        AddFccShop(shop, npcBase, resident);
                        continue;
                    }

                    if (MatchEventHandlerType(arg, EventHandlerType.SpecialShop))
                    {
                        var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(arg);
                        AddSpecialItem(specialShop, npcBase, resident);
                    }
                }
            }
        }
    }

    private static bool FixNPCInfo(ENpcBase npcBase, ENpcResident resident)
    {
        switch (npcBase.RowId)
        {
            case 1043463:

                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770601), npcBase, resident, ItemShopType.SpecialShop,
                               $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000")}\n{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q2_000_000")}");
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770659), npcBase, resident, ItemShopType.SpecialShop,
                               $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000")} \n {GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q2_000_005")}");
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770660), npcBase, resident, ItemShopType.SpecialShop,
                               $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000")}\n{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q2_000_010")}");
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770602), npcBase, resident, ItemShopType.SpecialShop,
                               $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_005")}");
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770603), npcBase, resident, ItemShopType.SpecialShop,
                               $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_010")}");
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770723), npcBase, resident, ItemShopType.SpecialShop,
                               $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_025")}");
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770734), npcBase, resident, ItemShopType.SpecialShop,
                               $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_030")}");
                return true;

                string GetNameFromKey(string key)
                {
                    return MJISpecialShopNames.TryGetValue(key, out var str) ? str : string.Empty;
                }
            case 1018655:
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769743), npcBase, resident);
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769744), npcBase, resident);
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770537), npcBase, resident);
                return true;
            case 1016289:
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769635), npcBase, resident);
                return true;
            case 1025047:
                for (uint i = 1769820; i <= 1769834; i++)
                {
                    var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(i);
                    AddSpecialItem(specialShop, npcBase, resident);
                }

                return true;
            case 1025763:
                AddGilShopItem(LuminaGetter.GetRowOrDefault<GilShop>(262919), npcBase, resident);
                return true;
            case 1027123:
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769934), npcBase, resident);
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769935), npcBase, resident);
                return true;
            case 1027124:
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1769937), npcBase, resident);
                return true;
            case 1033921:
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(1770282), npcBase, resident);
                return true;
            case 1035012:
                for (ushort i = 0; i <= 10; i++)
                {
                    var questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(14, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(15, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(19, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                }

                return true;
            case 1016135:

                for (uint i = 3; i <= 10; i++)
                {
                    for (ushort j = 0; j <= 12; j++)
                    {
                        var questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(i, j);
                        AddQuestReward(questClassJobReward, npcBase, resident);
                        AddQuestRewardCost(questClassJobReward, npcBase, GetCost(i));
                    }
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
                    AddQuestReward(questClassJobReward, npcBase, resident, [new(4, 30273)]);
                }

                return true;

            case 1032905:
                for (ushort i = 0; i <= 16; i++)
                {
                    var questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(13, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, [new(4, 30273)]);
                }

                for (ushort i = 0; i <= 16; i++)
                {
                    var questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(17, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase,
                    [
                        new(20, 31573),
                        new(20, 31574),
                        new(20, 31575)
                    ]);

                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(18, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, [new(6, 31576)]);

                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(20, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, [new(15, 32956)]);

                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(21, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, [new(15, 32959)]);

                    questClassJobReward = LuminaGetter.GetSubRowOrDefault<QuestClassJobReward>(22, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, [new(15, 33767)]);
                }

                return true;
            default:
                if (!SHBFateShopNPC.TryGetValue(npcBase.RowId, out var value)) return false;

                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(value), npcBase, resident);
                return true;
        }
    }
    
    #region 添加物品

    private static void AddSpecialItem(
        SpecialShop  specialShop,
        ENpcBase     npcBase,
        ENpcResident resident,
        ItemShopType shopType = ItemShopType.SpecialShop,
        string?      shop     = null)
    {
        foreach (var entry in specialShop.Item)
        {
            foreach (var t in entry.ReceiveItems)
            {
                var item = t.Item.Value;
                var costs = (from e in entry.ItemCosts
                             where e.ItemCost.IsValid && e.ItemCost.Value.Name != string.Empty
                             select new ShopItemCostInfo(e.CurrencyCost, ConvertCurrency(e.ItemCost.Value.RowId, specialShop).RowId)).ToList();

                var achievementDescription = string.Empty;
                if (shopType == ItemShopType.Achievement)
                {
                    achievementDescription = LuminaGetter.Get<Achievement>()
                                                         .Where(i => i.Item.Value.RowId == item.RowId)
                                                         .Select(d => d.Description)
                                                         .First()
                                                         .ExtractText();
                }

                AddItemGeneral(item.RowId, item.Name.ExtractText(), npcBase.RowId, resident.Singular.ExtractText(), shop, costs,
                               NPCIDToLocations.GetValueOrDefault(npcBase.RowId), shopType, achievementDescription);
            }
        }
    }

    private static void AddGilShopItem(GilShop gilShop, ENpcBase npcBase, ENpcResident resident, string shop = null)
    {
        for (ushort i = 0;; i++)
        {
            try
            {
                if (!LuminaGetter.TryGetSubRow(gilShop.RowId, out GilShopItem item, i)) continue;

                AddItemGeneral(item.Item.Value.RowId,
                               item.Item.Value.Name.ExtractText(),
                               npcBase.RowId,
                               resident.Singular.ExtractText(),
                               shop != null ? $"{shop}\n{gilShop.Name}" : gilShop.Name.ExtractText(),
                               [new(item.Item.Value.PriceMid, 1)],
                               NPCIDToLocations.GetValueOrDefault(npcBase.RowId), ItemShopType.GilShop);
            }
            catch
            {
                break;
            }
        }
    }

    private static void AddGcShopItem(GCShop gcId, ENpcBase npcBase, ENpcResident resident)
    {
        var seal = GrandCompanySeals.Find(i => i.Description.ExtractText().Contains($"{gcId.GrandCompany.Value.Name.ExtractText()}"));

        foreach (var category in LuminaGetter.Get<GCScripShopCategory>().Where(i => i.GrandCompany.RowId == gcId.GrandCompany.RowId))
        {
            for (ushort i = 0;; i++)
            {
                try
                {
                    var item = LuminaGetter.GetSub<GCScripShopItem>().GetSubrowOrDefault(category.RowId, i);
                    if (item == null) break;

                    if (item.Value.SortKey == 0) break;

                    AddItemGeneral(item.Value.Item.Value.RowId,
                                   item.Value.Item.Value.Name.ExtractText(),
                                   npcBase.RowId,
                                   resident.Singular.ExtractText(),
                                   null,
                                   [new(item.Value.CostGCSeals, seal.RowId)],
                                   NPCIDToLocations.GetValueOrDefault(npcBase.RowId), ItemShopType.GcShop);
                }
                catch (Exception) { break; }
            }
        }
    }

    private static void AddInclusionShop(InclusionShop inclusionShop, ENpcBase npcBase, ENpcResident resident)
    {
        foreach (var category in inclusionShop.Category)
        {
            if (category.Value.RowId == 0) continue;

            for (ushort i = 0;; i++)
            {
                try
                {
                    if (!LuminaGetter.TryGetSubRow(category.Value.InclusionShopSeries.RowId, out InclusionShopSeries series, i)) continue;

                    var specialShop = series.SpecialShop.Value;
                    var shop        = "";
                    if (!string.IsNullOrEmpty(inclusionShop.Unknown0.ExtractText()))
                        shop += $"{inclusionShop.Unknown0.ExtractText()}\n";
                    shop += $"{category.Value.Name}\n{specialShop.Name}";
                    AddSpecialItem(specialShop, npcBase, resident, shop: shop);
                }
                catch (Exception) { break; }
            }
        }
    }

    private static void AddFccShop(FccShop shop, ENpcBase npcBase, ENpcResident resident)
    {
        foreach (var t in shop.ItemData)
        {
            if (!LuminaGetter.TryGetRow(t.Item.RowId, out Item item)) continue;

            if (item.Name == string.Empty) continue;

            var cost = t.Cost;

            AddItemGeneral(item.RowId,
                           item.Name.ExtractText(),
                           npcBase.RowId,
                           resident.Singular.ExtractText(),
                           null,
                           [new(cost, 102233)],
                           NPCIDToLocations.GetValueOrDefault(npcBase.RowId),
                           ItemShopType.FcShop);
        }
    }

    private static void AddItemsInPrehandler(PreHandler preHandler, ENpcBase npcBase, ENpcResident resident)
    {
        var target = preHandler.Target.RowId;
        if (target == 0)
            return;

        if (MatchEventHandlerType(target, EventHandlerType.GilShop))
        {
            var gilShop = LuminaGetter.GetRowOrDefault<GilShop>(target);
            AddGilShopItem(gilShop, npcBase, resident);
            return;
        }

        if (MatchEventHandlerType(target, EventHandlerType.SpecialShop))
        {
            var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(target);
            AddSpecialItem(specialShop, npcBase, resident);
            return;
        }

        if (MatchEventHandlerType(target, EventHandlerType.InclusionShop))
        {
            var inclusionShop = LuminaGetter.GetRowOrDefault<InclusionShop>(target);
            AddInclusionShop(inclusionShop, npcBase, resident);
        }
    }

    private static void AddItemsInTopicSelect(TopicSelect topicSelect, ENpcBase npcBase, ENpcResident resident)
    {
        foreach (var data in topicSelect.Shop.Select(x => x.RowId))
        {
            if (data == 0)
                continue;

            if (MatchEventHandlerType(data, EventHandlerType.SpecialShop))
            {
                var specialShop = LuminaGetter.GetRowOrDefault<SpecialShop>(data);

                AddSpecialItem(specialShop, npcBase, resident, shop: $"{topicSelect.Name.ExtractText()}\n{specialShop.Name.ExtractText()}");

                continue;
            }

            if (MatchEventHandlerType(data, EventHandlerType.GilShop))
            {
                var gilShop = LuminaGetter.GetRowOrDefault<GilShop>(data);
                AddGilShopItem(gilShop, npcBase, resident, topicSelect.Name.ExtractText());
                continue;
            }

            if (MatchEventHandlerType(data, EventHandlerType.PreHandler))
            {
                var preHandler = LuminaGetter.GetRowOrDefault<PreHandler>(data);
                AddItemsInPrehandler(preHandler, npcBase, resident);
            }
        }
    }

    private static void AddCollectablesShop(CollectablesShop shop, ENpcBase npcBase, ENpcResident resident)
    {
        if (shop.Name.ExtractText() == string.Empty)
            return;

        foreach (var t in shop.ShopItems)
        {
            var         row = t.Value.RowId;

            if (row == 0)
                continue;

            for (ushort subRow = 0; subRow < 100; subRow++)
            {
                try
                {
                    var exchangeItem = LuminaGetter.GetSubRowOrDefault<CollectablesShopItem>(row, subRow);
                    var rewardItem   = LuminaGetter.GetRowOrDefault<CollectablesShopRewardItem>(exchangeItem.CollectablesShopRewardScrip.RowId);
                    var refine       = LuminaGetter.GetRowOrDefault<CollectablesShopRefine>(exchangeItem.CollectablesShopRefine.RowId);
                    if (exchangeItem.Item.RowId <= 1000)
                        continue;

                    var shopName = "";
                    if (!string.IsNullOrEmpty(shop.Name.ExtractText()))
                        shopName += $"{shop.Name.ExtractText()}\n";
                    shopName += $"{exchangeItem.CollectablesShopItemGroup.Value.Name.ExtractText()}";

                    AddItemGeneral(rewardItem.Item.Value.RowId,
                                   rewardItem.Item.Value.Name.ExtractText(),
                                   npcBase.RowId,
                                   resident.Singular.ExtractText(),
                                   shopName,
                                   [
                                       new(rewardItem.RewardLow, exchangeItem.Item.RowId, Collectablity: refine.LowCollectability),
                                       new(rewardItem.RewardMid, exchangeItem.Item.RowId, Collectablity: refine.MidCollectability),
                                       new(rewardItem.RewardHigh, exchangeItem.Item.RowId, Collectablity: refine.HighCollectability)
                                   ],
                                   NPCIDToLocations.GetValueOrDefault(npcBase.RowId),
                                   ItemShopType.CollectableExchange);
                }
                catch { break; }
            }
        }
    }

    private static void AddQuestReward(QuestClassJobReward questReward, ENpcBase npcBase, ENpcResident resident, List<ShopItemCostInfo> cost = null)
    {
        if (questReward.ClassJobCategory.RowId == 0)
            return;

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

            AddItemGeneral(rewardItem.RowId,
                           rewardItem.Value.Name.ExtractText(),
                           npcBase.RowId,
                           resident.Singular.ExtractText(), string.Empty,
                           cost,
                           NPCIDToLocations.GetValueOrDefault(npcBase.RowId),
                           ItemShopType.QuestReward);
        }
    }

    private static void AddQuestRewardCost(QuestClassJobReward questReward, ENpcBase npcBase, List<ShopItemCostInfo> cost)
    {
        if (cost == null || questReward.ClassJobCategory.RowId == 0)
            return;

        foreach (var rewardItem in questReward.RewardItem)
        {
            if (rewardItem.RowId == 0)
                break;

            AddItemCost(rewardItem.RowId, npcBase.RowId, cost);
        }
    }
    
    private static void AddAchievementItem()
    {
        for (var i = 1006004u; i <= 1006006; i++)
        {
            var npcBase  = LuminaGetter.GetRowOrDefault<ENpcBase>(i);
            var resident = LuminaGetter.GetRowOrDefault<ENpcResident>(i);

            for (var j = 1769898u; j <= 1769906; j++)
                AddSpecialItem(LuminaGetter.GetRowOrDefault<SpecialShop>(j), npcBase, resident, ItemShopType.Achievement);
        }
    }

    private static void AddItemCost(uint itemId, uint npcId, List<ShopItemCostInfo> cost)
    {
        if (itemId == 0)
            return;

        if (!ItemToItemShopInfos.TryGetValue(itemId, out var itemInfo))
            return;

        var result = itemInfo.NPCInfos.Find(i => i.ID == npcId);
        if (result == null)
            return;

        result.CostInfos.AddRange(cost);
    }

    private static void AddItemGeneral(
        uint                   itemID,
        string                 itemName,
        uint                   npcID,
        string                 npcName,
        string?                shopName,
        List<ShopItemCostInfo> cost,
        ShopNPCLocation        npcLocation,
        ItemShopType           shopType,
        string                 achievementDesc = "")
    {
        if (itemID == 0)
            return;

        if (DService.ClientState.ClientLanguage != ClientLanguage.Japanese && shopName == "アイテムの購入")
            shopName = string.Empty;

        if (!ItemToItemShopInfos.ContainsKey(itemID))
        {
            ItemToItemShopInfos.Add(itemID, new()
            {
                ID                     = itemID,
                Name                   = itemName,
                NPCInfos               = [new() { ID = npcID, Location = npcLocation, CostInfos = cost, Name = npcName, ShopName = shopName }],
                ShopType               = shopType,
                AchievementDescription = achievementDesc
            });
            return;
        }

        if (!ItemToItemShopInfos.TryGetValue(itemID, out var itemInfo))
            ItemToItemShopInfos.TryAdd(itemID, itemInfo = new()
            {
                ID                     = itemID,
                Name                   = itemName,
                NPCInfos               = [new() { ID = npcID, Location = npcLocation, CostInfos = cost, Name = npcName, ShopName = shopName }],
                ShopType               = shopType,
                AchievementDescription = achievementDesc
            });

        if (shopType == ItemShopType.Achievement && itemInfo.ShopType != ItemShopType.Achievement)
        {
            itemInfo.ShopType               = ItemShopType.Achievement;
            itemInfo.AchievementDescription = achievementDesc;
        }

        if (itemInfo.NPCInfos.Find(j => j.ID == npcID) == null)
            itemInfo.NPCInfos.Add(new() { ID = npcID, Location = npcLocation, Name = npcName, CostInfos = cost, ShopName = shopName });
    }

    #endregion

    #region 工具

    private static void ParseLgbFile(LgbFile lgbFile, TerritoryType sTerritoryType)
    {
        foreach (var sLgbGroup in lgbFile.Layers)
        {
            foreach (var instanceObject in sLgbGroup.InstanceObjects)
            {
                if (instanceObject.AssetType != LayerEntryType.EventNPC)
                    continue;

                var eventNpc = (LayerCommon.ENPCInstanceObject)instanceObject.Object;
                var npcRowId = eventNpc.ParentData.ParentData.BaseId;
                if (npcRowId == 0) continue;

                if (NPCIDToLocations.ContainsKey(npcRowId)) continue;

                if (!LuminaGetter.TryGetRow(npcRowId, out ENpcBase npcBase)) continue;
                if (!LuminaGetter.TryGetRow(npcRowId, out ENpcResident resident)) continue;

                var match = npcBase.ENpcData.Any(data => EventHandlerTypes.Any(i => MatchEventHandlerType(data.RowId, i)));
                if (!match) continue;

                var mapId = resident.Map;
                try
                {
                    var map = LuminaGetter.Get<Map>().First(i => i.TerritoryType.RowId == sTerritoryType.RowId && i.MapIndex == mapId);
                    NPCIDToLocations.Add(npcRowId, new(instanceObject.Transform.Translation.X, instanceObject.Transform.Translation.Z, sTerritoryType.RowId, map.RowId));
                }
                catch (InvalidOperationException)
                {
                    NPCIDToLocations.Add(npcRowId, new(instanceObject.Transform.Translation.X, instanceObject.Transform.Translation.Z, sTerritoryType.RowId));
                }
            }
        }
    }

    private static LgbFile? GetLgbFileFromBg(string bg) => 
        DService.Data.GetFile<LgbFile>("bg/" + bg[..(bg.IndexOf("/level/", StringComparison.Ordinal) + 1)] + "level/planevent.lgb");

    private static bool MatchEventHandlerType(uint data, EventHandlerType type) =>
        data >> 16 == (uint)type;

    private static Item ConvertCurrency(uint itemID, SpecialShop specialShop) =>
        itemID is >= 8 or 0
            ? LuminaGetter.GetRowOrDefault<Item>(itemID)
            : specialShop.UseCurrencyType switch
            {
                16 => LuminaGetter.GetRowOrDefault<Item>(Currencies[itemID]),
                8  => LuminaGetter.GetRowOrDefault<Item>(1),
                4  => LuminaGetter.GetRowOrDefault<Item>(LuminaGetter.Get<TomestonesItem>().First(i => i.Tomestones.Value.RowId == itemID).Item.RowId),
                _  => LuminaGetter.GetRowOrDefault<Item>(itemID)
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
    public string                 ShopName  { get; init; }
    public List<ShopItemCostInfo> CostInfos { get; init; }
    public ShopNPCLocation        Location  { get; init; }
}

public class ShopNPCLocation
{
    public ShopNPCLocation(float x, float y, uint territoryID, uint? map = null)
    {
        TexturePosition = new Vector2(x, y);
        TerritoryID     = territoryID;
        MapID           = map ?? GetTerritory().Map.RowId;
        MapPosition     = TextureToMap((int)x, (int)y, GetTerritory().Map.Value.SizeFactor);
    }

    public Vector2 TexturePosition { get; }
    public Vector2 MapPosition     { get; }
    public uint    TerritoryID     { get; }
    public uint    MapID           { get; }

    public float X => TexturePosition.X;
    public float Y => TexturePosition.Y;

    private TerritoryType GetTerritory() => 
        LuminaGetter.GetRowOrDefault<TerritoryType>(TerritoryID);
        
    private Map GetMap() => 
        LuminaGetter.GetRowOrDefault<Map>(MapID);
}

public record ShopItemCostInfo(uint Cost, uint ItemID, uint? Collectablity = null)
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
