using static OmenTools.ImGuiOm.ImGuiOm;

namespace OmenTools.Widgets;

public class DatePicker
{
    public Vector2 RegionSize { get; private set; }

    public delegate void DateSelectedDelegate();
    public event DateSelectedDelegate? DateSelected;

    private readonly string _weekDays;
    private string[] _weekDayNames;
    private DateTime _viewDate = DateTime.Now;
    private float _datePickerPagingWidth = 200f;

    /// <summary>
    /// Create a Date Picker
    /// </summary>
    /// <param name="localizedWeekDays">Localized week days string, should look like "Sun, Mon, Tue, Wed, Thu, Fri, Sat"</param>
    public DatePicker(string? localizedWeekDays = null)
    {
        _weekDays = localizedWeekDays ?? "Sun, Mon, Tue, Wed, Thu, Fri, Sat";
        _weekDayNames = _weekDays.Split(',');
    }

    public void Draw(ref DateTime currentDate)
    {
        ImGui.BeginGroup();
        DrawHeader();

        using (var table = ImRaii.Table("DatePicker", 7, ImGuiTableFlags.NoBordersInBody))
        {
            if (table)
            {
                DrawWeekDays();
                DrawDays(ref currentDate);
            }
        }

        ImGui.EndGroup();
        RegionSize = ImGui.GetItemRectSize();
    }

    private void DrawWeekDays()
    {
        foreach (var day in _weekDayNames)
        {
            ImGui.TableNextColumn();
            TextCentered(day);
        }
    }

    private void DrawDays(ref DateTime currentDate)
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.None);
        var firstDayOfMonth = new DateTime(_viewDate.Year, _viewDate.Month, 1);
        var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        var daysInMonth = DateTime.DaysInMonth(_viewDate.Year, _viewDate.Month);

        for (var i = 0; i < firstDayOfWeek; i++)
        {
            ImGui.TableNextColumn();
            TextCentered("");
        }

        for (var day = 1; day <= daysInMonth; day++)
        {
            ImGui.TableNextColumn();
            var isCurrentDate = currentDate.Year == _viewDate.Year && currentDate.Month == _viewDate.Month && currentDate.Day == day;

            if (isCurrentDate)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 0.6f, 1.0f, 1.0f));
                if (SelectableTextCentered(day.ToString(), false, ImGuiSelectableFlags.DontClosePopups))
                {
                    currentDate = new DateTime(_viewDate.Year, _viewDate.Month, day);
                    DateSelected?.Invoke();
                }
                ImGui.PopStyleColor();
            }
            else if (SelectableTextCentered(day.ToString(), false, ImGuiSelectableFlags.DontClosePopups))
            {
                currentDate = new DateTime(_viewDate.Year, _viewDate.Month, day);
                DateSelected?.Invoke();
            }
        }
    }

    public void DrawHeader()
    {
        CenterAlignFor(_datePickerPagingWidth);
        ImGui.BeginGroup();
        DrawNavigationButton("LastYear", FontAwesomeIcon.Backward, -1, true);
        ImGui.SameLine();
        DrawNavigationButton("LastMonth", ImGuiDir.Left, -1, false);
        ImGui.SameLine();
        ImGui.Text($"{_viewDate:yyyy.MM}");
        ImGui.SameLine();
        DrawNavigationButton("NextMonth", ImGuiDir.Right, 1, false);
        ImGui.SameLine();
        DrawNavigationButton("NextYear", FontAwesomeIcon.Forward, 1, true);
        ImGui.EndGroup();
        _datePickerPagingWidth = (int)ImGui.GetItemRectSize().X;
    }

    public void DrawNavigationButton(string id, object icon, int value, bool isYear)
    {
        if (isYear ? ButtonIcon(id, (FontAwesomeIcon)icon) : ImGui.ArrowButton(id, (ImGuiDir)icon))
        {
            _viewDate = isYear ? _viewDate.AddYears(value) : _viewDate.AddMonths(value);
        }
    }
}