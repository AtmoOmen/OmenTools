namespace OmenTools.Utils.FuzzyMatcher;

/// <summary>
///     定义一段可搜索文本及其参与匹配的方式，所有规则在建立索引时读取。
/// </summary>
/// <param name="Text">需要索引的原始文本。</param>
/// <param name="NativeWeight">原文命中时的基础评分。</param>
/// <param name="PinyinWeight">拼音命中时的基础评分；设为 0 可完全禁用该字段的拼音索引和匹配。</param>
/// <param name="AllowInitials">是否允许用拼音首字母或首字母与全拼混合匹配。</param>
/// <param name="AllowSingleCharacter">是否允许正向单字查询命中含有多个字符的字段。</param>
/// <param name="AllowChineseSplit">是否允许中文词组拆分后在限定距离内匹配。</param>
/// <param name="AllowAcronym">是否允许匹配英文单词或大小写边界形成的缩写。</param>
/// <param name="ShortMatchPenalty">长度不超过 3 个字符的原文包含匹配扣分；设为 0 不扣分。</param>
public readonly record struct FuzzySearchField
(
    string Text,
    int    NativeWeight,
    int    PinyinWeight,
    bool   AllowInitials        = true,
    bool   AllowSingleCharacter = true,
    bool   AllowChineseSplit    = true,
    bool   AllowAcronym         = true,
    int    ShortMatchPenalty    = 0
);
