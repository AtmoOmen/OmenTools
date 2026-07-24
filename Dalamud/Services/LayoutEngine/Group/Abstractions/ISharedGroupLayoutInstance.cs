using CSSharedGroupLayoutInstance = FFXIVClientStructs.FFXIV.Client.LayoutEngine.Group.SharedGroupLayoutInstance;
using SharedGroupStainFlags = FFXIVClientStructs.FFXIV.Client.LayoutEngine.Group.SharedGroupStainFlags;
using Transform = FFXIVClientStructs.FFXIV.Client.LayoutEngine.Transform;

namespace OmenTools.Dalamud.Services.LayoutEngine.Group.Abstractions;

public interface ISharedGroupLayoutInstance
{
    nint                   Address              { get; }
    Transform              Transform            { get; }
    uint                   PrefabFlags1         { get; }
    uint                   PrefabFlags2         { get; }
    byte                   PlayingTimelineIndex { get; }
    byte?                  DefaultStainIndex    { get; }
    byte?                  ChosenStainIndex     { get; }
    SharedGroupStainFlags? StainFlags           { get; }

    bool IsTimelinePlaying(uint index);

    bool IsTimelineIndexValid(uint index);

    bool TryApplyStain(byte stainIndex);

    unsafe CSSharedGroupLayoutInstance* ToStruct();

    static ISharedGroupLayoutInstance Create(nint address) => new SharedGroupLayoutInstance(address);
}
