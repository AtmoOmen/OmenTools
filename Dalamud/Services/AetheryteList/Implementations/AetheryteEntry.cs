using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace OmenTools.Dalamud.Services.AetheryteList;

internal readonly struct AetheryteEntry
(
    TeleportInfo data
) : IAetheryteEntry
{
    public uint AetheryteID => data.AetheryteId;

    public uint TerritoryID => data.TerritoryId;

    public byte SubIndex => data.SubIndex;

    public byte Ward => data.Ward;

    public byte Plot => data.Plot;

    public uint GilCost => data.GilCost;

    public bool IsFavourite => data.IsFavourite;

    public bool IsSharedHouse => data.IsSharedHouse;

    public bool IsApartment => data.IsApartment;

    public RowRef<Aetheryte> AetheryteData => AetheryteID.ToLuminaRowRef<Aetheryte>();
}
