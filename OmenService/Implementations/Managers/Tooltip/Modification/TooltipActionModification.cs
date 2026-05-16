using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.OmenService;

public sealed unsafe class TooltipActionModification : TooltipModification
{
    public required TooltipActionType Target { get; init; }

    private StringArrayData* StringArray =>
        AtkStage.Instance()->GetStringArrayData(StringArrayType.ItemDetail);
}
