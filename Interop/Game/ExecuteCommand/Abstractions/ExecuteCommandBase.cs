using OmenTools.Info.Game.Enums;

namespace OmenTools.Interop.Game.ExecuteCommand.Abstractions;

public abstract class ExecuteCommandBase
{
    public abstract ExecuteCommandFlag Flag { get; }
}
