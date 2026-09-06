using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Forms.Shared;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Devis;

/// <summary>Desktop equivalent of Components/Pages/Devis/DevisList.razor.</summary>
public class DevisListForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<DevisListForm>();

    private readonly Button _btnNew = new() { Left = 10, Top = 9, Width = 120, Text = "Nouveau Devis" };
    private readonly Button _btnEdit = new() { Left = 140, Top = 9, Width = 90, Text = "Modifier" };
    private readonly Button _btnClone = new() { Left = 240, Top = 9, Width = 90, Text = "Cloner" };
    private readonly Button _btnDelete = new() { Left = 340, Top = 9, Width = 90, Text = "Supprimer" };
    private readonly Button _btnRefresh = new() { Left = 440, Top = 9, Width = 90, Text = "Actualiser" };
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

    public DevisListForm()
    {
        Text = "Devis";
        Width = 940;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(_btnNew);
        Controls.Add(_btnEdit);
        Controls.Add(_btnClone);
        Controls.Add(_btnDelete);
        Controls.Add(_btnRefresh);
        Controls.Add(_grid);

        _grid.Columns.Add("Numero", "N° Devis");
        _grid.Columns.Add("Date", "Date");
        _grid.Columns.Add("Client", "Client");
        _grid.Columns.Add("MontantHT", "Montant HT");
        _grid.Columns.Add("MontantTVA", "TVA");
        _grid.Columns.Add("MontantTTC", "Total TTC");
        _grid.Columns.Add("EtatDevis", "État");

        _btnNew.Click += (_, _) => OpenEditor(null);
        _btnEdit.Click += (_, _) => EditSelected();
        _btnClone.Click += async (_, _) => await CloneSelectedAsync();
        _btnDelete.Click += async (_, _) => await DeleteSelectedAsync();
        _btnRefresh.Click += async (_, _) => await LoadAsync();
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditSelected(); };

        Load += async (_, _) => await LoadAsync();
    }

    private string? SelectedNumero()
        => _grid.SelectedRows.Count > 0 ? (string)_grid.SelectedRows[0].Cells["Numero"].Value : null;

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var devisService = scope.ServiceProvider.GetRequiredService<IDevisClientService>();

        List<DevisClient> devis;
        try
        {
            Logger.DebugLoadingList("devis");
            devis = await devisService.GetAllAsync();
            Logger.DebugListLoaded("devis", devis.Count);
        }
        catch (Exception ex)
        {
            Logger.ErrorListLoadFailed(ex, "devis");
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", "Devis", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _grid.Rows.Clear();
        foreach (var d in devis)
        {
            _grid.Rows.Add(
                d.NumeroDevis,
                d.DateDevis.ToString("dd/MM/yyyy"),
                d.Client?.NomClient,
                d.MontantHT.ToString("N3"),
                d.MontantTVA.ToString("N3"),
                d.MontantTTC.ToString("N3"),
                d.EtatDevis);
        }
    }

    private void EditSelected()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, "Sélectionnez un devis à modifier.", "Devis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        OpenEditor(numero);
    }

    private void OpenEditor(string? numero)
    {
        using var editor = new DevisEditForm(numero);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            _ = LoadAsync();
        }
    }

    private async Task CloneSelectedAsync()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, "Sélectionnez un devis à cloner.", "Devis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var scope = AppHost.CreateScope();
        var devisService = scope.ServiceProvider.GetRequiredService<IDevisClientService>();
        try
        {
            Logger.DebugCloning("devis", numero);
            var clone = await devisService.CloneAsync(numero);
            Logger.DebugCloned("Devis", numero, clone.NumeroDevis);
            await LoadAsync();
            OpenEditor(clone.NumeroDevis);
        }
        catch (Exception ex)
        {
            Logger.ErrorCloneFailed(ex, "devis", numero);
            MessageBox.Show(this, $"Erreur lors du clonage : {ex.Message}", "Devis", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, "Sélectionnez un devis à supprimer.", "Devis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(this, $"Supprimer le devis {numero} ?", "Confirmation",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        using var scope = AppHost.CreateScope();
        var devisService = scope.ServiceProvider.GetRequiredService<IDevisClientService>();
        try
        {
            Logger.DebugDeleting("devis", numero);
            await devisService.DeleteAsync(numero);
            Logger.DebugDeleted("Devis", numero);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Logger.WarningDeleteFailed(ex, "devis", numero);
            MessageBox.Show(this, $"Erreur de suppression : {ex.Message}", "Devis", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
