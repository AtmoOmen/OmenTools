using Lumina.Excel;
using Lumina;
using Lumina.Data;
using Lumina.Text;

namespace OmenTools.Infos;

[Sheet("leve/CraftLeveClient")]
public class CraftLeveClient : ExcelRow
{
    public SeString? Name { get; set; }
    public SeString? Text { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Name = parser.ReadColumn<SeString>(0);
        Text = parser.ReadColumn<SeString>(1);
    }
}

[Sheet("custom/004/HouFixCompanySubmarine_00447")]
public class CompanySubmarine : ExcelRow
{
    public SeString? Name { get; set; }
    public SeString? Text { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Name = parser.ReadColumn<SeString>(0);
        Text = parser.ReadColumn<SeString>(1);
    }
}

[Sheet("transport/Aetheryte")]
public class AetheryteTransport : ExcelRow
{
    public SeString? Identifier { get; set; }
    public SeString? Text { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Identifier = parser.ReadColumn<SeString>(0);
        Text = parser.ReadColumn<SeString>(1);
    }
}

[Sheet("custom/000/CmnDefRetainerCall_00010")]
public class RetainerCall : ExcelRow
{
    public SeString? Identifier { get; set; }
    public SeString? Text { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Identifier = parser.ReadColumn<SeString>(0);
        Text = parser.ReadColumn<SeString>(1);
    }
}

[Sheet("custom/001/CmnDefHousingGardeningPlant_00151")]
public class HousingGardeningPlant : ExcelRow
{
    public SeString? Identifier { get; set; }
    public SeString? Text { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Identifier = parser.ReadColumn<SeString>(0);
        Text = parser.ReadColumn<SeString>(1);
    }
}

[Sheet("custom/001/CmnDefHousingPersonalRoomEntrance_00178")]
public class HousingPersonalRoomEntrance : ExcelRow
{
    public SeString? Identifier { get; set; }
    public SeString? Text { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Identifier = parser.ReadColumn<SeString>(0);
        Text = parser.ReadColumn<SeString>(1);
    }
}

[Sheet("custom/004/HouFixCompanySubmarine_00447")]
public class HouFixCompanySubmarine : ExcelRow
{
    public SeString? Identifier { get; set; }
    public SeString? Text { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Identifier = parser.ReadColumn<SeString>(0);
        Text = parser.ReadColumn<SeString>(1);
    }
}

[Sheet("custom/001/CmnDefCompanyManufactory_00150")]
public class CompanyManufactory : ExcelRow
{
    public SeString? Identifier { get; set; }
    public SeString? Text { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Identifier = parser.ReadColumn<SeString>(0);
        Text = parser.ReadColumn<SeString>(1);
    }
}