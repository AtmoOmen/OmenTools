using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WeaponCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.Weapon;
    
    /// <summary>
    ///     拔出武器
    /// </summary>
    public void Draw() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 1, 1);

    /// <summary>
    ///     收回武器
    /// </summary>
    public void Sheathe() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 0, 1);
}
