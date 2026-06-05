using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WeaponCommand : ExecuteCommandBase
{
    /// <summary>
    ///     拔出武器
    /// </summary>
    public static void Draw() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ToggleWeapon, 1, 1);

    /// <summary>
    ///     收回武器
    /// </summary>
    public static void Sheathe() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ToggleWeapon, 0, 1);

    /// <summary>
    ///     拔出或收回武器
    /// </summary>
    public static void Toggle(bool isDraw) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ToggleWeapon, isDraw ? 1U : 0, 1);
}
