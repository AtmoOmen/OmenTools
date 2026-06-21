using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Info.Lumina.ExtraSheets;

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

    public ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static CompanyManufactory IExcelRow<CompanyManufactory>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
