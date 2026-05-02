using Dalamud.Game.Text.SeStringHandling;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.Extensions;

public static class BitMapFontIconExtension
{
    extension(BitmapFontIcon icon)
    {
        public ClassJob ToClassJob()
        {
            var iconValue = (int)icon;
            switch (iconValue)
            {
                case >= 128 and <= 167:
                {
                    var rowID = (uint)(iconValue - 127);
                    if (rowID is >= 1 and <= 40)
                        return LuminaGetter.GetRowOrDefault<ClassJob>(rowID);
                    break;
                }
                case 170:
                    return LuminaGetter.GetRowOrDefault<ClassJob>(41);
                case 171:
                    return LuminaGetter.GetRowOrDefault<ClassJob>(42);
                case 183:
                    return LuminaGetter.GetRowOrDefault<ClassJob>(43);
            }

            return LuminaGetter.GetRowOrDefault<ClassJob>(0);
        }
    }
}
