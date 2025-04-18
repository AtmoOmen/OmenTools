using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Lumina.Excel;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static RowRef<T> LuminaCreateRef<T>(uint rowId) where T : struct, IExcelRow<T> => new(DService.Data.Excel, rowId);
    
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
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
            if (sourceItemSelector(list[i]))
            {
                sourceIndex = i;
                break;
            }

        if (sourceIndex == targetedIndex) return;
        var item = list[sourceIndex];
        list.RemoveAt(sourceIndex);
        list.Insert(targetedIndex, item);
    }
}
