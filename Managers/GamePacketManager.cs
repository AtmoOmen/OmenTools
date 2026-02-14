using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Application.Network;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Network;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public unsafe class GamePacketManager : OmenServiceBase<GamePacketManager>
{
    #region 事件定义

    public delegate void PreSendPacketDelegate(ref bool isPrevented, int  opcode, ref nint packet, ref bool isPrioritize);
    public delegate void PostSendPacketDelegate(int     opcode,      nint packet, bool     isPrioritize);

    public delegate void PreReceivePacketDelegate(ref bool isPrevented, int   opcode, ref nint packet);
    public delegate void PostReceivePacketDelegate(int     opcode,      nint packet);

    #endregion
    
    public GamePacketManagerConfig Config { get; private set; } = null!;
    
    public void SendPackt<T>(T data) where T : unmanaged, IUpstreamPacket => 
        SendPacket(Framework.Instance()->NetworkModuleProxy, (byte*)&data, 0, 0x9876543); // 打个标记

    #region 注册

    public bool RegPreSendPacket(PreSendPacketDelegate         method, params PreSendPacketDelegate[]     methods) => RegisterGeneric(method, methods);
    public bool RegPostSendPacket(PostSendPacketDelegate       method, params PostSendPacketDelegate[]    methods) => RegisterGeneric(method, methods);
    public bool RegPreReceivePacket(PreReceivePacketDelegate   method, params PreReceivePacketDelegate[]  methods) => RegisterGeneric(method, methods);
    public bool RegPostReceivePacket(PostReceivePacketDelegate method, params PostReceivePacketDelegate[] methods) => RegisterGeneric(method, methods);
    
    public bool Unreg(params PreSendPacketDelegate[]     methods) => UnregisterGeneric(methods);
    public bool Unreg(params PostSendPacketDelegate[]    methods) => UnregisterGeneric(methods);
    public bool Unreg(params PreReceivePacketDelegate[]  methods) => UnregisterGeneric(methods);
    public bool Unreg(params PostReceivePacketDelegate[] methods) => UnregisterGeneric(methods);

    #endregion
    
    
    private delegate void* SendPacketDelegate(NetworkModuleProxy* module, byte* packet, uint a3, uint a4);
    private static readonly SendPacketDelegate? SendPacket =
        new CompSig("E8 ?? ?? ?? ?? 48 8B D6 48 8B CF E8 ?? ?? ?? ?? 48 8B 8C 24").GetDelegate<SendPacketDelegate>();

    private delegate bool                              SendPacketInternalDelegate(ZoneClient* zoneClient, nint packet, uint a3, uint a4, bool isPrioritize);
    private          Hook<SendPacketInternalDelegate>? SendPacketInternalHook;
    
    private delegate void                                 ReceivePacketInternalDelegate(PacketDispatcher* dispatcher, uint targetID, nint packet);
    private          Hook<ReceivePacketInternalDelegate>? ReceivePacketInternalHook;
    
    private delegate void PacketLogger(byte* packet);

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> methodsCollection = [];
    
    internal override void Init()
    {
        Config = LoadConfig<GamePacketManagerConfig>() ?? new();
        
        SendPacketInternalHook ??= DService.Instance().Hook.HookFromMemberFunction(
            typeof(ZoneClient.MemberFunctionPointers),
            "SendPacket", 
            (SendPacketInternalDelegate)SendPacketInternalDetour);
        ReceivePacketInternalHook ??=
            PacketDispatcher.StaticVirtualTablePointer->HookVFuncFromName("OnReceivePacket", (ReceivePacketInternalDelegate)ReceivePacketInternalDetour);
        
        GameState.Instance().Login  += OnLogin;
        GameState.Instance().Logout += OnLogout;
        if (GameState.IsLoggedIn)
            ToggleHooks(true);
    }
    
    internal override void Uninit()
    {
        GameState.Instance().Login  -= OnLogin;
        GameState.Instance().Logout -= OnLogout;
        
        SendPacketInternalHook?.Dispose();
        SendPacketInternalHook = null;
        
        ReceivePacketInternalHook?.Dispose();
        ReceivePacketInternalHook = null;
        
        methodsCollection.Clear();
    }

    #region Hook 与事件

    private void OnLogin() => 
        ToggleHooks(true);
    
    private void OnLogout() => 
        ToggleHooks(false);
    
    private bool SendPacketInternalDetour(ZoneClient* zoneClient, nint packet, uint a3, uint a4, bool isPrioritize)
    {
        var opcode = *(ushort*)packet;
        LogKnownGamePacket(*(ushort*)packet, (byte*)packet, isPrioritize);
        
        var isPrevented = false;
        if (a4 != 0x9876543 && methodsCollection.TryGetValue(typeof(PreSendPacketDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preFunction = (PreSendPacketDelegate)preDelegate;
                preFunction(ref isPrevented, opcode, ref packet, ref isPrioritize);
                
                if (isPrevented) 
                    return false;
            }
        }

        if (a4 == 0x9876543)
        {
            if (!isPrioritize)
                isPrioritize = true;
        }
        else
            HandlePacketPriority(ref isPrioritize);
        
        var original = SendPacketInternalHook.Original(zoneClient, packet, a3, a4, isPrioritize);

        if (a4 != 0x9876543 && methodsCollection.TryGetValue(typeof(PostSendPacketDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postFunction = (PostSendPacketDelegate)postDelegate;
                postFunction(*(ushort*)packet, packet, isPrioritize);
            }
        }
        
        return original;
    }
    
    private void ReceivePacketInternalDetour(PacketDispatcher* dispatcher, uint targetID, nint packet)
    {
        packet -= 16;

        var opcode = Marshal.ReadInt16(packet, 18);
        LogKnownReceivedPacket(opcode, targetID);
        
        var isPrevented = false;
        if (methodsCollection.TryGetValue(typeof(PreReceivePacketDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preFunction = (PreReceivePacketDelegate)preDelegate;
                preFunction(ref isPrevented, opcode, ref packet);
                
                if (isPrevented) 
                    return;
            }
        }
        
        ReceivePacketInternalHook.Original(dispatcher, targetID, packet + 16);
        
        if (methodsCollection.TryGetValue(typeof(PostReceivePacketDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postFunction = (PostReceivePacketDelegate)postDelegate;
                postFunction(opcode, packet);
            }
        }
    }

    #endregion

    private void LogKnownReceivedPacket(int opcode, uint targetID)
    {
        if (Config.ShowReceivedPacketOpcodeLog)
            Debug($"[Game Packet Manager] [下行] 操作码: {opcode} / 目标: 0x{targetID:X8}");
    }

    private void LogKnownGamePacket(int opcode, byte* packet, bool isPrioritize)
    {
        if (Config.ShowGamePacketOpcodeLog)
            Debug($"[Game Packet Manager] [上行] 操作码: {opcode} / 长度: {*(uint*)(packet + 8)} / 优先级: {isPrioritize}");
        
        if (PacketHandlers.TryGetValue(opcode, out var handler) && handler.ShouldLog())
            handler.Log(packet);
    }

    private static void LogPacket<T>(byte* packet) where T : unmanaged, IUpstreamPacket =>
        Debug($"[Game Packet Manager] {((T*)packet)->Log()}");

    private void ToggleHooks(bool isEnabled)
    {
        SendPacketInternalHook?.Toggle(isEnabled);
        ReceivePacketInternalHook?.Toggle(isEnabled);
    }
    
    private static void HandlePacketPriority(ref bool isPrioritize)
    {
        if (isPrioritize) return;
        // 采集状态
        if (Conditions.Instance()->Gathering) return;
        // 部队储物柜
        if (FreeCompanyChest != null) return;

        isPrioritize = true;
    }

    #region 注册 (私有)

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

    #endregion
    
    private static readonly Dictionary<int, PacketHandler> PacketHandlers = new()
    {
        [UpstreamOpcode.EventCompleteOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketEventCompleteLog,
                LogPacket<EventCompletePackt>
            ),

        [UpstreamOpcode.EventStartOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketEventStartLog,
                LogPacket<EventStartPackt>
            ),

        [UpstreamOpcode.EventActionOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketEventActionLog,
                LogPacket<EventActionPacket>
            ),

        [UpstreamOpcode.DiveStartOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketDiveStartLog,
                LogPacket<DiveStartPacket>
            ),

        [UpstreamOpcode.PositionUpdateOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketPositionUpdateLog,
                LogPacket<PositionUpdatePacket>
            ),

        [UpstreamOpcode.PositionUpdateInstanceOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketPositionUpdateLog,
                LogPacket<PositionUpdateInstancePacket>
            ),

        [UpstreamOpcode.TreasureOpenOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketTreasureOpenLog,
                LogPacket<TreasureOpenPacket>
            ),

        [UpstreamOpcode.HeartbeatOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketHeartbeatLog,
                LogPacket<HeartbeatPacket>
            ),

        [UpstreamOpcode.UseActionOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketUseActionLog,
                LogPacket<UseActionPacket>
            ),

        [UpstreamOpcode.UseActionLocationOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketUseActionLocationLog,
                LogPacket<UseActionLocationPacket>
            ),

        [UpstreamOpcode.MJIInteractOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketMJIInteractLog,
                LogPacket<MJIInteractPacket>
            ),

        [UpstreamOpcode.ExecuteCommandOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketExecuteCommandLog,
                LogPacket<ExecuteCommandPacket>
            ),

        [UpstreamOpcode.CharaCardOpenOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketCharaCardOpenLog,
                LogPacket<CharaCardOpenPacket>
            ),

        [UpstreamOpcode.HandOverItemOpcode] =
            new
            (
                () => Instance().Config.ShowGamePacketHandOverItemLog,
                LogPacket<HandOverItemPacket>
            )
    };
    
    private class PacketHandler(Func<bool> shouldLog, PacketLogger log)
    {
        public Func<bool>   ShouldLog { get; } = shouldLog;
        public PacketLogger Log       { get; } = log;
    }
    
    public class GamePacketManagerConfig : OmenServiceConfiguration
    {
        public bool ShowGamePacketOpcodeLog;
        public bool ShowGamePacketEventStartLog;
        public bool ShowGamePacketEventActionLog;
        public bool ShowGamePacketEventCompleteLog;
        public bool ShowGamePacketDiveStartLog;
        public bool ShowGamePacketPositionUpdateLog;
        public bool ShowGamePacketTreasureOpenLog;
        public bool ShowGamePacketHeartbeatLog;
        public bool ShowGamePacketUseActionLog;
        public bool ShowGamePacketUseActionLocationLog;
        public bool ShowGamePacketMJIInteractLog;
        public bool ShowGamePacketExecuteCommandLog;
        public bool ShowGamePacketCharaCardOpenLog;
        public bool ShowGamePacketHandOverItemLog;
        
        public bool ShowReceivedPacketOpcodeLog;

        public void Save() => 
            this.Save(DService.Instance().GetOmenService<GamePacketManager>());
    }
}
