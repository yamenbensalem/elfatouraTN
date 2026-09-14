using ClosedXML.Excel;

namespace Web_GestCom.Services;

public interface IExcelExportService
{
    /// <summary>Builds a single-sheet .xlsx workbook and returns its raw bytes.</summary>
    byte[] BuildWorkbook(string sheetName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows);
}

public class ExcelExportService : IExcelExportService
{
    public byte[] BuildWorkbook(string sheetName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(SanitizeSheetName(sheetName));

        for (var col = 0; col < headers.Count; col++)
        {
            var cell = sheet.Cell(1, col + 1);
            cell.Value = headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xF1, 0xF3, 0xF5);
        }

        var rowIndex = 2;
        foreach (var row in rows)
        {
            for (var col = 0; col < row.Count; col++)
                SetCellValue(sheet.Cell(rowIndex, col + 1), row[col]);
            rowIndex++;
        }

        if (headers.Count > 0)
        {
            sheet.SheetView.FreezeRows(1);
            sheet.RangeUsed()?.SetAutoFilter();
            sheet.Columns(1, headers.Count).AdjustToContents();
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                break;
            case string s:
                cell.Value = s;
                break;
            case DateTime dt:
                cell.Value = dt;
                cell.Style.DateFormat.Format = "dd/MM/yyyy";
                break;
            case bool b:
                cell.Value = b;
                break;
            case double d:
                cell.Value = d;
                break;
            case float f:
                cell.Value = (double)f;
                break;
            case decimal m:
                cell.Value = (double)m;
                break;
            case int or long or short:
                cell.Value = Convert.ToDouble(value);
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private static string SanitizeSheetName(string name)
    {
        // Excel sheet names: max 31 chars, and forbid : \ / ? * [ ]
        var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
        var clean = new string(name.Select(c => invalid.Contains(c) ? '-' : c).ToArray());
        return clean.Length > 31 ? clean[..31] : clean;
    }
}
