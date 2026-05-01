using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SetIndoorEnvironmentCommand : ExecuteCommandBase
{
    /// <summary>
    ///     调整房间环境
    /// </summary>
    /// <param name="light">房间亮度等级</param>
    /// <param name="enableSSAO">是否开启环境光遮蔽 (SSAO)</param>
    public static void Adjust(BrightnessLevel light, bool enableSSAO) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SetIndoorEnvironment, (uint)light, enableSSAO ? 0U : 1);

    public enum BrightnessLevel : uint
    {
        Brightest  = 0,
        VeryBright = 1,
        Bright     = 2,
        Medium     = 3,
        Dim        = 4,
        Darkest    = 5
    }
}
