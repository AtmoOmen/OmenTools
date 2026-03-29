using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Interop.Game.Lumina.ExtraSheets;

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

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static AetheryteTransport IExcelRow<AetheryteTransport>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
