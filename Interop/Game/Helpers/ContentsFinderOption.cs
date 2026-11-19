using System.Runtime.InteropServices;
using Dalamud.Game.Config;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Data;

namespace OmenTools.Interop.Game.Helpers;

/// <summary>
///     与客户端申请函数使用的十字节结构对应的任务搜索器设置。
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 10)]
public struct ContentsFinderOption : IEquatable<ContentsFinderOption>
{
    /// <summary>
    ///     允许加入进行中的任务。
    /// </summary>
    [FieldOffset(0)]
    public bool Supply;

    /// <summary>
    ///     国际服匹配语言位掩码：日语为 1，英语为 2，德语为 4，法语为 8；其他客户端固定发送 1。
    /// </summary>
    [FieldOffset(1)]
    public byte Languages;

    /// <summary>
    ///     解除人数与等级限制。
    /// </summary>
    [FieldOffset(2)]
    public bool UnrestrictedParty;

    /// <summary>
    ///     使用任务要求的最低装备品级。
    /// </summary>
    [FieldOffset(3)]
    public bool MinimalIL;

    /// <summary>
    ///     在解除限制时仍然进行等级同步。
    /// </summary>
    [FieldOffset(4)]
    public bool LevelSync;

    /// <summary>
    ///     禁用超越之力。
    /// </summary>
    [FieldOffset(5)]
    public bool SilenceEcho;

    /// <summary>
    ///     使用自由探索模式。
    /// </summary>
    [FieldOffset(6)]
    public bool ExplorerMode;

    /// <summary>
    ///     战利品分配规则。
    /// </summary>
    [FieldOffset(7)]
    public ContentsFinder.LootRule LootRules;

    /// <summary>
    ///     限制随机任务：练级的匹配目标，仅随机任务申请使用此项。
    /// </summary>
    [FieldOffset(8)]
    public bool IsLimitedLevelingRoulette;

    /// <summary>
    ///     普通副本申请携带的附加字节；默认值为零，随机任务申请不发送此项。
    /// </summary>
    [FieldOffset(9)]
    public byte RequestFlags;

    /// <summary>
    ///     使用当前匹配语言、其余选项关闭的任务搜索器设置。
    /// </summary>
    public static ContentsFinderOption Default => new()
    {
        Languages = Get().Languages
    };

    /// <summary>
    ///     读取当前任务搜索器设置与中途加入配置；国际服读取四种匹配语言，其他客户端使用固定值 1。
    /// </summary>
    /// <returns>当前设置的值副本。</returns>
    public static unsafe ContentsFinderOption Get()
    {
        ref var finder    = ref UIState.Instance()->ContentsFinder;
        var     config    = DService.Instance().GameConfig.UiConfig;
        byte    languages = 1;

        if (GameState.IsGL)
        {
            languages = 0;
            if (config.GetUInt(nameof(UiConfigOption.ContentsFinderUseLangTypeJA)) != 0)
                languages |= 1;

            if (config.GetUInt(nameof(UiConfigOption.ContentsFinderUseLangTypeEN)) != 0)
                languages |= 2;

            if (config.GetUInt(nameof(UiConfigOption.ContentsFinderUseLangTypeDE)) != 0)
                languages |= 4;

            if (config.GetUInt(nameof(UiConfigOption.ContentsFinderUseLangTypeFR)) != 0)
                languages |= 8;
        }

        return new()
        {
            Supply                    = config.GetUInt(nameof(UiConfigOption.ContentsFinderSupplyEnable)) != 0,
            Languages                 = languages,
            UnrestrictedParty         = finder.IsUnrestrictedParty,
            MinimalIL                 = finder.IsMinimalIL,
            LevelSync                 = finder.IsLevelSync,
            SilenceEcho               = finder.IsSilenceEcho,
            ExplorerMode              = finder.IsExplorerMode,
            LootRules                 = finder.LootRules,
            IsLimitedLevelingRoulette = finder.IsLimitedLevelingRoulette
        };
    }

    /// <summary>
    ///     返回各项申请设置的调试文本。
    /// </summary>
    /// <returns>包含匹配语言、申请标志及副本选项的文本。</returns>
    public override readonly string ToString() =>
        $"中途加入：{Supply}\n"                        +
        $"匹配语言：{Languages}\n"                     +
        $"解除限制：{UnrestrictedParty}\n"             +
        $"最低装等同步：{MinimalIL}\n"                   +
        $"等级同步：{LevelSync}\n"                     +
        $"禁用超越之力：{SilenceEcho}\n"                 +
        $"自由探索：{ExplorerMode}\n"                  +
        $"分配方式：{LootRules}\n"                     +
        $"限制练级任务目标：{IsLimitedLevelingRoulette}\n" +
        $"附加标志：{RequestFlags}";

    /// <summary>
    ///     比较全部申请设置是否相同。
    /// </summary>
    /// <param name="other">待比较的申请设置。</param>
    /// <returns>全部设置相同时返回真。</returns>
    public readonly bool Equals(ContentsFinderOption other) =>
        Supply                    == other.Supply                    &&
        Languages                 == other.Languages                 &&
        UnrestrictedParty         == other.UnrestrictedParty         &&
        MinimalIL                 == other.MinimalIL                 &&
        LevelSync                 == other.LevelSync                 &&
        SilenceEcho               == other.SilenceEcho               &&
        ExplorerMode              == other.ExplorerMode              &&
        LootRules                 == other.LootRules                 &&
        IsLimitedLevelingRoulette == other.IsLimitedLevelingRoulette &&
        RequestFlags              == other.RequestFlags;

    /// <summary>
    ///     比较指定对象是否具有相同的申请设置。
    /// </summary>
    /// <param name="obj">待比较的对象。</param>
    /// <returns>对象为相同设置时返回真。</returns>
    public override readonly bool Equals(object? obj) =>
        obj is ContentsFinderOption other && Equals(other);

    /// <summary>
    ///     计算全部申请设置的哈希值。
    /// </summary>
    /// <returns>与值相等比较一致的哈希值。</returns>
    public override readonly int GetHashCode() =>
        HashCode.Combine
        (
            HashCode.Combine(Supply,      Languages,    UnrestrictedParty, MinimalIL,                 LevelSync),
            HashCode.Combine(SilenceEcho, ExplorerMode, LootRules,         IsLimitedLevelingRoulette, RequestFlags)
        );

    /// <summary>
    ///     判断两组申请设置是否相等。
    /// </summary>
    /// <param name="left">左侧设置。</param>
    /// <param name="right">右侧设置。</param>
    /// <returns>全部设置相同时返回真。</returns>
    public static bool operator ==(ContentsFinderOption left, ContentsFinderOption right) =>
        left.Equals(right);

    /// <summary>
    ///     判断两组申请设置是否不同。
    /// </summary>
    /// <param name="left">左侧设置。</param>
    /// <param name="right">右侧设置。</param>
    /// <returns>任意设置不同时返回真。</returns>
    public static bool operator !=(ContentsFinderOption left, ContentsFinderOption right) =>
        !left.Equals(right);
}
