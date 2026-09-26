using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Forms.Shared;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Admin;

/// <summary>Desktop equivalent of Components/Pages/Admin/JournalActiviteList.razor.</summary>
public class JournalActiviteListForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<JournalActiviteListForm>();

    private readonly ComboBox _cmbLogin = new() { Left = 90, Top = 10, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbEntite = new() { Left = 320, Top = 10, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _dtpDebut = new() { Left = 550, Top = 10, Width = 120, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
    private readonly DateTimePicker _dtpFin = new() { Left = 680, Top = 10, Width = 120, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
    private readonly Button _btnFilter = new() { Left = 810, Top = 9, Width = 90, Text = "Filtrer" };
    private readonly Button _btnReset = new() { Left = 905, Top = 9, Width = 110, Text = "Réinitialiser" };

    private readonly NumericUpDown _numPurgeMonths = new() { Left = 130, Top = 45, Width = 60, Minimum = 1, Maximum = 120, Value = 12 };
    private readonly Button _btnPurge = new() { Left = 200, Top = 43, Width = 200, Text = "Purger les entrées anciennes" };

    private readonly DataGridView _grid = new()
    {
        Left = 10,
        Top = 80,
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

    private readonly Label _lblCount = new() { Left = 10, Top = 555, Width = 500, Text = string.Empty };

    public JournalActiviteListForm()
    {
        Text = "Journal d'Activité";
        Width = 1040;
        Height = 630;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(new Label { Left = 10, Top = 13, Width = 75, Text = "Utilisateur" });
        Controls.Add(_cmbLogin);
        Controls.Add(new Label { Left = 250, Top = 13, Width = 65, Text = "Entité" });
        Controls.Add(_cmbEntite);
        Controls.Add(new Label { Left = 505, Top = 13, Width = 40, Text = "Du" });
        Controls.Add(_dtpDebut);
        Controls.Add(new Label { Left = 635, Top = 13, Width = 40, Text = "Au" });
        Controls.Add(_dtpFin);
        Controls.Add(_btnFilter);
        Controls.Add(_btnReset);
        Controls.Add(_numPurgeMonths);
        Controls.Add(_btnPurge);
        Controls.Add(_grid);
        Controls.Add(_lblCount);

        _grid.Columns.Add("DateHeure", "Date / Heure");
        _grid.Columns.Add("Login", "Utilisateur");
        _grid.Columns.Add("Action", "Action");
        _grid.Columns.Add("Entite", "Entité");
        _grid.Columns.Add("Code", "Code");
        _grid.Columns.Add("Detail", "Détail");

        _btnFilter.Click += async (_, _) => await LoadAsync();
        _btnReset.Click += async (_, _) => await ResetFiltersAsync();
        _btnPurge.Click += async (_, _) => await PurgeAsync();

        Load += async (_, _) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadFiltersAsync();
        await LoadAsync();
    }

    private async Task LoadFiltersAsync()
    {
        using var scope = AppHost.CreateScope();
        var journalService = scope.ServiceProvider.GetRequiredService<IJournalActiviteService>();

        var logins = await journalService.GetLoginsDistinctsAsync();
        var entites = await journalService.GetEntitesDistinctesAsync();

        var selectedLogin = _cmbLogin.SelectedItem as string;
        var selectedEntite = _cmbEntite.SelectedItem as string;

        _cmbLogin.Items.Clear();
        _cmbLogin.Items.Add("(Tous)");
        _cmbLogin.Items.AddRange([.. logins]);
        _cmbLogin.SelectedItem = selectedLogin is not null && logins.Contains(selectedLogin) ? selectedLogin : "(Tous)";

        _cmbEntite.Items.Clear();
        _cmbEntite.Items.Add("(Toutes)");
        _cmbEntite.Items.AddRange([.. entites]);
        _cmbEntite.SelectedItem = selectedEntite is not null && entites.Contains(selectedEntite) ? selectedEntite : "(Toutes)";
    }

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var journalService = scope.ServiceProvider.GetRequiredService<IJournalActiviteService>();

        var login = _cmbLogin.SelectedItem as string is { } l && l != "(Tous)" ? l : null;
        var entite = _cmbEntite.SelectedItem as string is { } e && e != "(Toutes)" ? e : null;
        var debut = _dtpDebut.Checked ? _dtpDebut.Value.Date : (DateTime?)null;
        var fin = _dtpFin.Checked ? _dtpFin.Value.Date : (DateTime?)null;

        List<JournalActivite> journal;
        try
        {
            Logger.DebugLoadingList("journal d'activité");
            journal = await journalService.GetAllAsync(login, entite, debut, fin);
            Logger.DebugListLoaded("journal d'activité", journal.Count);
        }
        catch (Exception ex)
        {
            Logger.ErrorListLoadFailed(ex, "journal d'activité");
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", "Journal d'Activité", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _grid.Rows.Clear();
        foreach (var j in journal)
        {
            _grid.Rows.Add(j.DateHeure.ToString("dd/MM/yyyy HH:mm:ss"), j.Login, j.Action, j.Entite, j.CodeEntite, j.Detail);
        }
        _lblCount.Text = $"{journal.Count} entrée(s) — 1000 max.";
    }

    private async Task ResetFiltersAsync()
    {
        _cmbLogin.SelectedItem = "(Tous)";
        _cmbEntite.SelectedItem = "(Toutes)";
        _dtpDebut.Checked = false;
        _dtpFin.Checked = false;
        await LoadAsync();
    }

    private async Task PurgeAsync()
    {
        var months = (int)_numPurgeMonths.Value;
        var confirm = MessageBox.Show(this,
            $"Confirmer la suppression définitive des entrées de plus de {months} mois ? Cette action est irréversible.",
            "Purger le journal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        using var scope = AppHost.CreateScope();
        var journalService = scope.ServiceProvider.GetRequiredService<IJournalActiviteService>();
        try
        {
            var count = await journalService.PurgeAsync(months);
            MessageBox.Show(this, count > 0 ? $"{count} entrée(s) supprimée(s)." : "Aucune entrée à purger.",
                "Journal d'Activité", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadFiltersAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec de la purge du journal d'activité (seuil={Months} mois).", months);
            MessageBox.Show(this, $"Erreur : {ex.Message}", "Journal d'Activité", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
