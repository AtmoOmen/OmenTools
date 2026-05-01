using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RemoveFieldMarkerCommand : ExecuteCommandBase
{
    /// <summary>
    ///     移除场地标点
    /// </summary>
    public static void Remove(FieldMarkerPoint point) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RemoveFieldMarker, (uint)point);
}
