using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using T4C_GestCom_Desktop.Forms.Shared;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Forms.BonsReception;

/// <summary>Desktop equivalent of Components/Pages/BonsReception/BonReceptionForm.razor.</summary>
public class BonReceptionEditForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<BonReceptionEditForm>();

    private readonly string? _numero;
    private readonly bool _isNew;

    private readonly Label _lblNumero = new() { Left = 150, Top = 15, Width = 150, Font = new Font(Control.DefaultFont, FontStyle.Bold) };
    private readonly DateTimePicker _dtDate = new() { Left = 150, Top = 45, Width = 150, Format = DateTimePickerFormat.Short };
    private readonly ComboBox _cmbFournisseur = new() { Left = 320, Top = 45, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomFournisseur", ValueMember = "CodeFournisseur" };
    private readonly ComboBox _cmbCommande = new() { Left = 150, Top = 75, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NumeroCommandeAchat", ValueMember = "NumeroCommandeAchat" };
    private readonly ComboBox _cmbEtat = new() { Left = 420, Top = 75, Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbEtatFacture = new() { Left = 150, Top = 105, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtNote = new() { Left = 150, Top = 135, Width = 530, Height = 40, Multiline = true };

    private readonly ProductLinesEditor _lignesEditor = new(includeFodec: false, includeRemise: false, usePrixAchat: true)
        { Left = 15, Top = 185, Width = 830, Height = 220, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

    private readonly Label _lblTotalHT = new() { Left = 600, Top = 420, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalTVA = new() { Left = 600, Top = 440, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalTTC = new() { Left = 600, Top = 460, Width = 245, TextAlign = ContentAlignment.MiddleRight, Font = new Font(Control.DefaultFont, FontStyle.Bold) };

    private readonly Label _lblError = new() { Left = 15, Top = 490, Width = 700, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnSave = new() { Left = 15, Top = 515, Width = 100, Text = "Enregistrer" };
    private readonly Button _btnCancel = new() { Left = 125, Top = 515, Width = 100, Text = "Annuler" };
    private readonly Button _btnPrint = new() { Left = 235, Top = 515, Width = 110, Text = "Imprimer" };

    public BonReceptionEditForm(string? numero)
    {
        _numero = numero;
        _isNew = numero is null;

        Text = _isNew ? "Nouveau Bon de Réception" : $"Bon de Réception : {numero}";
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = true;
        MinimizeBox = false;
        ClientSize = new Size(870, 555);
        AutoScroll = true;
        CancelButton = _btnCancel;

        Controls.Add(new Label { Left = 15, Top = 18, Width = 130, Text = "N° Bon" });
        Controls.Add(_lblNumero);
        Controls.Add(new Label { Left = 15, Top = 48, Width = 130, Text = "Date *" });
        Controls.Add(_dtDate);
        Controls.Add(new Label { Left = 275, Top = 48, Width = 50, Text = "Fournisseur *" });
        Controls.Add(_cmbFournisseur);
        Controls.Add(new Label { Left = 15, Top = 78, Width = 130, Text = "Commande Achat" });
        Controls.Add(_cmbCommande);
        Controls.Add(new Label { Left = 405, Top = 78, Width = 20, Text = "État" });
        Controls.Add(_cmbEtat);
        Controls.Add(new Label { Left = 15, Top = 108, Width = 130, Text = "État Facturation" });
        Controls.Add(_cmbEtatFacture);
        Controls.Add(new Label { Left = 15, Top = 138, Width = 130, Text = "Note" });
        Controls.Add(_txtNote);
        Controls.Add(_lignesEditor);

        Controls.Add(new Label { Left = 380, Top = 420, Width = 220, Text = "Total HT" });
        Controls.Add(_lblTotalHT);
        Controls.Add(new Label { Left = 380, Top = 440, Width = 220, Text = "TVA" });
        Controls.Add(_lblTotalTVA);
        Controls.Add(new Label { Left = 380, Top = 460, Width = 220, Text = "Total TTC" });
        Controls.Add(_lblTotalTTC);

        Controls.Add(_lblError);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);
        if (!_isNew) Controls.Add(_btnPrint);

        _cmbEtat.Items.AddRange(["Ouvert", "Confirmé", "Annulé"]);
        _cmbEtatFacture.Items.AddRange(["Non Facturé", "Facturé"]);
        _lignesEditor.LinesChanged += (_, _) => RecalculerTotaux();
        _cmbFournisseur.SelectedIndexChanged += async (_, _) => await OnFournisseurChangedAsync();

        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        _btnPrint.Click += async (_, _) => await PrintAsync();

        Load += async (_, _) => await LoadAsync();
    }

    private void RecalculerTotaux()
    {
        var totalTTC = Math.Round(_lignesEditor.TotalHT + _lignesEditor.TotalTva, 3);

        _lblTotalHT.Text = _lignesEditor.TotalHT.ToString("N3");
        _lblTotalTVA.Text = _lignesEditor.TotalTva.ToString("N3");
        _lblTotalTTC.Text = totalTTC.ToString("N3");
    }

    private async Task OnFournisseurChangedAsync()
    {
        _cmbCommande.DataSource = null;
        _cmbCommande.Items.Clear();

        if (_cmbFournisseur.SelectedValue is not string codeFournisseur || string.IsNullOrWhiteSpace(codeFournisseur))
            return;

        using var scope = AppHost.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var commandes = await db.CommandesAchat
            .Where(c => c.CodeFournisseur == codeFournisseur)
            .OrderByDescending(c => c.DateCommandeAchat)
            .ToListAsync();

        _cmbCommande.DataSource = commandes;
        _cmbCommande.SelectedIndex = -1;
    }

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _cmbFournisseur.DataSource = await db.Fournisseurs.OrderBy(f => f.NomFournisseur).ToListAsync();
        var produits = await db.Produits.Include(p => p.TvaProduit).OrderBy(p => p.DesignationProduit).ToListAsync();
        _lignesEditor.SetProduits(produits);

        if (!_isNew)
        {
            var bonService = scope.ServiceProvider.GetRequiredService<IBonReceptionService>();
            var bon = await bonService.GetByNumeroAsync(_numero!);
            if (bon is null)
            {
                Logger.WarningNotFound("Bon de réception", _numero);
                MessageBox.Show(this, "Bon de réception introuvable.", "Bon de Réception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _lblNumero.Text = bon.NumeroBonReception;
            _dtDate.Value = bon.DateBonReception;
            _cmbFournisseur.SelectedValue = bon.CodeFournisseur;
            await OnFournisseurChangedAsync();
            _cmbCommande.SelectedValue = bon.NumeroCommandeAchat;
            _cmbEtat.SelectedItem = bon.EtatBonReception;
            _cmbEtatFacture.SelectedItem = bon.EtatFacture;
            _txtNote.Text = bon.Note;

            _lignesEditor.SetLignes(bon.Lignes.Select(l =>
                new LineRow(l.CodeProduit, l.Quantite, l.PrixUnitaire, 0, l.Tva, 0, l.MontantHT)));
        }
        else
        {
            _lblNumero.Text = "Auto-généré";
            _dtDate.Value = DateTime.Today;
            _cmbEtat.SelectedItem = "Ouvert";
            _cmbEtatFacture.SelectedItem = "Non Facturé";
        }

        RecalculerTotaux();
    }

    private async Task SaveAsync()
    {
        _lblError.Text = string.Empty;

        if (_cmbFournisseur.SelectedValue is not string codeFournisseur || string.IsNullOrWhiteSpace(codeFournisseur))
        {
            _lblError.Text = "Le fournisseur est obligatoire.";
            return;
        }

        if (!_lignesEditor.HasRows)
        {
            _lblError.Text = "Ajoutez au moins une ligne.";
            return;
        }

        if (_lignesEditor.HasEmptyProduit)
        {
            _lblError.Text = "Chaque ligne doit avoir un produit sélectionné.";
            return;
        }

        var lignes = _lignesEditor.GetLignes().Select(l => new LigneBonReception
        {
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Tva = l.Tva,
            MontantHT = l.MontantHT,
        }).ToList();

        var bon = new BonReception
        {
            NumeroBonReception = _numero ?? string.Empty,
            DateBonReception = _dtDate.Value.Date,
            CodeFournisseur = codeFournisseur,
            NumeroCommandeAchat = _cmbCommande.SelectedValue as string,
            EtatBonReception = (string)(_cmbEtat.SelectedItem ?? "Ouvert"),
            EtatFacture = (string)(_cmbEtatFacture.SelectedItem ?? "Non Facturé"),
            Note = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim(),
        };

        _btnSave.Enabled = false;
        try
        {
            Logger.DebugSaving("bon de réception", _numero, _isNew, lignes.Count);
            using var scope = AppHost.CreateScope();
            var bonService = scope.ServiceProvider.GetRequiredService<IBonReceptionService>();

            if (_isNew)
                await bonService.CreateAsync(bon, lignes);
            else
                await bonService.UpdateAsync(bon, lignes);

            Logger.DebugSaved("Bon de réception", bon.NumeroBonReception);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Logger.ErrorSaveFailed(ex, "bon de réception", _numero);
            _lblError.Text = $"Erreur : {ex.Message}";
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }

    private async Task PrintAsync()
    {
        if (_numero is null) return;

        _btnPrint.Enabled = false;
        try
        {
            using var scope = AppHost.CreateScope();
            var bonService = scope.ServiceProvider.GetRequiredService<IBonReceptionService>();
            var bon = await bonService.GetByNumeroAsync(_numero);
            if (bon is null) return;

            var rows = bon.Lignes.Select(l => (IReadOnlyList<string>)new[]
            {
                l.Produit?.DesignationProduit ?? l.CodeProduit,
                l.Quantite.ToString("0.###"),
                l.PrixUnitaire.ToString("0.###"),
                $"{l.Tva:0.##}%",
                l.MontantHT.ToString("0.###"),
            }).ToList();

            var totals = new List<(string, string, bool)>
            {
                ("Total HT", bon.MontantHT.ToString("0.###"), false),
                ("TVA", bon.MontantTVA.ToString("0.###"), false),
                ("Total TTC", $"{bon.MontantTTC:0.###} TND", true),
            };

            var headerRight = new List<(string, string)> { ("Facturation", bon.EtatFacture) };
            if (!string.IsNullOrWhiteSpace(bon.NumeroCommandeAchat))
                headerRight.Add(("Commande liée", bon.NumeroCommandeAchat));

            var model = new PrintDocumentModel(
                DocType: "BON DE RÉCEPTION",
                Numero: bon.NumeroBonReception,
                Date: bon.DateBonReception,
                Etat: bon.EtatBonReception,
                PartyLabel: "Fournisseur",
                PartyName: bon.Fournisseur?.NomFournisseur ?? bon.CodeFournisseur,
                PartyDetails: PartyDetailsHelper.ForFournisseur(bon.Fournisseur),
                HeaderRight: headerRight,
                ColumnHeaders: ["Désignation", "Qté", "Prix Achat HT", "TVA%", "Montant HT"],
                Rows: rows,
                Totals: totals,
                Note: bon.Note,
                Reglements: null,
                EntrepriseFooter: null);

            PrintDocumentBuilder.PreviewInBrowser(model);
        }
        catch (Exception ex)
        {
            Logger.ErrorPrintFailed(ex, "bon de réception", _numero);
            MessageBox.Show(this, $"Erreur : {ex.Message}", "Impression", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnPrint.Enabled = true;
        }
    }
}
