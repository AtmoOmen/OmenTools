using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class CraftCommand : ExecuteCommandBase
{
    /// <summary>
    ///     开始普通制作
    /// </summary>
    public static void Craft(uint recipeID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Craft, 0, recipeID);

    /// <summary>
    ///     开始简易制作
    /// </summary>
    public static void QuickSynthesis(uint recipeID, byte count) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Craft, 1, recipeID, count);

    /// <summary>
    ///     开始制作练习
    /// </summary>
    public static void Practice(uint recipeID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Craft, 2, recipeID);
}
