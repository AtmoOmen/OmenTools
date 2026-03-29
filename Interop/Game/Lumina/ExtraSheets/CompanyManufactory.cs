using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Interop.Game.Lumina.ExtraSheets;

[Sheet("custom/001/CmnDefCompanyManufactory_00150")]
public readonly struct CompanyManufactory
(
    ExcelPage page,
    uint      offset,
    uint      row
) : IExcelRow<CompanyManufactory>
{
    public ExcelPage ExcelPage => page;
    public uint      RowOffset => offset;
    public uint      RowId     => row;

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static CompanyManufactory IExcelRow<CompanyManufactory>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
