using System.Numerics;
using OmenTools.Extensions;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class PlaceFieldMarkerCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.PlaceFieldMarker;

    /// <summary>
    ///     放置场地标点
    /// </summary>
    public void Place(FieldMarkerPoint point, Vector3 position) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            Flag,
            (uint)point,
            (uint)(int)(position.X * 1000f),
            (uint)(int)(position.Y * 1000f),
            (uint)(int)(position.Z * 1000f)
        );
}