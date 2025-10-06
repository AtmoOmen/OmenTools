using System.Diagnostics.CodeAnalysis;
using Dalamud.Hooking;
using Dalamud.Interface.Textures.TextureWraps;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using OmenTools.Abstracts;
using LuminaAchievement = Lumina.Excel.Sheets.Achievement;

namespace OmenTools.Managers;

public unsafe class AchievementManager : OmenServiceBase
{
    public static Dictionary<uint, AchievementInfo> Infos { get; private set; } = [];
    
    private static readonly CompSig                                   ReceiveAchievementProgressSig = new("C7 81 ?? ?? ?? ?? ?? ?? ?? ?? 89 91 ?? ?? ?? ?? 44 89 81");
    private delegate        void                                      ReceiveAchievementProgressDelegate(Achievement* achievement, uint id, uint current, uint max);
    private static          Hook<ReceiveAchievementProgressDelegate>? ReceiveAchievementProgressHook;
    
    internal override void Init()
    {
        ReceiveAchievementProgressHook ??= ReceiveAchievementProgressSig.GetHook<ReceiveAchievementProgressDelegate>(ReceiveAchievementProgressDetour);
        ReceiveAchievementProgressHook.Enable();
    }

    public static bool TryGetAchievement(uint id, [NotNullWhen(true)] out AchievementInfo? achievementInfo)
    {
        achievementInfo = null;
        
        // 有人传假情报
        if (!LuminaGetter.TryGetRow<LuminaAchievement>(id, out _)) 
            return false;
        
        // 反正不管咋样更新就对了
        SendRequest(id);
        
        if (!Infos.TryGetValue(id, out var info))
            return false;
        
        achievementInfo = info;
        return true;
    }

    private static void SendRequest(uint id)
    {
        if (!Throttler.Throttle($"AchievementManager-Request{id}", 10_000)) return;
        ExecuteCommandManager.ExecuteCommand(ExecuteCommandFlag.RequestAchievement, id);
    }
    
    private static void ReceiveAchievementProgressDetour(Achievement* achievement, uint id, uint current, uint max)
    {
        ReceiveAchievementProgressHook.Original(achievement, id, current, max);
        Infos[id] = new(id, current, max);
    }

    internal override void Uninit()
    {
        ReceiveAchievementProgressHook?.Dispose();
        ReceiveAchievementProgressHook = null;

        Infos = [];
    }
}

public class AchievementInfo(uint id, uint current, uint max)
{
    public uint     ID          { get; init; } = id;
    public uint     Current     { get; init; } = current;
    public uint     Max         { get; init; } = max;
    public DateTime LastUpdated { get; init; } = DateTime.Now;
    
    public string              Name        => GetData().Name.ExtractText()        ?? string.Empty;
    public string              Description => GetData().Description.ExtractText() ?? string.Empty;
    public uint                Icon        => GetData().Icon;
    public IDalamudTextureWrap IconTexture => DService.Texture.GetFromGameIcon(new(Icon)).GetWrapOrEmpty();
    public bool                IsFinished  => Current == Max;
    

    public LuminaAchievement GetData() =>
        LuminaGetter.GetRow<LuminaAchievement>(ID).GetValueOrDefault();
}
