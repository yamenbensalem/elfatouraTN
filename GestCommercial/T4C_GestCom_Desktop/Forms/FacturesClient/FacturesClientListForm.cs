using Microsoft.Extensions.DependencyInjection;
using Serilog;
using T4C_GestCom_Desktop.Forms.Shared;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Forms.FacturesClient;

/// <summary>
/// Desktop equivalent of Components/Pages/FacturesClient/FacturesList.razor. The same list drives
/// both "Factures Client" and "Avoirs" in the web app (route prefix picks the mode); here the
/// constructor flag does the same job.
/// </summary>
public class FacturesClientListForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<FacturesClientListForm>();

    private readonly bool _isAvoir;
    private readonly string _docLabel;

    private readonly Button _btnNew = new() { Left = 10, Top = 9, Width = 120 };
    private readonly Button _btnEdit = new() { Left = 140, Top = 9, Width = 90, Text = "Modifier" };
    private readonly Button _btnClone = new() { Left = 240, Top = 9, Width = 90, Text = "Cloner" };
    private readonly Button _btnDelete = new() { Left = 340, Top = 9, Width = 90, Text = "Supprimer" };
    private readonly Button _btnRefresh = new() { Left = 440, Top = 9, Width = 90, Text = "Actualiser" };
    private readonly DataGridView _grid = new()
    {
        Left = 10,
        Top = 45,
        Width = 1000,
        Height = 500,
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };

    public FacturesClientListForm(bool isAvoir = false)
    {
        _isAvoir = isAvoir;
        _docLabel = isAvoir ? "avoir" : "facture";

        Text = isAvoir ? "Avoirs" : "Factures Client";
        _btnNew.Text = isAvoir ? "Nouvel Avoir" : "Nouvelle Facture";
        Width = 1040;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(_btnNew);
        Controls.Add(_btnEdit);
        Controls.Add(_btnClone);
        Controls.Add(_btnDelete);
        Controls.Add(_btnRefresh);
        Controls.Add(_grid);

        _grid.Columns.Add("Numero", isAvoir ? "N° Avoir" : "N° Facture");
        _grid.Columns.Add("Date", "Date");
        _grid.Columns.Add("Client", "Client");
        _grid.Columns.Add("MontantHT", "Montant HT");
        _grid.Columns.Add("MontantTVA", "TVA");
        _grid.Columns.Add("Timbre", "Timbre");
        _grid.Columns.Add("TotalTTC", "Total TTC");
        _grid.Columns.Add("EtatFacture", "État");
        _grid.Columns.Add("EtatReglement", "Règlement");

        _btnNew.Click += (_, _) => OpenEditor(null);
        _btnEdit.Click += (_, _) => EditSelected();
        _btnClone.Click += async (_, _) => await CloneSelectedAsync();
        _btnDelete.Click += async (_, _) => await DeleteSelectedAsync();
        _btnRefresh.Click += async (_, _) => await LoadAsync();
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditSelected(); };

        Load += async (_, _) => await LoadAsync();
    }

    private string? SelectedNumero()
        => _grid.SelectedRows.Count > 0 ? (string)_grid.SelectedRows[0].Cells["Numero"].Value : null;

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var factureService = scope.ServiceProvider.GetRequiredService<IFactureClientService>();

        List<Web_T4C_GestCom.Data.Models.FactureClient> factures;
        try
        {
            var entite = $"{_docLabel}s";
            Logger.DebugLoadingList(entite, $"avoirsOnly={_isAvoir}");
            factures = await factureService.GetAllAsync(avoirsOnly: _isAvoir);
            Logger.DebugListLoaded(entite, factures.Count);
        }
        catch (Exception ex)
        {
            Logger.ErrorListLoadFailed(ex, $"{_docLabel}s");
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _grid.Rows.Clear();
        foreach (var f in factures)
        {
            _grid.Rows.Add(
                f.NumeroFactureClient,
                f.DateFactureClient.ToString("dd/MM/yyyy"),
                f.Client?.NomClient,
                f.MontantHT.ToString("N3"),
                f.MontantTVA.ToString("N3"),
                f.Timbre.ToString("N3"),
                (f.MontantTTC + f.Timbre).ToString("N3"),
                f.EtatFacture,
                f.EtatReglement);
        }
    }

    private void EditSelected()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, $"Sélectionnez un {_docLabel} à modifier.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        OpenEditor(numero);
    }

    private void OpenEditor(string? numero)
    {
        using var editor = new FactureClientEditForm(numero, _isAvoir);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            _ = LoadAsync();
        }
    }

    private async Task CloneSelectedAsync()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, $"Sélectionnez un {_docLabel} à cloner.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var scope = AppHost.CreateScope();
        var factureService = scope.ServiceProvider.GetRequiredService<IFactureClientService>();
        try
        {
            Logger.DebugCloning(_docLabel, numero);
            var clone = await factureService.CloneAsync(numero, _isAvoir);
            Logger.DebugCloned(_docLabel, numero, clone.NumeroFactureClient);
            await LoadAsync();
            OpenEditor(clone.NumeroFactureClient);
        }
        catch (Exception ex)
        {
            Logger.ErrorCloneFailed(ex, _docLabel, numero);
            MessageBox.Show(this, $"Erreur lors du clonage : {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, $"Sélectionnez un {_docLabel} à supprimer.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(this, $"Supprimer {(_isAvoir ? "l'avoir" : "la facture")} {numero} ? Le stock sera réajusté.", "Confirmation",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        using var scope = AppHost.CreateScope();
        var factureService = scope.ServiceProvider.GetRequiredService<IFactureClientService>();
        try
        {
            Logger.DebugDeleting(_docLabel, numero);
            await factureService.DeleteAsync(numero);
            Logger.DebugDeleted(_docLabel, numero);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Logger.WarningDeleteFailed(ex, _docLabel, numero);
            MessageBox.Show(this, $"Erreur de suppression : {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
