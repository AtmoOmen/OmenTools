namespace OmenTools.OmenService;

public sealed class TooltipActionModification : TooltipModification
{
    public required TooltipActionType Target { get; init; }
}
