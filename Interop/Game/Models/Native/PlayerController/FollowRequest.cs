using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace OmenTools.Interop.Game.Models.Native;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct FollowRequest
{
    /// <summary>
    ///     跟随目标 GameObjectId (0xE0000000 = 无效)
    /// </summary>
    [FieldOffset(0)]
    public GameObjectId TargetGameObjectID;

    /// <summary>
    ///     跟随模式:
    ///     1 = 仅取消跟随
    ///     2 = 地面跟随 (解析目标, 存储 ID)
    ///     3 = 飞行跟随 (存储 IsWalking 状态)
    ///     4 = 标准目标跟随 (解析对象, 复制位置, 显示日志 #52)
    ///     5 = 潜行跟随 (设置标志位)
    /// </summary>
    [FieldOffset(8)]
    public FollowMode Mode;

    /// <summary>
    ///     静默标志: 非零时抑制 "开始跟随" 聊天日志
    /// </summary>
    [FieldOffset(9)]
    public byte SilentFlag;
    
    public enum FollowMode
    {
        Cancel       = 1,
        GroundFollow = 2,
        FlyFollow    = 3,
        Follow       = 4,
        SilentFollow = 5
    }
}
