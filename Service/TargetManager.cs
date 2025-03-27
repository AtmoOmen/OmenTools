using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace OmenTools.Service;

#pragma warning disable CS8629

internal sealed unsafe class TargetManager : ITargetManager
{
    public IGameObject? Target
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->GetHardTarget());
        set => Struct->SetHardTarget((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)value?.Address);
    }

    public IGameObject? MouseOverTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->MouseOverTarget);
        set => Struct->MouseOverTarget = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)value?.Address;
    }

    public IGameObject? FocusTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->FocusTarget);
        set => Struct->FocusTarget = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)value?.Address;
    }

    public IGameObject? PreviousTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->PreviousTarget);
        set => Struct->PreviousTarget = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)value?.Address;
    }

    public IGameObject? SoftTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->GetSoftTarget());
        set => Struct->SetSoftTarget((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)value?.Address);
    }

    public IGameObject? GPoseTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->GPoseTarget);
        set => Struct->GPoseTarget = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)value?.Address;
    }

    public IGameObject? MouseOverNameplateTarget
    {
        get => DService.ObjectTable.CreateObjectReference((nint)Struct->MouseOverNameplateTarget);
        set => Struct->MouseOverNameplateTarget = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)value?.Address;
    }

    private TargetSystem* Struct => TargetSystem.Instance();
}

public interface ITargetManager
{
    public IGameObject? Target { get; set; }
    
    public IGameObject? MouseOverTarget { get; set; }

    public IGameObject? FocusTarget { get; set; }

    public IGameObject? PreviousTarget { get; set; }

    public IGameObject? SoftTarget { get; set; }
    
    public IGameObject? GPoseTarget { get; set; }

    public IGameObject? MouseOverNameplateTarget { get; set; }
}
