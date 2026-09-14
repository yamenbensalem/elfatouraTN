using ClosedXML.Excel;
using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class ExcelExportServiceTests
{
    private readonly ExcelExportService _service = new();

    [Fact]
    public void BuildWorkbook_WritesHeaderRowAndDataRows()
    {
        // Arrange
        var headers = new[] { "Code", "Nom", "Montant" };
        var rows = new List<IReadOnlyList<object?>>
        {
            new object?[] { "CL00001", "Alpha SARL", 1234.5 },
            new object?[] { "CL00002", "Beta SA", 0.0 },
        };

        // Act
        var bytes = _service.BuildWorkbook("Clients", headers, rows);

        // Assert
        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        var sheet = workbook.Worksheet(1);

        Assert.Equal("Clients", sheet.Name);
        Assert.Equal("Code", sheet.Cell(1, 1).GetString());
        Assert.Equal("Nom", sheet.Cell(1, 2).GetString());
        Assert.Equal("Montant", sheet.Cell(1, 3).GetString());
        Assert.Equal("CL00001", sheet.Cell(2, 1).GetString());
        Assert.Equal("Alpha SARL", sheet.Cell(2, 2).GetString());
        Assert.Equal(1234.5, sheet.Cell(2, 3).GetDouble());
        Assert.Equal("CL00002", sheet.Cell(3, 1).GetString());
    }

    [Fact]
    public void BuildWorkbook_WithNoRows_StillWritesHeaderOnly()
    {
        var bytes = _service.BuildWorkbook("Vide", new[] { "A", "B" }, []);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        var sheet = workbook.Worksheet(1);

        Assert.Equal("A", sheet.Cell(1, 1).GetString());
        Assert.True(sheet.Cell(2, 1).IsEmpty());
    }

    [Fact]
    public void BuildWorkbook_WritesDateTimeAsDate()
    {
        var date = new DateTime(2026, 9, 14);
        var bytes = _service.BuildWorkbook("Dates", new[] { "Date" },
            new List<IReadOnlyList<object?>> { new object?[] { date } });

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        var cell = workbook.Worksheet(1).Cell(2, 1);

        Assert.Equal(date, cell.GetDateTime());
    }

    [Fact]
    public void BuildWorkbook_WritesNullCellAsEmpty()
    {
        var bytes = _service.BuildWorkbook("Nulls", new[] { "Val" },
            new List<IReadOnlyList<object?>> { new object?[] { null } });

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);

        Assert.True(workbook.Worksheet(1).Cell(2, 1).IsEmpty());
    }

    [Fact]
    public void BuildWorkbook_SheetNameOver31Chars_IsTruncated()
    {
        var longName = new string('X', 50);
        var bytes = _service.BuildWorkbook(longName, new[] { "A" }, []);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);

        Assert.True(workbook.Worksheet(1).Name.Length <= 31);
    }

    [Fact]
    public void BuildWorkbook_SheetNameWithInvalidChars_IsSanitized()
    {
        var bytes = _service.BuildWorkbook("Fact:ures/Test?", new[] { "A" }, []);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);

        Assert.DoesNotContain(':', workbook.Worksheet(1).Name);
        Assert.DoesNotContain('/', workbook.Worksheet(1).Name);
        Assert.DoesNotContain('?', workbook.Worksheet(1).Name);
    }
}
