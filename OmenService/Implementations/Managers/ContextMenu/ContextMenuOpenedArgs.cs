using System.Numerics;
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

    public ContextMenuOpenedArgs()
    {
        IsSelfInitiated     = true;
        DefaultAgentContext = AgentContext.Instance();
        Agent               = (AgentInterface*)DefaultAgentContext;
    }

    internal ContextMenuOpenedArgs Clone() =>
        new(Agent)
        {
            IsSelfInitiated       = IsSelfInitiated,
            Addon                 = Addon,
            AddonName             = AddonName,
            OwnerAddonID          = OwnerAddonID,
            Position              = Position,
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

    internal bool IsSelfInitiated { get; init; }

    public AgentInterface* Agent { get; set; }

    public AgentContext* DefaultAgentContext { get; set; }

    public AgentInventoryContext* InventoryAgentContext { get; set; }

    public AtkUnitBase* Addon { get; set; }

    public string? AddonName { get; set; }

    public uint OwnerAddonID { get; set; }

    /// <remarks>
    ///     菜单的屏幕坐标，未指定时取当前鼠标位置。
    /// </remarks>
    public Vector2? Position { get; set; }

    public string? TargetName { get; set; }

    public ulong TargetObjectID { get; set; }

    public ulong TargetContentID { get; set; }

    public short TargetHomeWorldID { get; set; }

    public InfoProxyCommonList.CharacterData* TargetCharacter { get; set; }

    public InventoryItem? TargetInventoryItem { get; set; }

    public InventoryType? TargetInventoryType { get; set; }

    public int? TargetSlot { get; set; }

    /// <remarks>
    ///     当前菜单目标的基础物品 ID，事件物品保留其原始 ID。
    /// </remarks>
    public uint TargetItemID { get; set; }

    public RowRef<Item> TargetItemRow =>
        TargetItemID == 0 ?
            default :
            TargetItemID.ToLuminaRowRef<Item>();

    /// <remarks>
    ///     当前菜单目标的幻化物品 ID，未应用幻化时为 0。
    /// </remarks>
    public uint TargetGlamourID { get; set; }

    public RowRef<Item> TargetGlamourRow =>
        TargetGlamourID == 0 ?
            default :
            TargetGlamourID.ToLuminaRowRef<Item>();

    public bool IsTargetPlayer =>
        TargetContentID != 0 && TargetCharacter is not null;

    public AgentContext* AsDefaultContext() =>
        DefaultAgentContext;

    public AgentInventoryContext* AsInventoryContext() =>
        InventoryAgentContext;
}
