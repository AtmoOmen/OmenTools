﻿using System.Runtime.InteropServices;
using OmenTools.Infos;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    private static readonly CompSig FireCallbackSig = new("E8 ?? ?? ?? ?? 0F B6 F0 48 8D 5C 24");
    private static readonly CompSig ListenerSig     = new("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 0F B7 FA");

    static HelpersOm()
    {
        FireCallback ??= FireCallbackSig.GetDelegate<FireCallbackDelegate>();
        Listener     ??= ListenerSig.GetDelegate<InvokeListener>();
    }
}
