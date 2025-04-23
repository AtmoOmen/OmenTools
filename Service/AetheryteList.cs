using System.Collections;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;

namespace OmenTools.Service;

internal sealed unsafe partial class AetheryteList : IAetheryteList
{
    private readonly Telepo* telepoInstance = Telepo.Instance();

    private AetheryteList() { }

    public int Length
    {
        get
        {
            if (DService.ObjectTable.LocalPlayer == null)
                return 0;

            this.Update();

            return this.telepoInstance->TeleportList.First == this.telepoInstance->TeleportList.Last ? 
                       0 : 
                       this.telepoInstance->TeleportList.Count;
        }
    }

    public IAetheryteEntry? this[int index]
    {
        get
        {
            if (index < 0 || index >= this.Length)
                return null;

            return DService.ObjectTable.LocalPlayer == null ? null : new AetheryteEntry(this.telepoInstance->TeleportList[index]);
        }
    }

    private void Update()
    {
        if (DService.ObjectTable.LocalPlayer == null)
            return;

        this.telepoInstance->UpdateAetheryteList();
    }
}

public interface IAetheryteEntry
{
    uint AetheryteId { get; }

    uint TerritoryId { get; }

    byte SubIndex { get; }

    byte Ward { get; }

    byte Plot { get; }

    uint GilCost { get; }

    bool IsFavourite { get; }

    bool IsSharedHouse { get; }

    bool IsApartment { get; }

    RowRef<Lumina.Excel.Sheets.Aetheryte> AetheryteData { get; }
}

internal sealed class AetheryteEntry : IAetheryteEntry
{
    private readonly TeleportInfo data;

    internal AetheryteEntry(TeleportInfo data) => this.data = data;

    public uint AetheryteId => this.data.AetheryteId;

    public uint TerritoryId => this.data.TerritoryId;

    public byte SubIndex => this.data.SubIndex;

    public byte Ward => this.data.Ward;

    public byte Plot => this.data.Plot;

    public uint GilCost => this.data.GilCost;

    public bool IsFavourite => this.data.IsFavourite != 0;
    
    public bool IsSharedHouse => this.data.IsSharedHouse;

    public bool IsApartment => this.data.IsApartment;

    public RowRef<Lumina.Excel.Sheets.Aetheryte> AetheryteData => 
        LuminaCreateRef<Lumina.Excel.Sheets.Aetheryte>(this.AetheryteId);
}

internal sealed partial class AetheryteList
{
    public int Count => this.Length;

    public IEnumerator<IAetheryteEntry> GetEnumerator()
    {
        for (var i = 0; i < this.Length; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

public interface IAetheryteList : IReadOnlyCollection<IAetheryteEntry>
{
    public int Length { get; }

    public IAetheryteEntry? this[int index] { get; }
}
