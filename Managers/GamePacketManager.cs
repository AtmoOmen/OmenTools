using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Network;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public unsafe class GamePacketManager : OmenServiceBase
{
    public static GamePacketManagerConfig Config { get; private set; } = null!;
    
    private static readonly CompSig SendPacketInternalSig =
        new("48 83 EC ?? 48 8B 89 ?? ?? ?? ?? 48 85 C9 74 ?? 44 89 44 24 ?? 4C 8D 44 24 ?? 44 89 4C 24 ?? 44 0F B6 4C 24");
    private delegate void* SendPacketInternalDelegate(NetworkModuleProxy* module, byte* packet, int a3, int a4, ushort priority);
    private static Hook<SendPacketInternalDelegate>? SendPacketInternalHook;
    
    private delegate void                                 ReceivePacketInternalDelegate(PacketDispatcher* dispatcher, uint targetID, byte* packet);
    private static   Hook<ReceivePacketInternalDelegate>? ReceivePacketInternalHook;
    
    private delegate bool SendPacketDelegate(NetworkModuleProxy* module, byte* packet, uint a3, uint a4);
    private static readonly SendPacketDelegate? SendPacket =
        new CompSig("E8 ?? ?? ?? ?? 48 8B D6 48 8B CF E8 ?? ?? ?? ?? 48 8B 8C 24").GetDelegate<SendPacketDelegate>();
    
    public delegate void PreSendPacketDelegate(ref bool isPrevented, int   opcode, ref byte* packet, ref ushort priority);
    public delegate void PostSendPacketDelegate(int opcode, byte* packet, ushort priority);
    
    public delegate void PreReceivePacketDelegate(ref bool isPrevented, int   opcode, ref byte* packet);
    public delegate void PostReceivePacketDelegate(int opcode, byte* packet);

    private static readonly ConcurrentDictionary<Type, ConcurrentBag<Delegate>> MethodsCollection = [];
    
    internal override void Init()
    {
        Config = LoadConfig<GamePacketManagerConfig>() ?? new();
        
        SendPacketInternalHook ??= SendPacketInternalSig.GetHook<SendPacketInternalDelegate>(SendPacketInternalDetour);
        ReceivePacketInternalHook ??=
            DService.Hook.HookFromVirtualTable<ReceivePacketInternalDelegate, PacketDispatcher.PacketDispatcherVirtualTable>(
                PacketDispatcher.StaticVirtualTablePointer, 
                "OnReceivePacket",
                ReceivePacketInternalDetour);
        
        GameState.Login  += OnLogin;
        GameState.Logout += OnLogout;
        if (DService.ClientState.IsLoggedIn)
            Toggle(true);
    }

    private static void OnLogin() => 
        Toggle(true);
    
    private static void OnLogout() => 
        Toggle(false);
    
    public static void Toggle(bool isEnabled)
    {
        SendPacketInternalHook?.Toggle(isEnabled);
        ReceivePacketInternalHook?.Toggle(isEnabled);
    }

    public static void SendPackt<T>(T data) where T : unmanaged, IGamePacket => 
        SendPacket(Framework.Instance()->NetworkModuleProxy, (byte*)&data, 0, 0x9876543); // 打个标记

    private static void* SendPacketInternalDetour(NetworkModuleProxy* module, byte* packet, int a3, int a4, ushort priority)
    {
        var opcode = *(ushort*)packet;
        LogKnownGamePacket(*(ushort*)packet, packet, priority);
        
        var isPrevented = false;
        if (a4 != 0x9876543 && MethodsCollection.TryGetValue(typeof(PreSendPacketDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preFunction = (PreSendPacketDelegate)preDelegate;
                preFunction(ref isPrevented, opcode, ref packet, ref priority);
                
                if (isPrevented) 
                    return (void*)nint.Zero;
            }
        }

        if (a4 == 0x9876543)
        {
            if (priority == 0)
                priority = 1;
        }
        else
            HandlePacketPriority(ref priority);
        
        var original = SendPacketInternalHook.Original(module, packet, a3, a4 == 0x9876543 ? 0 : a4, priority);

        if (a4 != 0x9876543 && MethodsCollection.TryGetValue(typeof(PostSendPacketDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postFunction = (PostSendPacketDelegate)postDelegate;
                postFunction(*(ushort*)packet, packet, priority);
            }
        }
        
        return original;
    }
    
    private static void ReceivePacketInternalDetour(PacketDispatcher* dispatcher, uint targetID, byte* packet)
    {
        packet -= 16;

        var opcode = Marshal.ReadInt16((nint)packet, 18);
        LogKnownReceivedPacket(opcode, targetID);
        
        var isPrevented = false;
        if (MethodsCollection.TryGetValue(typeof(PreReceivePacketDelegate), out var preDelegates))
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
        
        if (MethodsCollection.TryGetValue(typeof(PostReceivePacketDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postFunction = (PostReceivePacketDelegate)postDelegate;
                postFunction(opcode, packet);
            }
        }
    }

    private static void LogKnownReceivedPacket(int opcode, uint targetID)
    {
        if (Config.ShowReceivedPacketOpcodeLog)
            Debug($"[Game Packet Manager] [下行] 操作码: {opcode} / 目标: 0x{targetID:X8}");
    }

    private static void LogKnownGamePacket(int opcode, byte* packet, ushort priority)
    {
        if (Config.ShowGamePacketOpcodeLog)
            Debug($"[Game Packet Manager] [上行] 操作码: {opcode} / 长度: {*(uint*)(packet + 8)} / 优先级: {priority}");
        
        if (PacketHandlers.TryGetValue(opcode, out var handler) && handler.ShouldLog())
            handler.Log(packet);
    }

    private static void LogPacket<T>(byte* packet) where T : unmanaged, IGamePacket =>
        Debug($"[Game Packet Manager] {((T*)packet)->Log()}");

    private static void HandlePacketPriority(ref ushort priority)
    {
        if (priority > 0) return;
        // 采集状态或副本内
        if (DService.Condition.Any(ConditionFlag.Gathering, ConditionFlag.BoundByDuty)) return;
        // 部队储物柜
        if (FreeCompanyChest != null) return;

        priority = 1;
    }

    #region Event
    
    public static bool RegPreSendPacket(PreSendPacketDelegate            method)  => RegisterGeneric(method);
    public static bool RegPreSendPacket(params PreSendPacketDelegate[]   methods) => RegisterGeneric(methods);
    public static bool RegPostSendPacket(PostSendPacketDelegate          method)  => RegisterGeneric(method);
    public static bool RegPostSendPacket(params PostSendPacketDelegate[] methods) => RegisterGeneric(methods);
    
    public static bool RegPreReceivePacket(PreReceivePacketDelegate            method)  => RegisterGeneric(method);
    public static bool RegPreReceivePacket(params PreReceivePacketDelegate[]   methods) => RegisterGeneric(methods);
    public static bool RegPostReceivePacket(PostReceivePacketDelegate          method)  => RegisterGeneric(method);
    public static bool RegPostReceivePacket(params PostReceivePacketDelegate[] methods) => RegisterGeneric(methods);
    
    public static bool Unreg(params PreSendPacketDelegate[]     methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PostSendPacketDelegate[]    methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PreReceivePacketDelegate[]  methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PostReceivePacketDelegate[] methods) => UnregisterGeneric(methods);
    
    private static bool RegisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        var bag  = MethodsCollection.GetOrAdd(type, _ => []);
        foreach (var method in methods)
            bag.Add(method);

        return true;
    }

    private static bool UnregisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        if (MethodsCollection.TryGetValue(type, out var bag))
        {
            foreach (var method in methods)
            {
                var newBag = new ConcurrentBag<Delegate>(bag.Where(d => d != method));
                MethodsCollection[type] = newBag;
            }
            return true;
        }

        return false;
    }

    #endregion
    
    internal override void Uninit()
    {
        GameState.Login  -= OnLogin;
        GameState.Logout -= OnLogout;
        
        SendPacketInternalHook?.Dispose();
        SendPacketInternalHook = null;
        
        ReceivePacketInternalHook?.Dispose();
        ReceivePacketInternalHook = null;
        
        MethodsCollection.Clear();
    }
    
    private class PacketHandler(Func<bool> shouldLog, PacketLogger log)
    {
        public Func<bool>   ShouldLog { get; } = shouldLog;
        public PacketLogger Log       { get; } = log;
    }
    
    private delegate void PacketLogger(byte* packet);
    private static readonly Dictionary<int, PacketHandler> PacketHandlers = new()
    {
        {
            GamePacketOpcodes.EventCompleteOpcode,
            new(() => Config.ShowGamePacketEventCompleteLog,
                LogPacket<EventCompletePackt>)
        },
        {
            GamePacketOpcodes.EventStartOpcode,
            new(() => Config.ShowGamePacketEventStartLog,
                LogPacket<EventStartPackt>)
        },
        {
            GamePacketOpcodes.EventActionOpcode,
            new(() => Config.ShowGamePacketEventActionLog,
                LogPacket<EventActionPacket>)
        },
        {
            GamePacketOpcodes.DiveStartOpcode,
            new(() => Config.ShowGamePacketDiveStartLog,
                LogPacket<DiveStartPacket>)
        },
        {
            GamePacketOpcodes.PositionUpdateOpcode,
            new(() => Config.ShowGamePacketPositionUpdateLog,
                LogPacket<PositionUpdatePacket>)
        },
        {
            GamePacketOpcodes.PositionUpdateInstanceOpcode,
            new(() => Config.ShowGamePacketPositionUpdateLog,
                LogPacket<PositionUpdateInstancePacket>)
        },
        {
            GamePacketOpcodes.TreasureOpenOpcode,
            new(() => Config.ShowGamePacketTreasureOpenLog,
                LogPacket<TreasureOpenPacket>)
        },
        {
            GamePacketOpcodes.HeartbeatOpcode,
            new(() => Config.ShowGamePacketHeartbeatLog,
                LogPacket<HeartbeatPacket>)
        },
        {
            GamePacketOpcodes.UseActionOpcode,
            new(() => Config.ShowGamePacketUseActionLog,
                LogPacket<UseActionPacket>)
        },
        {
            GamePacketOpcodes.UseActionLocationOpcode,
            new(() => Config.ShowGamePacketUseActionLocationLog,
                LogPacket<UseActionLocationPacket>)
        },
        {
            GamePacketOpcodes.MJIInteractOpcode,
            new(() => Config.ShowGamePacketMJIInteractLog,
                LogPacket<MJIInteractPacket>)
        },
        {
            GamePacketOpcodes.ExecuteCommandOpcode,
            new(() => Config.ShowGamePacketExecuteCommandLog,
                LogPacket<ExecuteCommandPacket>)
        },
        {
            GamePacketOpcodes.CharaCardOpenOpcode,
            new(() => Config.ShowGamePacketCharaCardOpenLog,
                LogPacket<CharaCardOpenPacket>)
        },
        {
            GamePacketOpcodes.HandOverItemOpcode,
            new(() => Config.ShowGamePacketHandOverItemLog,
                LogPacket<HandOverItemPacket>)
        }
    };

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
            this.Save(DService.GetOmenService<GamePacketManager>());
    }
}
