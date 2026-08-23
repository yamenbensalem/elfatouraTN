using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using T4C_GestCom_Desktop.Forms.Shared;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Forms.FacturesFournisseur;

/// <summary>Desktop equivalent of Components/Pages/FacturesFournisseur/FactureFournisseurForm.razor.</summary>
public class FactureFournisseurEditForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<FactureFournisseurEditForm>();

    private readonly string? _numero;
    private readonly bool _isNew;
    private double _timbreFiscal;

    private readonly Label _lblNumero = new() { Left = 150, Top = 15, Width = 150, Font = new Font(Control.DefaultFont, FontStyle.Bold) };
    private readonly DateTimePicker _dtDate = new() { Left = 150, Top = 45, Width = 150, Format = DateTimePickerFormat.Short };
    private readonly ComboBox _cmbFournisseur = new() { Left = 320, Top = 45, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomFournisseur", ValueMember = "CodeFournisseur" };
    private readonly ComboBox _cmbEtat = new() { Left = 150, Top = 75, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _lblTimbre = new() { Left = 470, Top = 78, Width = 150 };
    private readonly TextBox _txtNote = new() { Left = 150, Top = 105, Width = 620, Height = 40, Multiline = true };

    private readonly ProductLinesEditor _lignesEditor = new(includeFodec: false, includeRemise: false, usePrixAchat: true)
        { Left = 15, Top = 155, Width = 830, Height = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

    private readonly Label _lblTotalHT = new() { Left = 600, Top = 395, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalTVA = new() { Left = 600, Top = 415, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalTTC = new() { Left = 600, Top = 435, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblNetAPayer = new() { Left = 600, Top = 455, Width = 245, TextAlign = ContentAlignment.MiddleRight, Font = new Font(Control.DefaultFont, FontStyle.Bold) };

    private readonly Label _lblError = new() { Left = 15, Top = 485, Width = 700, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnSave = new() { Left = 15, Top = 510, Width = 100, Text = "Enregistrer" };
    private readonly Button _btnCancel = new() { Left = 125, Top = 510, Width = 100, Text = "Annuler" };
    private readonly Button _btnPrint = new() { Left = 235, Top = 510, Width = 110, Text = "Imprimer" };

    // Règlements (only shown when editing an existing facture)
    private readonly DataGridView _gridReglements = new()
    {
        Left = 15,
        Top = 555,
        Width = 830,
        Height = 120,
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };
    private readonly Label _lblSolde = new() { Left = 15, Top = 680, Width = 300, Font = new Font(Control.DefaultFont, FontStyle.Bold) };
    private readonly DateTimePicker _dtReglementDate = new() { Left = 15, Top = 705, Width = 130, Format = DateTimePickerFormat.Short };
    private readonly ComboBox _cmbReglementMode = new() { Left = 155, Top = 705, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomModePayement", ValueMember = "CodeModePayement" };
    private readonly NumericUpDown _numReglementMontant = new() { Left = 335, Top = 705, Width = 110, DecimalPlaces = 3, Maximum = 999_999_999 };
    private readonly TextBox _txtReglementReference = new() { Left = 455, Top = 705, Width = 180 };
    private readonly Button _btnAddReglement = new() { Left = 645, Top = 703, Width = 100, Text = "Ajouter" };

    public FactureFournisseurEditForm(string? numero)
    {
        _numero = numero;
        _isNew = numero is null;

        Text = _isNew ? "Nouvelle Facture Fournisseur" : $"Facture Fournisseur : {numero}";
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = true;
        MinimizeBox = false;
        ClientSize = new Size(870, _isNew ? 550 : 760);
        AutoScroll = true;
        CancelButton = _btnCancel;

        Controls.Add(new Label { Left = 15, Top = 18, Width = 130, Text = "N° Facture" });
        Controls.Add(_lblNumero);
        Controls.Add(new Label { Left = 15, Top = 48, Width = 130, Text = "Date *" });
        Controls.Add(_dtDate);
        Controls.Add(new Label { Left = 275, Top = 48, Width = 50, Text = "Fournisseur *" });
        Controls.Add(_cmbFournisseur);
        Controls.Add(new Label { Left = 15, Top = 78, Width = 130, Text = "État" });
        Controls.Add(_cmbEtat);
        Controls.Add(new Label { Left = 370, Top = 78, Width = 90, Text = "Timbre Fiscal" });
        Controls.Add(_lblTimbre);
        Controls.Add(new Label { Left = 15, Top = 108, Width = 130, Text = "Note" });
        Controls.Add(_txtNote);
        Controls.Add(_lignesEditor);

        Controls.Add(new Label { Left = 380, Top = 395, Width = 220, Text = "Total HT" });
        Controls.Add(_lblTotalHT);
        Controls.Add(new Label { Left = 380, Top = 415, Width = 220, Text = "TVA" });
        Controls.Add(_lblTotalTVA);
        Controls.Add(new Label { Left = 380, Top = 435, Width = 220, Text = "Montant TTC" });
        Controls.Add(_lblTotalTTC);
        Controls.Add(new Label { Left = 380, Top = 455, Width = 220, Text = "Net à Payer" });
        Controls.Add(_lblNetAPayer);

        Controls.Add(_lblError);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);

        if (!_isNew)
        {
            Controls.Add(_btnPrint);
            Controls.Add(new Label { Left = 15, Top = 535, Width = 300, Text = "Règlements", Font = new Font(Control.DefaultFont, FontStyle.Bold) });
            Controls.Add(_gridReglements);
            _gridReglements.Columns.Add("Date", "Date");
            _gridReglements.Columns.Add("Mode", "Mode");
            _gridReglements.Columns.Add("Reference", "Référence");
            _gridReglements.Columns.Add("Montant", "Montant");

            Controls.Add(_lblSolde);
            Controls.Add(new Label { Left = 15, Top = 682, Width = 200, Text = "Nouveau règlement :" });
            Controls.Add(_dtReglementDate);
            Controls.Add(_cmbReglementMode);
            Controls.Add(_numReglementMontant);
            Controls.Add(_txtReglementReference);
            Controls.Add(_btnAddReglement);

            _btnAddReglement.Click += async (_, _) => await AddReglementAsync();
        }

        _cmbEtat.Items.AddRange(["Facture Ouverte", "Facture Validée", "Facture Annulée"]);
        _lignesEditor.LinesChanged += (_, _) => RecalculerTotaux();

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
        _lblNetAPayer.Text = (totalTTC + _timbreFiscal).ToString("N3");
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
            var modes = await db.ModesPayement.ToListAsync();
            _cmbReglementMode.DataSource = modes;
            _dtReglementDate.Value = DateTime.Today;

            var factureService = scope.ServiceProvider.GetRequiredService<IFactureFournisseurService>();
            var facture = await factureService.GetByNumeroAsync(_numero!);
            if (facture is null)
            {
                Logger.Warning("Facture fournisseur {Numero} introuvable à l'ouverture de l'éditeur.", _numero);
                MessageBox.Show(this, "Facture introuvable.", "Facture Fournisseur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _lblNumero.Text = facture.NumeroFactureFournisseur;
            _dtDate.Value = facture.DateFactureFournisseur;
            _cmbFournisseur.SelectedValue = facture.CodeFournisseur;
            _cmbEtat.SelectedItem = facture.EtatFacture;
            _txtNote.Text = facture.Note;
            _timbreFiscal = facture.Timbre;
            _lblTimbre.Text = _timbreFiscal.ToString("0.###");

            _lignesEditor.SetLignes(facture.Lignes.Select(l =>
                new LineRow(l.CodeProduit, l.Quantite, l.PrixUnitaire, 0, l.Tva, 0, l.MontantHT)));

            await ReloadReglementsAsync(factureService);
        }
        else
        {
            var config = scope.ServiceProvider.GetRequiredService<AppConfigService>();
            _timbreFiscal = config.TimbreFiscal;
            _lblTimbre.Text = _timbreFiscal.ToString("0.###");

            _lblNumero.Text = "Auto-généré";
            _dtDate.Value = DateTime.Today;
            _cmbEtat.SelectedItem = "Facture Ouverte";
        }

        RecalculerTotaux();
    }

    private async Task ReloadReglementsAsync(IFactureFournisseurService factureService)
    {
        var facture = await factureService.GetByNumeroAsync(_numero!);
        var solde = await factureService.GetSoldeAsync(_numero!);

        _gridReglements.Rows.Clear();
        if (facture?.Reglements is not null)
        {
            foreach (var r in facture.Reglements)
            {
                _gridReglements.Rows.Add(
                    r.DateReglement.ToString("dd/MM/yyyy"),
                    r.ModePayement?.NomModePayement,
                    r.Reference,
                    r.Montant.ToString("N3"));
            }
        }

        _lblSolde.Text = $"Solde restant : {solde:N3}";
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

        var lignes = _lignesEditor.GetLignes().Select(l => new LigneFactureFournisseur
        {
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Tva = l.Tva,
            MontantHT = l.MontantHT,
        }).ToList();

        var facture = new Web_T4C_GestCom.Data.Models.FactureFournisseur
        {
            NumeroFactureFournisseur = _numero ?? string.Empty,
            DateFactureFournisseur = _dtDate.Value.Date,
            CodeFournisseur = codeFournisseur,
            EtatFacture = (string)(_cmbEtat.SelectedItem ?? "Facture Ouverte"),
            Timbre = _timbreFiscal,
            Note = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim(),
        };

        _btnSave.Enabled = false;
        try
        {
            Logger.Debug("Enregistrement de la facture fournisseur {Numero} (nouveau={IsNew}, {Count} lignes).", _numero, _isNew, lignes.Count);
            using var scope = AppHost.CreateScope();
            var factureService = scope.ServiceProvider.GetRequiredService<IFactureFournisseurService>();

            if (_isNew)
                await factureService.CreateAsync(facture, lignes);
            else
                await factureService.UpdateAsync(facture, lignes);

            Logger.Debug("Facture fournisseur {Numero} enregistrée.", facture.NumeroFactureFournisseur);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec de l'enregistrement de la facture fournisseur {Numero}.", _numero);
            _lblError.Text = $"Erreur : {ex.Message}";
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }

    private async Task AddReglementAsync()
    {
        if (_numero is null) return;

        if (_cmbReglementMode.SelectedValue is not int codeMode)
        {
            MessageBox.Show(this, "Sélectionnez un mode de paiement.", "Règlement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var reglement = new ReglementFactureFournisseur
        {
            NumeroFactureFournisseur = _numero,
            DateReglement = _dtReglementDate.Value.Date,
            CodeModePayement = codeMode,
            Montant = (double)_numReglementMontant.Value,
            Reference = string.IsNullOrWhiteSpace(_txtReglementReference.Text) ? null : _txtReglementReference.Text.Trim(),
        };

        _btnAddReglement.Enabled = false;
        try
        {
            Logger.Debug("Ajout d'un règlement de {Montant} sur la facture fournisseur {Numero}.", reglement.Montant, _numero);
            using var scope = AppHost.CreateScope();
            var factureService = scope.ServiceProvider.GetRequiredService<IFactureFournisseurService>();

            await factureService.AddReglementAsync(reglement);
            await ReloadReglementsAsync(factureService);

            Logger.Debug("Règlement ajouté sur la facture fournisseur {Numero}.", _numero);
            _numReglementMontant.Value = 0;
            _txtReglementReference.Text = string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec de l'ajout du règlement sur la facture fournisseur {Numero}.", _numero);
            MessageBox.Show(this, $"Erreur : {ex.Message}", "Règlement", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnAddReglement.Enabled = true;
        }
    }

    private async Task PrintAsync()
    {
        if (_numero is null) return;

        _btnPrint.Enabled = false;
        try
        {
            using var scope = AppHost.CreateScope();
            var factureService = scope.ServiceProvider.GetRequiredService<IFactureFournisseurService>();
            var facture = await factureService.GetByNumeroAsync(_numero);
            if (facture is null) return;

            var rows = facture.Lignes.Select(l => (IReadOnlyList<string>)new[]
            {
                l.Produit?.DesignationProduit ?? l.CodeProduit,
                l.Quantite.ToString("0.###"),
                l.PrixUnitaire.ToString("0.###"),
                $"{l.Tva:0.##}%",
                l.MontantHT.ToString("0.###"),
            }).ToList();

            var totals = new List<(string, string, bool)>
            {
                ("Total HT", facture.MontantHT.ToString("0.###"), false),
                ("TVA", facture.MontantTVA.ToString("0.###"), false),
                ("Total TTC", facture.MontantTTC.ToString("0.###"), false),
                ("Timbre Fiscal", facture.Timbre.ToString("0.###"), false),
                ("Net à Payer", $"{facture.MontantTTC + facture.Timbre:0.###} TND", true),
            };

            var reglementsBlock = facture.Reglements is { Count: > 0 }
                ? (new[] { "Date", "Mode", "Référence", "Montant" }, (IReadOnlyList<string[]>)facture.Reglements.Select(r =>
                    new[] { r.DateReglement.ToString("dd/MM/yyyy"), r.ModePayement?.NomModePayement ?? "", r.Reference ?? "", r.Montant.ToString("0.###") }).ToList())
                : (ValueTuple<string[], IReadOnlyList<string[]>>?)null;

            var model = new PrintDocumentModel(
                DocType: "FACTURE FOURNISSEUR",
                Numero: facture.NumeroFactureFournisseur,
                Date: facture.DateFactureFournisseur,
                Etat: facture.EtatFacture,
                PartyLabel: "Fournisseur",
                PartyName: facture.Fournisseur?.NomFournisseur ?? facture.CodeFournisseur,
                PartyDetails: PartyDetailsHelper.ForFournisseur(facture.Fournisseur),
                HeaderRight: [("Règlement", facture.EtatReglement)],
                ColumnHeaders: ["Désignation", "Qté", "Prix Achat HT", "TVA%", "Montant HT"],
                Rows: rows,
                Totals: totals,
                Note: facture.Note,
                Reglements: reglementsBlock,
                EntrepriseFooter: null);

            PrintDocumentBuilder.PreviewInBrowser(model);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec de l'impression de la facture fournisseur {Numero}.", _numero);
            MessageBox.Show(this, $"Erreur : {ex.Message}", "Impression", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnPrint.Enabled = true;
        }
    }
}
