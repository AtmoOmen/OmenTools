using System.Runtime.InteropServices;

namespace OmenTools.Interop.Windows;

public static partial class WindowNative
{
    public static bool IsForeground(nint window) => GetForegroundWindow() == window;

    [LibraryImport(USER32)]
    private static partial nint GetForegroundWindow();

    [LibraryImport(USER32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsIconic(nint window);

    #region 常量

    private const string USER32 = "user32.dll";

    #endregion
}
