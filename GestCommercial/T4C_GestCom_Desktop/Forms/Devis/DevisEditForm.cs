using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using T4C_GestCom_Desktop.Forms.Shared;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Forms.Devis;

/// <summary>Desktop equivalent of Components/Pages/Devis/DevisForm.razor.</summary>
public class DevisEditForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<DevisEditForm>();

    private readonly string? _numero;
    private readonly bool _isNew;
    private double _timbre;

    private readonly Label _lblNumero = new() { Left = 150, Top = 15, Width = 150, Font = new Font(Control.DefaultFont, FontStyle.Bold) };
    private readonly DateTimePicker _dtDate = new() { Left = 150, Top = 45, Width = 150, Format = DateTimePickerFormat.Short };
    private readonly ComboBox _cmbClient = new() { Left = 320, Top = 45, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomClient", ValueMember = "CodeClient" };
    private readonly NumericUpDown _numRemise = new() { Left = 590, Top = 45, Width = 90, DecimalPlaces = 2, Maximum = 100 };
    private readonly ComboBox _cmbEtat = new() { Left = 150, Top = 75, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtNote = new() { Left = 150, Top = 105, Width = 530, Height = 40, Multiline = true };

    private readonly ProductLinesEditor _lignesEditor = new(includeFodec: false) { Left = 15, Top = 155, Width = 830, Height = 230, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

    private readonly Label _lblTotalHT = new() { Left = 600, Top = 400, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalTVA = new() { Left = 600, Top = 420, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalTTC = new() { Left = 600, Top = 440, Width = 245, TextAlign = ContentAlignment.MiddleRight, Font = new Font(Control.DefaultFont, FontStyle.Bold) };

    private readonly Label _lblError = new() { Left = 15, Top = 470, Width = 700, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnSave = new() { Left = 15, Top = 495, Width = 100, Text = "Enregistrer" };
    private readonly Button _btnCancel = new() { Left = 125, Top = 495, Width = 100, Text = "Annuler" };
    private readonly Button _btnPrint = new() { Left = 235, Top = 495, Width = 110, Text = "Imprimer" };

    public DevisEditForm(string? numero)
    {
        _numero = numero;
        _isNew = numero is null;

        Text = _isNew ? "Nouveau Devis" : $"Devis : {numero}";
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = true;
        MinimizeBox = false;
        ClientSize = new Size(870, 535);
        AutoScroll = true;
        CancelButton = _btnCancel;

        Controls.Add(new Label { Left = 15, Top = 18, Width = 130, Text = "N° Devis" });
        Controls.Add(_lblNumero);
        Controls.Add(new Label { Left = 15, Top = 48, Width = 130, Text = "Date *" });
        Controls.Add(_dtDate);
        Controls.Add(new Label { Left = 275, Top = 48, Width = 50, Text = "Client *" });
        Controls.Add(_cmbClient);
        Controls.Add(new Label { Left = 495, Top = 48, Width = 90, Text = "Remise (%)" });
        Controls.Add(_numRemise);
        Controls.Add(new Label { Left = 15, Top = 78, Width = 130, Text = "État" });
        Controls.Add(_cmbEtat);
        Controls.Add(new Label { Left = 15, Top = 108, Width = 130, Text = "Note" });
        Controls.Add(_txtNote);
        Controls.Add(_lignesEditor);

        Controls.Add(new Label { Left = 380, Top = 400, Width = 220, Text = "Total HT" });
        Controls.Add(_lblTotalHT);
        Controls.Add(new Label { Left = 380, Top = 420, Width = 220, Text = "TVA" });
        Controls.Add(_lblTotalTVA);
        Controls.Add(new Label { Left = 380, Top = 440, Width = 220, Text = "Total TTC" });
        Controls.Add(_lblTotalTTC);

        Controls.Add(_lblError);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);
        if (!_isNew) Controls.Add(_btnPrint);

        _cmbEtat.Items.AddRange(["Ouvert", "Confirmé", "Annulé"]);
        _numRemise.ValueChanged += (_, _) => RecalculerTotaux();
        _lignesEditor.LinesChanged += (_, _) => RecalculerTotaux();

        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        _btnPrint.Click += async (_, _) => await PrintAsync();

        Load += async (_, _) => await LoadAsync();
    }

    private void RecalculerTotaux()
    {
        var remiseMontant = _lignesEditor.TotalHT * (double)_numRemise.Value / 100;
        var totalTTC = Math.Round(_lignesEditor.TotalHT - remiseMontant + _lignesEditor.TotalTva, 3);

        _lblTotalHT.Text = _lignesEditor.TotalHT.ToString("N3");
        _lblTotalTVA.Text = _lignesEditor.TotalTva.ToString("N3");
        _lblTotalTTC.Text = totalTTC.ToString("N3");
    }

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _cmbClient.DataSource = await db.Clients.OrderBy(c => c.NomClient).ToListAsync();
        var produits = await db.Produits.Include(p => p.TvaProduit).OrderBy(p => p.DesignationProduit).ToListAsync();
        _lignesEditor.SetProduits(produits);

        if (!_isNew)
        {
            var devisService = scope.ServiceProvider.GetRequiredService<IDevisClientService>();
            var devis = await devisService.GetByNumeroAsync(_numero!);
            if (devis is null)
            {
                Logger.WarningNotFound("Devis", _numero);
                MessageBox.Show(this, "Devis introuvable.", "Devis", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _lblNumero.Text = devis.NumeroDevis;
            _dtDate.Value = devis.DateDevis;
            _cmbClient.SelectedValue = devis.CodeClient;
            _numRemise.Value = (decimal)devis.Remise;
            _cmbEtat.SelectedItem = devis.EtatDevis;
            _txtNote.Text = devis.Note;
            // Preserve the Timbre stamped at creation time — UpdateAsync trusts this field directly.
            _timbre = devis.Timbre;

            _lignesEditor.SetLignes(devis.Lignes.Select(l =>
                new LineRow(l.CodeProduit, l.Quantite, l.PrixUnitaire, l.Remise, l.Tva, 0, l.MontantHT)));
        }
        else
        {
            _lblNumero.Text = "Auto-généré";
            _dtDate.Value = DateTime.Today;
            _cmbEtat.SelectedItem = "Ouvert";
        }

        RecalculerTotaux();
    }

    private async Task SaveAsync()
    {
        _lblError.Text = string.Empty;

        if (_cmbClient.SelectedValue is not string codeClient || string.IsNullOrWhiteSpace(codeClient))
        {
            _lblError.Text = "Le client est obligatoire.";
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

        var lignes = _lignesEditor.GetLignes().Select(l => new LigneDevisClient
        {
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Remise = l.Remise,
            Tva = l.Tva,
            MontantHT = l.MontantHT,
        }).ToList();

        var devis = new DevisClient
        {
            NumeroDevis = _numero ?? string.Empty,
            DateDevis = _dtDate.Value.Date,
            CodeClient = codeClient,
            Remise = (double)_numRemise.Value,
            EtatDevis = (string)(_cmbEtat.SelectedItem ?? "Ouvert"),
            Timbre = _timbre,
            Note = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim(),
        };

        _btnSave.Enabled = false;
        try
        {
            Logger.DebugSaving("devis", _numero, _isNew, lignes.Count);
            using var scope = AppHost.CreateScope();
            var devisService = scope.ServiceProvider.GetRequiredService<IDevisClientService>();
            var config = scope.ServiceProvider.GetRequiredService<AppConfigService>();

            if (_isNew)
                await devisService.CreateAsync(devis, lignes, config);
            else
                await devisService.UpdateAsync(devis, lignes);

            Logger.DebugSaved("Devis", devis.NumeroDevis);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Logger.ErrorSaveFailed(ex, "devis", _numero);
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
            var devisService = scope.ServiceProvider.GetRequiredService<IDevisClientService>();
            var devis = await devisService.GetByNumeroAsync(_numero);
            if (devis is null) return;

            var rows = devis.Lignes.Select(l => (IReadOnlyList<string>)new[]
            {
                l.Produit?.DesignationProduit ?? l.CodeProduit,
                l.Quantite.ToString("0.###"),
                l.PrixUnitaire.ToString("0.###"),
                l.Remise > 0 ? $"{l.Remise:0.##}%" : "—",
                $"{l.Tva:0.##}%",
                l.MontantHT.ToString("0.###"),
            }).ToList();

            var totals = new List<(string, string, bool)> { ("Total HT", devis.MontantHT.ToString("0.###"), false) };
            if (devis.Remise > 0)
                totals.Add(($"Remise ({devis.Remise:0.##}%)", $"-{devis.MontantHT * devis.Remise / 100:0.###}", false));
            totals.Add(("TVA", devis.MontantTVA.ToString("0.###"), false));
            totals.Add(("Total TTC", $"{devis.MontantTTC:0.###} TND", true));

            var model = new PrintDocumentModel(
                DocType: "DEVIS",
                Numero: devis.NumeroDevis,
                Date: devis.DateDevis,
                Etat: devis.EtatDevis,
                PartyLabel: "Devis pour",
                PartyName: devis.Client?.NomClient ?? devis.CodeClient,
                PartyDetails: PartyDetailsHelper.ForClient(devis.Client),
                HeaderRight: [],
                ColumnHeaders: ["Désignation", "Qté", "Prix HT", "Rem%", "TVA%", "Montant HT"],
                Rows: rows,
                Totals: totals,
                Note: devis.Note,
                Reglements: null,
                EntrepriseFooter: null);

            PrintDocumentBuilder.PreviewInBrowser(model);
        }
        catch (Exception ex)
        {
            Logger.ErrorPrintFailed(ex, "devis", _numero);
            MessageBox.Show(this, $"Erreur : {ex.Message}", "Impression", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnPrint.Enabled = true;
        }
    }
}
