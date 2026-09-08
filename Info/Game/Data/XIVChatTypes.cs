using System.Collections.Frozen;
using Dalamud.Game.Text;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.Info.Game.Data;

public static class XIVChatTypes
{
    public static FrozenDictionary<XivChatType, string> ChatTypeToAddonText { get; } = new Dictionary<XivChatType, string>
    {
        [XivChatType.Say]             = LuminaWrapper.GetAddonText(1907),
        [XivChatType.Shout]           = LuminaWrapper.GetAddonText(1908),
        [XivChatType.TellIncoming]    = LuminaWrapper.GetAddonText(1909),
        [XivChatType.Party]           = LuminaWrapper.GetAddonText(1910),
        [XivChatType.Alliance]        = LuminaWrapper.GetAddonText(1932),
        [XivChatType.Ls1]             = LuminaWrapper.GetAddonText(7890),
        [XivChatType.Ls2]             = LuminaWrapper.GetAddonText(7891),
        [XivChatType.Ls3]             = LuminaWrapper.GetAddonText(7892),
        [XivChatType.Ls4]             = LuminaWrapper.GetAddonText(7893),
        [XivChatType.Ls5]             = LuminaWrapper.GetAddonText(7894),
        [XivChatType.Ls6]             = LuminaWrapper.GetAddonText(7895),
        [XivChatType.Ls7]             = LuminaWrapper.GetAddonText(7896),
        [XivChatType.Ls8]             = LuminaWrapper.GetAddonText(7897),
        [XivChatType.FreeCompany]     = LuminaWrapper.GetAddonText(1933),
        [XivChatType.NoviceNetwork]   = LuminaWrapper.GetAddonText(7898),
        [XivChatType.Yell]            = LuminaWrapper.GetAddonText(1931),
        [XivChatType.PvPTeam]         = LuminaWrapper.GetAddonText(7899),
        [XivChatType.CrossLinkShell1] = LuminaWrapper.GetAddonText(7866),
        [XivChatType.CrossLinkShell2] = LuminaWrapper.GetAddonText(8390),
        [XivChatType.CrossLinkShell3] = LuminaWrapper.GetAddonText(8391),
        [XivChatType.CrossLinkShell4] = LuminaWrapper.GetAddonText(8392),
        [XivChatType.CrossLinkShell5] = LuminaWrapper.GetAddonText(8393),
        [XivChatType.CrossLinkShell6] = LuminaWrapper.GetAddonText(8394),
        [XivChatType.CrossLinkShell7] = LuminaWrapper.GetAddonText(8395),
        [XivChatType.CrossLinkShell8] = LuminaWrapper.GetAddonText(8396),
        [XivChatType.Echo]            = LuminaWrapper.GetAddonText(1912)
    }.ToFrozenDictionary();
}
