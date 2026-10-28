using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Text.ReadOnly;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.Extensions;

public static class ContentsRouletteRoleExtension
{
    extension
    (
        scoped in ContentsRouletteRole role
    )
    {
        public BitmapFontIcon GetIcon() => role switch
        {
            ContentsRouletteRole.Tank   => BitmapFontIcon.Tank,
            ContentsRouletteRole.Healer => BitmapFontIcon.Healer,
            ContentsRouletteRole.Dps    => BitmapFontIcon.DPS
        };
        
        public ReadOnlySeString GetName() => role switch
        {
            ContentsRouletteRole.Tank   => LuminaWrapper.GetAddonTextSeString(2784),
            ContentsRouletteRole.Healer => LuminaWrapper.GetAddonTextSeString(2785),
            ContentsRouletteRole.Dps    => LuminaWrapper.GetAddonTextSeString(2786)
        };
    }
}
