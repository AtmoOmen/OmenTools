using System.Globalization;

namespace OmenTools.ImGuiOm.Markdown.Rendering;

internal static class ListMarker
{
    public static string For
    (
        bool  ordered,
        int   itemIndex,
        int   startNumber,
        bool? taskChecked
    )
    {
        if (taskChecked.HasValue)
            return taskChecked.Value ?
                       "[x]" :
                       "[ ]";

        if (ordered)
        {
            var number = startNumber + itemIndex;
            return number.ToString(CultureInfo.InvariantCulture) + ".";
        }

        // A bullet rather than '-', which would be indistinguishable from a literal hyphen in the text.
        return "\u2022";
    }
}
