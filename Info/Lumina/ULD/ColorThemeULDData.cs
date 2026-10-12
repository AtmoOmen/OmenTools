using System.Collections.Frozen;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Uld;

namespace OmenTools.Info.Lumina.ULD;

public sealed unsafe class ColorThemeULDData
{
    public FrozenDictionary<uint, UldRoot.TextureEntry>                     Assets         { get; }
    public FrozenDictionary<uint, UldRoot.NodeData>                         WidgetNodes    { get; }
    public FrozenDictionary<uint, FrozenDictionary<uint, UldRoot.NodeData>> ComponentNodes { get; }

    public ColorThemeULDData
    (
        UldFile file
    )
    {
        Assets      = file.AssetData.ToFrozenDictionary(x => x.Id);
        WidgetNodes = file.WidgetData.Nodes.ToFrozenDictionary(x => x.NodeId);
        ComponentNodes = file.Components.ToFrozenDictionary
        (
            x => x.Id,
            x => x.Nodes.ToFrozenDictionary(y => y.NodeId)
        );
    }

    public bool Matches
    (
        AtkUldManager* manager
    )
    {
        if (manager->Objects == null)
            return false;

        foreach (var definition in WidgetNodes.Values)
        {
            var node = manager->SearchNodeById(definition.NodeId);
            if (node == null || (uint)node->Type != definition.NodeType)
                return false;
        }

        for (var i = 0; i < manager->AssetCount; i++)
            if (!Assets.ContainsKey(manager->Assets[i].Id))
                return false;

        return true;
    }
}
