using System.Collections.Frozen;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.Info.Game.Data;

public static class FateAchievements
{
    public static uint VersionCount { get; } = (uint)(LuminaGetter.Get<ExVersion>().Count - 1);
    
    public static FrozenDictionary<uint, uint> AchievementToZone { get; } = new Dictionary<uint, uint>
    {
        [2343] = 813,  // 雷克兰德
        [2345] = 815,  // 安穆·艾兰
        [2346] = 816,  // 伊尔美格
        [2344] = 814,  // 珂露西亚岛
        [2347] = 817,  // 拉凯提卡大森林
        [2348] = 818,  // 黑风海
        [3022] = 956,  // 迷津
        [3023] = 957,  // 萨维奈岛
        [3024] = 958,  // 加雷马
        [3025] = 959,  // 叹息海
        [3026] = 961,  // 厄尔庇斯
        [3027] = 960,  // 天外天垓
        [3559] = 1187, // 奥阔帕恰山
        [3560] = 1188, // 克扎玛乌卡湿地
        [3561] = 1189, // 亚克特尔树海
        [3562] = 1190, // 夏劳尼荒野
        [3563] = 1191, // 遗产之地
        [3564] = 1192  // 活着的记忆
    }.ToFrozenDictionary();
    
    public static FrozenDictionary<uint, uint> ZoneToAchievement { get; } = 
        AchievementToZone.ToFrozenDictionary(x => x.Value, x => x.Key);
}
