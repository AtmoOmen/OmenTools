using Lumina.Excel.Sheets;
using OmenTools.Dalamud;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Interop.Game.Models;
using ContentRoulette = Lumina.Excel.Sheets.ContentRoulette;
using ContentsFinder = FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder;

namespace OmenTools.Interop.Game.Helpers;

/// <summary>
///     提交任务搜索器与剧情辅助器的副本申请。
/// </summary>
public static unsafe class ContentsFinderHelper
{
    static ContentsFinderHelper()
    {
        RequestContentsFinder         = RequestContentsFinderSig.GetDelegate<RequestContentsFinderDelegate>();
        RequestContentsFinderRoulette = RequestContentsFinderRouletteSig.GetDelegate<RequestContentsFinderRouletteDelegate>();
        CancelContentsFinder          = CancelContentsFinderSig.GetDelegate<CancelContentsFinderDelegate>();
    }

    /// <summary>
    ///     使用指定设置申请一个普通副本。
    /// </summary>
    /// <param name="rowID">任务在 ContentFinderCondition 表中的行编号。</param>
    /// <param name="option">本次申请设置，不修改游戏保存的设置。</param>
    /// <returns>参数有效且客户端发送函数接受请求时返回真，不表示服务器已批准申请。</returns>
    public static bool RequestDutyNormal(uint rowID, ContentsFinderOption option) =>
        RequestDutyNormal([rowID], option);

    /// <remarks>
    ///     单人进入多变迷宫需要在副本设置中设置为解除限制。
    /// </remarks>
    /// <summary>
    ///     同时申请一至五个不同的普通副本。
    /// </summary>
    /// <param name="rowIDs">ContentFinderCondition 行编号集合。</param>
    /// <param name="option">本次申请设置，不修改游戏保存的设置。</param>
    /// <returns>参数有效且客户端发送函数接受请求时返回真，不表示服务器已批准申请。</returns>
    public static bool RequestDutyNormal(ReadOnlySpan<uint> rowIDs, ContentsFinderOption option)
    {
        if (rowIDs.Length is < 1 or > 5 || (option.Languages & ~15) != 0 || option.LootRules > ContentsFinder.LootRule.Lootmaster)
            return false;

        for (var index = 0; index < rowIDs.Length; index++)
        {
            if (rowIDs[index] is 0 or > ushort.MaxValue ||
                !LuminaGetter.TryGetRow<ContentFinderCondition>(rowIDs[index], out _))
                return false;

            if (rowIDs[..index].Contains(rowIDs[index]))
                return false;
        }

        if (option.ExplorerMode)
        {
            option.Supply            = false;
            option.UnrestrictedParty = false;
            option.MinimalIL         = false;
            option.LevelSync         = false;
            option.SilenceEcho       = false;
            option.LootRules         = ContentsFinder.LootRule.Normal;
        }

        option.LevelSync                 &= option.UnrestrictedParty;
        option.IsLimitedLevelingRoulette =  false;

        DLog.Debug($"[ContentsFinderHelper] 一般副本（{string.Join(", ", (uint[])[.. rowIDs])}）\n{option}");
        fixed (uint* entries = rowIDs)
            return RequestContentsFinder(entries, (uint)rowIDs.Length, 0, &option);
    }

    /// <summary>
    ///     使用指定设置申请一个随机任务。
    /// </summary>
    /// <param name="rowID">ContentRoulette 行编号，必须能够用一个字节表示。</param>
    /// <param name="option">本次申请设置，附加申请标志不参与随机任务请求。</param>
    /// <returns>参数有效且客户端发送函数接受请求时返回真，不表示服务器已批准申请。</returns>
    public static bool RequestDutyRoulette(ushort rowID, ContentsFinderOption option)
    {
        if (rowID is 0 or > byte.MaxValue                                 ||
            (option.Languages & ~15) != 0                                 ||
            option.LootRules         > ContentsFinder.LootRule.Lootmaster ||
            !LuminaGetter.TryGetRow<ContentRoulette>(rowID, out _))
            return false;

        option.LevelSync &= option.UnrestrictedParty;
        DLog.Debug($"[ContentsFinderHelper] 随机任务 ({LuminaWrapper.GetContentRouletteName(rowID)})\n{option}");
        return RequestContentsFinderRoulette((byte)rowID, &option);
    }

    /// <summary>
    ///     发送撤回当前任务申请的请求。
    /// </summary>
    public static void CancelDutyApply() =>
        CancelContentsFinder(0);

    /// <summary>
    ///     根据副本要求与当前职业组建完整的辅助成员队伍并提交申请。
    /// </summary>
    /// <param name="dawnContentID">DawnContent 行编号，而非 ContentFinderCondition 行编号。</param>
    /// <returns>存在完整队伍并已调用申请指令时返回真；服务器仍会检查解锁与进入条件。</returns>
    public static bool RequestDutySupport(uint dawnContentID)
    {
        if (dawnContentID is 0 or > byte.MaxValue                                                       ||
            IObjectTable.Instance().LocalPlayer is not { } localPlayer                                  ||
            !LuminaGetter.TryGetRow<DawnContent>(dawnContentID, out var content)                        ||
            content.Content.ValueNullable is not { } duty                                               ||
            duty.ContentMemberType.ValueNullable is not { } memberType                                  ||
            !LuminaGetter.TryGetSubRowAll<DawnContentParticipable>(dawnContentID, out var participable) ||
            !LuminaGetter.TryGetRow<Role>(localPlayer.ClassJob.Value.Role, out var roleData))
            return false;

        Span<int> remaining = stackalloc int[4];
        remaining.Clear();
        remaining[1] = memberType.TanksPerParty;
        remaining[2] = memberType.MeleesPerParty + memberType.RangedPerParty;
        remaining[3] = memberType.HealersPerParty;

        var playerRole = roleData.Type;
        if (playerRole is < 1 or > 3 || remaining[playerRole] == 0)
            return false;

        remaining[playerRole]--;
        var memberCount = remaining[1] + remaining[2] + remaining[3];
        if (memberCount is < 1 or > 7)
            return false;

        var candidates = new Dictionary<uint, List<(byte MemberID, byte Role)>>();

        foreach (var entry in participable)
        {
            if (entry.DawnQuestMember.RowId == 0)
                break;

            if (entry.DawnQuestMember.RowId > byte.MaxValue                           ||
                entry.DawnQuestMember.ValueNullable is not { } member                 ||
                !LuminaGetter.TryGetRow<DawnMember>(member.Unknown0, out var jobInfo) ||
                !LuminaGetter.TryGetRow<ClassJob>(jobInfo.Unknown0, out var classJob) ||
                !LuminaGetter.TryGetRow<Role>(classJob.Role, out var memberRole)      ||
                memberRole.Type is < 1 or > 3)
                continue;

            var groupID = member.Unknown1 == 0 ?
                              member.RowId + 256 :
                              member.Unknown1;

            if (!candidates.TryGetValue(groupID, out var variants))
            {
                variants = [];
                candidates.Add(groupID, variants);
            }

            variants.Add(((byte)member.RowId, memberRole.Type));
        }

        var        groups   = candidates.Values.ToArray();
        Span<byte> selected = stackalloc byte[7];
        selected.Clear();
        if (!SelectSupportMembers(groups, 0, remaining, selected, 0, memberCount))
            return false;

        uint firstMembers = 0;
        uint lastMembers  = 0;

        for (var index = 0; index < memberCount; index++)
        {
            if (index < 4)
                firstMembers |= (uint)selected[index] << (index * 8);
            else
                lastMembers |= (uint)selected[index] << ((index - 4) * 8);
        }

        DLog.Debug($"[ContentsFinderHelper] 剧情辅助器 ({duty.Name})，成员编号: {string.Join(", ", (byte[])[.. selected[..memberCount]])}");
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestDutySupport);
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SendDutySupport, dawnContentID, firstMembers, lastMembers);
        return true;
    }

    private static bool SelectSupportMembers
    (
        List<(byte MemberID, byte Role)>[] groups,
        int                                groupIndex,
        Span<int>                          remaining,
        Span<byte>                         selected,
        int                                selectedCount,
        int                                memberCount
    )
    {
        if (selectedCount == memberCount)
            return true;

        if (groups.Length - groupIndex < memberCount - selectedCount)
            return false;

        foreach (var (memberID, role) in groups[groupIndex])
        {
            if (remaining[role] == 0)
                continue;

            remaining[role]--;
            selected[selectedCount] = memberID;
            if (SelectSupportMembers(groups, groupIndex + 1, remaining, selected, selectedCount + 1, memberCount))
                return true;

            remaining[role]++;
        }

        return SelectSupportMembers(groups, groupIndex + 1, remaining, selected, selectedCount, memberCount);
    }

    #region 常量

    private static readonly CompSig RequestContentsFinderSig = new("E8 ?? ?? ?? ?? 33 C0 E9 ?? ?? ?? ?? FE C8");
    private delegate        bool RequestContentsFinderDelegate(uint* contentsID, uint contentsCount, byte requestType, ContentsFinderOption* option);
    private static readonly RequestContentsFinderDelegate RequestContentsFinder;

    private static readonly CompSig RequestContentsFinderRouletteSig =
        new("48 89 5C 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 F9 48 8B DA");
    private delegate        bool                                  RequestContentsFinderRouletteDelegate(byte contentRouletteID, ContentsFinderOption* option);
    private static readonly RequestContentsFinderRouletteDelegate RequestContentsFinderRoulette;

    private static readonly CompSig CancelContentsFinderSig =
        new("E8 ?? ?? ?? ?? C6 43 ?? ?? E8 ?? ?? ?? ?? 83 C0 ?? 89 43 ?? 48 83 C4 ?? 5B C3 C6 43");
    private delegate        void                         CancelContentsFinderDelegate(byte requestType);
    private static readonly CancelContentsFinderDelegate CancelContentsFinder;

    #endregion
}
