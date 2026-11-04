using System.Runtime.InteropServices;

namespace OmenTools.Interop.Windows.Models;

/// <summary>
///     Windows TCP 连接的网络层统计信息。
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 88)]
public partial struct TCPInfo
{
    /// <summary>
    ///     当前连接已建立的时长，单位为毫秒。
    /// </summary>
    [FieldOffset(8)]
    public ulong ConnectionTime;

    /// <summary>
    ///     平滑往返耗时，单位为微秒。
    /// </summary>
    [FieldOffset(20)]
    public uint RoundTripTime;

    /// <summary>
    ///     当前连接累计发送的字节数。
    /// </summary>
    [FieldOffset(48)]
    public ulong BytesSent;

    /// <summary>
    ///     当前连接累计重传的字节数，按无符号 32 位计数器回绕。
    /// </summary>
    [FieldOffset(68)]
    public uint BytesRetransmitted;

    /// <summary>
    ///     查询已有套接字的 TCP 统计信息，返回 Winsock 错误码，成功时返回零。
    /// </summary>
    public static unsafe int Query(nuint socket, out TCPInfo info)
    {
        uint version = 0;
        var  result  = WSAIoctl(socket, SIO_TCP_INFO, in version, sizeof(uint), out info, (uint)sizeof(TCPInfo), out _, 0, 0);
        return result == 0 ?
                   0 :
                   WSAGetLastError();
    }

    [LibraryImport(WINSOCK)]
    private static partial int WSAIoctl
    (
        nuint       socket,
        uint        controlCode,
        in uint     input,
        uint        inputSize,
        out TCPInfo output,
        uint        outputSize,
        out uint    bytesReturned,
        nint        overlapped,
        nint        completionRoutine
    );

    [LibraryImport(WINSOCK)]
    private static partial int WSAGetLastError();

    #region 常量

    private const string WINSOCK      = "ws2_32.dll";
    private const uint   SIO_TCP_INFO = 0xD8000027;

    #endregion
}
