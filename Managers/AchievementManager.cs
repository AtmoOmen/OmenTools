using System.Diagnostics.CodeAnalysis;
using Dalamud.Hooking;
using Dalamud.Interface.Textures.TextureWraps;
using OmenTools.Abstracts;
using Achievement = FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement;
using LuminaAchievement = Lumina.Excel.Sheets.Achievement;

namespace OmenTools.Managers;

public unsafe class AchievementManager : OmenServiceBase<AchievementManager>
{
    public Dictionary<uint, AchievementInfo> Infos { get; private set; } = [];
    
    public bool TryGetAchievement(uint id, [NotNullWhen(true)] out AchievementInfo? achievementInfo)
    {
        achievementInfo = null;

        // 没登录你请求个什么
        if (!DService.Instance().ClientState.IsLoggedIn)
            return false;
        
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
    
    
    private delegate void                                      ReceiveAchievementProgressDelegate(Achievement* achievement, uint id, uint current, uint max);
    private          Hook<ReceiveAchievementProgressDelegate>? ReceiveAchievementProgressHook;
    
    internal override void Init()
    {
        GameState.Instance().Logout += OnLogout;
        
        ReceiveAchievementProgressHook ??=
            DService.Instance().Hook.HookFromMemberFunction<ReceiveAchievementProgressDelegate>(
                typeof(Achievement.MemberFunctionPointers),
                "ReceiveAchievementProgress",
                ReceiveAchievementProgressDetour);
        ReceiveAchievementProgressHook.Enable();
    }
    
    internal override void Uninit()
    {
        GameState.Instance().Logout -= OnLogout;
        
        ReceiveAchievementProgressHook?.Dispose();
        ReceiveAchievementProgressHook = null;
        
        Infos = [];
    }

    private void OnLogout() =>
        Infos.Clear();

    private static void SendRequest(uint id)
    {
        if (!Throttler.Throttle($"AchievementManager-Request-{id}", 10_000)) return;
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestAchievement, id);
    }
    
    private void ReceiveAchievementProgressDetour(Achievement* achievement, uint id, uint current, uint max)
    {
        ReceiveAchievementProgressHook.Original(achievement, id, current, max);
        Infos[id] = new(id, current, max);
    }
}

public record AchievementInfo(uint ID, uint Current, uint Max)
{
    public string              Name        => GetData().Name.ToString()        ?? string.Empty;
    public string              Description => GetData().Description.ToString() ?? string.Empty;
    public uint                Icon        => GetData().Icon;
    public IDalamudTextureWrap IconTexture => DService.Instance().Texture.GetFromGameIcon(new(Icon)).GetWrapOrEmpty();
    public bool                IsFinished  => Current == Max;
    
    public LuminaAchievement GetData() =>
        LuminaGetter.GetRow<LuminaAchievement>(ID).GetValueOrDefault();
}
