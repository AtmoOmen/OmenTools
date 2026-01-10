using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
        catch (Exception)
        {
            // ignored
        }
    }

    public static void OpenPath(string path, bool selectFile = false)
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
                return;

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

    public static async Task DeleteFileAsync(string filePath)
    {
        const int maxRetries = 3;
        const int delayMilliseconds = 100;

        for (var i = 0; i < maxRetries; i++)
        {
            try
            {
                if (File.Exists(filePath)) 
                    File.Delete(filePath);
                return;
            }
            catch (IOException)
            {
                if (i == maxRetries - 1) 
                    throw;
                
                await Task.Delay(delayMilliseconds);
            }
        }
    }

    public static async Task<string> ReadAllTextAsync(string path)
    {
        await using var sourceStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        using var reader = new StreamReader(sourceStream);
        return await reader.ReadToEndAsync();
    }

    public static async Task WriteAllTextAsync(string path, string contents)
    {
        var encodedText = Encoding.UTF8.GetBytes(contents);
        await using var sourceStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
    }

    public static async Task AppendTextAsync(string path, string contents)
    {
        await using var sourceStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true);
        await using var writer = new StreamWriter(sourceStream);
        await writer.WriteAsync(contents);
    }

    public static async Task CopyFileAsync(string sourcePath, string destinationPath)
    {
        await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        await using var destinationStream =
            new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await sourceStream.CopyToAsync(destinationStream);
    }

    public static async Task MoveFileAsync(string sourcePath, string destinationPath)
    {
        await CopyFileAsync(sourcePath, destinationPath);
        await Task.Run(() => File.Delete(sourcePath));
    }

    public static async Task<IEnumerable<string>> ReadAllLinesAsync(string path)
    {
        var lines = new List<string>();
        await using var sourceStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        using var reader = new StreamReader(sourceStream);
        while (await reader.ReadLineAsync() is { } line)
            lines.Add(line);
        return lines;
    }

    public static Task<bool> FileExistsAsync(string path) => Task.Run(() => File.Exists(path));
}
