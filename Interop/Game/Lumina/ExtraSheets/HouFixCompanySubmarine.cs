using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Interop.Game.Lumina.ExtraSheets;

[Sheet("custom/004/HouFixCompanySubmarine_00447")]
public readonly struct HouFixCompanySubmarine
(
    ExcelPage page,
    uint      offset,
    uint      row
) : IExcelRow<HouFixCompanySubmarine>
{
    public ExcelPage ExcelPage => page;
    public uint      RowOffset => offset;
    public uint      RowId     => row;

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static HouFixCompanySubmarine IExcelRow<HouFixCompanySubmarine>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
