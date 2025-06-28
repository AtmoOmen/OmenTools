using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;

namespace OmenTools.Infos;

public unsafe class LocalPlayerState
{
    private delegate ushort GetClassJobLevelDelegate(PlayerState* instance, uint classJobID, bool checkParentJob);
    private static readonly GetClassJobLevelDelegate GetClassJobLevelInternal =
        new CompSig("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? 4C 8D 3D").GetDelegate<GetClassJobLevelDelegate>();
    
    /// <summary>
    /// 当前玩家是否正在移动
    /// </summary>
    public static bool IsMoving => 
        AgentMap.Instance()->IsPlayerMoving;
    
    /// <summary>
    /// 当前玩家的 EntityID
    /// </summary>
    public static uint EntityID => 
        PlayerState.Instance()->EntityId;
    
    /// <summary>
    /// 当前 ClassJob 表数据
    /// </summary>
    public static ClassJob ClassJobData =>
        LuminaGetter.GetRow<ClassJob>(ClassJob).GetValueOrDefault();

    /// <summary>
    /// 当前职业
    /// </summary>
    public static uint ClassJob => 
        AgentHUD.Instance()->CharacterClassJobId;

    /// <summary>
    /// 当前玩家的 ContentID
    /// </summary>
    public static ulong ContentID => 
        PlayerState.Instance()->ContentId;

    /// <summary>
    /// 当前玩家的等级
    /// </summary>
    public static ushort CurrentLevel =>
        (ushort)PlayerState.Instance()->CurrentLevel;

    /// <summary>
    /// 当前玩家该职业下最高可以达到的等级
    /// </summary>
    public static ushort MaxLevel =>
        PlayerState.Instance()->MaxLevel;

    /// <summary>
    /// 当前玩家获得的最优队员推荐次数
    /// </summary>
    public static ushort Commendations =>
        (ushort)PlayerState.Instance()->PlayerCommendations;

    /// <summary>
    /// 当前是否为等级同步状态
    /// </summary>
    public static bool IsLevelSynced =>
        PlayerState.Instance()->IsLevelSynced != 0;

    /// <summary>
    /// 获取当前玩家指定职业的等级
    /// </summary>
    public static ushort GetClassJobLevel(uint classJobID, bool checkParentJob = true) =>
        ClassJob == classJobID ? CurrentLevel : GetClassJobLevelInternal(PlayerState.Instance(), classJobID, checkParentJob);
}
