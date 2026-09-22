using System.Runtime.InteropServices;
using FFXIVClientStructs.STD;

namespace OmenTools.Interop.Game.Models.Native;

[StructLayout(LayoutKind.Explicit, Size = 48)]
public unsafe struct AccountInfo
{
    private static readonly CompSig             GetInstanceSig = new("48 8B 05 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC 83 39");
    private delegate        AccountInfo*        GetInstanceDelegate();
    private static readonly GetInstanceDelegate GetInstance = GetInstanceSig.GetDelegate<GetInstanceDelegate>();

    [FieldOffset(0)]
    public AccountInfoState State;

    [FieldOffset(8)]
    public ulong AccountID;

    [FieldOffset(16)]
    public StdVector<AccountInfoCharacter> Characters;

    [FieldOffset(40)]
    public byte IsSaving;

    [FieldOffset(41)]
    public byte SaveResult;

    public static AccountInfo* Instance() =>
        GetInstance();
}
