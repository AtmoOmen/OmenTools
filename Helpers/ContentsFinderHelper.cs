using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using OmenTools;
using OmenTools.Helpers;
using OmenTools.Infos;
using ContentsFinder = FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder;

namespace OmenTools.Helpers;

public class ContentsFinderHelper
{
    private static readonly CompSig RequestContentsFinderSig =
        new("E8 ?? ?? ?? ?? 4C 8B B4 24 ?? ?? ?? ?? 4C 8B AC 24 ?? ?? ?? ?? 48 8B B4 24 ?? ?? ?? ?? 4C 8B BC 24");
    private delegate bool RequestContentsFinderDelegate(uint[] contentsID, uint contentsCount, uint a3, ref ContentsFinderOption option);
    private static readonly RequestContentsFinderDelegate RequestContentsFinder;

    private static readonly CompSig RequestContentsFinderRouletteSig =
        new("48 89 5C 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 F9 48 8B DA");
    private delegate void RequestContentsFinderRouletteDelegate(ushort contentRouletteID, ref ContentsFinderOption option);
    private static readonly RequestContentsFinderRouletteDelegate RequestContentsFinderRoulette;

    private static readonly CompSig CancelContentsFinderSig =
        new("E8 ?? ?? ?? ?? C6 43 ?? ?? E8 ?? ?? ?? ?? 83 C0 ?? 89 43 ?? 48 83 C4 ?? 5B C3 C6 43");
    private delegate void CancelContentsFinderDelegate(byte a1);
    private static readonly CancelContentsFinderDelegate CancelContentsFinder;
    
    private static readonly CompSig                ExecuteCommandSig = new("E8 ?? ?? ?? ?? 48 8B 06 48 8B CE FF 50 ?? E9 ?? ?? ?? ?? 49 8B CC");
    private delegate        nint                   ExecuteCommandDelegate(ExecuteCommandFlag command, int param1, int param2, int param3, int param4);
    private static readonly ExecuteCommandDelegate ExecuteCommand;

    /// <summary>
    /// 默认任务搜索器设置, 仅 Config817to820 这一项被设置为 true
    /// </summary>
    public static ContentsFinderOption DefaultOption => new() { Config817to820 = true };

    static ContentsFinderHelper()
    {
        RequestContentsFinder         = RequestContentsFinderSig.GetDelegate<RequestContentsFinderDelegate>();
        RequestContentsFinderRoulette = RequestContentsFinderRouletteSig.GetDelegate<RequestContentsFinderRouletteDelegate>();
        CancelContentsFinder          = CancelContentsFinderSig.GetDelegate<CancelContentsFinderDelegate>();
        ExecuteCommand                = ExecuteCommandSig.GetDelegate<ExecuteCommandDelegate>();
    }

    public static bool RequestDutyNormal(uint rowID, ContentsFinderOption option)
    {
        var optionFinal = option.Clone();
        return RequestContentsFinder([rowID], 1, 0, ref optionFinal);
    }

    /// <remarks>
    /// 单人进入多变迷宫需要在副本设置中设置为解除限制
    /// </remarks>
    public static bool RequestDutyNormal(uint[] rowIDs, ContentsFinderOption option)
    {
        var optionFinal = option.Clone();
        return RequestContentsFinder(rowIDs, (uint)rowIDs.Length, 0, ref optionFinal);
    }

    public static void RequestDutyRoulette(ushort rowID, ContentsFinderOption option)
    {
        var optionFinal = option.Clone();
        RequestContentsFinderRoulette(rowID, ref optionFinal);
    }

    public static void CancelDutyApply()
        => CancelContentsFinder(0);

    public static void RequestDutySupport(uint dawnContentID)
    {
        ExecuteCommand(ExecuteCommandFlag.RequestDutySupport, 0, 0, 0, 0);
        if (DService.ObjectTable.LocalPlayer is not { } localPlayer) return;
        var localRole = localPlayer.ClassJob.Value.Role;

        if (!LuminaGetter.TryGetRow<DawnContent>(dawnContentID, out var content)) return;
        var contentType = content.Content.Value.ContentMemberType.Value;
        var partyComposition = new
        {
            Tanks   = contentType.TanksPerParty,
            Healers = contentType.HealersPerParty,
            DPS     = contentType.MeleesPerParty + contentType.RangedPerParty,
            Total   = contentType.TanksPerParty  + contentType.HealersPerParty +
                      contentType.MeleesPerParty + contentType.RangedPerParty
        };

        var currentCount = new
        {
            Tanks   = localRole == 1 ? 1 : 0,
            Healers = localRole == 4 ? 1 : 0,
            DPS     = localRole is (2 or 3) ? 1 : 0
        };

        var members = LuminaGetter.GetSub<DawnContentParticipable>()
                                 .SelectMany(x => x)
                                 .Where(x => x.RowId == dawnContentID)
                                 .Select(x => new
                                 {
                                     QuestMember = LuminaGetter.GetRow<DawnQuestMember>(x.Unknown0),
                                     JobInfo = LuminaGetter.GetRow<DawnMember>(
                                         LuminaGetter.GetRow<DawnQuestMember>(x.Unknown0).GetValueOrDefault().Unknown0)
                                 })
                                 .Where(x => x.QuestMember != null && x.JobInfo != null)
                                 .Select(x => new
                                 {
                                     QuestMember = x.QuestMember.GetValueOrDefault(),
                                     JobInfo = x.JobInfo.GetValueOrDefault(),
                                 })
                                 .Select(x => new
                                 {
                                     // Class 实际为 DawnMemberUIParam, Unknown0 为 UI 显示的人物名称
                                     Name     = x.QuestMember.Class.Value.Unknown0,
                                     ID       = x.QuestMember.RowId,
                                     ClassJob = LuminaGetter.GetRow<ClassJob>(x.JobInfo.Unknown0)
                                 })
                                 .GroupBy(x => x.Name)
                                 .Select(x => new
                                 {
                                     Name = x.Key,
                                     Data = x.Select(d => (d.ID, d.ClassJob)).ToList()
                                 })
                                 .OrderBy(x => x.Data.Count)
                                 .ToList();

        var finalTeam = new List<uint>();
        var roleCount = currentCount;

        foreach (var member in members)
        {
            if (finalTeam.Count >= partyComposition.Total - 1) break;

            var data = member.Data.FirstOrDefault(job =>
            {
                var role = job.ClassJob.GetValueOrDefault().Role;
                switch (role)
                {
                    case 1 when roleCount.Tanks < partyComposition.Tanks:
                        roleCount = roleCount with { Tanks = roleCount.Tanks + 1 };
                        return true;
                    case 4 when roleCount.Healers < partyComposition.Healers:
                        roleCount = roleCount with { Healers = roleCount.Healers + 1 };
                        return true;
                    case (2 or 3) when roleCount.DPS < partyComposition.DPS:
                        roleCount = roleCount with { DPS = roleCount.DPS + 1 };
                        return true;
                    default:
                        return false;
                }
            });

            if (data.ClassJob == null) continue;
            finalTeam.Add(data.ID);
        }

        var parameters = finalTeam
                         .Select((id, index) => new
                         {
                             Value    = (long)(id * Math.Pow(256, index % 4)),
                             IsParam2 = index < 4
                         })
                         .Aggregate(new { Param2 = 0L, Param3 = 0L },
                                    (acc, curr) => curr.IsParam2
                                                       ? acc with { Param2 = acc.Param2 + curr.Value }
                                                       : acc with { Param3 = acc.Param3 + curr.Value });

        ExecuteCommand(ExecuteCommandFlag.SendDutySupport, (int)dawnContentID, (int)parameters.Param2, (int)parameters.Param3, 0);
    }
}

[StructLayout(LayoutKind.Explicit, Size = 10)]
public struct ContentsFinderOption
{
    /// <summary>
    /// 中途加入
    /// </summary>
    [FieldOffset(0)]
    public bool Supply;

    /// <summary>
    /// 通常为 true
    /// </summary>
    /// <remarks>
    /// 其代表的四个配置分别为:
    /// <para><c>817</c>: HotbarDispSetNum</para>
    /// <para><c>818</c>: HotbarDispSetChangeType</para>
    /// <para><c>819</c>: HotbarDispSetDragType</para>
    /// <para><c>820</c>: MainCommandType</para>
    /// <para>这四个配置只要有一个为 true, 则该项为 true</para>
    /// </remarks>
    [FieldOffset(1)]
    public bool Config817to820;

    /// <summary>
    /// 解除限制
    /// </summary>
    [FieldOffset(2)]
    public bool UnrestrictedParty;

    /// <summary>
    /// 最低装等同步
    /// </summary>
    [FieldOffset(3)]
    public bool MinimalIL;

    /// <summary>
    /// 等级同步
    /// </summary>
    [FieldOffset(4)]
    public bool LevelSync;

    /// <summary>
    /// 禁用超越之力
    /// </summary>
    [FieldOffset(5)]
    public bool SilenceEcho;

    /// <summary>
    /// 自由探索
    /// </summary>
    [FieldOffset(6)]
    public bool ExplorerMode;

    /// <summary>
    /// 分配方式
    /// </summary>
    [FieldOffset(7)]
    public ContentsFinder.LootRule LootRules;

    /// <summary>
    /// 限制“随机任务：练级”的随机目标
    /// </summary>
    [FieldOffset(8)]
    public bool IsLimitedLevelingRoulette;

    /// <summary>
    /// 通常为 false
    /// </summary>
    /// <remarks>源数据来源: *(bool*)((nint)AgentContentsFinder.Instance() + 7346)</remarks>
    [FieldOffset(9)]
    public bool Unknown9;

    public ContentsFinderOption Clone() => this;

    public static unsafe ContentsFinderOption Get()
    {
        var finder = UIState.Instance()->ContentsFinder;
        return new()
        {
            Config817to820            = true,
            ExplorerMode              = finder.IsExplorerMode,
            IsLimitedLevelingRoulette = finder.IsLimitedLevelingRoulette,
            LevelSync                 = finder.IsLevelSync,
            LootRules                 = finder.LootRules,
            MinimalIL                 = finder.IsMinimalIL,
            SilenceEcho               = finder.IsSilenceEcho,
            UnrestrictedParty         = finder.IsUnrestrictedParty,
        };
    }
}
