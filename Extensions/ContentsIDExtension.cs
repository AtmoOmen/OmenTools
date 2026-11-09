using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Text.ReadOnly;
using OmenTools.Interop.Game.Lumina;
using AgentContentsFinder = FFXIVClientStructs.Expansion.FFXIV.Client.UI.Agent.AgentContentsFinder;

namespace OmenTools.Extensions;

public static class ContentsIDExtension
{
    extension(AgentContentsFinder.ContentsID contentsID)
    {
        public ReadOnlySeString GetName() =>
            contentsID.ContentType switch
            {
                ContentsType.None     => string.Empty,
                ContentsType.Regular  => LuminaWrapper.GetContentName(contentsID.ID),
                ContentsType.Roulette => LuminaWrapper.GetContentRouletteName(contentsID.ID),
                _                     => throw new ArgumentOutOfRangeException(nameof(contentsID), contentsID, null)
            };
    }
}
