using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Forms.Shared;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.BonsReception;

/// <summary>Desktop equivalent of Components/Pages/BonsReception/BonReceptionList.razor.</summary>
public class BonsReceptionListForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<BonsReceptionListForm>();

    private readonly Button _btnNew = new() { Left = 10, Top = 9, Width = 110, Text = "Nouveau Bon" };
    private readonly Button _btnEdit = new() { Left = 125, Top = 9, Width = 90, Text = "Modifier" };
    private readonly Button _btnClone = new() { Left = 225, Top = 9, Width = 90, Text = "Cloner" };
    private readonly Button _btnDelete = new() { Left = 325, Top = 9, Width = 90, Text = "Supprimer" };
    private readonly Button _btnRefresh = new() { Left = 425, Top = 9, Width = 90, Text = "Actualiser" };
    private readonly DataGridView _grid = new()
    {
        Left = 10,
        Top = 45,
        Width = 950,
        Height = 500,
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };

    public BonsReceptionListForm()
    {
        Text = "Bons de Réception";
        Width = 990;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(_btnNew);
        Controls.Add(_btnEdit);
        Controls.Add(_btnClone);
        Controls.Add(_btnDelete);
        Controls.Add(_btnRefresh);
        Controls.Add(_grid);

        _grid.Columns.Add("Numero", "N° Bon");
        _grid.Columns.Add("Date", "Date");
        _grid.Columns.Add("Fournisseur", "Fournisseur");
        _grid.Columns.Add("NumeroCommande", "Commande Achat");
        _grid.Columns.Add("MontantHT", "Montant HT");
        _grid.Columns.Add("MontantTVA", "TVA");
        _grid.Columns.Add("MontantTTC", "Total TTC");
        _grid.Columns.Add("EtatBonReception", "État");
        _grid.Columns.Add("EtatFacture", "Facturation");

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
        var bonService = scope.ServiceProvider.GetRequiredService<IBonReceptionService>();

        List<BonReception> bons;
        try
        {
            Logger.DebugLoadingList("bons de réception");
            bons = await bonService.GetAllAsync();
            Logger.DebugListLoaded("bons de réception", bons.Count);
        }
        catch (Exception ex)
        {
            Logger.ErrorListLoadFailed(ex, "bons de réception");
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", "Bons de Réception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _grid.Rows.Clear();
        foreach (var b in bons)
        {
            _grid.Rows.Add(
                b.NumeroBonReception,
                b.DateBonReception.ToString("dd/MM/yyyy"),
                b.Fournisseur?.NomFournisseur,
                b.NumeroCommandeAchat ?? "-",
                b.MontantHT.ToString("N3"),
                b.MontantTVA.ToString("N3"),
                b.MontantTTC.ToString("N3"),
                b.EtatBonReception,
                b.EtatFacture);
        }
    }

    private void EditSelected()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, "Sélectionnez un bon à modifier.", "Bons de Réception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        OpenEditor(numero);
    }

    private void OpenEditor(string? numero)
    {
        using var editor = new BonReceptionEditForm(numero);
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
            MessageBox.Show(this, "Sélectionnez un bon à cloner.", "Bons de Réception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var scope = AppHost.CreateScope();
        var bonService = scope.ServiceProvider.GetRequiredService<IBonReceptionService>();
        try
        {
            Logger.DebugCloning("bon de réception", numero);
            var clone = await bonService.CloneAsync(numero);
            Logger.DebugCloned("Bon de réception", numero, clone.NumeroBonReception);
            await LoadAsync();
            OpenEditor(clone.NumeroBonReception);
        }
        catch (Exception ex)
        {
            Logger.ErrorCloneFailed(ex, "bon de réception", numero);
            MessageBox.Show(this, $"Erreur lors du clonage : {ex.Message}", "Bons de Réception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, "Sélectionnez un bon à supprimer.", "Bons de Réception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(this, $"Supprimer le bon {numero} ? Le stock sera réajusté.", "Confirmation",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        using var scope = AppHost.CreateScope();
        var bonService = scope.ServiceProvider.GetRequiredService<IBonReceptionService>();
        try
        {
            Logger.DebugDeleting("bon de réception", numero);
            await bonService.DeleteAsync(numero);
            Logger.DebugDeleted("Bon de réception", numero);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Logger.WarningDeleteFailed(ex, "bon de réception", numero);
            MessageBox.Show(this, $"Erreur de suppression : {ex.Message}", "Bons de Réception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
