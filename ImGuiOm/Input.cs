namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    #region UInt

    public static bool InputUInt(string label, ref uint value) 
        => InputScalarInternal(label, ImGuiDataType.U32, ref value, 0U, 0U, flags: ImGuiInputTextFlags.None);

    public static bool InputUInt(string label, ref uint value, uint step) 
        => InputScalarInternal(label, ImGuiDataType.U32, ref value, step, 0U);

    public static bool InputUInt(string label, ref uint value, uint step, uint step_fast) 
        => InputScalarInternal(label, ImGuiDataType.U32, ref value, step, step_fast);

    public static bool InputUInt(string label, ref uint value, uint step, uint step_fast, ImGuiInputTextFlags flags) 
        => InputScalarInternal(label, ImGuiDataType.U32, ref value, step, step_fast, flags: flags);

    #endregion

    #region Byte

    public static bool InputByte(string label, ref byte value)
        => InputScalarInternal(label, ImGuiDataType.U8, ref value);

    public static bool InputByte(string label, ref byte value, byte step)
        => InputScalarInternal(label, ImGuiDataType.U8, ref value, step);

    public static bool InputByte(string label, ref byte value, byte step, byte step_fast)
        => InputScalarInternal(label, ImGuiDataType.U8, ref value, step, step_fast);

    public static bool InputByte(string label, ref byte value, byte step, byte step_fast, ImGuiInputTextFlags flags)
        => InputScalarInternal(label, ImGuiDataType.U8, ref value, step, step_fast, flags: flags);

    #endregion

    #region Short

    public static bool InputShort(string label, ref short value)
        => InputScalarInternal(label, ImGuiDataType.S16, ref value);

    public static bool InputShort(string label, ref short value, short step)
        => InputScalarInternal(label, ImGuiDataType.S16, ref value, step);

    public static bool InputShort(string label, ref short value, short step, short step_fast)
        => InputScalarInternal(label, ImGuiDataType.S16, ref value, step, step_fast);

    public static bool InputShort(string label, ref short value, short step, short step_fast, ImGuiInputTextFlags flags)
        => InputScalarInternal(label, ImGuiDataType.S16, ref value, step, step_fast, flags: flags);

    #endregion

    #region UShort

    public static bool InputUShort(string label, ref ushort value)
        => InputScalarInternal(label, ImGuiDataType.U16, ref value);

    public static bool InputUShort(string label, ref ushort value, ushort step)
        => InputScalarInternal(label, ImGuiDataType.U16, ref value, step);

    public static bool InputUShort(string label, ref ushort value, ushort step, ushort step_fast)
        => InputScalarInternal(label, ImGuiDataType.U16, ref value, step, step_fast);

    public static bool InputUShort(string label, ref ushort value, ushort step, ushort step_fast, ImGuiInputTextFlags flags)
        => InputScalarInternal(label, ImGuiDataType.U16, ref value, step, step_fast, flags: flags);

    #endregion

    private static unsafe bool InputScalarInternal<T>(
        string label, ImGuiDataType dataType, ref T value, T? step = null, T? step_fast = null, string? format = null,
        ImGuiInputTextFlags flags = ImGuiInputTextFlags.None) where T : unmanaged
    {
        var valuePtr = stackalloc T[1];
        *valuePtr = value;

        T* stepPtr     = null;
        T* stepFastPtr = null;

        if (step.HasValue)
        {
            var stepVal = step.Value;
            stepPtr = &stepVal;
        }

        if (step_fast.HasValue)
        {
            var stepFastVal = step_fast.Value;
            stepFastPtr = &stepFastVal;
        }

        var result = ImGui.InputScalar(label, dataType, (nint)valuePtr, (nint)stepPtr, (nint)stepFastPtr, format, flags);

        if (result) value = *valuePtr;
        return result;
    }
}
