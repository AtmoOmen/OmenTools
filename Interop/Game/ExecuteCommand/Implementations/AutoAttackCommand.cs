using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AutoAttackCommand : ExecuteCommandBase
{
    /// <summary>
    ///     开启自动攻击状态
    /// </summary>
    public static void Enable(uint targetID, bool isManual = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AutoAttack, 1, targetID, isManual ? 1U : 0U);

    /// <summary>
    ///     禁用自动攻击状态
    /// </summary>
    public static void Disable(bool isManual = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AutoAttack, 0, 0xE0000000, isManual ? 1U : 0U);

    /// <summary>
    ///     设置自动攻击状态
    /// </summary>
    public static void Set(bool isEnabled, uint targetObjectID, bool isManual = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AutoAttack, isEnabled ? 1U : 0U, targetObjectID, isManual ? 1U : 0U);
}
