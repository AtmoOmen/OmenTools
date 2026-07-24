using CSSharedGroupLayoutInstance = FFXIVClientStructs.FFXIV.Client.LayoutEngine.Group.SharedGroupLayoutInstance;
using SharedGroupStainFlags = FFXIVClientStructs.FFXIV.Client.LayoutEngine.Group.SharedGroupStainFlags;
using Transform = FFXIVClientStructs.FFXIV.Client.LayoutEngine.Transform;

namespace OmenTools.Dalamud.Services.LayoutEngine.Group;

internal unsafe class SharedGroupLayoutInstance
(
    nint address
) : ISharedGroupLayoutInstance
{
    private CSSharedGroupLayoutInstance* Struct => (CSSharedGroupLayoutInstance*)Address;

    public nint      Address              { get; } = address;
    public Transform Transform            => Struct->Transform;
    public uint      PrefabFlags1         => Struct->PrefabFlags1;
    public uint      PrefabFlags2         => Struct->PrefabFlags2;
    public byte      PlayingTimelineIndex => Struct->PlayingTimelineIndex;

    public byte? DefaultStainIndex => Struct->StainInfo == null ?
                                          null :
                                          Struct->StainInfo->DefaultStainIndex;

    public byte? ChosenStainIndex => Struct->StainInfo == null ?
                                         null :
                                         Struct->StainInfo->ChosenStainIndex;

    public SharedGroupStainFlags? StainFlags => Struct->StainInfo == null ?
                                                    null :
                                                    Struct->StainInfo->Flags;

    public bool IsTimelinePlaying(uint index) => Struct->IsTimelinePlaying(index);

    public bool IsTimelineIndexValid(uint index) => Struct->IsTimelineIndexValid(index);

    public bool TryApplyStain(byte stainIndex) => Struct->TryApplyStain(stainIndex);

    public CSSharedGroupLayoutInstance* ToStruct() => Struct;
}
