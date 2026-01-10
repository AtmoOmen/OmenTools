using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    [DllImport("imm32.dll")]
    private static extern nint ImmGetContext(nint hwnd);

    [DllImport("imm32.dll")]
    private static extern bool ImmSetOpenStatus(nint himc, bool b);

    private static unsafe nint GameWindowHandle => Framework.Instance()->GameWindow->WindowHandle;
    
    /// <summary>
    /// 禁用输入法
    /// </summary>
    public static void DisableIME()
    {
        var himc = ImmGetContext(GameWindowHandle);
        ImmSetOpenStatus(himc, false);
    }
    
    /// <summary>
    /// 启用输入法
    /// </summary>
    public static void EnableIME()
    {
        var himc = ImmGetContext(GameWindowHandle);
        ImmSetOpenStatus(himc, true);
    }
}
