using System.Runtime.InteropServices;

namespace OmenTools.Infos;

public static class GamePacketOpcodes
{
    public static int EventStartOpcode             { get; private set; }
    public static int EventActionOpcode            { get; private set; }
    public static int EventCompleteOpcode          { get; private set; }
    public static int DiveStartOpcode              { get; private set; }
    public static int PositionUpdateOpcode         { get; private set; }
    public static int PositionUpdateInstanceOpcode { get; private set; }
    public static int TreasureOpenOpcode           { get; private set; }
    public static int HeartbeatOpcode              { get; private set; }
    public static int UseActionOpcode              { get; private set; }
    public static int UseActionLocationOpcode      { get; private set; }
    public static int MJIInteractOpcode            { get; private set; }
    public static int ExecuteCommandOpcode         { get; private set; }
    public static int CharaCardOpenOpcode          { get; private set; }
    public static int HandOverItemOpcode           { get; private set; }

    private static readonly CompSig EventStartOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 C7 44 24 ?? ?? ?? ?? ?? 89 5C 24 ?? 0F 85");
    
    private static readonly CompSig EventActionOpcodeBaseSig =
        new("E8 ?? ?? ?? ?? EB 3A 40 84 FF");

    private static readonly CompSig EventCompleteOpcodeBaseSig =
        new("E8 ?? ?? ?? ?? EB 10 48 8B 0D ?? ?? ?? ??");

    private static readonly CompSig DiveStartOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 C7 44 24 ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? B0");

    private static readonly CompSig PositionUpdateBaseSig = 
        new("C7 44 24 ?? ?? ?? ?? ?? F7 D9");
    
    private static readonly CompSig PositionUpdateInstanceBaseSig = 
        new("C7 44 24 ?? ?? ?? ?? ?? 48 8D 54 24 ?? 48 C7 44 24 ?? ?? ?? ?? ?? 0F 11 44 24");

    private static readonly CompSig TreasureOpenOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 C7 44 24 ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 ?? 45 33 C9");

    private static readonly CompSig HeartbeatOpcodeBaseSig = 
        new("C7 44 24 ?? ?? ?? ?? ?? 48 F7 F1");

    private static readonly CompSig UseActionOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 45 33 C0 48 C7 44 24 ?? ?? ?? ?? ?? 89 7C 24");
    
    private static readonly CompSig UseActionLocationOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 C7 44 24 ?? ?? ?? ?? ?? 89 74 24 ?? 40 88 6C 24");
    
    private static readonly CompSig MJIInteractOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 C7 44 24 ?? ?? ?? ?? ?? 0F 11 44 24 ?? E8 ?? ?? ?? ?? 48 8B AC 24");
    
    private static readonly CompSig ExecuteCommandOpcodeBaseSig =
        new("E8 ?? ?? ?? ?? FE 43 18");

    private static readonly CompSig CharaCardOpenOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 45 33 C0 48 C7 44 24 ?? ?? ?? ?? ?? C7 44 24 ?? ?? ?? ?? ?? C6 44 24 ?? ?? E8 ?? ?? ?? ?? 48 8B 8C 24 ?? ?? ?? ?? 48 33 CC E8 ?? ?? ?? ?? 48 81 C4 ?? ?? ?? ?? C3");

    private static readonly CompSig HandOverItemOpcodeBaseSig =
        new("E8 ?? ?? ?? ?? EB 3A 40 84 FF");

    static GamePacketOpcodes()
    {
        EventStartOpcode             = ReadOpcode("EventStart",             EventStartOpcodeBaseSig,        0x4);
        EventActionOpcode            = ReadOpcode("EventAction",            EventActionOpcodeBaseSig,       0x9E);
        EventCompleteOpcode          = ReadOpcode("EventComplete",          EventCompleteOpcodeBaseSig,     0x117);
        DiveStartOpcode              = ReadOpcode("DiveStart",              DiveStartOpcodeBaseSig,         0x4);
        PositionUpdateInstanceOpcode = ReadOpcode("PositionUpdateInstance", PositionUpdateInstanceBaseSig,  0x4);
        TreasureOpenOpcode           = ReadOpcode("TreasureOpen",           TreasureOpenOpcodeBaseSig,      0x4);
        HeartbeatOpcode              = ReadOpcode("Heartbeat",              HeartbeatOpcodeBaseSig,         0x4);
        UseActionOpcode              = ReadOpcode("UseAction",              UseActionOpcodeBaseSig,         0x4);
        UseActionLocationOpcode      = ReadOpcode("UseActionLocation",      UseActionLocationOpcodeBaseSig, 0x4);
        MJIInteractOpcode            = ReadOpcode("MJIInteract",            MJIInteractOpcodeBaseSig,       0x4);
        ExecuteCommandOpcode         = ReadOpcode("ExecuteCommand",         ExecuteCommandOpcodeBaseSig,    0x6D);
        CharaCardOpenOpcode          = ReadOpcode("CharaCardOpen",          CharaCardOpenOpcodeBaseSig,     0x4);
        HandOverItemOpcode           = ReadOpcode("HandOverItem",           HandOverItemOpcodeBaseSig,      0xBD);
        PositionUpdateOpcode         = ReadOpcode("PositionUpdate",         PositionUpdateBaseSig,          0x4);
    }

    private static int ReadOpcode(string name, CompSig sig, int offset)
    {
        try
        {
            return Marshal.ReadInt32(sig.ScanText() + offset);
        }
        catch (Exception ex)
        {
            Error($"尝试读取 OPCODE {name} 时发生错误", ex);
            Chat($"尝试读取 OPCODE {name} 发生错误, 请在 Discord 内上报\n" +
                 $"Error while reading OPCODE {name}. Please report on Discord.");
            throw;
        }
    }
    
    private static int ReadOpcodes(string name, params (CompSig sig, int offset)[] data)
    {
        Exception? exception = null;
        
        foreach (var (sig, offset) in data)
        {
            try
            {
                return Marshal.ReadInt32(sig.ScanText() + offset);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }
        
        Error($"尝试读取 OPCODE {name} 时发生错误", exception);
        Chat($"尝试读取 OPCODE {name} 发生错误, 请在 Discord 内上报\n" +
             $"Error while reading OPCODE {name}. Please report on Discord.");
        throw exception;
    }
}
