using Microsoft.VisualBasic.FileIO;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static void MoveToRecycleBin(string filePath)
    {
        try
        {
            FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }
        catch (Exception _)
        {
            // ignored
        }
    }

    public static void OpenFileOrFolder(string path, bool selectFile = false)
    {
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            string command;
            string arguments;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (selectFile)
                {
                    command = "explorer.exe";
                    arguments = $"/select,\"{path}\"";
                }
                else
                {
                    command = "explorer.exe";
                    arguments = path;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                command = "open";
                arguments = selectFile ? $"-R \"{path}\"" : path;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                command = "xdg-open";
                arguments = path;
            }
            else
            {
                return;
            }

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !selectFile,
                CreateNoWindow = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            };

            process.Start();
        }
        catch (Exception)
        {
            // ignored
        }
    }
}