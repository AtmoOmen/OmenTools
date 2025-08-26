using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Sequential)]
public struct LastInputInfo
{
    public uint Size;
    public uint LastInputTickCount;

    [DllImport("User32.dll")]
    public static extern bool GetLastInputInfo(ref LastInputInfo info);

    public static TimeSpan GetIdleTime()
    {
        var lastInputInfo = new LastInputInfo { Size = (uint)Marshal.SizeOf<LastInputInfo>() };

        GetLastInputInfo(ref lastInputInfo);

        return TimeSpan.FromMilliseconds(Environment.TickCount - (int)lastInputInfo.LastInputTickCount);
    }

    public static long GetIdleTimeTick()
    {
        var lastInputInfo = new LastInputInfo { Size = (uint)Marshal.SizeOf<LastInputInfo>() };

        GetLastInputInfo(ref lastInputInfo);

        return Environment.TickCount64 - lastInputInfo.LastInputTickCount;
    }
}
