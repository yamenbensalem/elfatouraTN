using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Shared;

/// <summary>One line of a product-line document (devis/commande/bon/facture), independent of which entity type it maps to.</summary>
public sealed record LineRow(string CodeProduit, double Quantite, double PrixUnitaire, double Remise, double Tva, double Fodec, double MontantHT);

/// <summary>
/// Reusable "Produit / Quantité / Prix [/ Remise] / TVA [/ FODEC] / Montant HT" grid editor, shared by
/// the Devis/CommandeVente/BonLivraison editors (with Remise, no FODEC) and the CommandeAchat/
/// BonReception/FactureFournisseur editors (no Remise, no FODEC on the purchase side — their
/// entities simply don't carry those columns). Mirrors the @for line-table + RecalculerLigne/
/// RecalculerTotaux pattern used in the matching .razor pages.
/// </summary>
public class ProductLinesEditor : Panel
{
    private readonly bool _includeFodec;
    private readonly bool _includeRemise;
    private readonly bool _usePrixAchat;
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };
    private readonly FlowLayoutPanel _buttonBar = new() { Dock = DockStyle.Bottom, Height = 32, FlowDirection = FlowDirection.LeftToRight };
    private readonly Button _btnAdd = new() { Width = 140, Text = "Ajouter une ligne" };
    private readonly Button _btnRemove = new() { Width = 140, Text = "Retirer la ligne" };

    private List<Produit> _produits = [];

    public event EventHandler? LinesChanged;

    public double TotalHT { get; private set; }
    public double TotalFodec { get; private set; }
    public double TotalTva { get; private set; }

    public ProductLinesEditor(bool includeFodec = false, bool includeRemise = true, bool usePrixAchat = false)
    {
        _includeFodec = includeFodec;
        _includeRemise = includeRemise;
        _usePrixAchat = usePrixAchat;

        Controls.Add(_grid);
        Controls.Add(_buttonBar);
        _buttonBar.Controls.Add(_btnAdd);
        _buttonBar.Controls.Add(_btnRemove);

        var colProduit = new DataGridViewComboBoxColumn
        {
            Name = "Produit",
            HeaderText = "Produit",
            DisplayMember = "DesignationProduit",
            ValueMember = "CodeProduit",
        };
        _grid.Columns.Add(colProduit);
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantite", HeaderText = "Quantité" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PrixUnitaire", HeaderText = _usePrixAchat ? "Prix Achat HT" : "Prix HT" });
        if (_includeRemise)
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remise", HeaderText = "Remise%" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tva", HeaderText = "TVA%" });
        if (_includeFodec)
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fodec", HeaderText = "FODEC%" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "MontantHT", HeaderText = "Montant HT", ReadOnly = true });

        _btnAdd.Click += (_, _) => AddRow(new LineRow("", 1, 0, 0, 19, 0, 0));
        _btnRemove.Click += (_, _) =>
        {
            if (_grid.SelectedRows.Count > 0)
            {
                _grid.Rows.Remove(_grid.SelectedRows[0]);
                Recalculate();
            }
        };

        _grid.CellEndEdit += (_, e) => OnCellEndEdit(e.RowIndex, e.ColumnIndex);
        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty && _grid.CurrentCell?.OwningColumn is DataGridViewComboBoxColumn)
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
    }

    public void SetProduits(List<Produit> produits)
    {
        _produits = produits;
        ((DataGridViewComboBoxColumn)_grid.Columns["Produit"]!).DataSource =
            produits.Select(p => new ProduitOption(p.CodeProduit, p.DesignationProduit)).ToList();
    }

    public void SetLignes(IEnumerable<LineRow> lignes)
    {
        _grid.Rows.Clear();
        foreach (var l in lignes)
            AddRow(l);
        Recalculate();
    }

    public List<LineRow> GetLignes()
    {
        var result = new List<LineRow>();
        foreach (DataGridViewRow row in _grid.Rows)
        {
            var codeProduit = row.Cells["Produit"].Value as string ?? string.Empty;
            result.Add(new LineRow(
                codeProduit,
                ParseCell(row, "Quantite"),
                ParseCell(row, "PrixUnitaire"),
                _includeRemise ? ParseCell(row, "Remise") : 0,
                ParseCell(row, "Tva"),
                _includeFodec ? ParseCell(row, "Fodec") : 0,
                ParseCell(row, "MontantHT")));
        }
        return result;
    }

    public bool HasRows => _grid.Rows.Count > 0;
    public bool HasEmptyProduit => _grid.Rows.Cast<DataGridViewRow>().Any(r => string.IsNullOrWhiteSpace(r.Cells["Produit"].Value as string));

    private void AddRow(LineRow l)
    {
        var values = new List<object> { l.CodeProduit, l.Quantite, l.PrixUnitaire };
        if (_includeRemise) values.Add(l.Remise);
        values.Add(l.Tva);
        if (_includeFodec) values.Add(l.Fodec);
        values.Add(l.MontantHT.ToString("0.###"));
        _grid.Rows.Add(values.ToArray());
    }

    private void OnCellEndEdit(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0) return;
        var row = _grid.Rows[rowIndex];
        var columnName = _grid.Columns[columnIndex].Name;

        if (columnName == "Produit")
        {
            var code = row.Cells["Produit"].Value as string;
            var produit = _produits.FirstOrDefault(p => p.CodeProduit == code);
            if (produit is not null)
            {
                var prix = _usePrixAchat && produit.PrixAchatTTC > 0 ? produit.PrixAchatTTC : produit.PrixVenteHT;
                row.Cells["PrixUnitaire"].Value = prix.ToString("0.###");
                row.Cells["Tva"].Value = (produit.TvaProduit?.TauxTvaProduit ?? 19).ToString("0.###");
                if (_includeFodec)
                    row.Cells["Fodec"].Value = produit.Fodec.ToString("0.###");
                if (_includeRemise)
                    row.Cells["Remise"].Value = "0";
            }
        }

        RecalculerLigne(row);
        Recalculate();
    }

    private static double ParseCell(DataGridViewRow row, string column)
        => double.TryParse(row.Cells[column].Value?.ToString(), out var v) ? v : 0;

    private void RecalculerLigne(DataGridViewRow row)
    {
        var quantite = ParseCell(row, "Quantite");
        var prixUnitaire = ParseCell(row, "PrixUnitaire");
        var remise = _includeRemise ? ParseCell(row, "Remise") : 0;
        var montantHT = LineCalculator.LineMontantHT(quantite, prixUnitaire, remise);
        row.Cells["MontantHT"].Value = montantHT.ToString("0.###");
    }

    private void Recalculate()
    {
        var lines = _grid.Rows.Cast<DataGridViewRow>().Select(row => new LineCalculator.LineAmounts(
            MontantHT: ParseCell(row, "MontantHT"),
            Tva: ParseCell(row, "Tva"),
            Fodec: _includeFodec ? ParseCell(row, "Fodec") : 0));

        var totals = LineCalculator.CalculateDocumentTotals(lines, remisePercent: 0);
        TotalHT = totals.TotalHT;
        TotalFodec = totals.TotalFodec;
        TotalTva = totals.TotalTva;

        LinesChanged?.Invoke(this, EventArgs.Empty);
    }
}
