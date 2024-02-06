namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    public static bool IsGameForeground()
    {
        var foregroundWindow = GetForegroundWindow();
        _ = GetWindowThreadProcessId(foregroundWindow, out var foregroundProcId);

        var currentProcess = Process.GetCurrentProcess();
        return currentProcess.Id == foregroundProcId;

    }
}