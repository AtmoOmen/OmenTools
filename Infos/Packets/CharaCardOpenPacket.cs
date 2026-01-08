using System.Runtime.InteropServices;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct CharaCardOpenPacket(ulong entityID) : IGamePacket
{
    [FieldOffset(0)]  public        int   Opcode   = GamePacketOpcodes.CharaCardOpenOpcode;
    [FieldOffset(8)]  public        uint  Length   = 32;
    [FieldOffset(32)] public        ulong EntityID = entityID;
    [FieldOffset(40)] public unsafe int   Flag0    = LocalPlayerState.EntityID == entityID ? 2 : 1;
    [FieldOffset(44)] public        int   Flag1    = 1;
    
    private unsafe delegate void PrepareCharaPortraitDataDelegate(AgentCharaCard* agent);
    private static readonly PrepareCharaPortraitDataDelegate PrepareCharaPortraitData =
        new CompSig("E8 ?? ?? ?? ?? 8B 05 ?? ?? ?? ?? 48 8D 0D").GetDelegate<PrepareCharaPortraitDataDelegate>();

    public string Log()
        => $"Chara Card Open 包体 ({Opcode} / 长度: {Length})\n" +
           $"Entity ID: {EntityID} | Flag0: {Flag0} | Flag1: {Flag1}";

    public unsafe void Send()
    {
        var agent = AgentCharaCard.Instance();
            
        PrepareCharaPortraitData(agent);
        GamePacketManager.Instance().SendPackt(this);
        MemoryHelper.Write((nint)((byte*)agent + 0x30), (byte)1);
    }
}
