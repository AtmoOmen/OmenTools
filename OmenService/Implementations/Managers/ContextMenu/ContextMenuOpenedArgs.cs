using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace OmenTools.OmenService;

public sealed unsafe class ContextMenuOpenedArgs
{
    internal ContextMenuOpenedArgs
    (
        AgentContext* agentContext
    )
    {
        Agent               = (AgentInterface*)agentContext;
        DefaultAgentContext = agentContext;
    }

    internal ContextMenuOpenedArgs
    (
        AgentInventoryContext* agentContext
    )
    {
        Agent                 = (AgentInterface*)agentContext;
        InventoryAgentContext = agentContext;
    }

    internal ContextMenuOpenedArgs
    (
        AgentInterface* agent
    ) =>
        Agent = agent;

    internal ContextMenuOpenedArgs Clone() =>
        new(Agent)
        {
            Addon                 = Addon,
            AddonName             = AddonName,
            OwnerAddonID          = OwnerAddonID,
            TargetObjectID        = TargetObjectID,
            TargetContentID       = TargetContentID,
            TargetHomeWorldID     = TargetHomeWorldID,
            TargetName            = TargetName,
            TargetCharacter       = TargetCharacter,
            TargetItem            = TargetItem,
            TargetInventoryID     = TargetInventoryID,
            TargetSlot            = TargetSlot,
            DefaultAgentContext   = DefaultAgentContext,
            InventoryAgentContext = InventoryAgentContext
        };

    public AgentInterface* Agent { get; }

    public AgentContext* DefaultAgentContext { get; internal set; }

    public AgentInventoryContext* InventoryAgentContext { get; internal set; }

    public AtkUnitBase* Addon { get; internal set; }

    public string? AddonName { get; internal set; }

    public uint OwnerAddonID { get; internal set; }

    public string? TargetName { get; internal set; }

    public ulong TargetObjectID { get; internal set; }

    public ulong TargetContentID { get; internal set; }

    public short TargetHomeWorldID { get; internal set; }

    public InfoProxyCommonList.CharacterData* TargetCharacter { get; internal set; }

    public InventoryItem? TargetItem { get; internal set; }

    public uint TargetItemID =>
        TargetItem is { } item ?
            item.GetItemId() :
            0;

    public InventoryType? TargetInventoryID { get; internal set; }

    public int? TargetSlot { get; internal set; }

    public bool IsTargetPlayer => TargetContentID != 0 && TargetCharacter is not null;

    public AgentContext* AsDefaultContext() =>
        DefaultAgentContext;

    public AgentInventoryContext* AsInventoryContext() =>
        InventoryAgentContext;
}
