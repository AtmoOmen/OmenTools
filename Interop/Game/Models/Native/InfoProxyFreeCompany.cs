using System.Runtime.InteropServices;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace OmenTools.Interop.Game.Models.Native;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct InfoProxyFreeCompany
{
    private static readonly CompSig GetRankNameSig =
        new("48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 85 D2 75");
    private delegate byte* GetRankNameDelegate
    (
        InfoProxyFreeCompany* proxy,
        RankData*             rank
    );
    private static readonly GetRankNameDelegate GetRankName = GetRankNameSig.GetDelegate<GetRankNameDelegate>();

    [FieldOffset(122)]
    public ushort TotalMembers;

    [FieldOffset(320)]
    public RankData Ranks;

    public RankData* GetRankData
    (
        byte rankIndex
    )
    {
        if (rankIndex >= RANK_COUNT) return null;

        fixed (InfoProxyFreeCompany* instance = &this)
            return &instance->Ranks + rankIndex;
    }

    public bool IsRankAssignable
    (
        byte rankIndex
    )
    {
        if (rankIndex >= RANK_COUNT - 1) return false;

        var rank = GetRankData(rankIndex);
        return rank != null && rank->Name[0] != 0;
    }

    public string GetRankNameText
    (
        byte rankIndex
    )
    {
        var rank = GetRankData(rankIndex);
        if (rank == null) return string.Empty;

        fixed (InfoProxyFreeCompany* instance = &this)
            return MemoryHelper.ReadStringNullTerminated((nint)GetRankName(instance, rank));
    }

    public string GetMemberRankNameText
    (
        uint memberExtraFlags
    )
        => GetRankNameText((byte)(memberExtraFlags >> 12));

    public List<(byte RankIndex, string Name)> GetAssignableRanks()
    {
        List<(byte, string)> ranks = [];

        for (byte i = 0; i < RANK_COUNT; i++)
        {
            if (!IsRankAssignable(i)) continue;

            var name = GetRankNameText(i);
            if (string.IsNullOrWhiteSpace(name)) continue;

            ranks.Add((i, name));
        }

        return ranks;
    }

    public static InfoProxyFreeCompany* Instance() =>
        (InfoProxyFreeCompany*)InfoModule.Instance()->GetInfoProxyById(InfoProxyId.FreeCompany);

    [StructLayout(LayoutKind.Explicit, Size = 88)]
    public struct RankData
    {
        [FieldOffset(32)]
        public ushort MemberCount;

        [FieldOffset(34)]
        public byte RankNumber;

        [FieldOffset(35)]
        public fixed byte Name[16];
    }
    
    #region 常量
    
    private const int RANK_COUNT = 16;
    
    #endregion
}
