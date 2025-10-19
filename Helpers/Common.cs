using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Lumina.Excel;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static bool IsPluginEnabled(string internalName, Version? minVersion = null) => 
        DService.PI.InstalledPlugins.Any(x => x.InternalName == internalName && x.IsLoaded && (minVersion == null || x.Version >= minVersion));
    
    public static RowRef<T> LuminaCreateRef<T>(uint rowID) where T : struct, IExcelRow<T> => 
        new(DService.Data.Excel, rowID);
    
    public static nint GetLuaFunctionByName(nint setupFunctionStartAddress, string apiName, int scanSize = 8192)
    {
        if (setupFunctionStartAddress == nint.Zero || string.IsNullOrEmpty(apiName))
            return nint.Zero;

        var functionBytes = new byte[scanSize];
        try
        {
            Marshal.Copy(setupFunctionStartAddress, functionBytes, 0, scanSize);
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
                var currentInstructionAddress = (long)setupFunctionStartAddress + i;
                var nextInstructionAddress    = currentInstructionAddress  + 7;
                var stringAddress             = nextInstructionAddress     + displacement;

                var referencedString = Marshal.PtrToStringAnsi((nint)stringAddress);
                if (referencedString == apiName)
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
                var currentInstructionAddress = (long)setupFunctionStartAddress + i;
                var nextInstructionAddress    = currentInstructionAddress  + 7;
                var targetFunctionAddress     = nextInstructionAddress     + displacement;

                return (nint)targetFunctionAddress;
            }
        }

        return nint.Zero;
    }
    
    /// <summary>
    /// 直接调用过不了混淆, 所以反射
    /// </summary>
    public static unsafe nint GetVFuncByName<T>(T* vtablePtr, string fieldName) where T : unmanaged
    {
        var vtType = typeof(T);
        var fi     = vtType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (fi == null) 
            throw new MissingFieldException(vtType.FullName, fieldName);

        var offAttr = fi.GetCustomAttribute<FieldOffsetAttribute>();
        if (offAttr == null) 
            throw new InvalidOperationException($"Field {fieldName} has no FieldOffset");

        var offset = offAttr.Value;

        return *(nint*)((byte*)vtablePtr + offset);
    }

    /// <summary>
    /// 直接调用过不了混淆, 所以反射
    /// </summary>
    public static nint GetMemberFuncByName(Type staticType, string propertyName) =>
        (nint)(staticType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)?.GetValue(null) ??
               throw new MissingMemberException(staticType.FullName, propertyName));

    public static bool IsInAnyParty() => 
        InfoProxyCrossRealm.IsCrossRealmParty() || DService.PartyList.Length >= 2;
    
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);
    
    public static unsafe void MinimizeWindow() => 
        ShowWindow(Framework.Instance()->GameWindow->WindowHandle, 6);

    public static void ExportToClipboard<T>(T config) where T : class
    {
        var json   = JsonConvert.SerializeObject(config, JsonSettings);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        Clipboard.SetText(base64);
    }

    public static T? ImportFromClipboard<T>() where T : class
    {
        var base64 = Clipboard.GetText();
        if (string.IsNullOrEmpty(base64)) return null;
        
        var json   = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        var config = JsonConvert.DeserializeObject<T>(json, JsonSettings);
        
        return config;
    }
    
    public static async Task WaitForCondition(Func<bool> condition, TimeSpan? timeout = null)
    {
        var tcs = new TaskCompletionSource<bool>();

        _ = DService.Framework.RunOnTick(async () =>
        {
            while (!condition())
            {
                await Task.Delay(100);
                if (tcs.Task.IsCompleted) return;
            }

            tcs.TrySetResult(true);
        });

        if (timeout.HasValue)
        {
            using var cts = new CancellationTokenSource(timeout.Value);
            cts.Token.Register(() => tcs.TrySetCanceled());
        }

        await tcs.Task;
    }

    public static DateTime UnixSecondToDateTime(long unixTimeStampS) 
        => DateTimeOffset.FromUnixTimeSeconds(unixTimeStampS).LocalDateTime;

    public static DateTime UnixMillisecondToDateTime(long unixTimeStampMS) 
        => DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStampMS).LocalDateTime;
    
    public static void MoveItemToPosition<T>(List<T> list, Func<T, bool> sourceItemSelector, int targetedIndex)
    {
        var sourceIndex = -1;
        for (var i = 0; i < list.Count; i++)
        {
            if (sourceItemSelector(list[i]))
            {
                sourceIndex = i;
                break;
            }
        }

        if (sourceIndex == targetedIndex) return;
        var item = list[sourceIndex];
        list.RemoveAt(sourceIndex);
        list.Insert(targetedIndex, item);
    }
}
