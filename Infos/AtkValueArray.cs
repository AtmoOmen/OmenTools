using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace OmenTools.Infos;

public unsafe class AtkValueArray : IDisposable
{
    public nint      Address { get; }
    public AtkValue* Pointer { get; }
    public int       Length  { get; }

    public static implicit operator AtkValue*(AtkValueArray arr) => arr.Pointer;
    
    public AtkValueArray(params object[] values)
    {
        Length  = values.Length;
        Address = Marshal.AllocHGlobal(Length * sizeof(AtkValue));
        Pointer = (AtkValue*)Address;

        for (var i = 0; i < Length; i++)
            EncodeValue(i, values[i]);
    }

    public void Dispose()
    {
        for (var i = 0; i < Length; i++)
            if (Pointer[i].Type == ValueType.String)
                Marshal.FreeHGlobal((IntPtr)Pointer[i].String);
        
        Marshal.FreeHGlobal(Address);
    }

    private void EncodeValue(int index, object value)
    {
        switch (value)
        {
            case uint uintValue:
                Pointer[index].SetUInt(uintValue);
                break;
            case int intValue:
                Pointer[index].SetInt(intValue);
                break;
            case float floatValue:
                Pointer[index].SetFloat(floatValue);
                break;
            case bool boolValue:
                Pointer[index].SetBool(boolValue);
                break;
            case string stringValue:
                var stringBytes = Encoding.UTF8.GetBytes(stringValue + '\0');
                var stringAlloc = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, stringAlloc, stringBytes.Length);
                Pointer[index].SetString((byte*)stringAlloc);
                break;
            case AtkValue atkValue:
                Pointer[index] = atkValue;
                break;
            default:
                throw new ArgumentException($"无法将类型 {value.GetType()} 转换为 AtkValue");
        }
    }
}
