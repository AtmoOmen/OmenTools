using System.Runtime.InteropServices;

namespace OmenTools.Extensions;

public static class RuntimeInformationExtension
{
    extension
    (
        RuntimeInformation
    )
    {
        public static bool IsWine() =>
            GetNtdllProcAddress(WINE_VERSION_EXPORT)  != nint.Zero ||
            GetNtdllProcAddress(WINE_BUILD_ID_EXPORT) != nint.Zero ||
            HasWineBootExecutable();

        public static string? GetWineVersion() =>
            GetWineStringExport(WINE_VERSION_EXPORT);

        public static string? GetWineBuildID() =>
            GetWineStringExport(WINE_BUILD_ID_EXPORT);
    }

    private static nint GetNtdllProcAddress
    (
        string procName
    )
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return nint.Zero;

        try
        {
            return NativeLibrary.TryLoad(NTDLL_MODULE_NAME, out var ntdll) &&
                   NativeLibrary.TryGetExport(ntdll, procName, out var procAddress) ?
                       procAddress :
                       nint.Zero;
        }
        catch
        {
            // ignored
        }

        return nint.Zero;
    }

    private static bool HasWineBootExecutable()
    {
        try
        {
            return File.Exists(Path.Combine(Environment.SystemDirectory, WINE_BOOT_EXECUTABLE));
        }
        catch
        {
            // ignored
        }

        return false;
    }

    private static string? GetWineStringExport
    (
        string exportName
    )
    {
        var procAddress = GetNtdllProcAddress(exportName);
        if (procAddress == nint.Zero)
            return null;

        try
        {
            var func = Marshal.GetDelegateForFunctionPointer<WineStringExportDelegate>(procAddress);
            return Marshal.PtrToStringAnsi(func());
        }
        catch
        {
            // ignored
        }

        return null;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint WineStringExportDelegate();

    #region 常量

    private const string NTDLL_MODULE_NAME    = "ntdll.dll";
    private const string WINE_VERSION_EXPORT  = "wine_get_version";
    private const string WINE_BUILD_ID_EXPORT = "wine_get_build_id";
    private const string WINE_BOOT_EXECUTABLE = "wineboot.exe";

    #endregion
}
