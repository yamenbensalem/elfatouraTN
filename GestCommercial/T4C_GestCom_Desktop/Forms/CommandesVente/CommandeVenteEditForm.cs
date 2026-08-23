using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using T4C_GestCom_Desktop.Forms.Shared;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Forms.CommandesVente;

/// <summary>Desktop equivalent of Components/Pages/CommandesVente/CommandeVenteForm.razor.</summary>
public class CommandeVenteEditForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<CommandeVenteEditForm>();

    private readonly string? _numero;
    private readonly bool _isNew;

    private readonly Label _lblNumero = new() { Left = 150, Top = 15, Width = 150, Font = new Font(Control.DefaultFont, FontStyle.Bold) };
    private readonly DateTimePicker _dtDate = new() { Left = 150, Top = 45, Width = 150, Format = DateTimePickerFormat.Short };
    private readonly ComboBox _cmbClient = new() { Left = 320, Top = 45, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomClient", ValueMember = "CodeClient" };
    private readonly NumericUpDown _numRemise = new() { Left = 590, Top = 45, Width = 90, DecimalPlaces = 2, Maximum = 100 };
    private readonly ComboBox _cmbEtat = new() { Left = 150, Top = 75, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbEtatLivraison = new() { Left = 380, Top = 75, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtNote = new() { Left = 150, Top = 105, Width = 530, Height = 40, Multiline = true };

    private readonly ProductLinesEditor _lignesEditor = new(includeFodec: false) { Left = 15, Top = 155, Width = 830, Height = 230, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

    private readonly Label _lblTotalHT = new() { Left = 600, Top = 400, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalTVA = new() { Left = 600, Top = 420, Width = 245, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblTotalTTC = new() { Left = 600, Top = 440, Width = 245, TextAlign = ContentAlignment.MiddleRight, Font = new Font(Control.DefaultFont, FontStyle.Bold) };

    private readonly Label _lblError = new() { Left = 15, Top = 470, Width = 700, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnSave = new() { Left = 15, Top = 495, Width = 100, Text = "Enregistrer" };
    private readonly Button _btnCancel = new() { Left = 125, Top = 495, Width = 100, Text = "Annuler" };
    private readonly Button _btnPrint = new() { Left = 235, Top = 495, Width = 110, Text = "Imprimer" };

    public CommandeVenteEditForm(string? numero)
    {
        _numero = numero;
        _isNew = numero is null;

        Text = _isNew ? "Nouvelle Commande Vente" : $"Commande Vente : {numero}";
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = true;
        MinimizeBox = false;
        ClientSize = new Size(870, 535);
        AutoScroll = true;
        CancelButton = _btnCancel;

        Controls.Add(new Label { Left = 15, Top = 18, Width = 130, Text = "N° Commande" });
        Controls.Add(_lblNumero);
        Controls.Add(new Label { Left = 15, Top = 48, Width = 130, Text = "Date *" });
        Controls.Add(_dtDate);
        Controls.Add(new Label { Left = 275, Top = 48, Width = 50, Text = "Client *" });
        Controls.Add(_cmbClient);
        Controls.Add(new Label { Left = 495, Top = 48, Width = 90, Text = "Remise (%)" });
        Controls.Add(_numRemise);
        Controls.Add(new Label { Left = 15, Top = 78, Width = 130, Text = "État" });
        Controls.Add(_cmbEtat);
        Controls.Add(new Label { Left = 305, Top = 78, Width = 70, Text = "Livraison" });
        Controls.Add(_cmbEtatLivraison);
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
        _cmbEtatLivraison.Items.AddRange(["Non Livré", "Partiellement Livré", "Livré"]);
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
            var commandeService = scope.ServiceProvider.GetRequiredService<ICommandeVenteService>();
            var commande = await commandeService.GetByNumeroAsync(_numero!);
            if (commande is null)
            {
                Logger.Warning("Commande vente {Numero} introuvable à l'ouverture de l'éditeur.", _numero);
                MessageBox.Show(this, "Commande introuvable.", "Commande Vente", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _lblNumero.Text = commande.NumeroCommandeVente;
            _dtDate.Value = commande.DateCommandeVente;
            _cmbClient.SelectedValue = commande.CodeClient;
            _numRemise.Value = (decimal)commande.Remise;
            _cmbEtat.SelectedItem = commande.EtatCommandeVente;
            _cmbEtatLivraison.SelectedItem = commande.EtatLivraison;
            _txtNote.Text = commande.Note;

            _lignesEditor.SetLignes(commande.Lignes.Select(l =>
                new LineRow(l.CodeProduit, l.Quantite, l.PrixUnitaire, l.Remise, l.Tva, 0, l.MontantHT)));
        }
        else
        {
            _lblNumero.Text = "Auto-généré";
            _dtDate.Value = DateTime.Today;
            _cmbEtat.SelectedItem = "Ouvert";
            _cmbEtatLivraison.SelectedItem = "Non Livré";
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

        var lignes = _lignesEditor.GetLignes().Select(l => new LigneCommandeVente
        {
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Remise = l.Remise,
            Tva = l.Tva,
            MontantHT = l.MontantHT,
        }).ToList();

        var commande = new CommandeVente
        {
            NumeroCommandeVente = _numero ?? string.Empty,
            DateCommandeVente = _dtDate.Value.Date,
            CodeClient = codeClient,
            Remise = (double)_numRemise.Value,
            EtatCommandeVente = (string)(_cmbEtat.SelectedItem ?? "Ouvert"),
            EtatLivraison = (string)(_cmbEtatLivraison.SelectedItem ?? "Non Livré"),
            Note = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim(),
        };

        _btnSave.Enabled = false;
        try
        {
            Logger.Debug("Enregistrement de la commande vente {Numero} (nouveau={IsNew}, {Count} lignes).", _numero, _isNew, lignes.Count);
            using var scope = AppHost.CreateScope();
            var commandeService = scope.ServiceProvider.GetRequiredService<ICommandeVenteService>();

            if (_isNew)
                await commandeService.CreateAsync(commande, lignes);
            else
                await commandeService.UpdateAsync(commande, lignes);

            Logger.Debug("Commande vente {Numero} enregistrée.", commande.NumeroCommandeVente);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec de l'enregistrement de la commande vente {Numero}.", _numero);
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
            var commandeService = scope.ServiceProvider.GetRequiredService<ICommandeVenteService>();
            var commande = await commandeService.GetByNumeroAsync(_numero);
            if (commande is null) return;

            var rows = commande.Lignes.Select(l => (IReadOnlyList<string>)new[]
            {
                l.Produit?.DesignationProduit ?? l.CodeProduit,
                l.Quantite.ToString("0.###"),
                l.PrixUnitaire.ToString("0.###"),
                l.Remise > 0 ? $"{l.Remise:0.##}%" : "—",
                $"{l.Tva:0.##}%",
                l.MontantHT.ToString("0.###"),
            }).ToList();

            var totals = new List<(string, string, bool)> { ("Total HT", commande.MontantHT.ToString("0.###"), false) };
            if (commande.Remise > 0)
                totals.Add(($"Remise ({commande.Remise:0.##}%)", $"-{commande.MontantHT * commande.Remise / 100:0.###}", false));
            totals.Add(("TVA", commande.MontantTVA.ToString("0.###"), false));
            totals.Add(("Total TTC", $"{commande.MontantTTC:0.###} TND", true));

            var model = new PrintDocumentModel(
                DocType: "COMMANDE VENTE",
                Numero: commande.NumeroCommandeVente,
                Date: commande.DateCommandeVente,
                Etat: commande.EtatCommandeVente,
                PartyLabel: "Commande pour",
                PartyName: commande.Client?.NomClient ?? commande.CodeClient,
                PartyDetails: PartyDetailsHelper.ForClient(commande.Client),
                HeaderRight: [("Livraison", commande.EtatLivraison)],
                ColumnHeaders: ["Désignation", "Qté", "Prix HT", "Rem%", "TVA%", "Montant HT"],
                Rows: rows,
                Totals: totals,
                Note: commande.Note,
                Reglements: null,
                EntrepriseFooter: null);

            PrintDocumentBuilder.PreviewInBrowser(model);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec de l'impression de la commande vente {Numero}.", _numero);
            MessageBox.Show(this, $"Erreur : {ex.Message}", "Impression", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnPrint.Enabled = true;
        }
    }
}
