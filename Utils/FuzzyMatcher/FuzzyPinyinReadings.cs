namespace OmenTools.Utils.FuzzyMatcher;

internal static class FuzzyPinyinReadings
{
    #region 常量

    internal static readonly Dictionary<char, string[]> POLYPHONIC_READINGS = new()
    {
        ['重'] = ["zhong", "chong"],
        ['行'] = ["xing", "hang"],
        ['长'] = ["chang", "zhang"],
        ['乐'] = ["le", "yue"],
        ['调'] = ["tiao", "diao"],
        ['藏'] = ["cang", "zang"],
        ['还'] = ["hai", "huan"],
        ['传'] = ["chuan", "zhuan"],
        ['解'] = ["jie", "xie"],
        ['角'] = ["jiao", "jue"],
        ['降'] = ["jiang", "xiang"],
        ['便'] = ["bian", "pian"],
        ['参'] = ["can", "shen", "cen"],
        ['数'] = ["shu", "shuo"],
        ['省'] = ["sheng", "xing"],
        ['朝'] = ["chao", "zhao"],
        ['单'] = ["dan", "shan", "chan"],
        ['校'] = ["xiao", "jiao"],
        ['薄'] = ["bao", "bo"],
        ['屏'] = ["ping", "bing"],
        ['塞'] = ["sai", "se"],
        ['差'] = ["cha", "chai", "ci"],
        ['血'] = ["xue", "xie"],
        ['恶'] = ["e", "wu"],
        ['着'] = ["zhe", "zhao", "zhuo"],
        ['得'] = ["de", "dei"],
        ['弹'] = ["dan", "tan"],
        ['觉'] = ["jue", "jiao"],
        ['给'] = ["gei", "ji"],
        ['强'] = ["qiang", "jiang"]
    };

    #endregion
}
