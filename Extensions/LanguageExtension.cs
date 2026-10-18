using System.Globalization;
using Lumina.Data;

namespace OmenTools.Extensions;

public static class LanguageExtension
{
    extension
    (
        Language language
    )
    {
        public CultureInfo ToCultureInfo() =>
            language switch
            {
                Language.Japanese           => CultureInfo.GetCultureInfo("ja-JP"),
                Language.English            => CultureInfo.GetCultureInfo("en-US"),
                Language.German             => CultureInfo.GetCultureInfo("de-DE"),
                Language.French             => CultureInfo.GetCultureInfo("fr-FR"),
                Language.ChineseSimplified  => CultureInfo.GetCultureInfo("zh-CN"),
                Language.TraditionalChinese => CultureInfo.GetCultureInfo("zh-TW"),
                Language.Korean             => CultureInfo.GetCultureInfo("ko-KR"),
                _                           => CultureInfo.InvariantCulture
            };
    }
}
