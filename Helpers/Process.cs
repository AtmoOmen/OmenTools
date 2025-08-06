using System.Runtime.InteropServices;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint FindWindowEx(nint hwndParent, nint hwndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern nint SendMessage(nint hWnd, WindowMessage Msg, nint wParam, nint lParam);

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

    public static void SendKeyDown(Keys key) => SendKeyDown((int)key);

    public static void SendKeyUp(Keys key) => SendKeyUp((int)key);

    public static void SendKeyDown(int key)
    {
        if (!TryFindGameWindow(out var h))
        {
            DService.Log.Error("未能找到游戏窗口, 按键按下发送失败");
            return;
        }

        SendMessage(h, WindowMessage.WM_KEYDOWN, key, 0);
    }

    public static void SendKeyUp(int key)
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

public enum WindowMessage : uint
{
    WM_KEYDOWN     = 0x0100,
    WM_KEYUP       = 0x0101,
    WM_XBUTTONDOWN = 0x020B,
    WM_XBUTTONUP   = 0x020C
}

public enum Keys
{
    None        = 0,
    LButton     = 0x01,
    RButton     = 0x02,
    Cancel      = 0x03,
    MButton     = 0x04,
    XButton1    = 0x05,
    XButton2    = 0x06,
    Back        = 0x08,
    Tab         = 0x09,
    Clear       = 0x0C,
    Return      = 0x0D,
    Enter       = Return,
    Shift       = 0x10,
    Control     = 0x11,
    Menu        = 0x12,
    Alt         = Menu,
    Pause       = 0x13,
    Capital     = 0x14,
    CapsLock    = Capital,
    Escape      = 0x1B,
    Space       = 0x20,
    Prior       = 0x21,
    PageUp      = Prior,
    Next        = 0x22,
    PageDown    = Next,
    End         = 0x23,
    Home        = 0x24,
    Left        = 0x25,
    Up          = 0x26,
    Right       = 0x27,
    Down        = 0x28,
    Select      = 0x29,
    Print       = 0x2A,
    Execute     = 0x2B,
    Snapshot    = 0x2C,
    PrintScreen = Snapshot,
    Insert      = 0x2D,
    Delete      = 0x2E,
    Help        = 0x2F,
    D0          = 0x30,
    D1          = 0x31,
    D2          = 0x32,
    D3          = 0x33,
    D4          = 0x34,
    D5          = 0x35,
    D6          = 0x36,
    D7          = 0x37,
    D8          = 0x38,
    D9          = 0x39,
    A           = 0x41,
    B           = 0x42,
    C           = 0x43,
    D           = 0x44,
    E           = 0x45,
    F           = 0x46,
    G           = 0x47,
    H           = 0x48,
    I           = 0x49,
    J           = 0x4A,
    K           = 0x4B,
    L           = 0x4C,
    M           = 0x4D,
    N           = 0x4E,
    O           = 0x4F,
    P           = 0x50,
    Q           = 0x51,
    R           = 0x52,
    S           = 0x53,
    T           = 0x54,
    U           = 0x55,
    V           = 0x56,
    W           = 0x57,
    X           = 0x58,
    Y           = 0x59,
    Z           = 0x5A,
    LWin        = 0x5B,
    RWin        = 0x5C,
    Apps        = 0x5D,
    Sleep       = 0x5F,
    NumPad0     = 0x60,
    NumPad1     = 0x61,
    NumPad2     = 0x62,
    NumPad3     = 0x63,
    NumPad4     = 0x64,
    NumPad5     = 0x65,
    NumPad6     = 0x66,
    NumPad7     = 0x67,
    NumPad8     = 0x68,
    NumPad9     = 0x69,
    Multiply    = 0x6A,
    Add         = 0x6B,
    Separator   = 0x6C,
    Subtract    = 0x6D,
    Decimal     = 0x6E,
    Divide      = 0x6F,
    F1          = 0x70,
    F2          = 0x71,
    F3          = 0x72,
    F4          = 0x73,
    F5          = 0x74,
    F6          = 0x75,
    F7          = 0x76,
    F8          = 0x77,
    F9          = 0x78,
    F10         = 0x79,
    F11         = 0x7A,
    F12         = 0x7B,
    F13         = 0x7C,
    F14         = 0x7D,
    F15         = 0x7E,
    F16         = 0x7F,
    F17         = 0x80,
    F18         = 0x81,
    F19         = 0x82,
    F20         = 0x83,
    F21         = 0x84,
    F22         = 0x85,
    F23         = 0x86,
    F24         = 0x87,
    NumLock     = 0x90,
    Scroll      = 0x91,
    LShift      = 0xA0,
    RShift      = 0xA1,
    LControl    = 0xA2,
    RControl    = 0xA3,
    LMenu       = 0xA4,
    RMenu       = 0xA5
}
