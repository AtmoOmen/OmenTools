using static PInvoke.User32;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
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
}