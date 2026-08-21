using Microsoft.Extensions.DependencyInjection;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Forms.CommandesVente;

/// <summary>Desktop equivalent of Components/Pages/CommandesVente/CommandeVenteList.razor.</summary>
public class CommandesVenteListForm : Form
{
    private readonly Button _btnNew = new() { Left = 10, Top = 9, Width = 140, Text = "Nouvelle Commande" };
    private readonly Button _btnEdit = new() { Left = 155, Top = 9, Width = 90, Text = "Modifier" };
    private readonly Button _btnClone = new() { Left = 255, Top = 9, Width = 90, Text = "Cloner" };
    private readonly Button _btnDelete = new() { Left = 355, Top = 9, Width = 90, Text = "Supprimer" };
    private readonly Button _btnRefresh = new() { Left = 455, Top = 9, Width = 90, Text = "Actualiser" };
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

    public CommandesVenteListForm()
    {
        Text = "Commandes Vente";
        Width = 940;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(_btnNew);
        Controls.Add(_btnEdit);
        Controls.Add(_btnClone);
        Controls.Add(_btnDelete);
        Controls.Add(_btnRefresh);
        Controls.Add(_grid);

        _grid.Columns.Add("Numero", "N° Commande");
        _grid.Columns.Add("Date", "Date");
        _grid.Columns.Add("Client", "Client");
        _grid.Columns.Add("MontantHT", "Montant HT");
        _grid.Columns.Add("MontantTVA", "TVA");
        _grid.Columns.Add("MontantTTC", "Total TTC");
        _grid.Columns.Add("EtatCommandeVente", "État");
        _grid.Columns.Add("EtatLivraison", "Livraison");

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
        var commandeService = scope.ServiceProvider.GetRequiredService<ICommandeVenteService>();

        List<CommandeVente> commandes;
        try
        {
            commandes = await commandeService.GetAllAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", "Commandes Vente", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _grid.Rows.Clear();
        foreach (var c in commandes)
        {
            _grid.Rows.Add(
                c.NumeroCommandeVente,
                c.DateCommandeVente.ToString("dd/MM/yyyy"),
                c.Client?.NomClient,
                c.MontantHT.ToString("N3"),
                c.MontantTVA.ToString("N3"),
                c.MontantTTC.ToString("N3"),
                c.EtatCommandeVente,
                c.EtatLivraison);
        }
    }

    private void EditSelected()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, "Sélectionnez une commande à modifier.", "Commandes Vente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        OpenEditor(numero);
    }

    private void OpenEditor(string? numero)
    {
        using var editor = new CommandeVenteEditForm(numero);
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
            MessageBox.Show(this, "Sélectionnez une commande à cloner.", "Commandes Vente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var scope = AppHost.CreateScope();
        var commandeService = scope.ServiceProvider.GetRequiredService<ICommandeVenteService>();
        try
        {
            var clone = await commandeService.CloneAsync(numero);
            await LoadAsync();
            OpenEditor(clone.NumeroCommandeVente);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erreur lors du clonage : {ex.Message}", "Commandes Vente", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        var numero = SelectedNumero();
        if (numero is null)
        {
            MessageBox.Show(this, "Sélectionnez une commande à supprimer.", "Commandes Vente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(this, $"Supprimer la commande {numero} ?", "Confirmation",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        using var scope = AppHost.CreateScope();
        var commandeService = scope.ServiceProvider.GetRequiredService<ICommandeVenteService>();
        try
        {
            await commandeService.DeleteAsync(numero);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erreur de suppression : {ex.Message}", "Commandes Vente", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
