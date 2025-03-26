using System.Runtime.InteropServices;
using static PInvoke.User32;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualProtect(nint lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

    private const uint PAGE_READWRITE = 0x04;

    public static unsafe void WriteProtectedMemory<T>(nint targetAddress, T value) where T : unmanaged
    {
        var size = (uint)Marshal.SizeOf(typeof(T));

        var success = VirtualProtect(targetAddress, size, PAGE_READWRITE, out var oldProtect);
        if (!success)
        {
            throw new InvalidOperationException("Failed to change memory protection.");
        }

        var ptr = targetAddress.ToPointer();
        *(T*)ptr = value;

        VirtualProtect(targetAddress, size, oldProtect, out _);
    }
    
    public static unsafe T ReadProtectedMemory<T>(nint sourceAddress) where T : unmanaged
    {
        var size = (uint)Marshal.SizeOf(typeof(T));

        var success = VirtualProtect(sourceAddress, size, PAGE_READWRITE, out var oldProtect);
        if (!success)
        {
            throw new InvalidOperationException("Failed to change memory protection.");
        }

        var value = *(T*)sourceAddress.ToPointer();

        VirtualProtect(sourceAddress, size, oldProtect, out _);

        return value;
    }

    public static bool TryFindGameWindow(out nint hwnd)
    {
        hwnd = nint.Zero;
        while (true)
        {
            hwnd = FindWindowEx(nint.Zero, hwnd, "FFXIVGAME", null);
            if (hwnd == nint.Zero) break;
            _ = GetWindowThreadProcessId(hwnd, out var pid);
            if (pid == Environment.ProcessId) break;
        }

        return hwnd != nint.Zero;
    }

    public static bool ApplicationIsActivated()
    {
        var activatedHandle = GetForegroundWindow();
        if (activatedHandle == nint.Zero)
            return false;

        var procId = Environment.ProcessId;
        _ = GetWindowThreadProcessId(activatedHandle, out var activeProcId);

        return activeProcId == procId;
    }

    public static bool SendKeypress(Keys key) => SendKeypress((int)key);

    public static bool SendMousepress(Keys key) => SendKeypress((int)key);

    public static bool SendKeypress(int key)
    {
        if(!TryFindGameWindow(out var h))
        {
            DService.Log.Error("未能找到游戏窗口, 按键发送失败");
            return false;
        }

        SendMessage(h, WindowMessage.WM_KEYDOWN, key, 0);
        SendMessage(h, WindowMessage.WM_KEYUP, key, 0);
        return true;
    }

    public static void SendMousepress(int key)
    {
        if (!TryFindGameWindow(out var h))
        {
            DService.Log.Error("未能找到游戏窗口, 鼠标点击发送失败");
            return;
        }

        switch (key)
        {
            // XButton1
            case 1 | 4:
            {
                var wparam = MAKEWPARAM(0, 0x0001);
                SendMessage(h, WindowMessage.WM_XBUTTONDOWN, wparam, 0);
                SendMessage(h, WindowMessage.WM_XBUTTONUP, wparam, 0);
                break;
            }
            // XButton2
            case 2 | 4:
            {
                var wparam = MAKEWPARAM(0, 0x0002);
                SendMessage(h, WindowMessage.WM_XBUTTONDOWN, wparam, 0);
                SendMessage(h, WindowMessage.WM_XBUTTONUP, wparam, 0);
                break;
            }
            default:
                DService.Log.Error($"尝试发送非法按键 {key}, 已阻止");
                break;
        }
    }

    public static void SendKeydown(Keys key) => SendKeydown((int)key);

    public static void SendKeyup(Keys key) => SendKeyup((int)key);

    public static void SendKeydown(int key)
    {
        if (!TryFindGameWindow(out var h))
        {
            DService.Log.Error("未能找到游戏窗口, 按键按下发送失败");
            return;
        }

        SendMessage(h, WindowMessage.WM_KEYDOWN, key, 0);
    }

    public static void SendKeyup(int key)
    {
        if (!TryFindGameWindow(out var h))
        {
            DService.Log.Error("未能找到游戏窗口, 按键释放发送失败");
            return;
        }

        SendMessage(h, WindowMessage.WM_KEYUP, key, 0);
    }

    public static async Task<bool> SendKeypressLongPressAsync(Keys key, int durationMilliseconds)
    {
        if (!TryFindGameWindow(out var h))
        {
            DService.Log.Error("未能找到游戏窗口, 按键发送失败");
            return false;
        }

        SendMessage(h, WindowMessage.WM_KEYDOWN, (int)key, 0);
        await Task.Delay(durationMilliseconds);
        SendMessage(h, WindowMessage.WM_KEYUP, (int)key, 0);
        return true;
    }


    public static int MAKEWPARAM(int l, int h) => (l & 0xFFFF) | (h << 16);
}
