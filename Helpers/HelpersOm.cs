using System.Runtime.InteropServices;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    static HelpersOm()
    {
        FireCallback ??=
            Marshal.GetDelegateForFunctionPointer<FireCallbackDelegate>(
                DService.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F B6 F0 48 8D 5C 24"));

        Listener ??= Marshal.GetDelegateForFunctionPointer<InvokeListener>(
            DService.SigScanner.ScanText("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 0F B7 FA"));
    }
}