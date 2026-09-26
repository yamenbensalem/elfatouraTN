using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Admin;

/// <summary>Desktop equivalent of Components/Pages/Admin/RolesGestion.razor.</summary>
public class RolesGestionForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<RolesGestionForm>();

    private static readonly string[] Modules =
    [
        "clients", "factures", "devis", "commandes-vente", "bons-livraison",
        "fournisseurs", "commandes-achat", "bons-reception", "factures-fournisseur", "produits",
    ];
    private static readonly (string Action, string Label)[] Actions =
    [
        ("view", "Voir"), ("create", "Créer"), ("update", "Modifier"), ("delete", "Supprimer"),
    ];

    private readonly ListBox _lstRoles = new() { Left = 10, Top = 35, Width = 220, Height = 480 };
    private readonly Label _lblSelectedRole = new() { Left = 240, Top = 10, Width = 400, Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold) };
    private readonly DataGridView _grid = new()
    {
        Left = 240,
        Top = 35,
        Width = 550,
        Height = 480,
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };
    private readonly Button _btnSave = new() { Left = 240, Top = 525, Width = 120, Text = "Enregistrer" };
    private readonly Label _lblStatus = new() { Left = 370, Top = 530, Width = 400, ForeColor = Color.SeaGreen };

    private List<AppRole> _roles = [];
    private List<Permission> _permissions = [];
    private AppRole? _selectedRole;

    public RolesGestionForm()
    {
        Text = "Rôles & Permissions";
        Width = 830;
        Height = 610;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(new Label { Left = 10, Top = 10, Width = 220, Text = "Rôles" });
        Controls.Add(_lstRoles);
        Controls.Add(_lblSelectedRole);
        Controls.Add(_grid);
        Controls.Add(_btnSave);
        Controls.Add(_lblStatus);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Module", HeaderText = "Module", ReadOnly = true });
        foreach (var (action, label) in Actions)
        {
            _grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = action, HeaderText = label });
        }

        _lstRoles.DisplayMember = "Name";
        _lstRoles.SelectedIndexChanged += async (_, _) => await SelectRoleAsync();
        _btnSave.Click += async (_, _) => await SaveAsync();

        Load += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        try
        {
            Logger.Debug("Chargement des rôles et permissions.");
            _roles = await db.AppRoles.OrderBy(r => r.Id).ToListAsync();
            _permissions = await db.Permissions.ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec du chargement des rôles et permissions.");
            MessageBox.Show(this, $"Erreur de chargement : {ex.Message}", "Rôles & Permissions", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _lstRoles.DataSource = _roles;
        BuildModuleRows([]);
    }

    private void BuildModuleRows(HashSet<int> granted)
    {
        _grid.Rows.Clear();
        foreach (var module in Modules)
        {
            var rowIndex = _grid.Rows.Add(ModuleLabel(module));
            var row = _grid.Rows[rowIndex];
            foreach (var (action, _) in Actions)
            {
                var perm = _permissions.FirstOrDefault(p => p.Feature == module && p.Action == action);
                var cell = row.Cells[action];
                if (perm is null)
                {
                    cell.ReadOnly = true;
                    ((DataGridViewCheckBoxCell)cell).Value = false;
                }
                else
                {
                    ((DataGridViewCheckBoxCell)cell).Value = granted.Contains(perm.Id);
                    cell.Tag = perm.Id;
                }
            }
        }
    }

    private async Task SelectRoleAsync()
    {
        _selectedRole = _lstRoles.SelectedItem as AppRole;
        if (_selectedRole is null)
        {
            _lblSelectedRole.Text = string.Empty;
            BuildModuleRows([]);
            return;
        }

        _lblSelectedRole.Text = $"Permissions — {_selectedRole.Name}";

        using var scope = AppHost.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        var grantedIds = await db.RolePermissions
            .Where(rp => rp.RoleId == _selectedRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        BuildModuleRows([.. grantedIds]);
    }

    private async Task SaveAsync()
    {
        if (_selectedRole is null)
        {
            MessageBox.Show(this, "Sélectionnez un rôle.", "Rôles & Permissions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var scope = AppHost.CreateScope();
        var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

        if (tenantService.CurrentCompanyId is int currentCompanyId && _selectedRole.CompanyId != currentCompanyId)
        {
            _lblStatus.ForeColor = Color.Firebrick;
            _lblStatus.Text = "Vous ne pouvez modifier que les rôles de votre entreprise.";
            return;
        }

        var grantedPermissionIds = _grid.Rows
            .Cast<DataGridViewRow>()
            .SelectMany(r => Actions.Select(a => r.Cells[a.Action]))
            .Where(c => c.Tag is int && (bool)((DataGridViewCheckBoxCell)c).Value)
            .Select(c => (int)c.Tag!)
            .ToList();

        _btnSave.Enabled = false;
        try
        {
            Logger.Debug("Enregistrement des permissions du rôle {RoleId} ({RoleName}) : {Count} permission(s).",
                _selectedRole.Id, _selectedRole.Name, grantedPermissionIds.Count);

            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using var db = await dbFactory.CreateDbContextAsync();

            var existing = await db.RolePermissions.Where(rp => rp.RoleId == _selectedRole.Id).ToListAsync();
            db.RolePermissions.RemoveRange(existing);
            foreach (var permId in grantedPermissionIds)
                db.RolePermissions.Add(new RolePermission { RoleId = _selectedRole.Id, PermissionId = permId });
            await db.SaveChangesAsync();

            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
            var affectedUsers = await db.UserRoles
                .Where(ur => ur.RoleId == _selectedRole.Id)
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();
            foreach (var userId in affectedUsers)
                permissionService.InvalidateUser(userId);

            _lblStatus.ForeColor = Color.SeaGreen;
            _lblStatus.Text = "Permissions enregistrées.";
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec de l'enregistrement des permissions du rôle {RoleId}.", _selectedRole.Id);
            _lblStatus.ForeColor = Color.Firebrick;
            _lblStatus.Text = $"Erreur : {ex.Message}";
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }

    private static string ModuleLabel(string module) => module switch
    {
        "clients" => "Clients",
        "factures" => "Factures clients",
        "devis" => "Devis",
        "commandes-vente" => "Commandes vente",
        "bons-livraison" => "Bons de livraison",
        "fournisseurs" => "Fournisseurs",
        "commandes-achat" => "Commandes achat",
        "bons-reception" => "Bons de réception",
        "factures-fournisseur" => "Factures fournisseur",
        "produits" => "Produits",
        _ => module,
    };
}
