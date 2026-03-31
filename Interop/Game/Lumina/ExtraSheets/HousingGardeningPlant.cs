using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Interop.Game.Lumina.ExtraSheets;

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

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static HousingGardeningPlant IExcelRow<HousingGardeningPlant>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
