using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct PlayerController
{
    [FieldOffset(16)]
    public PlayerMoveControllerWalk MoveControllerWalk;

    [FieldOffset(336)]
    public PlayerMoveControllerFly MoveControllerFly;

    /// <summary>
    /// 在新跟随开始时, 这里会被设置
    /// </summary>
    [FieldOffset(1072)]
    public GameObjectId FollowTargetIDStart;

    /// <summary>
    /// 开始跟随的时候这里必定为4
    /// </summary>
    [FieldOffset(1409)]
    public byte FollowState;

    [FieldOffset(1072)]
    public GameObjectId UnknownObjectID1072;

    [FieldOffset(1168)]
    public GameObjectId FollowTargetID;

    [FieldOffset(1176)]
    public GameObjectId UnknownObjectID1176;

    [FieldOffset(1189)]
    public bool IsFollowing;

    [FieldOffset(1208)]
    public GameObjectId UnknownObjectID1208;

    [FieldOffset(1377)]
    public byte ControlMode;

    private static readonly CompSig InstanceSig =
        new("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? 32 C0");
    private static PlayerController* instance;
    
    public static PlayerController* Instance()
    {
        if (instance == null) 
            instance = InstanceSig.GetStatic<PlayerController>();
        
        return instance;
    }
    
    [FieldOffset(1408)]
    private ushort moveState;

    /// <remarks>
    /// 0 - 无 (刚刚切换区域等) <br/>
    /// 1 - 正常行走 <br/>
    /// 3 - 自动前进 <br/>
    /// </remarks>
    public byte MoveState
    {
        get => (byte)(moveState >> 8);
        set => moveState = (ushort)((moveState & 0x00FF) | (value << 8));
    }
}

[StructLayout(LayoutKind.Explicit, Size = 320)]
public unsafe struct PlayerMoveControllerWalk
{
    [FieldOffset(16)]
    public Vector3 MovementDir;

    [FieldOffset(60)]
    public bool IsMoving;
    
    [FieldOffset(68)]
    public float CurrentSpeed;
    
    [FieldOffset(64)]
    public bool IsJumping;

    [FieldOffset(88)]
    public float BaseMovementSpeed;

    [FieldOffset(136)]
    public float Unknown136;
    
    [FieldOffset(140)]
    public float Unknown140;
    
    [FieldOffset(144)]
    public float MovementDirRelToCharacterFacing;

    [FieldOffset(148)]
    public byte Forced;

    [FieldOffset(160)]
    public Vector3 MovementDirWorld;

    [FieldOffset(176)]
    public float RotationDir;

    [FieldOffset(272)]
    public uint MovementState;

    [FieldOffset(276)]
    public float MovementLeft;

    [FieldOffset(280)]
    public float MovementFwd;

    [FieldOffset(292)]
    public bool IsMovementLocked;

    private delegate float GetTargetSpeedPtrDelegate(PlayerMoveControllerWalk* controller);
    private static readonly GetTargetSpeedPtrDelegate GetTargetSpeedPtr =
        new CompSig("E8 ?? ?? ?? ?? 0F 28 F0 E9 ?? ?? ?? ?? 8B 81").GetDelegate<GetTargetSpeedPtrDelegate>();
    
    public float GetTargetSpeed()
    {
        fixed (PlayerMoveControllerWalk* controller = &this)
            return GetTargetSpeedPtr(controller);
    }
    
    private delegate bool IsAbleToSetMisdirectionStateDelegate(PlayerMoveControllerWalk* controller);
    private static readonly IsAbleToSetMisdirectionStateDelegate IsAbleToSetMisdirectionStatePtr =
        new CompSig("40 53 48 83 EC 20 48 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 75 78").GetDelegate<IsAbleToSetMisdirectionStateDelegate>();
    
    public bool IsAbleToSetMisdirectionState()
    {
        fixed (PlayerMoveControllerWalk* controller = &this)
            return IsAbleToSetMisdirectionStatePtr(controller);
    }

    private static readonly CompSig           InstanceSig = new("48 8D 0D ?? ?? ?? ?? 45 0F 28 D4");
    private static          PlayerMoveControllerWalk* instance;
    
    public static PlayerMoveControllerWalk* Instance()
    {
        if (instance == null) 
            instance = InstanceSig.GetStatic<PlayerMoveControllerWalk>();
        
        return instance;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0xB0)]
public struct PlayerMoveControllerFly
{
    [FieldOffset(0x10)]
    public float unk10; // x coord?

    [FieldOffset(0x14)]
    public float unk14; // y coord?

    [FieldOffset(0x18)]
    public float unk18; // z coord?

    [FieldOffset(0x40)]
    public float unk40;

    [FieldOffset(0x44)]
    public float unk44;

    [FieldOffset(0x48)]
    public uint unk48;

    [FieldOffset(76)]
    public uint unk4C;

    [FieldOffset(0x50)]
    public uint unk50;

    [FieldOffset(88)]
    public float unk58;

    [FieldOffset(0x5C)]
    public float unk5C;

    [FieldOffset(102)]
    public byte IsFlying;

    [FieldOffset(0x88)]
    public uint unk88;

    [FieldOffset(140)]
    public uint unk8C;

    [FieldOffset(0x90)]
    public uint unk90;

    [FieldOffset(148)]
    public float unk94; // speed?

    [FieldOffset(0x98)]
    public float unk98;

    [FieldOffset(0x9C)]
    public float AngularAscent;
}


