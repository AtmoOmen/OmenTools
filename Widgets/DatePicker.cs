using System.Numerics;
using Dalamud.Interface;
using static OmenTools.ImGuiOm.ImGuiOm;

namespace OmenTools.Widgets;

public class DatePicker
{
    private float datePickerPagingWidth = 200f;

    private DateTime viewDate = DateTime.Now;

    /// <summary>
    ///     Create a Date Picker
    /// </summary>
    /// <param name="localizedWeekDays">Localized week days string, should look like "Sun, Mon, Tue, Wed, Thu, Fri, Sat"</param>
    public DatePicker(string localizedWeekDays = "Sun, Mon, Tue, Wed, Thu, Fri, Sat")
    {
        WeekDays = localizedWeekDays.Split(',');
        if (WeekDays.Length != 7)
            throw new ArgumentException("Weekdays array must contain exactly 7 elements.");
    }

    public Vector2  Size     { get; private set; }
    public string[] WeekDays { get; }

    public bool Draw(ref DateTime currentDate)
    {
        var state = false;
        ImGui.BeginGroup();
        DrawHeader();

        if (ImGui.BeginTable("DatePicker", 7, ImGuiTableFlags.NoBordersInBody))
        {
            DrawWeekDays();
            state = DrawDays(ref currentDate);
            ImGui.EndTable();
        }

        ImGui.EndGroup();
        Size = ImGui.GetItemRectSize();

        return state;
    }

    public void DrawHeader()
    {
        CenterAlignFor(datePickerPagingWidth);

        ImGui.BeginGroup();

        DrawNavigationButton("LastYear", FontAwesomeIcon.Backward, -1, true);

        ImGui.SameLine();
        DrawNavigationButton("LastMonth", ImGuiDir.Left, -1, false);

        ImGui.SameLine();
        ImGui.Text($"{viewDate:yyyy.MM}");

        ImGui.SameLine();
        DrawNavigationButton("NextMonth", ImGuiDir.Right, 1, false);

        ImGui.SameLine();
        DrawNavigationButton("NextYear", FontAwesomeIcon.Forward, 1, true);

        ImGui.EndGroup();

        datePickerPagingWidth = Math.Max(ImGui.GetItemRectSize().X, Size.X);
    }

    private void DrawWeekDays()
    {
        foreach (var day in WeekDays)
        {
            ImGui.TableNextColumn();
            TextCentered(day);
        }
    }

    private bool DrawDays(ref DateTime currentDate)
    {
        var state = false;

        ImGui.TableNextRow(ImGuiTableRowFlags.None);
        var firstDayOfMonth = new DateTime(viewDate.Year, viewDate.Month, 1);
        var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        var daysInMonth = DateTime.DaysInMonth(viewDate.Year, viewDate.Month);

        for (var i = 0; i < firstDayOfWeek; i++)
        {
            ImGui.TableNextColumn();
            TextCentered("");
        }

        for (var day = 1; day <= daysInMonth; day++)
        {
            ImGui.TableNextColumn();
            var isCurrentDate = currentDate.Year == viewDate.Year && currentDate.Month == viewDate.Month &&
                                currentDate.Day == day;

            if (isCurrentDate)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 0.6f, 1.0f, 1.0f));
                if (SelectableTextCentered(day.ToString(), false, ImGuiSelectableFlags.DontClosePopups))
                {
                    currentDate = new DateTime(viewDate.Year, viewDate.Month, day);
                    state = true;
                }

                ImGui.PopStyleColor();
            }
            else if (SelectableTextCentered(day.ToString(), false, ImGuiSelectableFlags.DontClosePopups))
            {
                currentDate = new DateTime(viewDate.Year, viewDate.Month, day);
                state = true;
            }
        }

        return state;
    }

    public void DrawNavigationButton(string id, object icon, int value, bool isYear)
    {
        if (isYear ? ButtonIcon(id, (FontAwesomeIcon)icon) : ImGui.Button(icon.ToString()))
            viewDate = isYear ? viewDate.AddYears(value) : viewDate.AddMonths(value);
    }
}
