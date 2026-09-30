using Lumina.Data;

namespace OmenTools.Info.Game.Data;

/// <summary>
/// 收录游戏内标准的标点符号或用词。
/// </summary>
public static class UITexts
{
    /// <summary>
    /// 冒号<br/>
    /// 若为半角冒号的语言，则会后导一个半角空格，与全角冒号视觉保持统一。
    /// </summary>
    public static string Colon { get; } =
        GameState.ClientLanguage is
            Language.ChineseSimplified or
            Language.ChineseTraditional or
            Language.TraditionalChinese ?
            "：" :
            ": ";
    
    /// <summary>
    /// 空格
    /// </summary>
    public static char Space { get; } =
        IsFullWidthLanguage ?
            '　' :
            ' ';
    
    /// <summary>
    /// 左方括号<br/>
    /// 适用于补充说明。非对同一行已有文本的补充说明统一用半角方括号。
    /// </summary>
    public static char LeftSquareBracket { get; } =
        IsFullWidthLanguage ?
            '［' :
            '[';
    
    /// <summary>
    /// 右方括号<br/>
    /// 适用于补充说明。非对同一行已有文本的补充说明统一用半角方括号。
    /// </summary>
    public static char RightSquareBracket { get; } =
        IsFullWidthLanguage ?
            '］' :
            ']';

    public static bool IsFullWidthLanguage { get; } =
        GameState.ClientLanguage is
            Language.ChineseSimplified
            or Language.ChineseTraditional
            or Language.TraditionalChinese
            or Language.Japanese
            or Language.Korean;
}
