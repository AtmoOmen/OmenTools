using System.Runtime.InteropServices;

namespace OmenTools.Extensions;

public static class RuntimeInformationExtension
{
    extension(RuntimeInformation)
    {
        public static bool IsWine() =>
            GetNtdllProcAddress("wine_get_version") != nint.Zero;

        public static string? GetWineVersion() =>
            GetWineStringExport("wine_get_version");

        public static string? GetWineBuildID() =>
            GetWineStringExport("wine_get_build_id");
    }

    private static nint GetNtdllProcAddress(string procName)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return nint.Zero;

        var ntdll = GetModuleHandle("ntdll.dll");
        return ntdll == nint.Zero ?
                   nint.Zero :
                   GetProcAddress(ntdll, procName);
    }

    private static string? GetWineStringExport(string exportName)
    {
        var procAddress = GetNtdllProcAddress(exportName);
        if (procAddress == nint.Zero)
            return null;

        var func = Marshal.GetDelegateForFunctionPointer<WineStringExportDelegate>(procAddress);
        return Marshal.PtrToStringAnsi(func());
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint WineStringExportDelegate();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
    private static extern nint GetModuleHandle(string moduleName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = false)]
    private static extern nint GetProcAddress(nint moduleHandle, string procName);
}
