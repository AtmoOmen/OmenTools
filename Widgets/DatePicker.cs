using System.Globalization;
using System.Numerics;
using static OmenTools.ImGuiOm.ImGuiOm;

namespace OmenTools.Widgets;

public class DatePicker
{
    private CultureInfo cultureInfo;
    private float       currentWidth = 200f;
    private bool        isYearView;
    private DateTime    viewDate = StandardTimeManager.Instance().Now;

    private string[] weekDays = [];

    /// <summary>
    ///     初始化日期选择器。
    /// </summary>
    /// <param name="culture">特定的文化信息，默认为当前系统文化</param>
    public DatePicker(CultureInfo? culture = null)
    {
        cultureInfo = culture ?? CultureInfo.CurrentCulture;
        UpdateLocalizedStrings();
    }

    public string DateFormat { get; set; } = "yyyy.MM";

    /// <summary>
    ///     绘制日期选择器。
    /// </summary>
    /// <param name="label">组件标签/ID</param>
    /// <param name="currentDate">当前选中的日期</param>
    /// <param name="flags">可选的标志位</param>
    /// <returns>如果日期发生改变返回 true</returns>
    public bool Draw(string label, ref DateTime currentDate, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        var changed = false;

        using var group = ImRaii.Group();
        using var id    = ImRaii.PushId(label);

        DrawHeader();

        ImGui.Spacing();

        if (isYearView)
            changed |= DrawYearPicker();
        else
        {
            using var table = ImRaii.Table("CalendarBody", 7, ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.SizingStretchSame);

            if (table)
            {
                DrawWeekDays();
                changed |= DrawDays(ref currentDate, flags);
            }
        }

        currentWidth = ImGui.GetItemRectSize().X;

        return changed;
    }

    private void DrawHeader()
    {
        CenterAlignFor(currentWidth);

        using var group = ImRaii.Group();

        var buttonSize = new Vector2(ImGui.GetFrameHeight());

        if (ButtonIcon("##PrevYear", FontAwesomeIcon.AngleDoubleLeft, buttonSize))
            viewDate = viewDate.AddYears(-1);
        ImGui.SameLine(0, 2);
        if (ButtonIcon("##PrevMonth", FontAwesomeIcon.AngleLeft, buttonSize))
            viewDate = viewDate.AddMonths(-1);

        ImGui.SameLine();

        var title          = viewDate.ToString(DateFormat, cultureInfo);
        var titleWidth     = ImGui.CalcTextSize(title).X;
        var availableWidth = ImGui.GetContentRegionAvail().X - (buttonSize.X * 2 + 8);

        var spaceSides = (availableWidth - titleWidth) / 2;
        if (spaceSides > 0)
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + spaceSides);

        if (ImGui.Selectable(title, isYearView, ImGuiSelectableFlags.None, new Vector2(titleWidth, 0)))
            isYearView = !isYearView;

        ImGui.SameLine();

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + spaceSides);

        if (ButtonIcon("##NextMonth", FontAwesomeIcon.AngleRight, buttonSize))
            viewDate = viewDate.AddMonths(1);
        ImGui.SameLine(0, 2);
        if (ButtonIcon("##NextYear", FontAwesomeIcon.AngleDoubleRight, buttonSize))
            viewDate = viewDate.AddYears(1);
    }

    private void DrawWeekDays()
    {
        using var font = ImRaii.PushFont(UiBuilder.DefaultFont);

        using var color = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);

        foreach (var day in weekDays)
        {
            ImGui.TableNextColumn();
            TextCentered(day);
        }
    }

    private bool DrawDays(ref DateTime currentDate, ImGuiSelectableFlags flags)
    {
        var changed = false;

        var firstDayOfMonth = new DateTime(viewDate.Year, viewDate.Month, 1);
        var daysInMonth     = DateTime.DaysInMonth(viewDate.Year, viewDate.Month);

        var firstDayOfWeek   = (int)cultureInfo.DateTimeFormat.FirstDayOfWeek;
        var currentDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        var offset           = (currentDayOfWeek - firstDayOfWeek + 7) % 7;

        for (var i = 0; i < offset; i++)
            ImGui.TableNextColumn();

        for (var day = 1; day <= daysInMonth; day++)
        {
            ImGui.TableNextColumn();

            var date       = new DateTime(viewDate.Year, viewDate.Month, day);
            var isSelected = date.Date == currentDate.Date;
            var isToday    = date.Date == StandardTimeManager.Instance().Now.Date;
            
            using var colorStack = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.CheckMark], isToday && !isSelected);
            
            if (SelectableTextCentered(day.ToString(), isSelected, flags | ImGuiSelectableFlags.DontClosePopups))
            {
                currentDate = date;
                changed     = true;
            }
        }

        return changed;
    }

    private bool DrawYearPicker()
    {
        var currentYear = viewDate.Year;

        var startYear = currentYear - 5;

        using var table = ImRaii.Table("YearPicker", 4, ImGuiTableFlags.NoBordersInBody);
        if (!table) return false;

        for (var i = 0; i < 12; i++)
        {
            ImGui.TableNextColumn();
            var year       = startYear + i;
            var isSelected = year == currentYear;

            using var color = ImRaii.PushColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.Header], isSelected);
            if (ImGui.Button($"{year}", new(-1, 0)))
            {
                viewDate   = new DateTime(year, viewDate.Month, viewDate.Day);
                isYearView = false;
            }
        }

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        if (ImGui.Button("<<", new Vector2(-1, 0)))
            viewDate = viewDate.AddYears(-10);

        ImGui.TableSetColumnIndex(3);
        if (ImGui.Button(">>", new Vector2(-1, 0)))
            viewDate = viewDate.AddYears(10);

        return false;
    }

    private void UpdateLocalizedStrings()
    {
        weekDays = cultureInfo.DateTimeFormat.AbbreviatedDayNames;
        var firstDay = (int)cultureInfo.DateTimeFormat.FirstDayOfWeek;

        if (firstDay != 0)
        {
            var rotated = new string[7];
            for (var i = 0; i < 7; i++)
                rotated[i] = weekDays[(i + firstDay) % 7];
            weekDays = rotated;
        }
    }
}
