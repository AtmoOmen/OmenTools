using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OmenTools.Interop.Windows.Helpers;

public class FileHelper
{
    public static void OpenByPath(string path, bool selectFile = false)
    {
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            string command;
            string arguments;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                command = "explorer.exe";

                arguments = selectFile ? $"/select,\"{path}\"" : path;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                command   = "open";
                arguments = selectFile ? $"-R \"{path}\"" : path;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                command   = "xdg-open";
                arguments = path;
            }
            else
                return;

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName        = command,
                Arguments       = arguments,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !selectFile,
                CreateNoWindow  = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            };

            process.Start();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public static async Task CopyAsync(string sourcePath, string destinationPath)
    {
        await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        await using var destinationStream =
            new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await sourceStream.CopyToAsync(destinationStream);
    }
}
