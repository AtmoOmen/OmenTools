using System.Reflection;
using System.Runtime.InteropServices;
using Dalamud.Hooking;

namespace OmenTools.Extensions;

public static class ReflectionExtensions
{
    private const BindingFlags ALL_FLAGS    = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private const BindingFlags STATIC_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    
    extension<T>(scoped ref T vtable) where T : unmanaged
    {
        /// <summary>
        /// 直接调用过不了混淆, 所以反射
        /// </summary>
        public unsafe nint GetVFuncByName(string fieldName)
        {
            fixed(T* vtablePtr = &vtable)
            {
                ArgumentNullException.ThrowIfNull(vtablePtr);
                
                var vtType = typeof(T);
                var fi     = vtType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (fi == null)
                    throw new MissingFieldException(vtType.FullName, fieldName);

                var offAttr = fi.GetCustomAttribute<FieldOffsetAttribute>();
                if (offAttr == null)
                    throw new InvalidOperationException($"字段 {fieldName} 无 FieldOffset 属性, 虚函数读取失败");

                var offset = offAttr.Value;

                return *(nint*)((byte*)vtablePtr + offset);
            }
        }
        
        public unsafe Hook<TDetour> HookVFuncFromName<TDetour>(string fieldName, TDetour detour) where TDetour : Delegate
        {
            fixed(T* vtablePtr = &vtable)
            {
                ArgumentNullException.ThrowIfNull(vtablePtr);
                return DService.Instance().Hook.HookFromAddress(vtablePtr->GetVFuncByName(fieldName), detour);
            }
        }
    }
    
    extension(nint luaSetupFunctionStartAddress)
    {
        public nint GetLuaFunctionByName(string functionName, int scanSize = 8192)
        {
            if (luaSetupFunctionStartAddress == nint.Zero || string.IsNullOrEmpty(functionName))
                return nint.Zero;

            var functionBytes = new byte[scanSize];
            try
            {
                Marshal.Copy(luaSetupFunctionStartAddress, functionBytes, 0, scanSize);
            }
            catch
            {
                return nint.Zero;
            }

            // lea r8, [rip + displacement]
            // 4C 8D 05 xx xx xx xx
            byte[] leaStringPattern = [0x4C, 0x8D, 0x05];

            // lea r9, [rip + displacement]
            // 4C 8D 0D xx xx xx xx
            byte[] leaFunctionPattern = [0x4C, 0x8D, 0x0D];

            var stringLeaIndex = -1;

            for (var i = 0; i <= functionBytes.Length - 7; i++)
            {
                if (functionBytes[i]     == leaStringPattern[0] &&
                    functionBytes[i + 1] == leaStringPattern[1] &&
                    functionBytes[i + 2] == leaStringPattern[2])
                {
                    var displacement              = BitConverter.ToInt32(functionBytes, i + 3);
                    var currentInstructionAddress = (long)luaSetupFunctionStartAddress + i;
                    var nextInstructionAddress    = currentInstructionAddress          + 7;
                    var stringAddress             = nextInstructionAddress             + displacement;

                    var referencedString = Marshal.PtrToStringAnsi((nint)stringAddress);
                    if (referencedString == functionName)
                    {
                        stringLeaIndex = i;
                        break;
                    }
                }
            }

            if (stringLeaIndex == -1)
                return nint.Zero;
        
            var searchLimit = Math.Max(0, stringLeaIndex - 100);
            for (var i = stringLeaIndex - 1; i >= searchLimit; i--)
                // lea r9, [rip + ...]
            {
                if (i + 7                < functionBytes.Length   &&
                    functionBytes[i]     == leaFunctionPattern[0] &&
                    functionBytes[i + 1] == leaFunctionPattern[1] &&
                    functionBytes[i + 2] == leaFunctionPattern[2])
                {
                    var displacement              = BitConverter.ToInt32(functionBytes, i + 3);
                    var currentInstructionAddress = (long)luaSetupFunctionStartAddress + i;
                    var nextInstructionAddress    = currentInstructionAddress          + 7;
                    var targetFunctionAddress     = nextInstructionAddress             + displacement;

                    return (nint)targetFunctionAddress;
                }
            }

            return nint.Zero;
        }
    }
    
    extension(object obj)
    {
        public object? GetFieldOrProperty(string name) =>
            obj.GetType().GetField(name, ALL_FLAGS)?.GetValue(obj) ??
            obj.GetType().GetProperty(name, ALL_FLAGS)?.GetValue(obj);

        public T? GetFieldOrProperty<T>(string name) => 
            (T?)obj.GetFieldOrProperty(name);

        public void SetFieldOrProperty(string name, object value)
        {
            var field = obj.GetType().GetField(name, ALL_FLAGS);
            if (field != null)
                field.SetValue(obj, value);
            else
                obj.GetType().GetProperty(name, ALL_FLAGS)?.SetValue(obj, value);
        }

        public object? GetStaticFieldOrProperty(string type, string name) =>
            obj.GetType().Assembly.GetType(type)?.GetField(name, STATIC_FLAGS)?.GetValue(null) ??
            obj.GetType().Assembly.GetType(type)?.GetProperty(name, STATIC_FLAGS)?.GetValue(null);

        public T? GetStaticFieldOrProperty<T>(string type, string name) => 
            (T?)obj.GetStaticFieldOrProperty(type, name);

        public void SetStaticFieldOrProperty(string type, string name, object value)
        {
            var field = obj.GetType().Assembly.GetType(type)?.GetField(name, STATIC_FLAGS);
            if (field != null)
                field.SetValue(null, value);
            else
                obj.GetType().Assembly.GetType(type)?.GetProperty(name, STATIC_FLAGS)?.SetValue(null, value);
        }

        public object? Call(string name, object[] @params, bool matchExactArgumentTypes = false)
        {
            var info = !matchExactArgumentTypes
                           ? obj.GetType().GetMethod(name, ALL_FLAGS)
                           : obj.GetType().GetMethod(name, ALL_FLAGS, @params.Select(x => x.GetType()).ToArray());
            return info?.Invoke(obj, @params);
        }

        public T? Call<T>(string name, object[] @params, bool matchExactArgumentTypes = false) => 
            (T?)obj.Call(name, @params, matchExactArgumentTypes);
    }
}
