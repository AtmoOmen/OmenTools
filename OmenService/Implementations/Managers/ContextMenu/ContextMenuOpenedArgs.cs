using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel;
using Lumina.Excel.Sheets;
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
            TargetInventoryItem   = TargetInventoryItem,
            TargetItemID          = TargetItemID,
            TargetGlamourID       = TargetGlamourID,
            TargetInventoryType   = TargetInventoryType,
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

    public InventoryItem? TargetInventoryItem { get; internal set; }
    
    public InventoryType? TargetInventoryType { get; internal set; }

    public int? TargetSlot { get; internal set; }

    /// <remarks>
    ///     当前菜单目标的基础物品 ID，事件物品保留其原始 ID。
    /// </remarks>
    public uint TargetItemID { get; internal set; }

    public RowRef<Item> TargetItemRow =>
        TargetItemID == 0 ? default : TargetItemID.ToLuminaRowRef<Item>();

    /// <remarks>
    ///     当前菜单目标的幻化物品 ID，未应用幻化时为 0。
    /// </remarks>
    public uint TargetGlamourID { get; internal set; }

    public RowRef<Item> TargetGlamourRow =>
        TargetGlamourID == 0 ? default : TargetGlamourID.ToLuminaRowRef<Item>();

    public bool IsTargetPlayer => 
        TargetContentID != 0 && TargetCharacter is not null;

    public AgentContext* AsDefaultContext() =>
        DefaultAgentContext;

    public AgentInventoryContext* AsInventoryContext() =>
        InventoryAgentContext;
}
