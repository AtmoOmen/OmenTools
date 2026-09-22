using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Data;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Interop.Game.Helpers;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Interop.Game.Models.Native;
using OmenTools.OmenService.Abstractions;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Threading.TaskHelper;
using OmenTools.Threading.TaskHelper.Enums;
using Action = System.Action;

namespace OmenTools.OmenService;

public partial class MovementManager : OmenServiceBase<MovementManager>
{
    public delegate bool TPSmartInZoneDelegate
    (
        Vector3         pos,
        ITPSmartParams? param = null
    );
    public TPSmartInZoneDelegate? TPSmartInZoneHandler { get; set; }

    public delegate bool TPSmartBetweenZoneDelegate
    (
        uint            zone,
        Vector3?        pos,
        ITPSmartParams? param = null
    );
    public TPSmartBetweenZoneDelegate? TPSmartBetweenZoneHandler { get; set; }

    public Func<bool>? IsBusyHandler     { get; set; }
    public Action?     AbortTasksHandler { get; set; }

    public TaskHelper? TaskHelper { get; set; }

    public List<EventTransportBase> EventTransportations { get; private set; } = [];

    public bool AlwaysMinGilTransport
    {
        get => config.AlwaysMinGilTransport;
        set => config.AlwaysMinGilTransport = value;
    }

    public bool IsManagerBusy =>
        TaskHelper?.IsBusy != false || (IsBusyHandler?.Invoke() ?? false);

    public void AbortTasks() =>
        AbortTasksHandler?.Invoke();

    public void SaveConfig() =>
        config.Save();

    #region 瞬移

    /// <summary>
    ///     区域内瞬移, 根据区域以及目的地性质智能选择应对方式
    /// </summary>
    public bool TPSmart_InZone
    (
        Vector3         pos,
        ITPSmartParams? param = null
    ) =>
        TPSmartInZoneHandler is { } handler && handler(pos, param);

    /// <summary>
    ///     区域间瞬移, 根据区域以及目的地性质智能选择应对方式
    /// </summary>
    public bool TPSmart_BetweenZone
    (
        uint            zone,
        Vector3?        pos   = null,
        ITPSmartParams? param = null
    ) =>
        TPSmartBetweenZoneHandler is { } handler && handler(zone, pos, param);

    /// <summary>
    ///     通过写入玩家位置内存的方式瞬移
    /// </summary>
    public unsafe bool TPPlayerAddress
    (
        Vector3 pos
    )
    {
        if (LocalPlayerState.Object is not { } localPlayer)
            return false;

        if (ICondition.Instance().Any(ConditionFlag.InFlight, ConditionFlag.Diving))
            PlayerController.Instance()->MoveControllerFly.MountPosition = pos;
        else
            localPlayer.ToStruct()->SetPosition(pos.X, pos.Y, pos.Z);

        return LocalPlayerState.DistanceTo2DSquared(pos.ToVector2()) <= 9;
    }

    /// <summary>
    ///     若上下存在可用地面, 则瞬移至地面
    /// </summary>
    public bool TPGround()
    {
        if (LocalPlayerState.Object is not { } localPlayer ||
            !RaycastHelper.TryGetNearestVerticalHit(localPlayer.Position, out var hitInfo))
            return false;

        TPSmart_InZone(hitInfo.Point);
        return true;
    }

    /// <summary>
    ///     通过传送的方式进行瞬移
    /// </summary>
    /// <remarks>
    ///     不适用: 区域内没有以太之光
    /// </remarks>
    public unsafe bool TPTeleport
    (
        Vector3 pos
    )
    {
        if (AetheryteRecordManager.Instance()
                                  .GetNearestAetheryte(GameState.TerritoryType, pos)
            is not { } aetheryte)
            return false;
        if (LocalPlayerState.Object == null)
            return false;

        if (!UIModule.IsScreenReady())
            return TPPlayerAddress(pos);

        TaskHelper.Enqueue
        (
            () => Telepo.Instance()->Teleport(aetheryte.RowID, aetheryte.SubIndex),
            "读条进行传送",
            weight: 1000
        );
        TaskHelper.Enqueue
        (
            () => GameState.TerritoryType == aetheryte.ZoneID &&
                  LocalPlayerState.Object != null             &&
                  !UIModule.IsScreenReady(),
            "等待进入传送状态",
            weight: 1000,
            timeoutMS: 8_000,
            timeoutBehaviour: TaskAbortBehaviour.AbortAll
        );
        TaskHelper.Enqueue
        (
            () =>
            {
                if (UIModule.IsScreenReady())
                    return true;

                TPPlayerAddress(pos);
                return false;
            },
            "进行瞬移",
            weight: 1000
        );

        return true;
    }

    #endregion

    #region 平滑移动

    /// <summary>
    ///     平滑移动至目标地点, 很大概率可能需要拦截移动包
    /// </summary>
    public Task TPSmoothAsync
    (
        Vector3            targetPosition,
        float              speed,
        CancellationToken? cancellationToken
    )
    {
        if (IObjectTable.Instance().LocalPlayer is not { } localPlayer)
            return Task.CompletedTask;

        cancellationToken ??= CancellationToken.None;
        return IFramework.Instance().Run
        (async () =>
            {
                try
                {
                    unsafe
                    {
                        PlayerController.Instance()->MoveControllerWalk.IsMovementInputLocked = true;
                    }

                    while (localPlayer != null && cancellationToken?.IsCancellationRequested == false)
                    {
                        // 禁止自动前进
                        unsafe
                        {
                            if (PlayerController.Instance()->MoveState == 3)
                                PlayerController.Instance()->MoveState = 1;
                        }

                        var nextPosition = GetTPSmoothNextPosition
                        (
                            localPlayer,
                            targetPosition,
                            speed,
                            out var isArrived
                        );
                        ExecuteTPSmoothPosition(localPlayer, nextPosition);
                        if (isArrived) break;

                        await IFramework.Instance().DelayTicks(1);
                    }
                }
                finally
                {
                    unsafe
                    {
                        PlayerController.Instance()->MoveControllerWalk.IsMovementInputLocked = false;
                    }
                }
            }
        );
    }

    /// <summary>
    ///     平滑移动至目标地点, 需要每帧调用, 很大概率可能需要拦截移动包
    /// </summary>
    public unsafe bool TPSmooth
    (
        Vector3 targetPosition,
        float   speed
    )
    {
        if (IObjectTable.Instance().LocalPlayer is not { } localPlayer)
            return false;

        // 禁止自动前进
        if (PlayerController.Instance()->MoveState == 3)
            PlayerController.Instance()->MoveState = 1;

        var nextPosition = GetTPSmoothNextPosition
        (
            localPlayer,
            targetPosition,
            speed,
            out var isArrived
        );
        if (isArrived)
            return true;

        ExecuteTPSmoothPosition(localPlayer, nextPosition);
        return true;
    }

    #endregion

    /// <summary>
    ///     下坐骑
    /// </summary>
    public void Dismount
    (
        bool restoreControlMode = true
    )
    {
        if (IObjectTable.Instance().LocalPlayer is not { } localPlayer) return;
        if (!ICondition.Instance()[ConditionFlag.Mounted]) return;

        var controlMode = CurrentControlMode;
        var rot         = localPlayer.Rotation;

        if (ICondition.Instance()[ConditionFlag.InFlight])
        {
            if (!RaycastHelper.TryGetGroundPosition(localPlayer.Position, out var groundPosition)) return;

            TPPlayerAddress(groundPosition);

            if (GameState.ContentFinderCondition == 0)
                new PositionUpdatePacket(rot, groundPosition, GetMoveType(PositionUpdatePacket.MoveType.Fly0)).Send();
            else
                new PositionUpdateInstancePacket(rot, groundPosition, GetInstanceMoveType(PositionUpdateInstancePacket.MoveType.Fly0)).Send();

            ExecuteCommandManager.Instance()
                                 .ExecuteCommandComplexLocation(ExecuteCommandComplexFlag.Dismount, groundPosition, RotationHelper.CharaToPacket(rot), 1);
        }
        else
            ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Dismount);

        if (restoreControlMode)
        {
            TaskHelper.Enqueue
            (
                () => !ICondition.Instance().Any(ConditionFlag.Mounted, ConditionFlag.Jumping),
                weight: 1000
            );
            TaskHelper.Enqueue
            (
                () => SetCurrentControlMode(controlMode),
                weight: 1000
            );
        }
    }

    #region 工具

    /// <summary>
    ///     获取副本外位置更新包移动类型
    /// </summary>
    public PositionUpdatePacket.MoveType GetMoveType
    (
        PositionUpdatePacket.MoveType moveType
    ) =>
        (PositionUpdatePacket.MoveType)((uint)moveType + ((uint)CurrentZoneMoveState << 16));

    /// <summary>
    ///     获取副本内位置更新包移动类型
    /// </summary>
    public PositionUpdateInstancePacket.MoveType GetInstanceMoveType
    (
        PositionUpdateInstancePacket.MoveType moveType
    ) =>
        (PositionUpdateInstancePacket.MoveType)((uint)moveType + ((uint)CurrentZoneMoveState << 16));

    public static unsafe bool TryGetExitRangeInstance
    (
        out ExitRangeLayoutInstance* instance,
        Predicate<nint>?             predicate = null
    )
    {
        instance = null;

        if (!LayoutWorld.Instance()->ActiveLayout->InstancesByType.TryGetValuePointer
                (InstanceType.ExitRange, out var exitRanges))
            return false;

        foreach (var exitRange in exitRanges->Value->Values)
        {
            if (exitRange.IsNull)
                continue;

            var pExitRange = (ExitRangeLayoutInstance*)exitRange.Value;
            if (pExitRange->ReturnInstance == null ||
                !LuminaGetter.TryGetRow<TerritoryType>(pExitRange->TerritoryType, out _))
                continue;

            if (predicate != null && !predicate((nint)pExitRange))
                continue;

            instance = pExitRange;
            return true;
        }

        return false;
    }

    /// <summary>
    ///     是否仅 Y 轴(纵轴) 方向上发生了变更
    /// </summary>
    public static bool IsOnlyYAxisChanged
    (
        Vector3 orig,
        Vector3 after,
        float   xzTolerance = 2f,
        float   yTolerance  = 0.0001f
    ) =>
        Math.Abs(orig.X - after.X) < xzTolerance &&
        Math.Abs(orig.Z - after.Z) < xzTolerance &&
        Math.Abs(orig.Y - after.Y) >= yTolerance;

    /// <summary>
    ///     是否需要变更环境 (潜水)
    /// </summary>
    public static bool? IsNeedEnvironmentChange
    (
        Vector3 pos0,
        Vector3 pos1,
        uint    zone = 0
    )
    {
        if (zone == 0)
            zone = GameState.TerritoryType;
        if (zone == 0) return null;

        if (!Positions.DivableAreas.TryGetValue(zone, out var divablePolygons)) return false;

        var isYAxisAcrossZero = (pos0.Y > 0 && pos1.Y < 0) || (pos0.Y < 0 && pos1.Y > 0);
        if (!isYAxisAcrossZero) return false;

        var isPos0Divable = divablePolygons.Any(x => x.IsPointInPolygon(pos0.ToVector2()));
        var isPos1Divable = divablePolygons.Any(x => x.IsPointInPolygon(pos1.ToVector2()));

        return isPos0Divable || isPos1Divable;
    }

    /// <summary>
    ///     设定当前的移动模式
    /// </summary>
    public unsafe void SetCurrentControlMode
    (
        MovementControlMode mode
    )
    {
        if (CurrentMoveModeInternal == null) return;

        Marshal.WriteByte((nint)CurrentMoveModeInternal, (byte)mode);
    }

    /// <summary>
    ///     当前移动控制方式
    /// </summary>
    public unsafe MovementControlMode CurrentControlMode
    {
        get => (MovementControlMode)(*CurrentMoveModeInternal);
        set => *CurrentMoveModeInternal = (byte)value;
    }

    /// <summary>
    ///     当前区域内移动状态, 用于移动发包的 MoveType
    /// </summary>
    public unsafe ushort CurrentZoneMoveState =>
        (ushort)(GetCurrentZoneMoveState(CurrentZoneMoveStateBaseInstance) % 4);

    #endregion
}
