using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Infos;

public class NodeState
{
    public Vector2 Position;
    public Vector2 Position2;
    public Vector2 Size;
    public bool    Visible;

    public static unsafe NodeState Get(AtkResNode* node)
    {
        var position = GetNodePosition(node);
        var scale    = GetNodeScale(node);
        var size     = new Vector2(node->Width, node->Height) * scale;

        return new NodeState()
        {
            Position  = position,
            Position2 = position + size,
            Visible   = GetNodeVisible(node),
            Size      = size,
        };
    }
}
