using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Info.Lumina.ExtraSheets;

[Sheet("custom/000/CmnDefRetainerCall_00010")]
public readonly struct RetainerCall
(
    ExcelPage page,
    uint      offset,
    uint      row
) : IExcelRow<RetainerCall>
{
    public ExcelPage ExcelPage => page;
    public uint      RowOffset => offset;
    public uint      RowId     => row;

    public ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static RetainerCall IExcelRow<RetainerCall>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
