using System.Runtime.InteropServices;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    static HelpersOm()
    {
        FireCallback ??=
            Marshal.GetDelegateForFunctionPointer<FireCallbackDelegate>(
                DService.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F B6 F0 48 8D 5C 24"));
    }
}