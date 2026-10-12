using System.Collections.Frozen;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Uld;

namespace OmenTools.Info.Lumina.ULD;

public sealed unsafe class ColorThemeULDData
{
    public FrozenDictionary<uint, UldRoot.TextureEntry>                     Assets              { get; }
    public FrozenDictionary<uint, UldRoot.NodeData>                         WidgetNodes         { get; }
    public FrozenDictionary<uint, FrozenDictionary<uint, UldRoot.NodeData>> ComponentNodes      { get; }
    public FrozenDictionary<uint, ColorThemeTimelineFrame[]>                ColorTimelineFrames { get; }

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

        var colorTimelineFrames = new Dictionary<uint, ColorThemeTimelineFrame[]>();

        foreach (var timeline in file.Timelines)
        {
            var frames = new List<ColorThemeTimelineFrame>();

            for (var animationIndex = 0; animationIndex < timeline.FrameData.Length; animationIndex++)
            {
                var animation     = timeline.FrameData[animationIndex];
                var visitedGroups = new HashSet<UldRoot.KeyUsage>();

                foreach (var group in animation.KeyGroups)
                {
                    if (group.Usage is not (UldRoot.KeyUsage.TextColor or UldRoot.KeyUsage.EdgeColor) ||
                        group.Frames.Length == 0                                                      ||
                        !visitedGroups.Add(group.Usage))
                        continue;

                    for (var frameIndex = 0; frameIndex < group.Frames.Length; frameIndex++)
                    {
                        var (header, value) = group.Frames[frameIndex] switch
                        {
                            Keyframes.UInt1Keyframe frame => (frame.Keyframe, frame.Value),
                            Keyframes.Byte3Keyframe frame =>
                                (frame.Keyframe, (uint)(frame.Value[0] | (frame.Value[1] << 8) | (frame.Value[2] << 16) | (frame.Padding << 24))),
                            Keyframes.SByte3Keyframe frame =>
                                (frame.Keyframe, (uint)((byte)frame.Value[0] | ((byte)frame.Value[1] << 8) | ((byte)frame.Value[2] << 16) | (frame.Padding << 24))),
                            Keyframes.UShort1Keyframe frame =>
                                (frame.Keyframe, (uint)(frame.Value | (frame.Padding[0] << 16) | (frame.Padding[1] << 24))),
                            _ => (null, 0u)
                        };

                        if (value >> 24 == 0)
                            continue;

                        frames.Add
                        (
                            new ColorThemeTimelineFrame
                            (
                                animationIndex,
                                (int)group.Usage,
                                frameIndex,
                                (ushort)header.Time,
                                value & 0x00FFFFFF
                            )
                        );
                    }
                }
            }

            if (frames.Count != 0)
                colorTimelineFrames.Add(timeline.Id, [.. frames]);
        }

        ColorTimelineFrames = colorTimelineFrames.ToFrozenDictionary();
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
