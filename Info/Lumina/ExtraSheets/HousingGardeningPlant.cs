using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Info.Lumina.ExtraSheets;

[Sheet("custom/001/CmnDefHousingGardeningPlant_00151")]
public readonly struct HousingGardeningPlant
(
    ExcelPage page,
    uint      offset,
    uint      row
) : IExcelRow<HousingGardeningPlant>
{
    public ExcelPage ExcelPage => page;
    public uint      RowOffset => offset;
    public uint      RowId     => row;

    public ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static HousingGardeningPlant IExcelRow<HousingGardeningPlant>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
