using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FishCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.Fish;

    /// <summary>
    ///     抛竿
    /// </summary>
    public void Cast() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 0);

    /// <summary>
    ///     收杆
    /// </summary>
    public void Quit() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 1);

    /// <summary>
    ///     提钩
    /// </summary>
    public void Hook() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 2);

    /// <summary>
    ///     换饵
    /// </summary>
    public void ChangeBait(uint itemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 4, itemID);

    /// <summary>
    ///     坐下
    /// </summary>
    public void Sit() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 5);

    /// <summary>
    ///     强力提钩
    /// </summary>
    public void PowerfulHookset() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 10);

    /// <summary>
    ///     精准提钩
    /// </summary>
    public void PrecisionHookset() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 11);

    /// <summary>
    ///     耐心
    /// </summary>
    public void Patience() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 13);

    /// <summary>
    ///     耐心 2
    /// </summary>
    public void Patience2() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 14);

    /// <summary>
    ///     熟练妙招
    /// </summary>
    public void ThaliaksFavor() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 24);

    /// <summary>
    ///     游动饵
    /// </summary>
    public void Mooch(uint baitIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 25, baitIndex);
}