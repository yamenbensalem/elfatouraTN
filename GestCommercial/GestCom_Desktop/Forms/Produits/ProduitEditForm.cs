using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Forms.Shared;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Produits;

/// <summary>Desktop equivalent of Components/Pages/Produits/ProduitForm.razor.</summary>
public class ProduitEditForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<ProduitEditForm>();

    private readonly string? _code;
    private readonly bool _isNew;
    private List<TvaProduit> _tvas = [];
    private bool _loaded;

    private readonly TextBox _txtCode = new() { Left = 170, Top = 20, Width = 150 };
    private readonly TextBox _txtDesignation = new() { Left = 170, Top = 50, Width = 320 };
    private readonly ComboBox _cmbCategorie = new() { Left = 170, Top = 80, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomCategorieProduit", ValueMember = "CodeCategorieProduit" };
    private readonly ComboBox _cmbUnite = new() { Left = 170, Top = 110, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomUniteProduit", ValueMember = "CodeUniteProduit" };
    private readonly ComboBox _cmbFabriquant = new() { Left = 170, Top = 140, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomFabriquantProduit", ValueMember = "CodeFabriquantProduit" };
    private readonly ComboBox _cmbDevise = new() { Left = 170, Top = 170, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomDevise", ValueMember = "CodeDevise" };

    private readonly NumericUpDown _numPrixAchatTTC = new() { Left = 170, Top = 210, Width = 120, DecimalPlaces = 3, Maximum = 999_999_999 };
    private readonly NumericUpDown _numTauxMarge = new() { Left = 170, Top = 240, Width = 120, DecimalPlaces = 2, Maximum = 100_000 };
    private readonly ComboBox _cmbTva = new() { Left = 170, Top = 270, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomTvaProduit", ValueMember = "CodeTvaProduit" };
    private readonly NumericUpDown _numFodec = new() { Left = 170, Top = 300, Width = 120, DecimalPlaces = 2, Maximum = 100_000 };
    private readonly NumericUpDown _numPrixVenteHT = new() { Left = 170, Top = 330, Width = 120, DecimalPlaces = 3, Maximum = 999_999_999 };
    private readonly NumericUpDown _numPrixVenteTTC = new() { Left = 170, Top = 360, Width = 120, DecimalPlaces = 3, Maximum = 999_999_999 };
    private readonly NumericUpDown _numRemise = new() { Left = 170, Top = 390, Width = 120, DecimalPlaces = 2, Maximum = 100 };
    private readonly NumericUpDown _numRemiseMax = new() { Left = 170, Top = 420, Width = 120, DecimalPlaces = 2, Maximum = 100 };

    private readonly NumericUpDown _numQuantite = new() { Left = 170, Top = 460, Width = 120, DecimalPlaces = 3, Maximum = 999_999_999 };
    private readonly NumericUpDown _numStockMinimal = new() { Left = 170, Top = 490, Width = 120, DecimalPlaces = 3, Maximum = 999_999_999 };
    private readonly TextBox _txtRayon = new() { Left = 170, Top = 520, Width = 150 };
    private readonly TextBox _txtEtage = new() { Left = 170, Top = 550, Width = 150 };

    private readonly Label _lblError = new() { Left = 20, Top = 590, Width = 480, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnSave = new() { Left = 170, Top = 620, Width = 100, Text = "Enregistrer" };
    private readonly Button _btnCancel = new() { Left = 280, Top = 620, Width = 100, Text = "Annuler" };

    public ProduitEditForm(string? code)
    {
        _code = code;
        _isNew = code is null;

        Text = _isNew ? "Nouveau Produit" : $"Produit : {code}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 660);
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        AddField("Code Produit", _txtCode, 20);
        AddField("Désignation *", _txtDesignation, 50);
        AddField("Catégorie", _cmbCategorie, 80);
        AddField("Unité", _cmbUnite, 110);
        AddField("Fabricant", _cmbFabriquant, 140);
        AddField("Devise", _cmbDevise, 170);
        AddField("Prix Achat TTC", _numPrixAchatTTC, 210);
        AddField("Taux Marge (%)", _numTauxMarge, 240);
        AddField("TVA", _cmbTva, 270);
        AddField("FODEC (%)", _numFodec, 300);
        AddField("Prix Vente HT", _numPrixVenteHT, 330);
        AddField("Prix Vente TTC", _numPrixVenteTTC, 360);
        AddField("Remise (%)", _numRemise, 390);
        AddField("Remise Max (%)", _numRemiseMax, 420);
        AddField("Quantité", _numQuantite, 460);
        AddField("Stock Minimal", _numStockMinimal, 490);
        AddField("Rayon", _txtRayon, 520);
        AddField("Étage", _txtEtage, 550);

        Controls.Add(_lblError);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);

        _txtCode.Enabled = _isNew;
        _txtCode.PlaceholderText = "Auto-généré";

        _numPrixAchatTTC.ValueChanged += (_, _) => RecalculatePrix();
        _numTauxMarge.ValueChanged += (_, _) => RecalculatePrix();
        _numFodec.ValueChanged += (_, _) => RecalculatePrix();
        _cmbTva.SelectedIndexChanged += (_, _) => RecalculatePrix();

        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Load += async (_, _) => await LoadAsync();
    }

    private void AddField(string label, Control input, int top)
    {
        Controls.Add(new Label { Left = 20, Top = top + 3, Width = 140, Text = label });
        Controls.Add(input);
    }

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _cmbCategorie.DataSource = await db.CategoriesProduit.ToListAsync();
        _cmbUnite.DataSource = await db.UnitesProduit.ToListAsync();
        _cmbFabriquant.DataSource = await db.FabriquantsProduit.ToListAsync();
        _cmbDevise.DataSource = await db.Devises.ToListAsync();
        _tvas = await db.TvasProduit.ToListAsync();
        _cmbTva.DataSource = _tvas;

        if (!_isNew)
        {
            var produitService = scope.ServiceProvider.GetRequiredService<IProduitService>();
            var produit = await produitService.GetByCodeAsync(_code!);
            if (produit is null)
            {
                Logger.WarningNotFound("Produit", _code);
                MessageBox.Show(this, "Produit introuvable.", "Produit", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            Populate(produit);
        }

        _loaded = true;
    }

    private void Populate(Produit p)
    {
        _txtCode.Text = p.CodeProduit;
        _txtDesignation.Text = p.DesignationProduit;
        _cmbCategorie.SelectedValue = p.CodeCategorieProduit;
        _cmbUnite.SelectedValue = p.CodeUniteProduit;
        _cmbFabriquant.SelectedValue = p.CodeFabriquantProduit;
        _cmbDevise.SelectedValue = p.CodeDevise;
        _numPrixAchatTTC.Value = (decimal)p.PrixAchatTTC;
        _numTauxMarge.Value = (decimal)p.TauxMarge;
        _cmbTva.SelectedValue = p.CodeTvaProduit;
        _numFodec.Value = (decimal)p.Fodec;
        _numPrixVenteHT.Value = (decimal)p.PrixVenteHT;
        _numPrixVenteTTC.Value = (decimal)p.PrixVenteTTC;
        _numRemise.Value = (decimal)p.Remise;
        _numRemiseMax.Value = (decimal)p.RemiseMaximale;
        _numQuantite.Value = (decimal)p.Quantite;
        _numStockMinimal.Value = (decimal)p.StockMinimal;
        _txtRayon.Text = p.Rayon;
        _txtEtage.Text = p.Etage;
    }

    private void RecalculatePrix()
    {
        if (!_loaded) return;

        var tva = _tvas.FirstOrDefault(t => t.CodeTvaProduit == (int)(_cmbTva.SelectedValue ?? 0))?.TauxTvaProduit ?? 0;
        var prixVenteHT = Math.Round((double)_numPrixAchatTTC.Value * (1 + (double)_numTauxMarge.Value / 100), 3);
        var prixVenteTTC = Math.Round(prixVenteHT * (1 + ((double)_numFodec.Value + tva) / 100), 3);

        _numPrixVenteHT.Value = (decimal)prixVenteHT;
        _numPrixVenteTTC.Value = (decimal)prixVenteTTC;
    }

    private async Task SaveAsync()
    {
        _lblError.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(_txtDesignation.Text))
        {
            _lblError.Text = "La désignation est obligatoire.";
            return;
        }

        _btnSave.Enabled = false;
        try
        {
            Logger.DebugSaving("produit", _code, _isNew);
            using var scope = AppHost.CreateScope();
            var produitService = scope.ServiceProvider.GetRequiredService<IProduitService>();

            // UpdateAsync does db.Produits.Update(produit) — a full entity replace. For an existing
            // produit this must be the loaded entity, mutated, not a freshly built one: a fresh object
            // would carry CompanyId = null (not exposed in this form) and either fail tenant-ownership
            // validation or, for a SuperAdmin session where that check is skipped, silently orphan the
            // produit from its tenant. Mirrors ProduitForm.razor, which binds _produit = the loaded entity.
            var produit = _isNew ? new Produit() : await produitService.GetByCodeAsync(_code!)
                ?? throw new InvalidOperationException("Produit introuvable.");

            produit.CodeProduit = _txtCode.Text.Trim();
            produit.DesignationProduit = _txtDesignation.Text.Trim();
            produit.CodeCategorieProduit = (int)(_cmbCategorie.SelectedValue ?? 1);
            produit.CodeUniteProduit = (int)(_cmbUnite.SelectedValue ?? 1);
            produit.CodeFabriquantProduit = (int)(_cmbFabriquant.SelectedValue ?? 1);
            produit.CodeDevise = (int)(_cmbDevise.SelectedValue ?? 1);
            produit.PrixAchatTTC = (double)_numPrixAchatTTC.Value;
            produit.TauxMarge = (double)_numTauxMarge.Value;
            produit.CodeTvaProduit = (int)(_cmbTva.SelectedValue ?? 1);
            produit.Fodec = (double)_numFodec.Value;
            produit.PrixVenteHT = (double)_numPrixVenteHT.Value;
            produit.PrixVenteTTC = (double)_numPrixVenteTTC.Value;
            produit.Remise = (double)_numRemise.Value;
            produit.RemiseMaximale = (double)_numRemiseMax.Value;
            produit.Quantite = (double)_numQuantite.Value;
            produit.StockMinimal = (double)_numStockMinimal.Value;
            produit.Rayon = string.IsNullOrWhiteSpace(_txtRayon.Text) ? null : _txtRayon.Text.Trim();
            produit.Etage = string.IsNullOrWhiteSpace(_txtEtage.Text) ? null : _txtEtage.Text.Trim();

            if (_isNew)
                await produitService.AddAsync(produit);
            else
                await produitService.UpdateAsync(produit);

            Logger.DebugSaved("Produit", produit.CodeProduit);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Logger.ErrorSaveFailed(ex, "produit", _code);
            _lblError.Text = $"Erreur : {ex.Message}";
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }
}
