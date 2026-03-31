using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Interop.Game.Lumina.ExtraSheets;

[Sheet("leve/CraftLeveClient")]
public readonly struct CraftLeveClient
(
    ExcelPage page,
    uint      offset,
    uint      row
) : IExcelRow<CraftLeveClient>
{
    public ExcelPage ExcelPage => page;
    public uint      RowOffset => offset;
    public uint      RowId     => row;

    public readonly ReadOnlySeString Name => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text => page.ReadString(offset + 4, offset);

    static CraftLeveClient IExcelRow<CraftLeveClient>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
