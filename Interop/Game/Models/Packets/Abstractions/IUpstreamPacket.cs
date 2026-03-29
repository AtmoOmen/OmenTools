namespace OmenTools.Interop.Game.Models.Packets.Abstractions;

public interface IUpstreamPacket
{
    string Log();

    void Send();
}
