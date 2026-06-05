using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class GlassesCommand : ExecuteCommandBase
{
    /// <summary>
    ///     装备面部配饰
    /// </summary>
    public static void Equip(uint glassesSlot, uint glassesID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EquipFacewear, glassesSlot, glassesID);
}
