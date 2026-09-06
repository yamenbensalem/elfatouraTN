using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Forms.Shared;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Admin;

/// <summary>Desktop equivalent of Components/Pages/Admin/UtilisateurForm.razor.</summary>
public class UtilisateurEditForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<UtilisateurEditForm>();

    private readonly int? _id;
    private readonly bool _isNew;

    private readonly TextBox _txtLogin = new() { Left = 150, Top = 20, Width = 200 };
    private readonly TextBox _txtPrenom = new() { Left = 150, Top = 50, Width = 200 };
    private readonly TextBox _txtNom = new() { Left = 150, Top = 80, Width = 200 };
    private readonly TextBox _txtEmail = new() { Left = 150, Top = 110, Width = 250 };
    private readonly ComboBox _cmbRole = new() { Left = 150, Top = 140, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

    // New-user password
    private readonly TextBox _txtPassword = new() { Left = 150, Top = 175, Width = 200, PasswordChar = '•' };
    private readonly TextBox _txtConfirmPassword = new() { Left = 150, Top = 205, Width = 200, PasswordChar = '•' };

    // Existing-user "change password"
    private readonly TextBox _txtNewPassword = new() { Left = 150, Top = 175, Width = 200, PasswordChar = '•', PlaceholderText = "Nouveau mot de passe" };
    private readonly TextBox _txtConfirmNewPassword = new() { Left = 150, Top = 205, Width = 200, PasswordChar = '•', PlaceholderText = "Confirmer" };
    private readonly Button _btnChangePassword = new() { Left = 360, Top = 173, Width = 140, Text = "Changer le mot de passe" };
    private readonly Label _lblPasswordInfo = new() { Left = 150, Top = 235, Width = 350, ForeColor = Color.DarkGreen, Text = string.Empty };

    private readonly Label _lblError = new() { Left = 20, Top = 265, Width = 480, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnSave = new() { Left = 150, Top = 295, Width = 100, Text = "Enregistrer" };
    private readonly Button _btnCancel = new() { Left = 260, Top = 295, Width = 100, Text = "Annuler" };

    public UtilisateurEditForm(int? id)
    {
        _id = id;
        _isNew = id is null;

        Text = _isNew ? "Nouvel Utilisateur" : "Modifier Utilisateur";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 335);
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        AddField("Login *", _txtLogin, 20);
        AddField("Prénom *", _txtPrenom, 50);
        AddField("Nom *", _txtNom, 80);
        AddField("Email", _txtEmail, 110);
        AddField("Rôle", _cmbRole, 140);

        _cmbRole.Items.AddRange(["Employé", "Manager", "Admin"]);
        _txtLogin.Enabled = _isNew;

        if (_isNew)
        {
            AddField("Mot de passe *", _txtPassword, 175);
            AddField("Confirmer *", _txtConfirmPassword, 205);
        }
        else
        {
            Controls.Add(new Label { Left = 20, Top = 178, Width = 130, Text = "Nouveau mot de passe" });
            Controls.Add(_txtNewPassword);
            Controls.Add(_txtConfirmNewPassword);
            Controls.Add(_btnChangePassword);
            Controls.Add(_lblPasswordInfo);
            _btnChangePassword.Click += async (_, _) => await ChangePasswordAsync();
        }

        Controls.Add(_lblError);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);

        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Load += async (_, _) => await LoadAsync();
    }

    private void AddField(string label, Control input, int top)
    {
        Controls.Add(new Label { Left = 20, Top = top + 3, Width = 120, Text = label });
        Controls.Add(input);
    }

    private async Task LoadAsync()
    {
        if (_isNew)
        {
            _cmbRole.SelectedItem = "Employé";
            return;
        }

        using var scope = AppHost.CreateScope();
        var utilisateurService = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();
        var utilisateur = await utilisateurService.GetByIdAsync(_id!.Value);
        if (utilisateur is null)
        {
            Logger.WarningNotFound("Utilisateur", _id?.ToString());
            MessageBox.Show(this, "Utilisateur introuvable.", "Utilisateur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.Cancel;
            Close();
            return;
        }

        _txtLogin.Text = utilisateur.Login;
        _txtPrenom.Text = utilisateur.Prenom;
        _txtNom.Text = utilisateur.Nom;
        _txtEmail.Text = utilisateur.Email;
#pragma warning disable CS0618 // Role: legacy column, still the field this form edits (mirrors UtilisateurForm.razor)
        _cmbRole.SelectedItem = utilisateur.Role;
#pragma warning restore CS0618
    }

    private async Task SaveAsync()
    {
        _lblError.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(_txtLogin.Text) || string.IsNullOrWhiteSpace(_txtPrenom.Text) || string.IsNullOrWhiteSpace(_txtNom.Text))
        {
            _lblError.Text = "Login, prénom et nom sont obligatoires.";
            return;
        }

        _btnSave.Enabled = false;
        try
        {
            // Jamais le mot de passe dans le log — voir GestCommercial/.claude/rules/security.md.
            Logger.DebugSaving("utilisateur", _txtLogin.Text.Trim(), _isNew);
            using var scope = AppHost.CreateScope();
            var utilisateurService = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();

            if (_isNew)
            {
                if (_txtPassword.Text.Length < 6)
                {
                    _lblError.Text = "Le mot de passe doit faire au moins 6 caractères.";
                    return;
                }
                if (_txtPassword.Text != _txtConfirmPassword.Text)
                {
                    _lblError.Text = "Les mots de passe ne correspondent pas.";
                    return;
                }
                if (await utilisateurService.LoginExistsAsync(_txtLogin.Text.Trim()))
                {
                    _lblError.Text = "Ce login est déjà utilisé.";
                    return;
                }

                var utilisateur = new Utilisateur
                {
                    Login = _txtLogin.Text.Trim(),
                    Prenom = _txtPrenom.Text.Trim(),
                    Nom = _txtNom.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim(),
#pragma warning disable CS0618
                    Role = (string)(_cmbRole.SelectedItem ?? "Employé"),
#pragma warning restore CS0618
                };

                await utilisateurService.AddAsync(utilisateur, _txtPassword.Text);
            }
            else
            {
                // UpdateAsync does a full entity replace (db.Utilisateurs.Update(...)), trusting every
                // field on the passed object — it must be the loaded entity, mutated, not a fresh one,
                // or PasswordHash/CompanyId/IsSuperAdmin/etc. would be wiped (mirrors UtilisateurForm.razor,
                // which binds _utilisateur = the entity loaded from GetByIdAsync).
                var utilisateur = await utilisateurService.GetByIdAsync(_id!.Value)
                    ?? throw new InvalidOperationException("Utilisateur introuvable.");

                utilisateur.Login = _txtLogin.Text.Trim();
                utilisateur.Prenom = _txtPrenom.Text.Trim();
                utilisateur.Nom = _txtNom.Text.Trim();
                utilisateur.Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim();
#pragma warning disable CS0618
                utilisateur.Role = (string)(_cmbRole.SelectedItem ?? "Employé");
#pragma warning restore CS0618

                await utilisateurService.UpdateAsync(utilisateur);
            }

            Logger.DebugSaved("Utilisateur", _txtLogin.Text.Trim());
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Logger.ErrorSaveFailed(ex, "utilisateur", _txtLogin.Text.Trim());
            _lblError.Text = $"Erreur : {ex.Message}";
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }

    private async Task ChangePasswordAsync()
    {
        _lblPasswordInfo.Text = string.Empty;
        _lblError.Text = string.Empty;

        if (_txtNewPassword.Text.Length < 6)
        {
            _lblError.Text = "Le mot de passe doit faire au moins 6 caractères.";
            return;
        }
        if (_txtNewPassword.Text != _txtConfirmNewPassword.Text)
        {
            _lblError.Text = "Les mots de passe ne correspondent pas.";
            return;
        }

        _btnChangePassword.Enabled = false;
        try
        {
            // Jamais le mot de passe dans le log — voir GestCommercial/.claude/rules/security.md.
            Logger.Debug("Changement de mot de passe pour l'utilisateur {Id}.", _id);
            using var scope = AppHost.CreateScope();
            var utilisateurService = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();
            await utilisateurService.ChangePasswordAsync(_id!.Value, _txtNewPassword.Text);

            Logger.Debug("Mot de passe modifié pour l'utilisateur {Id}.", _id);
            _txtNewPassword.Text = string.Empty;
            _txtConfirmNewPassword.Text = string.Empty;
            _lblPasswordInfo.Text = "Mot de passe modifié.";
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec du changement de mot de passe pour l'utilisateur {Id}.", _id);
            _lblError.Text = $"Erreur : {ex.Message}";
        }
        finally
        {
            _btnChangePassword.Enabled = true;
        }
    }
}
