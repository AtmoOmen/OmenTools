using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Network;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.STD;
using InteropGenerator.Runtime;
using InteropGenerator.Runtime.Attributes;

namespace OmenTools.Interop.Game.Models.Native;

// TODO: FFCS
[StructLayout(LayoutKind.Explicit, Size = 0x8C8)]
public unsafe struct LobbyUIClientEX
{
    [FieldOffset(0x08)]
    public LobbyData* LobbyData;

    [FieldOffset(0x10)]
    public NetworkModuleProxy* NetworkModuleProxy;

    [FieldOffset(0x30)]
    public StdVector<LobbyDataCenterWorldEntry> CurrentDataCenterWorlds;

    [FieldOffset(0x48)]
    public LobbySubscriptionInfo* SubscriptionInfo;

    [FieldOffset(0xF8)]
    public StdVector<LobbyUIClientCharacterEntry> CurrentDataCenterCharacters;
}

// TODO: FFCS
[StructLayout(LayoutKind.Explicit, Size = 0x758)]
public unsafe struct LobbyUIClientCharacterEntry
{
    [FieldOffset(0x08)]
    public ulong ContentID;

    [FieldOffset(0x10)]
    public byte Index;

    [FieldOffset(0x11)]
    public CharaSelectCharacterEntryLoginFlags LoginFlags;

    [FieldOffset(0x18)]
    public ushort CurrentWorldID;

    [FieldOffset(0x1A)]
    public ushort HomeWorldID;

    [FieldOffset(0x2C)]
    [FixedSizeArray(true)]
    private fixed byte name[32];

    public string Name
    {
        get
        {
            fixed (byte* ptr = name)
                return new CStringPointer(ptr).ToString();
        }
    }

    [FieldOffset(0x4C)]
    [FixedSizeArray(true)]
    private fixed byte currentWorldName[32];

    public string CurrentWorldName
    {
        get
        {
            fixed (byte* ptr = currentWorldName)
                return new CStringPointer(ptr).ToString();
        }
    }

    [FieldOffset(0x6C)]
    [FixedSizeArray(true)]
    private fixed byte homeWorldName[32];

    public string HomeWorldName
    {
        get
        {
            fixed (byte* ptr = homeWorldName)
                return new CStringPointer(ptr).ToString();
        }
    }

    [FieldOffset(0x8C)]
    [FixedSizeArray(true)]
    private fixed byte rawJSON[1024];

    public string RawJSON
    {
        get
        {
            fixed (byte* ptr = rawJSON)
                return Encoding.UTF8.GetString(new Span<byte>(ptr, 1024));
        }
    }

    [FieldOffset(0x4C0)]
    public ClientSelectData ClientSelectData;

    [FieldOffset(0x600)]
    public ClientSelectData ClientSelectData2;

    [FieldOffset(0x740)]
    public ulong ContentIDMirror;

    [FieldOffset(0x748)]
    public byte Flag;

    [FieldOffset(0x74C)]
    private int Unk74C;

    [FieldOffset(0x750)]
    public byte DeletedFlag;
}
