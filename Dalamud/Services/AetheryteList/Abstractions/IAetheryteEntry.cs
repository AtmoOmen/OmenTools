using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace OmenTools.Dalamud.Services.AetheryteList.Abstractions;

public interface IAetheryteEntry
{
    uint AetheryteID { get; }

    uint TerritoryID { get; }

    byte SubIndex { get; }

    byte Ward { get; }

    byte Plot { get; }

    uint GilCost { get; }

    bool IsFavourite { get; }

    bool IsSharedHouse { get; }

    bool IsApartment { get; }

    RowRef<Aetheryte> AetheryteData { get; }
}
