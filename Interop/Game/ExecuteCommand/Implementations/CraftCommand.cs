using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class CraftCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.Craft;

    /// <summary>
    ///     开始普通制作
    /// </summary>
    public void Normal(uint recipeID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 0, recipeID);

    /// <summary>
    ///     开始简易制作
    /// </summary>
    public void Quick(uint recipeID, uint count) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 1, recipeID, count);

    /// <summary>
    ///     开始制作练习
    /// </summary>
    public void Trial(uint recipeID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 2, recipeID);
}