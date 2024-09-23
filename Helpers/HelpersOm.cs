using System.Runtime.InteropServices;
using OmenTools.Infos;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static readonly CompSig FireCallbackSig = new("E8 ?? ?? ?? ?? 0F B6 F0 48 8D 5C 24");
    public static readonly CompSig ListenerSig = new("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 0F B7 FA");

    static HelpersOm()
    {
        FireCallback ??= Marshal.GetDelegateForFunctionPointer<FireCallbackDelegate>(FireCallbackSig.ScanText());
        Listener ??= Marshal.GetDelegateForFunctionPointer<InvokeListener>(ListenerSig.ScanText());
    }
}