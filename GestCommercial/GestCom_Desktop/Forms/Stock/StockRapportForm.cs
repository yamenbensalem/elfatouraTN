using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Forms.Shared;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Stock;

/// <summary>Desktop equivalent of Components/Pages/Stock/StockRapport.razor.</summary>
public class StockRapportForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<StockRapportForm>();

    private readonly TextBox _txtSearch = new() { Left = 10, Top = 10, Width = 250 };
    private readonly ComboBox _cmbCategorie = new() { Left = 270, Top = 9, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Label", ValueMember = "Code" };
    private readonly CheckBox _chkAlerteOnly = new() { Left = 480, Top = 12, Width = 160, Text = "Alertes uniquement" };
    private readonly Button _btnRefresh = new() { Left = 650, Top = 9, Width = 90, Text = "Actualiser" };
    private readonly Button _btnExport = new() { Left = 750, Top = 9, Width = 110, Text = "Export Excel" };

    private readonly Label _lblTotalProduits = new() { Left = 10, Top = 45, Width = 220, Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold) };
    private readonly Label _lblEnAlerte = new() { Left = 240, Top = 45, Width = 220, Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), ForeColor = Color.DarkOrange };
    private readonly Label _lblValeurAchat = new() { Left = 470, Top = 45, Width = 260, Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), ForeColor = Color.SeaGreen };
    private readonly Label _lblValeurVente = new() { Left = 740, Top = 45, Width = 260, Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), ForeColor = Color.SteelBlue };

    private readonly DataGridView _grid = new()
    {
        Left = 10,
        Top = 75,
        Width = 1000,
        Height = 470,
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };

    private List<Produit> _all = [];

    /// <summary>Named type instead of mixing plain strings with CategorieProduit in the same
    /// ComboBox — WinForms' DisplayMember binding needs one consistent shape per item, and this
    /// also carries the "(Toutes catégories)" sentinel (Code = null) alongside real categories.</summary>
    private sealed record CategorieOption(int? Code, string Label);

    public StockRapportForm()
    {
        Text = "État du Stock";
        Width = 1040;
        Height = 620;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(_txtSearch);
        Controls.Add(_cmbCategorie);
        Controls.Add(_chkAlerteOnly);
        Controls.Add(_btnRefresh);
        Controls.Add(_btnExport);
        Controls.Add(_lblTotalProduits);
        Controls.Add(_lblEnAlerte);
        Controls.Add(_lblValeurAchat);
        Controls.Add(_lblValeurVente);
        Controls.Add(_grid);

        _txtSearch.PlaceholderText = "Code ou désignation...";

        _grid.Columns.Add("Code", "Code");
        _grid.Columns.Add("Designation", "Désignation");
        _grid.Columns.Add("Categorie", "Catégorie");
        _grid.Columns.Add("Quantite", "Qté Stock");
        _grid.Columns.Add("StockMinimal", "Stock Min");
        _grid.Columns.Add("Unite", "Unité");
        _grid.Columns.Add("PrixAchatTTC", "PA TTC");
        _grid.Columns.Add("PrixVenteHT", "PV HT");
        _grid.Columns.Add("ValeurStock", "Valeur Stock");
        _grid.Columns.Add("Etat", "État");

        _txtSearch.TextChanged += (_, _) => ApplyFilter();
        _cmbCategorie.SelectedIndexChanged += (_, _) => ApplyFilter();
        _chkAlerteOnly.CheckedChanged += (_, _) => ApplyFilter();
        _btnRefresh.Click += async (_, _) => await LoadAsync();
        _btnExport.Click += (_, _) => ExcelExportHelper.ExportGrid(this, _grid, "stock", "État du Stock");

        Load += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var produitService = scope.ServiceProvider.GetRequiredService<IProduitService>();

        try
        {
            Logger.DebugLoadingList("état du stock");
            var categories = await db.CategoriesProduit.AsNoTracking().ToListAsync();
            _all = await produitService.GetAllAsync();
            Logger.DebugListLoaded("état du stock", _all.Count);

            var selectedCode = (_cmbCategorie.SelectedItem as CategorieOption)?.Code;
            List<CategorieOption> options =
            [
                new CategorieOption(null, "(Toutes catégories)"),
                .. categories.Select(c => new CategorieOption(c.CodeCategorieProduit, c.NomCategorieProduit)),
            ];
            _cmbCategorie.DataSource = options;
            _cmbCategorie.SelectedItem = options.FirstOrDefault(o => o.Code == selectedCode) ?? options[0];
        }
        catch (Exception ex)
        {
            Logger.ErrorListLoadFailed(ex, "état du stock");
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", "État du Stock", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var search = _txtSearch.Text.Trim();
        var categorieCode = (_cmbCategorie.SelectedItem as CategorieOption)?.Code;
        var alerteOnly = _chkAlerteOnly.Checked;

        var filtered = _all.Where(p =>
            (string.IsNullOrEmpty(search)
                || p.DesignationProduit.Contains(search, StringComparison.OrdinalIgnoreCase)
                || p.CodeProduit.Contains(search, StringComparison.OrdinalIgnoreCase))
            && (categorieCode is null || p.CodeCategorieProduit == categorieCode)
            && (!alerteOnly || p.Quantite <= p.StockMinimal))
            .ToList();

        _grid.Rows.Clear();
        foreach (var p in filtered)
        {
            var enAlerte = p.Quantite <= p.StockMinimal;
            var row = _grid.Rows[_grid.Rows.Add(
                p.CodeProduit,
                p.DesignationProduit,
                p.CategorieProduit?.NomCategorieProduit,
                p.Quantite.ToString("0.###"),
                p.StockMinimal.ToString("0.###"),
                p.UniteProduit?.NomUniteProduit,
                p.PrixAchatTTC.ToString("0.###"),
                p.PrixVenteHT.ToString("0.###"),
                (p.Quantite * p.PrixVenteHT).ToString("0.###"),
                enAlerte ? "Alerte" : "OK")];
            if (enAlerte)
                row.DefaultCellStyle.BackColor = Color.MistyRose;
        }

        _lblTotalProduits.Text = $"{_all.Count} référence(s)";
        _lblEnAlerte.Text = $"{_all.Count(p => p.Quantite <= p.StockMinimal)} en alerte stock";
        _lblValeurAchat.Text = $"Valeur stock (PA TTC) : {_all.Sum(p => p.Quantite * p.PrixAchatTTC):0.###}";
        _lblValeurVente.Text = $"Valeur stock (PV HT) : {_all.Sum(p => p.Quantite * p.PrixVenteHT):0.###}";
    }
}
