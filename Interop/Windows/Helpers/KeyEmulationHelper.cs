using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Task = System.Threading.Tasks.Task;

namespace OmenTools.Interop.Windows.Helpers;

public static class KeyEmulationHelper
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern nint SendMessage(nint hWnd, WindowMessage msg, nint wParam, nint lParam);

    private static unsafe nint GameWindowHandle => Framework.Instance()->GameWindow->WindowHandle;

    public static bool SendKeypress(Keys key) =>
        SendKeypress((int)key);

    public static bool SendMousepress(Keys key) =>
        SendKeypress((int)key);

    public static bool SendKeypress(int key)
    {
        SendMessage(GameWindowHandle, WindowMessage.WmKeydown, key, 0);
        SendMessage(GameWindowHandle, WindowMessage.WmKeyup,   key, 0);
        return true;
    }

    public static void SendMousepress(int key)
    {
        switch (key)
        {
            // XButton1
            case 1 | 4:
            {
                var wparam = MakeWParam(0, 0x0001);
                SendMessage(GameWindowHandle, WindowMessage.WmXbuttondown, wparam, 0);
                SendMessage(GameWindowHandle, WindowMessage.WmXbuttonup,   wparam, 0);
                break;
            }
            // XButton2
            case 2 | 4:
            {
                var wparam = MakeWParam(0, 0x0002);
                SendMessage(GameWindowHandle, WindowMessage.WmXbuttondown, wparam, 0);
                SendMessage(GameWindowHandle, WindowMessage.WmXbuttonup,   wparam, 0);
                break;
            }
            default:
                DService.Instance().Log.Error($"尝试发送非法按键 {key}, 已阻止");
                break;
        }
    }

    public static void SendKeyDown(Keys key) =>
        SendKeyDown((int)key);

    public static void SendKeyUp(Keys key) =>
        SendKeyUp((int)key);

    public static void SendKeyDown(int key) =>
        SendMessage(GameWindowHandle, WindowMessage.WmKeydown, key, 0);

    public static void SendKeyUp(int key) =>
        SendMessage(GameWindowHandle, WindowMessage.WmKeyup, key, 0);

    public static async Task<bool> SendKeypressLongPressAsync(Keys key, int durationMilliseconds)
    {
        SendMessage(GameWindowHandle, WindowMessage.WmKeydown, (int)key, 0);
        await Task.Delay(durationMilliseconds);
        SendMessage(GameWindowHandle, WindowMessage.WmKeyup, (int)key, 0);
        return true;
    }

    private static int MakeWParam(int l, int h) => l & 0xFFFF | h << 16;

    private enum WindowMessage : uint
    {
        WmKeydown     = 0x0100,
        WmKeyup       = 0x0101,
        WmXbuttondown = 0x020B,
        WmXbuttonup   = 0x020C
    }
}
