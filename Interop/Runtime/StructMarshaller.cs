using System.Runtime.InteropServices;

namespace OmenTools.Interop.Runtime;

public static class StructMarshaller
{
    public static T FromBytes<T>(byte[] bytes) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        if (bytes.Length < size)
            throw new ArgumentException($"字节长度不足以读取 {typeof(T).Name}。", nameof(bytes));

        var buffer = Marshal.AllocHGlobal(size);

        try
        {
            Marshal.Copy(bytes, 0, buffer, size);
            return Marshal.PtrToStructure<T>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public static byte[] ToBytes<T>(T value) where T : struct
    {
        var size   = Marshal.SizeOf<T>();
        var buffer = Marshal.AllocHGlobal(size);

        try
        {
            Marshal.StructureToPtr(value, buffer, false);
            var bytes = new byte[size];
            Marshal.Copy(buffer, bytes, 0, size);
            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
