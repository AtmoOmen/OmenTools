using System.Collections.Concurrent;
using System.Collections.Immutable;
using Dalamud.Hooking;
using OmenTools.Abstracts;
using BattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;

namespace OmenTools.Managers;

public unsafe class PlayerStatusManager : OmenServiceBase<PlayerStatusManager>
{
    public PlayerStatusManagerConfig Config { get; private set; } = null!;

    #region 外部委托

    public delegate void GainStatusDelegate(IBattleChara player, ushort id, ushort param, ushort stackCount, TimeSpan remainingTime, ulong sourceID);

    public delegate void LoseStatusDelegate(IBattleChara player, ushort id, ushort param, ushort stackCount, ulong sourceID);

    #endregion

    #region 内部 Hook

    private static readonly CompSig OnGainStatusSig = new("48 8B C4 55 57 41 54 41 56");
    private delegate void OnGainStatusDelegate
    (
        BattleChara** player,
        ushort        statusID,
        float         remainingTime,
        ushort        statusParam,
        ulong         sourceID,
        ushort        stackCount
    );
    private Hook<OnGainStatusDelegate>? OnGainStatusHook;

    private static readonly CompSig OnLoseStatusSig = new("E8 ?? ?? ?? ?? FF C6 48 83 C3 10 83 FE 3C");
    private delegate void OnLoseStatusDelegate
    (
        BattleChara** player,
        ushort        statusID,
        ushort        statusParam,
        ulong         sourceID,
        ushort        stackCount
    );
    private Hook<OnLoseStatusDelegate>? OnLoseStatusHook;

    #endregion

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> methodsCollection = [];

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

    private bool RegisterGeneric<T>(T method, params T[] methods) where T : Delegate
    {
        var type = typeof(T);

        methodsCollection.AddOrUpdate
        (
            type,
            _ =>
            {
                var list = ImmutableList.Create<Delegate>(method);
                return methods.Length > 0 ? list.AddRange(methods) : list;
            },
            (_, currentList) =>
            {
                var newList = currentList.Add(method);
                return methods.Length > 0 ? newList.AddRange(methods) : newList;
            }
        );

        return true;
    }

    private bool UnregisterGeneric<T>(params T[] methods) where T : Delegate
    {
        if (methods is not { Length: > 0 }) return false;

        var type = typeof(T);

        while (methodsCollection.TryGetValue(type, out var currentList))
        {
            var newList = currentList.RemoveRange(methods);

            if (newList == currentList)
                return false;

            if (newList.IsEmpty)
            {
                var kvp = new KeyValuePair<Type, ImmutableList<Delegate>>(type, currentList);
                if (((ICollection<KeyValuePair<Type, ImmutableList<Delegate>>>)methodsCollection).Remove(kvp))
                    return true;
            }
            else
            {
                if (methodsCollection.TryUpdate(type, newList, currentList))
                    return true;
            }
        }

        return false;
    }


    public bool RegGain(GainStatusDelegate method, params GainStatusDelegate[] methods) => RegisterGeneric(method, methods);

    public bool RegLose(LoseStatusDelegate method, params LoseStatusDelegate[] methods) => RegisterGeneric(method, methods);

    public bool Unreg(params GainStatusDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params LoseStatusDelegate[] methods) => UnregisterGeneric(methods);

    #endregion

    #region Hooks

    private void OnGainStatusDetour(BattleChara** player, ushort statusID, float remainingTime, ushort statusParam, ulong sourceID, ushort stackCount)
    {
        OnGainStatusHook.Original(player, statusID, remainingTime, statusParam, sourceID, stackCount);

        if (statusID == 0 || player == null || *player == null) return;
        if (IBattleChara.Create((nint)(*player)) is not { } battleChara) return;

        if (Config.ShowGainStatusLog)
        {
            var sourceName = sourceID == 0xE000_0000 ? "本地对象" : DService.Instance().ObjectTable.SearchByEntityID((uint)sourceID)?.Name.ToString() ?? "未知对象";
            Debug
            (
                $"[Player Status Manager] Gain Status\n"                               +
                $"玩家: {battleChara.Name} ({battleChara.EntityID})\n"                   +
                $"状态效果: {LuminaWrapper.GetStatusName(statusID)} ({statusID})\n"        +
                $"剩余时间: {remainingTime:F1} 秒 | 参数: {statusParam} | 层数: {stackCount}\n" +
                $"来源: {sourceName} ({sourceID})"
            );
        }

        if (methodsCollection.TryGetValue(typeof(GainStatusDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var gainStatus = (GainStatusDelegate)postDelegate;
                gainStatus(battleChara, statusID, statusParam, stackCount, TimeSpan.FromSeconds(remainingTime), sourceID);
            }
        }
    }

    private void OnLoseStatusDetour(BattleChara** player, ushort statusID, ushort statusParam, ulong sourceID, ushort stackCount)
    {
        OnLoseStatusHook.Original(player, statusID, statusParam, sourceID, stackCount);

        if (statusID == 0 || player == null || *player == null) return;
        if (IBattleChara.Create((nint)(*player)) is not { } battleChara) return;

        if (Config.ShowLoseStatusLog)
        {
            var sourceName = sourceID == 0xE000_0000 ? "本地对象" : DService.Instance().ObjectTable.SearchByEntityID((uint)sourceID)?.Name.ToString() ?? "未知对象";
            Debug
            (
                $"[Player Status Manager] Lose Status\n"                        +
                $"玩家: {battleChara.Name} ({battleChara.EntityID})\n"            +
                $"状态效果: {LuminaWrapper.GetStatusName(statusID)} ({statusID})\n" +
                $"参数: {statusParam} | 层数: {stackCount}\n"                       +
                $"来源: {sourceName} ({sourceID})"
            );
        }

        if (methodsCollection.TryGetValue(typeof(LoseStatusDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var loseStatus = (LoseStatusDelegate)postDelegate;
                loseStatus(battleChara, statusID, statusParam, stackCount, sourceID);
            }
        }
    }

    #endregion

    public class PlayerStatusManagerConfig : OmenServiceConfiguration
    {
        public bool ShowGainStatusLog;
        public bool ShowLoseStatusLog;

        public void Save() =>
            this.Save(DService.Instance().GetOmenService<GameResourceManager>());
    }
}
