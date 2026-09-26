using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Shared;

/// <summary>
/// Desktop equivalent of Web_GestCom's ExcelDownloadHelper: builds the same .xlsx bytes via the
/// shared IExcelExportService (Core), then writes them to disk via a SaveFileDialog instead of
/// triggering a browser download. Exports straight from the grid's current rows/columns rather
/// than re-querying, so it matches exactly what the user has on screen (including any active
/// search filter) — the same list every List screen already builds via LoadAsync.
/// </summary>
internal static class ExcelExportHelper
{
    private static readonly ILogger Logger = Log.ForContext(typeof(ExcelExportHelper));

    public static void ExportGrid(Form owner, DataGridView grid, string fileNamePrefix, string sheetName)
    {
        var columns = grid.Columns
            .Cast<DataGridViewColumn>()
            .Where(c => c.Visible)
            .OrderBy(c => c.DisplayIndex)
            .ToList();

        if (grid.Rows.Count == 0)
        {
            MessageBox.Show(owner, "Aucune donnée à exporter.", "Export Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "Classeur Excel (*.xlsx)|*.xlsx",
            FileName = $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
        };
        if (dialog.ShowDialog(owner) != DialogResult.OK)
            return;

        var headers = columns.Select(c => c.HeaderText).ToList();
        var rows = grid.Rows
            .Cast<DataGridViewRow>()
            .Select(r => (IReadOnlyList<object?>)columns.Select(c => r.Cells[c.Index].Value).ToList())
            .ToList();

        try
        {
            using var scope = AppHost.CreateScope();
            var excelExport = scope.ServiceProvider.GetRequiredService<IExcelExportService>();
            var bytes = excelExport.BuildWorkbook(sheetName, headers, rows);
            File.WriteAllBytes(dialog.FileName, bytes);
            Logger.Debug("Export Excel : {Fichier} ({Lignes} lignes).", dialog.FileName, rows.Count);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec de l'export Excel vers {Fichier}.", dialog.FileName);
            MessageBox.Show(owner, $"Erreur lors de l'export : {ex.Message}", "Export Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
