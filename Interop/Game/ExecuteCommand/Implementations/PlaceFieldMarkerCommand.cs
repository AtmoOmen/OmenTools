using System.Numerics;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class PlaceFieldMarkerCommand : ExecuteCommandBase
{
    /// <summary>
    ///     放置场地标点
    /// </summary>
    public static void Place(FieldMarkerPoint point, Vector3 position) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.PlaceFieldMarker,
            (uint)point,
            (uint)(int)(position.X * 1000f),
            (uint)(int)(position.Y * 1000f),
            (uint)(int)(position.Z * 1000f)
        );
}
