using System.Collections.Concurrent;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using OmenTools.Abstracts;
using BattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;

namespace OmenTools.Managers;

public unsafe class UseActionManager : OmenServiceBase
{
    public static UseActionManagerConfig Config { get; private set; } = null!;
    
    #region Delegates

    public delegate void PreUseActionDelegate(
        ref bool                        isPrevented,
        ref ActionType                  actionType,
        ref uint                        actionID,
        ref ulong                       targetID,
        ref uint                        extraParam,
        ref ActionManager.UseActionMode queueState,
        ref uint                        comboRouteID);

    public delegate void PostUseActionDelegate(
        bool                        result,
        ActionType                  actionType,
        uint                        actionID,
        ulong                       targetID,
        uint                        extraParam,
        ActionManager.UseActionMode queueState,
        uint                        comboRouteID);

    public delegate void PreUseActionLocationDelegate(
        ref bool       isPrevented,
        ref ActionType type,
        ref uint       actionID,
        ref ulong      targetID,
        ref Vector3    location,
        ref uint       extraParam,
        ref byte       a7);

    public delegate void PostUseActionLocationDelegate(
        bool       result,
        ActionType actionType,
        uint       actionID,
        ulong      targetID,
        Vector3    location,
        uint       extraParam,
        byte       a7);

    public delegate void PreIsActionOffCooldownDelegate(
        ref bool   isPrevented,
        ActionType actionType,
        uint       actionID,
        ref float  queueTime);

    public delegate void PreCharacterCompleteCastDelegate(
        ref bool         isPrevented,
        ref IBattleChara player,
        ref ActionType   type,
        ref uint         actionID,
        ref uint         spellID,
        ref GameObjectId animationTargetID,
        ref Vector3     location,
        ref float        rotation,
        ref short        lastUsedActionSequence,
        ref int          animationVariation,
        ref int          ballistaEntityID);

    public delegate void PostCharacterCompleteCastDelegate(
        nint         result,
        IBattleChara player,
        ActionType   type,
        uint         actionID,
        uint         spellID,
        GameObjectId animationTargetID,
        Vector3      location,
        float        rotation,
        short        lastUsedActionSequence,
        int          animationVariation,
        int          ballistaEntityID);

    public delegate void PreCharacterStartCastDelegate(
        ref bool         isPrevented,
        ref IBattleChara player,
        ref ActionType   type,
        ref uint         actionID,
        ref nint         a4,
        ref float        rotation,
        ref float        a6);

    public delegate void PostCharacterStartCastDelegate(
        nint         result,
        IBattleChara player,
        ActionType   type,
        uint         actionID,
        nint         a4,
        float        rotation,
        float        a6);

    #endregion

    #region Hooks

    internal static readonly CompSig UseActionSig = new("E8 ?? ?? ?? ?? B0 01 EB B6 ?? ?? ?? ?? ?? ?? ??");
    internal delegate bool UseActionDelegate(
        ActionManager* actionManager, 
        ActionType actionType, 
        uint actionID, 
        ulong targetID,
        uint extraParam, 
        ActionManager.UseActionMode queueState, 
        uint comboRouteID, 
        bool* outOptAreaTargeted);
    internal static Hook<UseActionDelegate>? UseActionHook;
    
    internal static readonly CompSig UseActionLocationSig = new("E8 ?? ?? ?? ?? 40 3A C7 0F 85 ?? ?? ?? ??");
    internal delegate bool UseActionLocationDelegate(
        ActionManager* manager,
        ActionType     type,
        uint           actionID,
        ulong          targetID,
        Vector3*       location,
        uint           extraParam,
        byte           a7);
    internal static Hook<UseActionLocationDelegate>? UseActionLocationHook;
    
    internal static readonly CompSig                     CanPlayerUseActionSig = new("48 89 5C 24 ?? 57 48 83 EC ?? 8B DA 48 8D 0D");
    internal delegate        bool                        CanPlayerUseActionDelegate(ActionManager* manager, uint actionID);
    internal static readonly CanPlayerUseActionDelegate? CanPlayerUseAction = CanPlayerUseActionSig.GetDelegate<CanPlayerUseActionDelegate>();
    
    internal static readonly CompSig                            IsActionOffCooldownSig = new("E8 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? F6 05 ?? ?? ?? ?? ?? 74");
    internal delegate        bool                               IsActionOffCooldownDelegate(ActionManager* manager, ActionType actionType, uint actionID);
    internal static          Hook<IsActionOffCooldownDelegate>? IsActionOffCooldownHook;
    
    internal static readonly CompSig CharacterCompleteCastSig = new("E8 ?? ?? ?? ?? 41 0F B6 56 ?? 44 0F 28 8C 24 ?? ?? ?? ??");
    internal delegate nint CharacterCompleteCastDelegate(
        BattleChara* player,
        uint         type,
        uint         actionID,
        uint         spellID,
        GameObjectId animationTargetID,
        Vector3*     location,
        float        rotation,
        short        lastUsedActionSequence,
        int          animationVariation,
        int          ballistaEntityID);
    internal static Hook<CharacterCompleteCastDelegate>? CharacterCompleteCastHook;
    
    internal static readonly CompSig CharacterStartCastSig = new("E8 ?? ?? ?? ?? 80 7E 22 11");
    internal delegate nint CharacterStartCastDelegate(BattleChara* player, ActionType type, uint actionID, nint a4, float rotation, float a6);
    internal static Hook<CharacterStartCastDelegate>? CharacterStartCastHook;

    #endregion

    private static readonly ConcurrentDictionary<Type, ConcurrentBag<Delegate>> methodsCollection = [];

    internal override void Init()
    {
        Config = LoadConfig<UseActionManagerConfig>() ?? new();
        
        UseActionHook             ??= UseActionSig.GetHook<UseActionDelegate>(UseActionDetour);
        UseActionLocationHook     ??= UseActionLocationSig.GetHook<UseActionLocationDelegate>(UseActionLocationDetour);
        IsActionOffCooldownHook   ??= IsActionOffCooldownSig.GetHook<IsActionOffCooldownDelegate>(IsActionOffCooldownDetour);
        CharacterCompleteCastHook ??= CharacterCompleteCastSig.GetHook<CharacterCompleteCastDelegate>(CharacterCompleteCastDetour);
        CharacterStartCastHook    ??= CharacterStartCastSig.GetHook<CharacterStartCastDelegate>(CharacterStartCastDetour);

        EnableHooks();
    }

    private static bool RegisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        var bag = methodsCollection.GetOrAdd(type, _ => []);
        foreach (var method in methods)
            bag.Add(method);

        EnableHooks();
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

    #region Register

    public static bool RegPreUseAction(params PreUseActionDelegate[] methods) => RegisterGeneric(methods);
    public static bool RegPreUseAction(PreUseActionDelegate          methods) => RegisterGeneric(methods);
    
    public static bool RegUseAction(params PostUseActionDelegate[] methods) => RegisterGeneric(methods);
    public static bool RegUseAction(PostUseActionDelegate          methods) => RegisterGeneric(methods);
    
    public static bool RegPreUseActionLocation(params PreUseActionLocationDelegate[] methods) => RegisterGeneric(methods);
    public static bool RegPreUseActionLocation(PreUseActionLocationDelegate          methods) => RegisterGeneric(methods);
    
    public static bool RegUseActionLocation(params PostUseActionLocationDelegate[] methods) => RegisterGeneric(methods);
    public static bool RegUseActionLocation(PostUseActionLocationDelegate          methods) => RegisterGeneric(methods);
    
    public static bool RegPreIsActionOffCooldown(params PreIsActionOffCooldownDelegate[] methods) => RegisterGeneric(methods);
    public static bool RegPreIsActionOffCooldown(PreIsActionOffCooldownDelegate          methods) => RegisterGeneric(methods);
    
    public static bool RegPreCharacterCompleteCast(params PreCharacterCompleteCastDelegate[] methods) => RegisterGeneric(methods);
    public static bool RegPreCharacterCompleteCast(PreCharacterCompleteCastDelegate          methods) => RegisterGeneric(methods);
    
    public static bool RegCharacterCompleteCast(params PostCharacterCompleteCastDelegate[] methods) => RegisterGeneric(methods);
    public static bool RegCharacterCompleteCast(PostCharacterCompleteCastDelegate          methods) => RegisterGeneric(methods);
    
    public static bool RegPreCharacterStartCast(params PreCharacterStartCastDelegate[] methods) => RegisterGeneric(methods);
    public static bool RegPreCharacterStartCast(PreCharacterStartCastDelegate          methods) => RegisterGeneric(methods);
    
    public static bool RegCharacterStartCast(params PostCharacterStartCastDelegate[] methods) => RegisterGeneric(methods);
    public static bool RegCharacterStartCast(PostCharacterStartCastDelegate          methods) => RegisterGeneric(methods);

    public static bool Unreg(params PreUseActionDelegate[]              methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PostUseActionDelegate[]             methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PreUseActionLocationDelegate[]      methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PostUseActionLocationDelegate[]     methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PreIsActionOffCooldownDelegate[]    methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PreCharacterCompleteCastDelegate[]  methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PostCharacterCompleteCastDelegate[] methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PreCharacterStartCastDelegate[]     methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PostCharacterStartCastDelegate[]    methods) => UnregisterGeneric(methods);

    #endregion

    #region Hooks

    public static void EnableHooks()
    {
        UseActionHook?.Enable();
        UseActionLocationHook?.Enable();
        IsActionOffCooldownHook?.Enable();
        CharacterCompleteCastHook?.Enable();
        CharacterStartCastHook?.Enable();
    }

    public static void DisableHooks()
    {
        UseActionHook?.Disable();
        UseActionLocationHook?.Disable();
        IsActionOffCooldownHook?.Disable();
        CharacterCompleteCastHook?.Disable();
        CharacterStartCastHook?.Disable();
    }

    private static bool UseActionDetour(
        ActionManager* actionManager, 
        ActionType                  actionType, 
        uint actionID,    
        ulong targetID,
        uint           extraParam,    
        ActionManager.UseActionMode queueState, 
        uint comboRouteID, 
        bool* outOptAreaTargeted)
    {
        if (Config.ShowUseActionLog)
            Debug($"[Use Action Manager] 一般类技能\n类型:{actionType} | ID:{actionID} | 目标ID: {targetID}\n" +
                  $"额外参数: {extraParam} | 队列状态: {queueState} | 连击路径ID: {comboRouteID}");

        var isPrevented = false;
        if (methodsCollection.TryGetValue(typeof(PreUseActionDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preUseAction = (PreUseActionDelegate)preDelegate;
                preUseAction(ref isPrevented, ref actionType, ref actionID, ref targetID, ref extraParam,
                             ref queueState, ref comboRouteID);
                if (isPrevented) return false;
            }
        }

        var original = UseActionHook.Original(actionManager, actionType, actionID, targetID, extraParam, queueState, comboRouteID, outOptAreaTargeted);

        if (methodsCollection.TryGetValue(typeof(PostUseActionDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postUseAction = (PostUseActionDelegate)postDelegate;
                postUseAction(original, actionType, actionID, targetID, extraParam, queueState, comboRouteID);
            }
        }

        return original;
    }

    private static bool UseActionLocationDetour(
        ActionManager* manager, ActionType type, uint actionID, ulong targetID, Vector3* location, uint extraParam, byte a7)
    {
        if (Config.ShowUseActionLocationLog)
            Debug(
                $"[Use Action Manager] 地面类技能\n类型:{type} | ID:{actionID} | 目标ID: {targetID} | 地点:{*location} | 额外参数:{extraParam}");

        var isPrevented = false;
        var location0 = *location;
        if (methodsCollection.TryGetValue(typeof(PreUseActionLocationDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preUseAction = (PreUseActionLocationDelegate)preDelegate;
                preUseAction(ref isPrevented, ref type, ref actionID, ref targetID, ref location0, ref extraParam, ref a7);
                if (isPrevented) return false;
            }
        }

        var original = UseActionLocationHook.Original(manager, type, actionID, targetID, &location0, extraParam, a7);
        
        if (methodsCollection.TryGetValue(typeof(PostUseActionLocationDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postUseAction = (PostUseActionLocationDelegate)postDelegate;
                postUseAction(original, type, actionID, targetID, location0, extraParam, a7);
            }
        }

        return original;
    }

    private static bool IsActionOffCooldownDetour(ActionManager* manager, ActionType actionType, uint actionID)
    {
        if (manager == null) return false;

        var isPrevented = false;
        var queueTime = 0.5f;
        if (methodsCollection.TryGetValue(typeof(PreIsActionOffCooldownDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                ((PreIsActionOffCooldownDelegate)preDelegate)(ref isPrevented, actionType, actionID, ref queueTime);
                if (isPrevented) return false;
            }
        }

        var recastDetailForAction = manager->GetRecastGroupDetail(manager->GetRecastGroup((int)actionType, actionID));
        if (recastDetailForAction == null) return false;

        var additionDetail = manager->GetAdditionalRecastGroup(actionType, actionID);
        var recastDetail   = manager->GetRecastGroupDetail(additionDetail);
        if (recastDetail != null && recastDetail->IsActive && recastDetail->Total - recastDetail->Elapsed > queueTime)
            return false;

        var spellID = ActionManager.GetSpellIdForAction(actionType, actionID);
        var maxCharges = ActionManager.GetMaxCharges(spellID, 100);

        var canPlayerUseAction = CanPlayerUseAction(manager, recastDetailForAction->ActionId);
        var currentTotal = recastDetailForAction->Total;
        var currentElapsed = recastDetailForAction->Elapsed;

        if (canPlayerUseAction)
            return !((currentTotal / maxCharges) - currentElapsed > queueTime);
        else
            return !(currentTotal - currentElapsed > queueTime);
    }

    private static nint CharacterCompleteCastDetour(
        BattleChara* player,
        uint         type,
        uint         actionID,
        uint         spellID,
        GameObjectId animationTargetID,
        Vector3*     locationPtr,
        float        rotation,
        short        lastUsedActionSequence,
        int          animationVariation,
        int          ballistaEntityID)
    {
        var actionType   = (ActionType)type;
        var battlePlayer = IBattleChara.Create((nint)player);
        var location     = *locationPtr;
        
        if (Config.ShowCharacterCompleteCastLog)
            Debug($"[Use Action Manager] 技能释放完成\n"                                                                               +
                  $"对象: {battlePlayer.Name} ({battlePlayer.GameObjectID}) | 类型: {type} | 技能 ID: {actionID} | 施法 ID: {spellID}\n" +
                  $"动画目标 ID: {(ulong)animationTargetID} | 地点: {location} | 面向: {rotation}\n"                                     +
                  $"技能序列: {lastUsedActionSequence} | 动画变化: {animationVariation} | 弩炮实体 ID: {ballistaEntityID}");

        var isPrevented = false;
        if (methodsCollection.TryGetValue(typeof(PreCharacterCompleteCastDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preCharacterCompleteCast = (PreCharacterCompleteCastDelegate)preDelegate;
                preCharacterCompleteCast(
                    ref isPrevented,
                    ref battlePlayer,
                    ref actionType,
                    ref actionID,
                    ref spellID,
                    ref animationTargetID,
                    ref location,
                    ref rotation,
                    ref lastUsedActionSequence,
                    ref animationVariation,
                    ref ballistaEntityID);
                if (isPrevented) return nint.Zero;
            }
        }

        var original = CharacterCompleteCastHook.Original(
            battlePlayer.ToStruct(),
            (uint)actionType,
            actionID,
            spellID,
            animationTargetID,
            &location,
            rotation,
            lastUsedActionSequence,
            animationVariation,
            ballistaEntityID);

        if (methodsCollection.TryGetValue(typeof(PostCharacterCompleteCastDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postCharacterCompleteCast = (PostCharacterCompleteCastDelegate)postDelegate;
                postCharacterCompleteCast(
                    original,
                    battlePlayer,
                    actionType,
                    actionID,
                    spellID,
                    animationTargetID,
                    location,
                    rotation,
                    lastUsedActionSequence,
                    animationVariation,
                    ballistaEntityID);
            }
        }

        return original;
    }

    private static nint CharacterStartCastDetour(
        BattleChara* player,
        ActionType   type,
        uint         actionID,
        nint         a4,
        float        rotation,
        float        a6)
    {
        var battlePlayer = IBattleChara.Create((nint)player);

        if (Config.ShowCharacterStartCastLog)
            Debug($"[Use Action Manager] 技能开始释放\n"                                                                          +
                  $"对象: {battlePlayer.Name} ({battlePlayer.GameObjectID}) | 类型: {type} | ID: {actionID} | 面向: {rotation}\n" +
                  $"P4: {a4} | P6: {a6}");

        var isPrevented = false;
        if (methodsCollection.TryGetValue(typeof(PreCharacterStartCastDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preCharacterStartCast = (PreCharacterStartCastDelegate)preDelegate;
                preCharacterStartCast(ref isPrevented, ref battlePlayer, ref type, ref actionID, ref a4, ref rotation, ref a6);

                if (isPrevented)
                    return nint.Zero;
            }
        }

        var original = CharacterStartCastHook.Original(battlePlayer.ToStruct(), type, actionID, a4, rotation, a6);

        if (methodsCollection.TryGetValue(typeof(PostCharacterStartCastDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postCharacterStartCast = (PostCharacterStartCastDelegate)postDelegate;
                postCharacterStartCast(original, battlePlayer, type, actionID, a4, rotation, a6);
            }
        }

        return original;
    }

    #endregion

    #region Invokes

    public static bool UseAction(
        ActionType                  type,
        uint                        actionID,
        ulong                       targetID     = 0xE000_0000,
        uint                        extraParam   = 0,
        ActionManager.UseActionMode queueState   = 0,
        uint                        comboRouteID = 0) =>
        UseActionHook.Original(ActionManager.Instance(), type, actionID, targetID, extraParam, queueState, comboRouteID, null);

    public static bool UseActionLocation(
        ActionType type,
        uint       actionID,
        ulong      targetID   = 0xE000_0000,
        Vector3    location   = default,
        uint       extraParam = 0U,
        byte       a7         = 0) =>
        UseActionLocationHook.Original(ActionManager.Instance(), type, actionID, targetID, &location, extraParam, a7);

    public static bool IsActionOffCooldown(ActionType actionType, uint actionID)
        => IsActionOffCooldownHook.Original(ActionManager.Instance(), actionType, actionID);

    public static nint CharacterCompleteCast(
        IBattleChara player,
        ActionType   type,
        uint         actionID,
        uint         spellID,
        GameObjectId animationTargetID,
        Vector3      location,
        float        rotation,
        short        lastUsedActionSequence,
        int          animationVariation,
        int          ballistaEntityID) =>
        CharacterCompleteCastHook.Original(player.ToStruct(), 
                                           (uint)type, 
                                           actionID, 
                                           spellID, 
                                           animationTargetID, 
                                           &location, 
                                           rotation, 
                                           lastUsedActionSequence, 
                                           animationVariation, 
                                           ballistaEntityID);

    public static nint CharacterStartCast(
        IBattleChara player,
        ActionType   type,
        uint         actionID,
        nint         a4,
        float        rotation,
        float        a6) =>
        CharacterStartCastHook.Original(player.ToStruct(), type, actionID, a4, rotation, a6);

    public static bool UseActionCallDetour(
        ActionType                  type,
        uint                        actionID,
        ulong                       targetID     = 0xE000_0000,
        uint                        extraParam   = 0,
        ActionManager.UseActionMode queueState   = 0,
        uint                        comboRouteID = 0) =>
        UseActionDetour(ActionManager.Instance(), type, actionID, targetID, extraParam, queueState, comboRouteID, null);

    public static bool UseActionLocationCallDetour(
        ActionType type,
        uint       actionID,
        ulong      targetID   = 0xE000_0000,
        Vector3    location   = default,
        uint       extraParam = 0U,
        byte       a7         = 0) =>
        UseActionLocationDetour(ActionManager.Instance(), type, actionID, targetID, &location, extraParam, a7);

    public static bool IsActionOffCooldownCallDetour(ActionType actionType, uint actionID) => 
        IsActionOffCooldownDetour(ActionManager.Instance(), actionType, actionID);

    public static nint CharacterCompleteCastCallDetour(
        IBattleChara player,
        ActionType   type,
        uint         actionID,
        uint         spellID,
        GameObjectId animationTargetID,
        Vector3      location,
        float        rotation,
        short        lastUsedActionSequence,
        int          animationVariation,
        int          ballistaEntityID) =>
        CharacterCompleteCastDetour(player.ToStruct(),
                                    (uint)type,
                                    actionID,
                                    spellID,
                                    animationTargetID,
                                    &location,
                                    rotation,
                                    lastUsedActionSequence,
                                    animationVariation,
                                    ballistaEntityID);

    public static nint CharacterStartCastCallDetour(
        IBattleChara player,
        ActionType   type,
        uint         actionID,
        nint         a4,
        float        rotation,
        float        a6) =>
        CharacterStartCastDetour(player.ToStruct(),
                                 type,
                                 actionID,
                                 a4,
                                 rotation,
                                 a6);

    #endregion

    internal override void Uninit()
    {
        DisableHooks();

        UseActionHook?.Dispose();
        UseActionHook = null;

        UseActionLocationHook?.Dispose();
        UseActionLocationHook = null;

        IsActionOffCooldownHook?.Dispose();
        IsActionOffCooldownHook = null;
        
        CharacterCompleteCastHook?.Dispose();
        CharacterCompleteCastHook = null;
        
        CharacterStartCastHook?.Dispose();
        CharacterStartCastHook = null;
        
        methodsCollection.Clear();
    }
    
    public class UseActionManagerConfig : OmenServiceConfiguration
    {
        public bool ShowUseActionLog;
        public bool ShowUseActionLocationLog;
        public bool ShowCharacterCompleteCastLog;
        public bool ShowCharacterStartCastLog;

        public void Save() => 
            this.Save(DService.GetOmenService<UseActionManager>());
    }
}
