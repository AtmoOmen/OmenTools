using System.Collections;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace OmenTools.Service;

#region AetheryteList

public interface IAetheryteList : IReadOnlyCollection<IAetheryteEntry>
{
    public int Length { get; }

    public IAetheryteEntry? this[int index] { get; }
}

internal sealed unsafe partial class AetheryteList : IAetheryteList
{
    private readonly Telepo* telepoInstance = Telepo.Instance();

    public int Length
    {
        get
        {
            if (DService.Instance().ObjectTable.LocalPlayer == null)
                return 0;

            Update();

            if (telepoInstance->TeleportList.First == telepoInstance->TeleportList.Last)
                return 0;

            return telepoInstance->TeleportList.Count;
        }
    }

    public IAetheryteEntry? this[int index]
    {
        get
        {
            if (index < 0 || index >= Length)
                return null;

            if (DService.Instance().ObjectTable.LocalPlayer == null)
                return null;

            return new AetheryteEntry(telepoInstance->TeleportList[index]);
        }
    }

    private void Update()
    {
        if (DService.Instance().ObjectTable.LocalPlayer == null)
            return;

        telepoInstance->UpdateAetheryteList();
    }
}

internal sealed partial class AetheryteList
{
    public int Count => Length;

    public IEnumerator<IAetheryteEntry> GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private struct Enumerator(AetheryteList aetheryteList) : IEnumerator<IAetheryteEntry>
    {
        private int index = -1;

        public IAetheryteEntry Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (++index < aetheryteList.Length)
            {
                Current = aetheryteList[index];
                return true;
            }

            Current = null;
            return false;
        }

        public void Reset() => 
            index = -1;

        public void Dispose()
        {
        }
    }
}

#endregion

#region AetheryteEntry

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

internal readonly struct AetheryteEntry(TeleportInfo data) : IAetheryteEntry
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

#endregion
