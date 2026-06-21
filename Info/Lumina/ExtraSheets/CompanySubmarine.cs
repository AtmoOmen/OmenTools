using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Info.Lumina.ExtraSheets;

[Sheet("custom/004/HouFixCompanySubmarine_00447")]
public readonly struct CompanySubmarine
(
    ExcelPage page,
    uint      offset,
    uint      row
) : IExcelRow<CompanySubmarine>
{
    public ExcelPage ExcelPage => page;
    public uint      RowOffset => offset;
    public uint      RowId     => row;

    public ReadOnlySeString Name => page.ReadString(offset,     offset);
    public ReadOnlySeString Text => page.ReadString(offset + 4, offset);

    static CompanySubmarine IExcelRow<CompanySubmarine>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
