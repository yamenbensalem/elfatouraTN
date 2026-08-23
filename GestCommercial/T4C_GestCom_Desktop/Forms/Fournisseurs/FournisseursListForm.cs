using Microsoft.Extensions.DependencyInjection;
using Serilog;
using T4C_GestCom_Desktop.Forms.Shared;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Forms.Fournisseurs;

/// <summary>Desktop equivalent of Components/Pages/Fournisseurs/FournisseursList.razor.</summary>
public class FournisseursListForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<FournisseursListForm>();

    private readonly TextBox _txtSearch = new() { Left = 10, Top = 10, Width = 250 };
    private readonly Button _btnSearch = new() { Left = 265, Top = 9, Width = 90, Text = "Rechercher" };
    private readonly Button _btnNew = new() { Left = 365, Top = 9, Width = 90, Text = "Nouveau" };
    private readonly Button _btnEdit = new() { Left = 465, Top = 9, Width = 90, Text = "Modifier" };
    private readonly Button _btnDelete = new() { Left = 565, Top = 9, Width = 90, Text = "Supprimer" };
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

    public FournisseursListForm()
    {
        Text = "Fournisseurs";
        Width = 940;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(_txtSearch);
        Controls.Add(_btnSearch);
        Controls.Add(_btnNew);
        Controls.Add(_btnEdit);
        Controls.Add(_btnDelete);
        Controls.Add(_grid);

        _grid.Columns.Add("CodeFournisseur", "Code");
        _grid.Columns.Add("NomFournisseur", "Nom");
        _grid.Columns.Add("MatriculeFiscale", "Matricule Fiscale");
        _grid.Columns.Add("Tel", "Téléphone");
        _grid.Columns.Add("Ville", "Ville");
        _grid.Columns.Add("EtatFournisseur", "État");

        _btnSearch.Click += async (_, _) => await LoadAsync();
        _txtSearch.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await LoadAsync(); } };
        _btnNew.Click += (_, _) => OpenEditor(null);
        _btnEdit.Click += (_, _) => EditSelected();
        _btnDelete.Click += async (_, _) => await DeleteSelectedAsync();
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditSelected(); };

        Load += async (_, _) => await LoadAsync();
    }

    private string? SelectedCode()
        => _grid.SelectedRows.Count > 0 ? (string)_grid.SelectedRows[0].Cells["CodeFournisseur"].Value : null;

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var fournisseurService = scope.ServiceProvider.GetRequiredService<IFournisseurService>();

        List<Fournisseur> fournisseurs;
        try
        {
            Logger.DebugLoadingList("fournisseurs", $"recherche={_txtSearch.Text.Trim()}");
            fournisseurs = await fournisseurService.GetAllAsync(_txtSearch.Text.Trim() is { Length: > 0 } s ? s : null);
            Logger.DebugListLoaded("fournisseurs", fournisseurs.Count);
        }
        catch (Exception ex)
        {
            Logger.ErrorListLoadFailed(ex, "fournisseurs");
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", "Fournisseurs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _grid.Rows.Clear();
        foreach (var f in fournisseurs)
        {
            _grid.Rows.Add(f.CodeFournisseur, f.NomFournisseur, f.MatriculeFiscale, f.Tel, f.Ville, f.EtatFournisseur);
        }
    }

    private void EditSelected()
    {
        var code = SelectedCode();
        if (code is null)
        {
            MessageBox.Show(this, "Sélectionnez un fournisseur à modifier.", "Fournisseurs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        OpenEditor(code);
    }

    private void OpenEditor(string? code)
    {
        using var editor = new FournisseurEditForm(code);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            _ = LoadAsync();
        }
    }

    private async Task DeleteSelectedAsync()
    {
        var code = SelectedCode();
        if (code is null)
        {
            MessageBox.Show(this, "Sélectionnez un fournisseur à supprimer.", "Fournisseurs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(this, $"Supprimer le fournisseur {code} ?", "Confirmation",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        using var scope = AppHost.CreateScope();
        var fournisseurService = scope.ServiceProvider.GetRequiredService<IFournisseurService>();
        try
        {
            Logger.DebugDeleting("fournisseur", code);
            await fournisseurService.DeleteAsync(code);
            Logger.DebugDeleted("Fournisseur", code);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Logger.WarningDeleteFailed(ex, "fournisseur", code);
            var message = DeleteErrorMessageHelper.Build(ex,
                "Ce fournisseur ne peut pas etre supprime car il est lie a des factures, commandes ou bons de reception. Supprimez d'abord les documents lies, puis reessayez.");
            MessageBox.Show(this, message, "Suppression impossible", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
