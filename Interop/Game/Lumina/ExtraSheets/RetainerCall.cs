using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Interop.Game.Lumina.ExtraSheets;

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

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static RetainerCall IExcelRow<RetainerCall>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
