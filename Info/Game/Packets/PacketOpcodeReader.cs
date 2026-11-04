using System.Runtime.InteropServices;
using OmenTools.Interop.Game.Models;

namespace OmenTools.Info.Game.Packets;

internal static class PacketOpcodeReader
{
    internal static int Read(string name, params ReadOnlySpan<(CompSig Signature, int Offset)> signatures)
    {
        var scanner = ISigScanner.Instance();

        foreach (var (signature, offset) in signatures)
        {
            if (scanner.TryScanText(signature.Get(), out var address))
                return Marshal.ReadInt32(address, offset);
        }

        throw new InvalidOperationException($"无法定位数据包操作码: {name}。");
    }
}
