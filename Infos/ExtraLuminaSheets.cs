using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace OmenTools.Infos;

[Sheet("leve/CraftLeveClient")]
public readonly struct CraftLeveClient(ExcelPage page, uint offset, uint row) : IExcelRow<CraftLeveClient>
{
    public uint RowId => row;

    public readonly ReadOnlySeString Name => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text => page.ReadString(offset + 4, offset);

    static CraftLeveClient IExcelRow<CraftLeveClient>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}

[Sheet("custom/004/HouFixCompanySubmarine_00447")]
public readonly struct CompanySubmarine(ExcelPage page, uint offset, uint row) : IExcelRow<CompanySubmarine>
{
    public uint RowId => row;

    public readonly ReadOnlySeString Name => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text => page.ReadString(offset + 4, offset);

    static CompanySubmarine IExcelRow<CompanySubmarine>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}

[Sheet("transport/Aetheryte")]
public readonly struct AetheryteTransport(ExcelPage page, uint offset, uint row) : IExcelRow<AetheryteTransport>
{
    public uint RowId => row;

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static AetheryteTransport IExcelRow<AetheryteTransport>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}

[Sheet("custom/000/CmnDefRetainerCall_00010")]
public readonly struct RetainerCall(ExcelPage page, uint offset, uint row) : IExcelRow<RetainerCall>
{
    public uint RowId => row;

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static RetainerCall IExcelRow<RetainerCall>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}

[Sheet("custom/001/CmnDefHousingGardeningPlant_00151")]
public readonly struct HousingGardeningPlant(ExcelPage page, uint offset, uint row) : IExcelRow<HousingGardeningPlant>
{
    public uint RowId => row;

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static HousingGardeningPlant IExcelRow<HousingGardeningPlant>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}

[Sheet("custom/001/CmnDefHousingPersonalRoomEntrance_00178")]
public readonly struct HousingPersonalRoomEntrance(ExcelPage page, uint offset, uint row) : IExcelRow<HousingPersonalRoomEntrance>
{
    public uint RowId => row;

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static HousingPersonalRoomEntrance IExcelRow<HousingPersonalRoomEntrance>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}

[Sheet("custom/004/HouFixCompanySubmarine_00447")]
public readonly struct HouFixCompanySubmarine(ExcelPage page, uint offset, uint row) : IExcelRow<HouFixCompanySubmarine>
{
    public uint RowId => row;

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static HouFixCompanySubmarine IExcelRow<HouFixCompanySubmarine>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}

[Sheet("custom/001/CmnDefCompanyManufactory_00150")]
public readonly struct CompanyManufactory(ExcelPage page, uint offset, uint row) : IExcelRow<CompanyManufactory>
{
    public uint RowId => row;

    public readonly ReadOnlySeString Identifier => page.ReadString(offset,     offset);
    public readonly ReadOnlySeString Text       => page.ReadString(offset + 4, offset);

    static CompanyManufactory IExcelRow<CompanyManufactory>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
