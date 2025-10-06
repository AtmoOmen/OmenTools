using System.Collections.Concurrent;
using Dalamud.Hooking;
using OmenTools.Abstracts;
using BattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;

namespace OmenTools.Managers;

public unsafe class PlayerStatusManager : OmenServiceBase
{
    public static PlayerStatusManagerConfig Config { get; private set; } = null!;
    
    #region 外部委托

    public delegate void GainStatusDelegate(BattleChara* player, ushort id, ushort param, ushort stackCount, TimeSpan remainingTime, ulong sourceID);
    
    public delegate void LoseStatusDelegate(BattleChara* player, ushort id, ushort param, ushort stackCount, ulong sourceID);

    #endregion

    #region 内部 Hook

    private static readonly CompSig OnGainStatusSig = new("E8 ?? ?? ?? ?? 80 BC 24 C0 00 00 00 00");
    private delegate void OnGainStatusDelegate(BattleChara** player, ushort statusID, float remainingTime, ushort statusParam, ulong sourceID, ushort stackCount);
    private static Hook<OnGainStatusDelegate>? OnGainStatusHook;
    
    private static readonly CompSig OnLoseStatusSig = new("E8 ?? ?? ?? ?? FF C6 48 83 C3 10 83 FE 3C");
    private delegate void OnLoseStatusDelegate(BattleChara** player, ushort statusID, ushort statusParam, ulong sourceID, ushort stackCount);
    private static Hook<OnLoseStatusDelegate>? OnLoseStatusHook;

    #endregion
    
    private static readonly ConcurrentDictionary<Type, ConcurrentBag<Delegate>> methodsCollection = [];

    internal override void Init()
    {
        Config = LoadConfig<PlayerStatusManagerConfig>() ?? new();
        
        OnGainStatusHook ??= OnGainStatusSig.GetHook<OnGainStatusDelegate>(OnGainStatusDetour);
        OnGainStatusHook.Enable();
        
        OnLoseStatusHook ??= OnLoseStatusSig.GetHook<OnLoseStatusDelegate>(OnLoseStatusDetour);
        OnLoseStatusHook.Enable();
    }

    internal override void Uninit()
    {
        OnGainStatusHook?.Dispose();
        OnGainStatusHook = null;
        
        OnLoseStatusHook?.Dispose();
        OnLoseStatusHook = null;
    }

    #region 注册

    private static bool RegisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        var bag  = methodsCollection.GetOrAdd(type, _ => []);
        foreach (var method in methods)
            bag.Add(method);

        return true;
    }

    private static bool UnregisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        if (methodsCollection.TryGetValue(type, out var bag))
        {
            foreach (var method in methods)
            {
                var newBag = new ConcurrentBag<Delegate>(bag.Where(d => d != method));
                methodsCollection[type] = newBag;
            }
            
            return true;
        }

        return false;
    }
    
    
    public static bool RegGainStatus(GainStatusDelegate          methods) => RegisterGeneric(methods);
    public static bool RegGainStatus(params GainStatusDelegate[] methods) => RegisterGeneric(methods);
    
    public static bool RegLoseStatus(LoseStatusDelegate          methods) => RegisterGeneric(methods);
    public static bool RegLoseStatus(params LoseStatusDelegate[] methods) => RegisterGeneric(methods);

    public static bool Unreg(params GainStatusDelegate[] methods) => UnregisterGeneric(methods);
    public static bool Unreg(params LoseStatusDelegate[] methods) => UnregisterGeneric(methods);

    #endregion

    #region Hooks

    private static void OnGainStatusDetour(BattleChara** player, ushort statusID, float remainingTime, ushort statusParam, ulong sourceID, ushort stackCount)
    {
        OnGainStatusHook.Original(player, statusID, remainingTime, statusParam, sourceID, stackCount);

        if (statusID == 0) return;
        
        try
        {
            if (Config.ShowGainStatusLog)
            {
                var sourceName = sourceID == 0xE000_0000 ? "本地对象" : DService.ObjectTable.SearchByEntityID((uint)sourceID)?.Name.ExtractText() ?? "未知对象";
                Debug(
                    $"[Player Status Manager] Gain Status\n"                                        +
                    $"玩家: {(*player)->NameString} ({(*player)->EntityId})\n"                        +
                    $"状态效果: {LuminaWrapper.GetStatusName(statusID)} ({statusID})\n"                 +
                    $"剩余时间: {remainingTime:F1} 秒 | 参数: {statusParam} | Stack Count: {stackCount}\n" +
                    $"来源: {sourceName} ({sourceID})");
            }
        }
        catch
        {
            // ignored
        }
        
        if (methodsCollection.TryGetValue(typeof(GainStatusDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var gainStatus = (GainStatusDelegate)postDelegate;
                gainStatus(*player, statusID, statusParam, stackCount, TimeSpan.FromSeconds(remainingTime), sourceID);
            }
        }
    }
    
    private static void OnLoseStatusDetour(BattleChara** player, ushort statusID, ushort statusParam, ulong sourceID, ushort stackCount)
    {
        OnLoseStatusHook.Original(player, statusID, statusParam, sourceID, stackCount);

        if (statusID == 0) return;
        
        try
        {
            if (Config.ShowLoseStatusLog)
            {
                var sourceName = sourceID == 0xE000_0000 ? "本地对象" : DService.ObjectTable.SearchByEntityID((uint)sourceID)?.Name.ExtractText() ?? "未知对象";
                Debug(
                    $"[Player Status Manager] Lose Status\n"                        +
                    $"玩家: {(*player)->NameString} ({(*player)->EntityId})\n"        +
                    $"状态效果: {LuminaWrapper.GetStatusName(statusID)} ({statusID})\n" +
                    $"参数: {statusParam} | Stack Count: {stackCount}\n"              +
                    $"来源: {sourceName} ({sourceID})");
            }
        }
        catch
        {
            // ignored
        }
        
        if (methodsCollection.TryGetValue(typeof(LoseStatusDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var loseStatus = (LoseStatusDelegate)postDelegate;
                loseStatus(*player, statusID, statusParam, stackCount, sourceID);
            }
        }
    }

    #endregion
    
    public class PlayerStatusManagerConfig : OmenServiceConfiguration
    {
        public bool ShowGainStatusLog;
        public bool ShowLoseStatusLog;

        public void Save() => 
            this.Save(DService.GetOmenService<GameResourceManager>());
    }
}
