using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Forms.Shared;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Produits;

/// <summary>Desktop equivalent of Components/Pages/Produits/ProduitsList.razor.</summary>
public class ProduitsListForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<ProduitsListForm>();

    private readonly TextBox _txtSearch = new() { Left = 10, Top = 10, Width = 250 };
    private readonly Button _btnSearch = new() { Left = 265, Top = 9, Width = 90, Text = "Rechercher" };
    private readonly Button _btnNew = new() { Left = 365, Top = 9, Width = 90, Text = "Nouveau" };
    private readonly Button _btnEdit = new() { Left = 465, Top = 9, Width = 90, Text = "Modifier" };
    private readonly Button _btnDelete = new() { Left = 565, Top = 9, Width = 90, Text = "Supprimer" };
    private readonly DataGridView _grid = new()
    {
        Left = 10,
        Top = 45,
        Width = 900,
        Height = 500,
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };

    public ProduitsListForm()
    {
        Text = "Produits";
        Width = 940;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(_txtSearch);
        Controls.Add(_btnSearch);
        Controls.Add(_btnNew);
        Controls.Add(_btnEdit);
        Controls.Add(_btnDelete);
        Controls.Add(_grid);

        _grid.Columns.Add("CodeProduit", "Code");
        _grid.Columns.Add("DesignationProduit", "Désignation");
        _grid.Columns.Add("CategorieProduit", "Catégorie");
        _grid.Columns.Add("PrixVenteHT", "P.V HT");
        _grid.Columns.Add("PrixVenteTTC", "P.V TTC");
        _grid.Columns.Add("Quantite", "Stock");
        _grid.Columns.Add("UniteProduit", "Unité");
        _grid.Columns.Add("TvaProduit", "TVA");
        _grid.Columns.Add("StockMinimal", "Stock Min.");

        _btnSearch.Click += async (_, _) => await LoadAsync();
        _txtSearch.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await LoadAsync(); } };
        _btnNew.Click += (_, _) => OpenEditor(null);
        _btnEdit.Click += (_, _) => EditSelected();
        _btnDelete.Click += async (_, _) => await DeleteSelectedAsync();
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditSelected(); };

        Load += async (_, _) => await LoadAsync();
    }

    private string? SelectedCode()
        => _grid.SelectedRows.Count > 0 ? (string)_grid.SelectedRows[0].Cells["CodeProduit"].Value : null;

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var produitService = scope.ServiceProvider.GetRequiredService<IProduitService>();

        List<Produit> produits;
        try
        {
            Logger.DebugLoadingList("produits", $"recherche={_txtSearch.Text.Trim()}");
            produits = await produitService.GetAllAsync(_txtSearch.Text.Trim() is { Length: > 0 } s ? s : null);
            Logger.DebugListLoaded("produits", produits.Count);
        }
        catch (Exception ex)
        {
            Logger.ErrorListLoadFailed(ex, "produits");
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", "Produits", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _grid.Rows.Clear();
        foreach (var p in produits)
        {
            var row = _grid.Rows[_grid.Rows.Add(
                p.CodeProduit,
                p.DesignationProduit,
                p.CategorieProduit?.NomCategorieProduit,
                p.PrixVenteHT.ToString("N3"),
                p.PrixVenteTTC.ToString("N3"),
                p.Quantite,
                p.UniteProduit?.NomUniteProduit,
                p.TvaProduit is null ? null : $"{p.TvaProduit.TauxTvaProduit:0.##}%",
                p.StockMinimal)];
            if (p.Quantite <= p.StockMinimal)
                row.DefaultCellStyle.BackColor = Color.MistyRose;
        }
    }

    private void EditSelected()
    {
        var code = SelectedCode();
        if (code is null)
        {
            MessageBox.Show(this, "Sélectionnez un produit à modifier.", "Produits", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        OpenEditor(code);
    }

    private void OpenEditor(string? code)
    {
        using var editor = new ProduitEditForm(code);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            _ = LoadAsync();
        }
    }

    private async Task DeleteSelectedAsync()
    {
        var code = SelectedCode();
        if (code is null)
        {
            MessageBox.Show(this, "Sélectionnez un produit à supprimer.", "Produits", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(this, $"Supprimer le produit {code} ?", "Confirmation",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        using var scope = AppHost.CreateScope();
        var produitService = scope.ServiceProvider.GetRequiredService<IProduitService>();
        try
        {
            Logger.DebugDeleting("produit", code);
            await produitService.DeleteAsync(code);
            Logger.DebugDeleted("Produit", code);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Logger.WarningDeleteFailed(ex, "produit", code);
            var message = DeleteErrorMessageHelper.Build(ex,
                "Ce produit ne peut pas etre supprime car il est utilise dans des lignes de devis, commandes, bons ou factures. Supprimez d'abord les documents lies, puis reessayez.");
            MessageBox.Show(this, message, "Suppression impossible", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
