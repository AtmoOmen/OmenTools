using ImGuiNET;
using System.Runtime.InteropServices;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool InputUInt(string label, ref uint value) 
        => InputUIntInternal(label, ref value, null, null, null, ImGuiInputTextFlags.None);

    public static bool InputUInt(string label, ref uint value, uint step) 
        => InputUIntInternal(label, ref value, step, null, null, ImGuiInputTextFlags.None);

    public static bool InputUInt(string label, ref uint value, uint step, uint step_fast) 
        => InputUIntInternal(label, ref value, step, step_fast, null, ImGuiInputTextFlags.None);

    public static bool InputUInt(string label, ref uint value, uint step, uint step_fast, ImGuiInputTextFlags flags) 
        => InputUIntInternal(label, ref value, step, step_fast, null, flags);

    private static bool InputUIntInternal(string label, ref uint value, uint? step, uint? step_fast, string? format, ImGuiInputTextFlags flags)
    {
        var valuePtr = Marshal.AllocHGlobal(sizeof(uint));
        try
        {
            Marshal.WriteInt32(valuePtr, unchecked((int)value));

            var stepPtr = nint.Zero;
            var stepFastPtr = nint.Zero;

            if (step.HasValue)
            {
                stepPtr = Marshal.AllocHGlobal(sizeof(uint));
                Marshal.WriteInt32(stepPtr, unchecked((int)step.Value));
            }

            if (step_fast.HasValue)
            {
                stepFastPtr = Marshal.AllocHGlobal(sizeof(uint));
                Marshal.WriteInt32(stepFastPtr, unchecked((int)step_fast.Value));
            }

            try
            {
                var result = ImGui.InputScalar(label, ImGuiDataType.U32, valuePtr, stepPtr, stepFastPtr, format, flags);
                if (result)
                {
                    value = unchecked((uint)Marshal.ReadInt32(valuePtr));
                }
                return result;
            }
            finally
            {
                if (stepPtr != IntPtr.Zero) Marshal.FreeHGlobal(stepPtr);
                if (stepFastPtr != IntPtr.Zero) Marshal.FreeHGlobal(stepFastPtr);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(valuePtr);
        }
    }
}