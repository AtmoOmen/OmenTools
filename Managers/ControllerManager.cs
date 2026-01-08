using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public unsafe class ControllerManager : OmenServiceBase<ControllerManager>
{
    /// <summary>
    ///     使用 RGB 值设置光条颜色。
    /// </summary>
    /// <param name="rgb">包含 R、G、B 值的 Vector3 (0.0 到 1.0)</param>
    public void SetColor(Vector3 rgb) => 
        Color = rgb;

    /// <summary>
    ///     设置左右电机的振动强度。
    /// </summary>
    /// <param name="motor">包含左右电机强度的 Vector2 (0.0 到 1.0)</param>
    public void SetMotor(Vector2 motor) => 
        MotorCustom = motor;

    /// <summary>
    ///     启动电机强度值队列，按顺序处理 (频率为 0.5 秒)。
    /// </summary>
    /// <param name="motors">包含左右电机强度的 Vector2 列表 (0.0 到 1.0)</param>
    public void StartMotorQueue(List<Vector2> motors)
    {
        MotorQueue.Clear();
        foreach (var motor in motors)
            MotorQueue.Enqueue(motor);
    }

    /// <summary>
    ///     中止当前电机队列，停止所有正在进行的振动效果。
    /// </summary>
    public void AbortMotorQueue() => 
        MotorQueue.Clear();
    
    
    private readonly CompSig                 WriteHidSig = new("E8 ?? ?? ?? ?? 8B 4B 1C 8B F8");
    private delegate byte                    WriteHIDDelegate(int device, void* report, ushort length);
    private          Hook<WriteHIDDelegate>? WriteHidHook;

    private readonly Queue<Vector2> MotorQueue = [];
    private          Vector3        Color;
    private          Vector2        MotorCustom;
    
    internal override void Init()
    {
        WriteHidHook ??= WriteHidSig.GetHook<WriteHIDDelegate>(WriteHIDDetour);
        WriteHidHook.Enable();
    }

    internal override void Uninit()
    {
        WriteHidHook?.Dispose();
        WriteHidHook = null;
    }

    private byte WriteHIDDetour(int device, void* reportPtr, ushort length)
    {
        if (length != 0x2F + 0x01)
            return WriteHidHook.Original(device, reportPtr, length);

        var report = (Report*)reportPtr;

        // color
        if (Color != Vector3.Zero)
        {
            report->Detail.Flag1               = (byte)ControlFlag1.LightbarControlEnable;
            report->Detail.LightBarColourRed   = (byte)Color.X;
            report->Detail.LightBarColourGreen = (byte)Color.Y;
            report->Detail.LightBarColourBlue  = (byte)Color.Z;
        }

        // motor
        if (MotorQueue.Count > 0)
        {
            report->Detail.Flag0 = (byte)ControlFlag0.HapticsSelect;
            report->Detail.Flag2 = (byte)ControlFlag2.CompatibleVibration2;
            var next = MotorQueue.Dequeue();
            report->Detail.MotorLeft  = (byte)(next.X * 255);
            report->Detail.MotorRight = (byte)(next.Y * 255);
        }
        if (MotorCustom != Vector2.Zero)
        {
            report->Detail.Flag0      = (byte)ControlFlag0.HapticsSelect;
            report->Detail.Flag2      = (byte)ControlFlag2.CompatibleVibration2;
            report->Detail.MotorLeft  = (byte)(MotorCustom.X * 255);
            report->Detail.MotorRight = (byte)(MotorCustom.Y * 255);
        }

        // original
        return WriteHidHook.Original(device, report, length);
    }
    
    public class Presets
    {
        /// <summary>
        ///     在指定持续时间内创建脉冲膨胀效果。
        /// </summary>
        /// <param name="duration">达到最大强度所需的秒数</param>
        /// <param name="maxIntensity">0.0 到 1.0</param>
        /// <returns>随时间变化的电机强度值列表</returns>
        public List<Vector2> Pulsing(float duration, float maxIntensity = 1.0f)
        {
            var list = new List<Vector2>();
            if (duration <= 0)
                return list;

            var steps = (int)(duration / 0.5f);
            maxIntensity = Math.Clamp(maxIntensity, 0.0f, 1.0f);

            for (var i = 0; i <= steps; i++)
            {
                var t        = i * 0.5f;
                var progress = duration > 0 ? t / duration : 0;

                var envelope    = 4.0f * (progress - progress * progress);
                var oscillation = MathF.Abs(MathF.Sin(t * MathF.PI));

                var finalStrength = maxIntensity * envelope * oscillation;
                var leftMotor     = 0.6f * finalStrength;
                var rightMotor    = 0.6f * finalStrength;

                list.Add(new Vector2(leftMotor, rightMotor));
            }
            return list;
        }

        /// <summary>
        ///     在指定持续时间内将振动强度从 0 逐渐增加到最大强度。
        /// </summary>
        /// <param name="duration">达到最大强度所需的秒数</param>
        /// <param name="maxIntensity">0.0 到 1.0</param>
        /// <param name="power">进度提升的幂次，影响增加曲线的形状 (默认为 2.0，表示二次曲线)</param>
        /// <returns>随时间变化的电机强度值列表</returns>
        public List<Vector2> SmoothUp(float duration, float maxIntensity = 1.0f, float power = 2.0f)
        {
            var list = new List<Vector2>();
            if (duration <= 0)
                return list;

            var steps = (int)(duration / 0.5f);
            maxIntensity = Math.Clamp(maxIntensity, 0.0f, 1.0f);

            for (var i = 0; i <= steps; i++)
            {
                var t        = i * 0.5f;
                var progress = duration > 0 ? t / duration : 0;

                var finalStrength = maxIntensity * MathF.Pow(progress, power);
                var leftMotor     = 0.8f * finalStrength;
                var rightMotor    = 0.6f * finalStrength;

                list.Add(new Vector2(leftMotor, rightMotor));
            }
            return list;
        }
    }

    #region Models

    [StructLayout(LayoutKind.Explicit, Size = 0x2F + 0x01)]
    private struct Report
    {
        [FieldOffset(0x00)]
        public byte ID;

        [FieldOffset(0x01)]
        public Context Detail;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x2F)]
    private struct Context
    {
        [FieldOffset(0x00)]
        public byte Flag0;

        [FieldOffset(0x01)]
        public byte Flag1;

        [FieldOffset(0x02)]
        public byte MotorRight;

        [FieldOffset(0x03)]
        public byte MotorLeft;

        [FieldOffset(0x04)]
        public fixed byte Reserved[4];

        [FieldOffset(0x08)]
        public byte MicButtonLed;

        [FieldOffset(0x09)]
        public byte PowerSaveControl;

        [FieldOffset(0x0A)]
        public fixed byte TriggerR2[10];

        [FieldOffset(0x14)]
        public byte Unk;

        [FieldOffset(0x15)]
        public fixed byte TriggerL2[10];

        [FieldOffset(0x1E)]
        public fixed byte Unk2[8];

        [FieldOffset(0x26)]
        public byte Flag2;

        [FieldOffset(0x27)]
        public fixed byte Reserved3[2];

        [FieldOffset(0x29)]
        public byte LightBarSetup;

        [FieldOffset(0x2A)]
        public byte LedBrightness;

        [FieldOffset(0x2B)]
        public byte PlayerLeds;

        [FieldOffset(0x2C)]
        public byte LightBarColourRed;

        [FieldOffset(0x2D)]
        public byte LightBarColourGreen;

        [FieldOffset(0x2E)]
        public byte LightBarColourBlue;
    }

    [Flags]
    private enum ControlFlag0 : byte
    {
        None                   = 0,
        CompatibleVibration    = 1 << 0,
        HapticsSelect          = 1 << 1,
        AdapterTriggerR2Select = 1 << 2,
        AdapterTriggerL2Select = 1 << 3
    }

    [Flags]
    private enum ControlFlag1 : byte
    {
        None                         = 0,
        MicMuteLedControlEnable      = 1 << 0,
        PowerSaveControlEnable       = 1 << 1,
        LightbarControlEnable        = 1 << 2,
        ReleaseLeds                  = 1 << 3,
        PlayerIndicatorControlEnable = 1 << 4
    }

    [Flags]
    private enum ControlFlag2 : byte
    {
        None                       = 0,
        LightbarSetupControlEnable = 1 << 1,
        CompatibleVibration2       = 1 << 2
    }

    #endregion
}
