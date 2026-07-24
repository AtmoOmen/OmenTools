using Lumina.Excel;
using LuminaStatus = Lumina.Excel.Sheets.Status;

namespace OmenTools.Dalamud.Services.Game.Abstractions;

public interface IStatus : IEquatable<IStatus>
{
    nint Address { get; }

    uint StatusID { get; }

    RowRef<LuminaStatus> GameData { get; }

    ushort Param { get; }

    float RemainingTime { get; }

    uint SourceID { get; }

    IGameObject? SourceObject { get; }

    static unsafe IStatus Create(nint address) => new Status((FFXIVClientStructs.FFXIV.Client.Game.Status*)address);
}
