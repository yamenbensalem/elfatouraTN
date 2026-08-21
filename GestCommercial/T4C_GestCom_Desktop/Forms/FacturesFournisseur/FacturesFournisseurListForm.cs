using Microsoft.Extensions.DependencyInjection;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Forms.FacturesFournisseur;

/// <summary>Desktop equivalent of Components/Pages/FacturesFournisseur/FactureFournisseurList.razor.</summary>
public class FacturesFournisseurListForm : Form
{
    private readonly Button _btnNew = new() { Left = 10, Top = 9, Width = 120, Text = "Nouvelle Facture" };
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

    public FacturesFournisseurListForm()
    {
        Text = "Factures Fournisseur";
        Width = 1040;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(_btnNew);
        Controls.Add(_btnEdit);
        Controls.Add(_btnClone);
        Controls.Add(_btnDelete);
        Controls.Add(_btnRefresh);
        Controls.Add(_grid);

        _grid.Columns.Add("Numero", "N° Facture");
        _grid.Columns.Add("Date", "Date");
        _grid.Columns.Add("Fournisseur", "Fournisseur");
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
        var factureService = scope.ServiceProvider.GetRequiredService<IFactureFournisseurService>();

        List<Web_T4C_GestCom.Data.Models.FactureFournisseur> factures;
        try
        {
            factures = await factureService.GetAllAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", "Factures Fournisseur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _grid.Rows.Clear();
        foreach (var f in factures)
        {
            _grid.Rows.Add(
                f.NumeroFactureFournisseur,
                f.DateFactureFournisseur.ToString("dd/MM/yyyy"),
                f.Fournisseur?.NomFournisseur,
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
            MessageBox.Show(this, "Sélectionnez une facture à modifier.", "Factures Fournisseur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        OpenEditor(numero);
    }

    private void OpenEditor(string? numero)
    {
        using var editor = new FactureFournisseurEditForm(numero);
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
            MessageBox.Show(this, "Sélectionnez une facture à cloner.", "Factures Fournisseur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var scope = AppHost.CreateScope();
        var factureService = scope.ServiceProvider.GetRequiredService<IFactureFournisseurService>();
        try
        {
            var clone = await factureService.CloneAsync(numero);
            await LoadAsync();
            OpenEditor(clone.NumeroFactureFournisseur);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erreur lors du clonage : {ex.Message}", "Factures Fournisseur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, "Sélectionnez une facture à supprimer.", "Factures Fournisseur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(this, $"Supprimer la facture {numero} ? Le stock sera réajusté.", "Confirmation",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        using var scope = AppHost.CreateScope();
        var factureService = scope.ServiceProvider.GetRequiredService<IFactureFournisseurService>();
        try
        {
            await factureService.DeleteAsync(numero);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erreur de suppression : {ex.Message}", "Factures Fournisseur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
