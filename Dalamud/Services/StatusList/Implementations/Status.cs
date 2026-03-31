using System.Diagnostics.CodeAnalysis;
using Lumina.Excel;
using OmenTools.Dalamud.Services.StatusList.Abstractions;

namespace OmenTools.Dalamud.Services.StatusList.Implementations;

internal readonly unsafe struct Status
(
    FFXIVClientStructs.FFXIV.Client.Game.Status* ptr
) : IStatus
{
    public nint Address => (nint)ptr;

    public uint StatusID => ptr->StatusId;

    public RowRef<Lumina.Excel.Sheets.Status> GameData => ptr->StatusId.ToLuminaRowRef<Lumina.Excel.Sheets.Status>();

    public ushort Param => ptr->Param;

    public float RemainingTime => ptr->RemainingTime;

    public uint SourceID => ptr->SourceObject.ObjectId;

    public IGameObject? SourceObject => DService.Instance().ObjectTable.SearchByID(SourceID);

    public static bool operator ==(Status x, Status y) => x.Equals(y);

    public static bool operator !=(Status x, Status y) => !(x == y);

    public bool Equals(IStatus? other) =>
        StatusID == other.StatusID && SourceID == other.SourceID && Param == other.Param && RemainingTime == other.RemainingTime;

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Status fate && Equals(fate);

    public override int GetHashCode() =>
        HashCode.Combine(StatusID, SourceID, Param, RemainingTime);
}
