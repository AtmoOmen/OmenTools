using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace OmenTools.Info.Lumina.ExtraSheets;

// TODO：等待 Lumina 更新
[Sheet("World", 0xF59CC6AF)]
public readonly struct WorldTemp
(
    ExcelPage page,
    uint      offset,
    uint      row
) : IExcelRow<WorldTemp>
{
    public ExcelPage ExcelPage => page;
    public uint      RowOffset => offset;
    public uint      RowId     => row;

    public ReadOnlySeString         InternalName       => page.ReadString(offset,     offset);
    public ReadOnlySeString         Name               => page.ReadString(offset + 4, offset);
    public RowRef<WorldPhysicalDC>  PhysicalDataCenter => new(page.Module, page.ReadUInt8(offset + 8), page.Language);
    public RowRef<WorldDCGroupType> DataCenter         => new(page.Module, page.ReadUInt8(offset + 9), page.Language);
    public byte                     UserType           => page.ReadUInt8(offset      + 10);
    public byte                     Unknown1           => page.ReadUInt8(offset      + 11);
    public byte                     Region             => page.ReadUInt8(offset      + 12);
    public bool                     Unknown2           => page.ReadPackedBool(offset + 13, 0);
    public bool                     IsPublic           => page.ReadPackedBool(offset + 13, 1);

    static WorldTemp IExcelRow<WorldTemp>.Create
    (
        ExcelPage page,
        uint      offset,
        uint      row
    ) =>
        new(page, offset, row);
}
