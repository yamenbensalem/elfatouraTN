using Microsoft.JSInterop;

namespace Web_GestCom.Services;

/// <summary>
/// Builds an .xlsx workbook via <see cref="IExcelExportService"/> (Core, host-agnostic) and
/// triggers a browser download of the bytes via wwwroot/js/download.js. Web-only glue — Desktop
/// would write the same IExcelExportService bytes straight to disk via a SaveFileDialog instead.
/// </summary>
public static class ExcelDownloadHelper
{
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static async Task DownloadAsync(
        IJSRuntime js,
        IExcelExportService excelExport,
        string fileName,
        string sheetName,
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<object?>> rows)
    {
        var bytes = excelExport.BuildWorkbook(sheetName, headers, rows);
        var base64 = Convert.ToBase64String(bytes);
        await js.InvokeVoidAsync("downloadFileFromBytes", fileName, base64, XlsxContentType);
    }
}
