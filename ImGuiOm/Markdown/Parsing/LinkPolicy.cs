using System.ComponentModel;
using System.Diagnostics;

namespace OmenTools.ImGuiOm.Markdown.Parsing;

internal static class LinkPolicy
{
    public static bool ShouldAutoOpen
    (
        string url
    )
    {
        if (string.IsNullOrEmpty(url))
            return false;

        foreach (var scheme in AutoOpenSchemes)
        {
            if (url.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static void Activate
    (
        string          url,
        Action<string>? onClicked
    )
    {
        if (string.IsNullOrEmpty(url))
            return;

        if (onClicked is not null)
        {
            onClicked(url);
            return;
        }

        if (!ShouldAutoOpen(url))
            return;

        try
        {
            using var process = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException or NotSupportedException)
        {
            // ignored
        }
    }

    #region 常量

    private static readonly string[] AutoOpenSchemes = ["http://", "https://", "mailto:"];

    #endregion
}
