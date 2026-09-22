namespace OmenTools.OmenService;

public enum MovementControlMode : byte
{
    None     = 0,
    Normal   = 1,
    Combat   = 2,
    AutoMove = 3,
    Follow   = 4,
    Unknown5 = 5
}
