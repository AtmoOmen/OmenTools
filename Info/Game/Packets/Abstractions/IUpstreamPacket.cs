namespace OmenTools.Info.Game.Packets.Abstractions;

public interface IUpstreamPacket
{
    string Log();

    void Send();
}
