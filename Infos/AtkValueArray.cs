﻿using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace OmenTools.Infos;

public unsafe class AtkValueArray : IDisposable
{
    public AtkValueArray(params object[] values)
    {
        Length = values.Length;
        Address = Marshal.AllocHGlobal(Length * sizeof(AtkValue));
        Pointer = (AtkValue*)Address;

        for (var i = 0; i < Length; i++)
            EncodeValue(i, values[i]);
    }

    public nint      Address { get; }
    public AtkValue* Pointer { get; }
    public int       Length  { get; }

    public static implicit operator AtkValue*(AtkValueArray arr) => arr.Pointer;

    public void Dispose()
    {
        for (var i = 0; i < Length; i++)
        {
            if (Pointer[i].Type == ValueType.String)
                Marshal.FreeHGlobal((nint)Pointer[i].String.Value);
        }

        Marshal.FreeHGlobal(Address);
    }

    private void EncodeValue(int index, object value)
    {
        switch (value)
        {
            case uint uintValue:
                Pointer[index] = new AtkValue { Type = ValueType.UInt, UInt = uintValue };
                break;
            case int intValue:
                Pointer[index] = new AtkValue { Type = ValueType.Int, Int = intValue };
                break;
            case float floatValue:
                Pointer[index] = new AtkValue { Type = ValueType.Float, Float = floatValue };
                break;
            case bool boolValue:
                Pointer[index] = new AtkValue { Type = ValueType.Bool, Byte = Convert.ToByte(boolValue) };
                break;
            case string stringValue:
                var stringBytes = Encoding.UTF8.GetBytes(stringValue + '\0');
                var stringAlloc = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, stringAlloc, stringBytes.Length);
                Pointer[index] = new AtkValue { Type = ValueType.String, String = (byte*)stringAlloc };
                break;
            case AtkValue atkValue:
                Pointer[index] = atkValue;
                break;
            default:
                throw new ArgumentException($"Unable to convert type {value.GetType()} to AtkValue");
        }
    }
}
