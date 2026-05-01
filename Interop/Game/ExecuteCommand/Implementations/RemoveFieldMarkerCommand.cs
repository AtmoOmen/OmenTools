using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RemoveFieldMarkerCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RemoveFieldMarker;

    /// <summary>
    ///     移除场地标点
    /// </summary>
    public void Remove(FieldMarkerPoint point) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)point);
}
