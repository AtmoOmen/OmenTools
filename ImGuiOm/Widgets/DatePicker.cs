using System.Globalization;
using System.Numerics;
using static OmenTools.ImGuiOm.ImGuiOm;
using StandardTimeManager = OmenTools.OmenService.StandardTimeManager;

namespace OmenTools.ImGuiOm.Widgets;

public class DatePicker
{
    private const float NAVIGATION_BUTTON_SPACING = 2f;

    private CultureInfo cultureInfo;
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

    public float Width { get; set; }

    /// <summary>
    ///     绘制日期选择器。
    /// </summary>
    /// <param name="label">组件标签/ID</param>
    /// <param name="currentDate">当前选中的日期</param>
    /// <param name="flags">可选的标志位</param>
    /// <returns>如果日期发生改变返回 true</returns>
    public bool Draw(string label, ref DateTime currentDate, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        var changed     = false;
        var pickerWidth = ResolvePickerWidth();

        using var id = ImRaii.PushId(label);
        CenterWithinAvailableWidth(pickerWidth);
        using var group = ImRaii.Group();

        DrawHeader(pickerWidth);

        ImGui.Spacing();

        if (isYearView)
            changed |= DrawYearPicker(pickerWidth);
        else
        {
            using var table = ImRaii.Table
            (
                "CalendarBody",
                7,
                ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.SizingStretchSame,
                new Vector2(pickerWidth, 0f)
            );

            if (table)
            {
                DrawWeekDays();
                changed |= DrawDays(ref currentDate, flags);
            }
        }

        return changed;
    }

    private void DrawHeader(float pickerWidth)
    {
        using var group = ImRaii.Group();

        var buttonSize    = new Vector2(ImGui.GetFrameHeight());
        var navGroupWidth = GetNavigationGroupWidth(buttonSize.X);
        var startX        = ImGui.GetCursorPosX();

        if (ButtonIcon("##PrevYear", FontAwesomeIcon.AngleDoubleLeft, buttonSize))
            viewDate = viewDate.AddYears(-1);
        ImGui.SameLine(0, NAVIGATION_BUTTON_SPACING);
        if (ButtonIcon("##PrevMonth", FontAwesomeIcon.AngleLeft, buttonSize))
            viewDate = viewDate.AddMonths(-1);

        var title          = viewDate.ToString(DateFormat, cultureInfo);
        var titleWidth     = ImGui.CalcTextSize(title).X;
        var titleAreaWidth = MathF.Max(0f, pickerWidth - navGroupWidth                            * 2);
        var titleStartX    = startX + navGroupWidth + MathF.Max(0f, (titleAreaWidth - titleWidth) / 2f);

        ImGui.SameLine();
        ImGui.SetCursorPosX(titleStartX);
        if (ImGui.Selectable(title, isYearView, ImGuiSelectableFlags.DontClosePopups, new Vector2(titleWidth, 0)))
            isYearView = !isYearView;

        ImGui.SameLine();
        ImGui.SetCursorPosX(startX + pickerWidth - navGroupWidth);

        if (ButtonIcon("##NextMonth", FontAwesomeIcon.AngleRight, buttonSize))
            viewDate = viewDate.AddMonths(1);
        ImGui.SameLine(0, NAVIGATION_BUTTON_SPACING);
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

    private bool DrawYearPicker(float pickerWidth)
    {
        var currentYear = viewDate.Year;

        var startYear = currentYear - 5;

        using var table = ImRaii.Table
        (
            "YearPicker",
            4,
            ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.SizingStretchSame,
            new Vector2(pickerWidth, 0f)
        );
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

    private float ResolvePickerWidth()
    {
        if (Width > 0f)
            return Width;

        var titleWidth  = ImGui.CalcTextSize(viewDate.ToString(DateFormat, cultureInfo)).X;
        var buttonWidth = ImGui.GetFrameHeight();
        var cellWidth   = GetCalendarCellWidth();

        return MathF.Max(titleWidth + GetNavigationGroupWidth(buttonWidth) * 2, cellWidth * 7f);
    }

    private float GetCalendarCellWidth()
    {
        var maxTextWidth = ImGui.CalcTextSize("0000").X;
        foreach (var day in weekDays)
            maxTextWidth = MathF.Max(maxTextWidth, ImGui.CalcTextSize(day).X);

        maxTextWidth = MathF.Max(maxTextWidth, ImGui.CalcTextSize("30").X);

        var style = ImGui.GetStyle();
        return maxTextWidth + style.FramePadding.X * 2f + style.CellPadding.X * 2f;
    }

    private static float GetNavigationGroupWidth(float buttonWidth) =>
        buttonWidth * 2f + NAVIGATION_BUTTON_SPACING;

    private static void CenterWithinAvailableWidth(float itemWidth)
    {
        var remainingWidth = ImGui.GetContentRegionAvail().X - itemWidth;
        if (remainingWidth > 0f)
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + remainingWidth / 2f);
    }
}
