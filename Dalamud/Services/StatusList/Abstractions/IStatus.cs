using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace OmenTools.Dalamud.Services.StatusList.Abstractions;

public interface IStatus : IEquatable<IStatus>
{
    nint Address { get; }

    uint StatusID { get; }

    RowRef<Status> GameData { get; }

    ushort Param { get; }

    float RemainingTime { get; }

    uint SourceID { get; }

    IGameObject? SourceObject { get; }

    static unsafe IStatus Create(nint address) => new Implementations.Status((FFXIVClientStructs.FFXIV.Client.Game.Status*)address);
}
