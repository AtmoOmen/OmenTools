using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Info.Lumina.ExtraSheets;

[Sheet("transport/Aetheryte")]
public readonly struct AetheryteTransport
(
    ExcelPage page,
    uint      offset,
    uint      row
) : IExcelRow<AetheryteTransport>
{
    public ExcelPage ExcelPage => page;
    public uint      RowOffset => offset;
    public uint      RowId     => row;

    public ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static AetheryteTransport IExcelRow<AetheryteTransport>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
