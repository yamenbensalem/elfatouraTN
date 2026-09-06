using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Forms.Shared;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Admin;

/// <summary>Desktop equivalent of Components/Pages/Admin/UtilisateursList.razor.</summary>
public class UtilisateursListForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<UtilisateursListForm>();

    private readonly Button _btnNew = new() { Left = 10, Top = 9, Width = 130, Text = "Nouvel Utilisateur" };
    private readonly Button _btnEdit = new() { Left = 145, Top = 9, Width = 90, Text = "Modifier" };
    private readonly Button _btnToggleActif = new() { Left = 245, Top = 9, Width = 130, Text = "Activer/Désactiver" };
    private readonly Button _btnRefresh = new() { Left = 380, Top = 9, Width = 90, Text = "Actualiser" };
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

    public UtilisateursListForm()
    {
        Text = "Utilisateurs";
        Width = 940;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(_btnNew);
        Controls.Add(_btnEdit);
        Controls.Add(_btnToggleActif);
        Controls.Add(_btnRefresh);
        Controls.Add(_grid);

        _grid.Columns.Add("Id", "Id");
        _grid.Columns["Id"]!.Visible = false;
        _grid.Columns.Add("Login", "Login");
        _grid.Columns.Add("NomComplet", "Nom Complet");
        _grid.Columns.Add("Email", "Email");
#pragma warning disable CS0618 // Role: legacy column, still the source of truth for this admin screen (mirrors UtilisateursList.razor)
        _grid.Columns.Add("Role", "Rôle");
#pragma warning restore CS0618
        _grid.Columns.Add("Etat", "État");
        _grid.Columns.Add("DateCreation", "Créé le");

        _btnNew.Click += (_, _) => OpenEditor(null);
        _btnEdit.Click += (_, _) => EditSelected();
        _btnToggleActif.Click += async (_, _) => await ToggleActifSelectedAsync();
        _btnRefresh.Click += async (_, _) => await LoadAsync();
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditSelected(); };

        Load += async (_, _) => await LoadAsync();
    }

    private int? SelectedId()
        => _grid.SelectedRows.Count > 0 ? (int)_grid.SelectedRows[0].Cells["Id"].Value : null;

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var utilisateurService = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();

        List<Utilisateur> utilisateurs;
        try
        {
            Logger.DebugLoadingList("utilisateurs");
            utilisateurs = await utilisateurService.GetAllAsync();
            Logger.DebugListLoaded("utilisateurs", utilisateurs.Count);
        }
        catch (Exception ex)
        {
            Logger.ErrorListLoadFailed(ex, "utilisateurs");
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", "Utilisateurs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _grid.Rows.Clear();
        foreach (var u in utilisateurs)
        {
#pragma warning disable CS0618
            _grid.Rows.Add(u.Id, u.Login, u.NomComplet, u.Email, u.Role, u.Actif ? "Actif" : "Inactif", u.DateCreation.ToString("dd/MM/yyyy"));
#pragma warning restore CS0618
        }
    }

    private void EditSelected()
    {
        var id = SelectedId();
        if (id is null)
        {
            MessageBox.Show(this, "Sélectionnez un utilisateur à modifier.", "Utilisateurs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        OpenEditor(id);
    }

    private void OpenEditor(int? id)
    {
        using var editor = new UtilisateurEditForm(id);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            _ = LoadAsync();
        }
    }

    private async Task ToggleActifSelectedAsync()
    {
        var id = SelectedId();
        if (id is null)
        {
            MessageBox.Show(this, "Sélectionnez un utilisateur.", "Utilisateurs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var etat = (string)_grid.SelectedRows[0].Cells["Etat"].Value;
        var isActif = etat == "Actif";

        using var scope = AppHost.CreateScope();
        var utilisateurService = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();
        try
        {
            Logger.Debug("Changement d'état de l'utilisateur {Id} (actif={NouvelEtat}).", id, !isActif);
            if (isActif)
                await utilisateurService.DesactiverAsync(id.Value);
            else
                await utilisateurService.ActiverAsync(id.Value);

            await LoadAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec du changement d'état de l'utilisateur {Id}.", id);
            MessageBox.Show(this, $"Erreur : {ex.Message}", "Utilisateurs", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
