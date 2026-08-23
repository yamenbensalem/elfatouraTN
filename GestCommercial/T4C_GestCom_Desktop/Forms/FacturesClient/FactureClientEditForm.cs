using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using T4C_GestCom_Desktop.Forms.Shared;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Forms.FacturesClient;

/// <summary>Desktop equivalent of Components/Pages/FacturesClient/FactureForm.razor (handles both factures and avoirs).</summary>
public class FactureClientEditForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<FactureClientEditForm>();

    private readonly string? _numero;
    private readonly bool _isNew;
    private readonly bool _isAvoir;
    private List<Produit> _produits = [];
    private List<ModePayement> _modesPayement = [];
    private double _timbreFiscal;
    private string _loadedEtatReglement = "Non Réglé";

    private readonly Label _lblNumero = new() { Left = 150, Top = 15, Width = 150, Font = new Font(Control.DefaultFont, FontStyle.Bold) };
    private readonly DateTimePicker _dtDate = new() { Left = 150, Top = 45, Width = 150, Format = DateTimePickerFormat.Short };
    private readonly ComboBox _cmbClient = new() { Left = 320, Top = 45, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomClient", ValueMember = "CodeClient" };
    private readonly NumericUpDown _numRemise = new() { Left = 590, Top = 45, Width = 90, DecimalPlaces = 2, Maximum = 100 };
    private readonly ComboBox _cmbEtat = new() { Left = 150, Top = 75, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _lblTimbre = new() { Left = 590, Top = 78, Width = 150 };
    private readonly TextBox _txtNote = new() { Left = 150, Top = 105, Width = 690, Height = 40, Multiline = true };

    private readonly DataGridView _gridLignes = new()
    {
        Left = 15,
        Top = 155,
        Width = 830,
        Height = 200,
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };
    private readonly Button _btnAddLigne = new() { Left = 15, Top = 360, Width = 140, Text = "Ajouter une ligne" };
    private readonly Button _btnRemoveLigne = new() { Left = 165, Top = 360, Width = 140, Text = "Retirer la ligne" };

    private readonly Label _lblTotalHT = new() { Left = 600, Top = 395, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalFodec = new() { Left = 600, Top = 415, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalTVA = new() { Left = 600, Top = 435, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalTTC = new() { Left = 600, Top = 455, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblNetAPayer = new() { Left = 600, Top = 475, Width = 245, TextAlign = ContentAlignment.MiddleRight, Font = new Font(Control.DefaultFont, FontStyle.Bold) };

    private readonly Label _lblError = new() { Left = 15, Top = 505, Width = 700, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnSave = new() { Left = 15, Top = 530, Width = 100, Text = "Enregistrer" };
    private readonly Button _btnCancel = new() { Left = 125, Top = 530, Width = 100, Text = "Annuler" };
    private readonly Button _btnPrint = new() { Left = 235, Top = 530, Width = 110, Text = "Imprimer" };

    // Règlements (only shown when editing an existing facture)
    private readonly DataGridView _gridReglements = new()
    {
        Left = 15,
        Top = 575,
        Width = 830,
        Height = 120,
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };
    private readonly Label _lblSolde = new() { Left = 15, Top = 700, Width = 300, Font = new Font(Control.DefaultFont, FontStyle.Bold) };
    private readonly DateTimePicker _dtReglementDate = new() { Left = 15, Top = 725, Width = 130, Format = DateTimePickerFormat.Short };
    private readonly ComboBox _cmbReglementMode = new() { Left = 155, Top = 725, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomModePayement", ValueMember = "CodeModePayement" };
    private readonly NumericUpDown _numReglementMontant = new() { Left = 335, Top = 725, Width = 110, DecimalPlaces = 3, Maximum = 999_999_999 };
    private readonly TextBox _txtReglementReference = new() { Left = 455, Top = 725, Width = 180 };
    private readonly Button _btnAddReglement = new() { Left = 645, Top = 723, Width = 100, Text = "Ajouter" };

    public FactureClientEditForm(string? numero, bool isAvoir = false)
    {
        _numero = numero;
        _isNew = numero is null;
        _isAvoir = isAvoir;

        Text = _isAvoir
            ? (_isNew ? "Nouvel Avoir" : $"Avoir : {numero}")
            : (_isNew ? "Nouvelle Facture" : $"Facture : {numero}");
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = true;
        MinimizeBox = false;
        ClientSize = new Size(870, _isNew ? 570 : 780);
        AutoScroll = true;
        AcceptButton = null;
        CancelButton = _btnCancel;

        BuildHeader();
        BuildLignesGrid();
        BuildTotalsPanel();

        Controls.Add(_lblError);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);

        if (!_isNew)
        {
            Controls.Add(_btnPrint);
            BuildReglementsSection();
        }

        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        _btnPrint.Click += async (_, _) => await PrintAsync();

        Load += async (_, _) => await LoadAsync();
    }

    private void BuildHeader()
    {
        Controls.Add(new Label { Left = 15, Top = 18, Width = 130, Text = "N° Facture" });
        Controls.Add(_lblNumero);
        Controls.Add(new Label { Left = 15, Top = 48, Width = 130, Text = "Date *" });
        Controls.Add(_dtDate);
        Controls.Add(new Label { Left = 275, Top = 48, Width = 50, Text = "Client *" });
        Controls.Add(_cmbClient);
        Controls.Add(new Label { Left = 495, Top = 48, Width = 90, Text = "Remise (%)" });
        Controls.Add(_numRemise);
        Controls.Add(new Label { Left = 15, Top = 78, Width = 130, Text = "État" });
        Controls.Add(_cmbEtat);
        Controls.Add(new Label { Left = 495, Top = 78, Width = 90, Text = "Timbre Fiscal" });
        Controls.Add(_lblTimbre);
        Controls.Add(new Label { Left = 15, Top = 108, Width = 130, Text = "Note" });
        Controls.Add(_txtNote);

        _cmbEtat.Items.AddRange(["Facture Ouverte", "Facture Livrée", "Facture Clôturée"]);
        _numRemise.ValueChanged += (_, _) => RecalculerTotaux();
    }

    private void BuildLignesGrid()
    {
        Controls.Add(_gridLignes);
        Controls.Add(_btnAddLigne);
        Controls.Add(_btnRemoveLigne);

        var colProduit = new DataGridViewComboBoxColumn
        {
            Name = "Produit",
            HeaderText = "Produit",
            DisplayMember = "DesignationProduit",
            ValueMember = "CodeProduit",
            DataPropertyName = "Produit",
        };
        _gridLignes.Columns.Add(colProduit);
        _gridLignes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantite", HeaderText = "Quantité" });
        _gridLignes.Columns.Add(new DataGridViewTextBoxColumn { Name = "PrixUnitaire", HeaderText = "Prix HT" });
        _gridLignes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remise", HeaderText = "Remise%" });
        _gridLignes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tva", HeaderText = "TVA%" });
        _gridLignes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fodec", HeaderText = "FODEC%" });
        _gridLignes.Columns.Add(new DataGridViewTextBoxColumn { Name = "MontantHT", HeaderText = "Montant HT", ReadOnly = true });

        _btnAddLigne.Click += (_, _) => _gridLignes.Rows.Add("", "1", "0", "0", "19", "0", "0");
        _btnRemoveLigne.Click += (_, _) =>
        {
            if (_gridLignes.SelectedRows.Count > 0)
            {
                _gridLignes.Rows.Remove(_gridLignes.SelectedRows[0]);
                RecalculerTotaux();
            }
        };

        _gridLignes.CellEndEdit += (_, e) => OnLigneCellEndEdit(e.RowIndex, e.ColumnIndex);
        _gridLignes.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_gridLignes.IsCurrentCellDirty && _gridLignes.CurrentCell?.OwningColumn is DataGridViewComboBoxColumn)
                _gridLignes.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
    }

    private void BuildTotalsPanel()
    {
        Controls.Add(new Label { Left = 380, Top = 395, Width = 220, Text = "Total HT" });
        Controls.Add(_lblTotalHT);
        Controls.Add(new Label { Left = 380, Top = 415, Width = 220, Text = "FODEC" });
        Controls.Add(_lblTotalFodec);
        Controls.Add(new Label { Left = 380, Top = 435, Width = 220, Text = "TVA" });
        Controls.Add(_lblTotalTVA);
        Controls.Add(new Label { Left = 380, Top = 455, Width = 220, Text = "Montant TTC" });
        Controls.Add(_lblTotalTTC);
        Controls.Add(new Label { Left = 380, Top = 475, Width = 220, Text = "Net à Payer" });
        Controls.Add(_lblNetAPayer);
    }

    private void BuildReglementsSection()
    {
        Controls.Add(new Label { Left = 15, Top = 555, Width = 300, Text = "Règlements", Font = new Font(Control.DefaultFont, FontStyle.Bold) });
        Controls.Add(_gridReglements);
        _gridReglements.Columns.Add("Date", "Date");
        _gridReglements.Columns.Add("Mode", "Mode");
        _gridReglements.Columns.Add("Reference", "Référence");
        _gridReglements.Columns.Add("Montant", "Montant");
        _gridReglements.Columns.Add("Note", "Note");

        Controls.Add(_lblSolde);
        Controls.Add(new Label { Left = 15, Top = 705, Width = 200, Text = "Nouveau règlement :" });
        Controls.Add(_dtReglementDate);
        Controls.Add(_cmbReglementMode);
        Controls.Add(_numReglementMontant);
        Controls.Add(_txtReglementReference);
        Controls.Add(_btnAddReglement);

        _btnAddReglement.Click += async (_, _) => await AddReglementAsync();
    }

    private void OnLigneCellEndEdit(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0) return;
        var row = _gridLignes.Rows[rowIndex];
        var columnName = _gridLignes.Columns[columnIndex].Name;

        if (columnName == "Produit")
        {
            var code = row.Cells["Produit"].Value as string;
            var produit = _produits.FirstOrDefault(p => p.CodeProduit == code);
            if (produit is not null)
            {
                row.Cells["PrixUnitaire"].Value = produit.PrixVenteHT.ToString("0.###");
                row.Cells["Tva"].Value = (produit.TvaProduit?.TauxTvaProduit ?? 19).ToString("0.###");
                row.Cells["Fodec"].Value = produit.Fodec.ToString("0.###");
                row.Cells["Remise"].Value = "0";
            }
        }

        RecalculerLigne(row);
        RecalculerTotaux();
    }

    private static double ParseCell(DataGridViewRow row, string column)
        => double.TryParse(row.Cells[column].Value?.ToString(), out var v) ? v : 0;

    private void RecalculerLigne(DataGridViewRow row)
    {
        var quantite = ParseCell(row, "Quantite");
        var prixUnitaire = ParseCell(row, "PrixUnitaire");
        var remise = ParseCell(row, "Remise");
        var montantHT = LineCalculator.LineMontantHT(quantite, prixUnitaire, remise);
        row.Cells["MontantHT"].Value = montantHT.ToString("0.###");
    }

    private (double HT, double Fodec, double TVA, double TTC) CalculateTotals()
    {
        var lines = _gridLignes.Rows.Cast<DataGridViewRow>().Select(row => new LineCalculator.LineAmounts(
            MontantHT: ParseCell(row, "MontantHT"),
            Tva: ParseCell(row, "Tva"),
            Fodec: ParseCell(row, "Fodec")));

        var totals = LineCalculator.CalculateDocumentTotals(lines, remisePercent: (double)_numRemise.Value);
        return (totals.TotalHT, totals.TotalFodec, totals.TotalTva, totals.TotalTTC);
    }

    private void RecalculerTotaux()
    {
        var (totalHT, totalFodec, totalTVA, totalTTC) = CalculateTotals();

        _lblTotalHT.Text = totalHT.ToString("N3");
        _lblTotalFodec.Text = totalFodec.ToString("N3");
        _lblTotalTVA.Text = totalTVA.ToString("N3");
        _lblTotalTTC.Text = totalTTC.ToString("N3");
        _lblNetAPayer.Text = (totalTTC + _timbreFiscal).ToString("N3");
    }

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<AppConfigService>();

        _timbreFiscal = config.TimbreFiscal;
        _lblTimbre.Text = _timbreFiscal.ToString("0.###");

        _cmbClient.DataSource = await db.Clients.OrderBy(c => c.NomClient).ToListAsync();
        _produits = await db.Produits.Include(p => p.TvaProduit).OrderBy(p => p.DesignationProduit).ToListAsync();
        ((DataGridViewComboBoxColumn)_gridLignes.Columns["Produit"]!).DataSource = _produits.Select(p => new ProduitOption(p.CodeProduit, p.DesignationProduit)).ToList();

        if (!_isNew)
        {
            _modesPayement = await db.ModesPayement.ToListAsync();
            _cmbReglementMode.DataSource = _modesPayement;
            _dtReglementDate.Value = DateTime.Today;

            var factureService = scope.ServiceProvider.GetRequiredService<IFactureClientService>();
            var facture = await factureService.GetByNumeroAsync(_numero!);
            if (facture is null)
            {
                Logger.WarningNotFound(_isAvoir ? "Avoir" : "Facture", _numero);
                MessageBox.Show(this, _isAvoir ? "Avoir introuvable." : "Facture introuvable.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _lblNumero.Text = facture.NumeroFactureClient;
            _dtDate.Value = facture.DateFactureClient;
            _cmbClient.SelectedValue = facture.CodeClient;
            _numRemise.Value = (decimal)facture.Remise;
            _cmbEtat.SelectedItem = facture.EtatFacture;
            _txtNote.Text = facture.Note;
            _loadedEtatReglement = facture.EtatReglement;
            // Preserve the Timbre stamped at creation time — UpdateAsync trusts this field directly
            // and the fiscal rate in AppConfig may have changed since, but an already-issued document
            // shouldn't retroactively change.
            _timbreFiscal = facture.Timbre;
            _lblTimbre.Text = _timbreFiscal.ToString("0.###");

            foreach (var l in facture.Lignes)
            {
                _gridLignes.Rows.Add(l.CodeProduit, l.Quantite, l.PrixUnitaire, l.Remise, l.Tva, l.Fodec, l.MontantHT.ToString("0.###"));
            }

            await ReloadReglementsAsync(factureService);
        }
        else
        {
            _lblNumero.Text = "Auto-généré";
            _dtDate.Value = DateTime.Today;
            _cmbEtat.SelectedItem = "Facture Ouverte";
        }

        RecalculerTotaux();
    }

    private async Task ReloadReglementsAsync(IFactureClientService factureService)
    {
        var facture = await factureService.GetByNumeroAsync(_numero!);
        var solde = await factureService.GetSoldeAsync(_numero!);

        // AddReglementAsync updates EtatReglement in the DB as a side effect — keep the cached value
        // used by SaveAsync in sync, or a header save right after adding a règlement would revert it.
        if (facture is not null)
            _loadedEtatReglement = facture.EtatReglement;

        _gridReglements.Rows.Clear();
        if (facture?.Reglements is not null)
        {
            foreach (var r in facture.Reglements)
            {
                _gridReglements.Rows.Add(
                    r.DateReglement.ToString("dd/MM/yyyy"),
                    r.ModePayement?.NomModePayement,
                    r.Reference,
                    r.Montant.ToString("N3"),
                    r.Note);
            }
        }

        _lblSolde.Text = $"Solde restant : {solde:N3}";
    }

    private async Task SaveAsync()
    {
        _lblError.Text = string.Empty;

        if (_cmbClient.SelectedValue is not string codeClient || string.IsNullOrWhiteSpace(codeClient))
        {
            _lblError.Text = "Le client est obligatoire.";
            return;
        }

        if (_gridLignes.Rows.Count == 0)
        {
            _lblError.Text = "Ajoutez au moins une ligne.";
            return;
        }

        var lignes = new List<LigneFactureClient>();
        foreach (DataGridViewRow row in _gridLignes.Rows)
        {
            var codeProduit = row.Cells["Produit"].Value as string;
            if (string.IsNullOrWhiteSpace(codeProduit))
            {
                _lblError.Text = "Chaque ligne doit avoir un produit sélectionné.";
                return;
            }

            lignes.Add(new LigneFactureClient
            {
                CodeProduit = codeProduit,
                Quantite = ParseCell(row, "Quantite"),
                PrixUnitaire = ParseCell(row, "PrixUnitaire"),
                Remise = ParseCell(row, "Remise"),
                Tva = ParseCell(row, "Tva"),
                Fodec = ParseCell(row, "Fodec"),
                MontantHT = ParseCell(row, "MontantHT"),
            });
        }

        var (totalHT, totalFodec, totalTVA, totalTTC) = CalculateTotals();

        var facture = new Web_T4C_GestCom.Data.Models.FactureClient
        {
            NumeroFactureClient = _numero ?? string.Empty,
            DateFactureClient = _dtDate.Value.Date,
            CodeClient = codeClient,
            Remise = (double)_numRemise.Value,
            EtatFacture = (string)(_cmbEtat.SelectedItem ?? "Facture Ouverte"),
            // UpdateAsync (unlike CreateAsync) trusts these fields directly rather than recalculating —
            // it must receive the freshly computed totals and the payment status must survive the edit.
            MontantHT = totalHT,
            Fodec = totalFodec,
            MontantTVA = totalTVA,
            MontantTTC = totalTTC,
            Timbre = _timbreFiscal,
            EtatReglement = _isNew ? "Non Réglé" : _loadedEtatReglement,
            Note = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim(),
            IsAvoir = _isAvoir,
        };

        _btnSave.Enabled = false;
        try
        {
            Logger.DebugSaving(_isAvoir ? "avoir" : "facture", _numero, _isNew, lignes.Count);
            using var scope = AppHost.CreateScope();
            var factureService = scope.ServiceProvider.GetRequiredService<IFactureClientService>();
            var config = scope.ServiceProvider.GetRequiredService<AppConfigService>();

            if (_isNew)
                await factureService.CreateAsync(facture, lignes, config);
            else
                await factureService.UpdateAsync(facture, lignes);

            Logger.DebugSaved(_isAvoir ? "Avoir" : "Facture", facture.NumeroFactureClient);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Logger.ErrorSaveFailed(ex, _isAvoir ? "avoir" : "facture", _numero);
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

        var reglement = new ReglementFactureClient
        {
            NumeroFactureClient = _numero,
            DateReglement = _dtReglementDate.Value.Date,
            CodeModePayement = codeMode,
            Montant = (double)_numReglementMontant.Value,
            Reference = string.IsNullOrWhiteSpace(_txtReglementReference.Text) ? null : _txtReglementReference.Text.Trim(),
        };

        _btnAddReglement.Enabled = false;
        try
        {
            Logger.DebugAddingReglement("la facture", _numero, reglement.Montant);
            using var scope = AppHost.CreateScope();
            var factureService = scope.ServiceProvider.GetRequiredService<IFactureClientService>();

            await factureService.AddReglementAsync(reglement);
            await ReloadReglementsAsync(factureService);

            Logger.DebugReglementAdded("la facture", _numero);
            _numReglementMontant.Value = 0;
            _txtReglementReference.Text = string.Empty;
        }
        catch (Exception ex)
        {
            Logger.ErrorReglementFailed(ex, "la facture", _numero);
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
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var factureService = scope.ServiceProvider.GetRequiredService<IFactureClientService>();

            var facture = await factureService.GetByNumeroAsync(_numero);
            if (facture is null) return;
            var entreprise = await db.Entreprises.FirstOrDefaultAsync();

            var headerRight = new List<(string, string)> { ("État règlement", facture.EtatReglement) };
            var regle = facture.Reglements?.Sum(r => r.Montant) ?? 0;
            if (regle > 0) headerRight.Add(("Réglé", $"{regle:0.###} TND"));

            var rows = facture.Lignes.Select(l =>
            {
                var ttc = Math.Round(l.MontantHT * (1 + l.Tva / 100), 3);
                return (IReadOnlyList<string>)new[]
                {
                    l.Produit?.DesignationProduit ?? l.CodeProduit,
                    l.Quantite.ToString("0.###"),
                    l.PrixUnitaire.ToString("0.###"),
                    l.Remise > 0 ? $"{l.Remise:0.##}%" : "—",
                    $"{l.Tva:0.##}%",
                    l.MontantHT.ToString("0.###"),
                    ttc.ToString("0.###"),
                };
            }).ToList();

            var totals = new List<(string, string, bool)> { ("Total HT", facture.MontantHT.ToString("0.###"), false) };
            if (facture.Remise > 0)
                totals.Add(($"Remise ({facture.Remise:0.##}%)", $"-{facture.MontantHT * facture.Remise / 100:0.###}", false));
            if (facture.Fodec > 0)
                totals.Add(("FODEC", facture.Fodec.ToString("0.###"), false));
            totals.Add(("TVA", facture.MontantTVA.ToString("0.###"), false));
            totals.Add(("Total TTC", facture.MontantTTC.ToString("0.###"), false));
            totals.Add(("Timbre Fiscal", facture.Timbre.ToString("0.###"), false));
            totals.Add(("Net à Payer", $"{facture.MontantTTC + facture.Timbre:0.###} TND", true));

            var reglementsBlock = facture.Reglements is { Count: > 0 }
                ? (new[] { "Date", "Mode", "Référence", "Montant" }, (IReadOnlyList<string[]>)facture.Reglements.Select(r =>
                    new[] { r.DateReglement.ToString("dd/MM/yyyy"), r.ModePayement?.NomModePayement ?? "", r.Reference ?? "", r.Montant.ToString("0.###") }).ToList())
                : (ValueTuple<string[], IReadOnlyList<string[]>>?)null;

            var model = new PrintDocumentModel(
                DocType: _isAvoir ? "AVOIR" : "FACTURE CLIENT",
                Numero: facture.NumeroFactureClient,
                Date: facture.DateFactureClient,
                Etat: facture.EtatFacture,
                PartyLabel: "Facturé à",
                PartyName: facture.Client?.NomClient ?? facture.CodeClient,
                PartyDetails: PartyDetailsHelper.ForClient(facture.Client),
                HeaderRight: headerRight,
                ColumnHeaders: ["Désignation", "Qté", "Prix HT", "Rem%", "TVA%", "Montant HT", "Montant TTC"],
                Rows: rows,
                Totals: totals,
                Note: facture.Note,
                Reglements: reglementsBlock,
                EntrepriseFooter: entreprise?.NomEntreprise);

            PrintDocumentBuilder.PreviewInBrowser(model);
        }
        catch (Exception ex)
        {
            Logger.ErrorPrintFailed(ex, "la facture", _numero);
            MessageBox.Show(this, $"Erreur : {ex.Message}", "Impression", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnPrint.Enabled = true;
        }
    }

}
