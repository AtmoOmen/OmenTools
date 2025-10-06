using FFXIVClientStructs.FFXIV.Client.Game;

namespace OmenTools.Infos;

public static partial class InfosOm
{
    public static float GlobalFontScale => FontManager.GlobalFontScale;
    
    public static readonly List<InventoryType> PlayerInventories =
    [
        InventoryType.Inventory1, 
        InventoryType.Inventory2,
        InventoryType.Inventory3, 
        InventoryType.Inventory4
    ];

    public static readonly List<InventoryType> PlayerArmoryInventories =
    [
        InventoryType.EquippedItems,
        InventoryType.ArmoryMainHand,
        InventoryType.ArmoryOffHand,
        InventoryType.ArmoryHead,
        InventoryType.ArmoryBody,
        InventoryType.ArmoryHands,
        InventoryType.ArmoryWaist,
        InventoryType.ArmoryLegs,
        InventoryType.ArmoryFeets,
        InventoryType.ArmoryEar,
        InventoryType.ArmoryNeck,
        InventoryType.ArmoryWrist,
        InventoryType.ArmoryRings,
        InventoryType.Inventory1,
        InventoryType.Inventory2,
        InventoryType.Inventory3,
        InventoryType.Inventory4
    ];
    
    public static readonly List<InventoryType> RetainerInventories =
    [
        InventoryType.RetainerPage1, 
        InventoryType.RetainerPage2, 
        InventoryType.RetainerPage3, 
        InventoryType.RetainerPage4, 
        InventoryType.RetainerPage5,
        InventoryType.RetainerPage6,
        InventoryType.RetainerPage7,
    ];
    
    public static readonly List<InventoryType> RetainerAllInventories =
    [
        InventoryType.RetainerPage1, 
        InventoryType.RetainerPage2, 
        InventoryType.RetainerPage3, 
        InventoryType.RetainerPage4, 
        InventoryType.RetainerPage5,
        InventoryType.RetainerPage6,
        InventoryType.RetainerPage7,
        InventoryType.RetainerEquippedItems,
        InventoryType.RetainerGil,
        InventoryType.RetainerCrystals,
        InventoryType.RetainerMarket
    ];
}
