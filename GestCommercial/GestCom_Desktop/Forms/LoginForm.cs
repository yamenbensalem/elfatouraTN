using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms;

public class LoginForm : Form
{
    private readonly TextBox _txtLogin = new() { Left = 120, Top = 30, Width = 200 };
    private readonly TextBox _txtPassword = new() { Left = 120, Top = 65, Width = 200, PasswordChar = '•' };
    private readonly Label _lblError = new() { Left = 20, Top = 100, Width = 300, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnLogin = new() { Left = 120, Top = 130, Width = 100, Text = "Connexion" };

    public LoginForm()
    {
        Text = "GestCom — Connexion";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(340, 180);
        AcceptButton = _btnLogin;

        Controls.Add(new Label { Left = 20, Top = 33, Width = 90, Text = "Identifiant" });
        Controls.Add(new Label { Left = 20, Top = 68, Width = 90, Text = "Mot de passe" });
        Controls.Add(_txtLogin);
        Controls.Add(_txtPassword);
        Controls.Add(_lblError);
        Controls.Add(_btnLogin);

        _btnLogin.Click += async (_, _) => await AuthenticateAsync();
    }

    private async Task AuthenticateAsync()
    {
        _lblError.Text = string.Empty;
        var login = _txtLogin.Text.Trim();
        var password = _txtPassword.Text;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            _lblError.Text = "Identifiant et mot de passe requis.";
            return;
        }

        _btnLogin.Enabled = false;
        try
        {
            using var scope = AppHost.CreateScope();
            var utilisateurs = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();

            var user = await utilisateurs.AuthentifierAsync(login, password);
            if (user is null)
            {
                // Jamais le mot de passe dans le log — voir GestCommercial/.claude/rules/security.md.
                Log.Warning("Tentative de connexion échouée (identifiants invalides) pour {Login}", login);
                _lblError.Text = "Identifiant ou mot de passe incorrect.";
                return;
            }

            var role = await utilisateurs.GetPrimaryRoleNameAsync(user.Id, user.IsSuperAdmin);
            AppHost.Session.SignIn(user.Id, user.CompanyId, user.Login, role, user.IsSuperAdmin);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            // ex.ToString() (pas juste .Message) dans le journal : la pile d'appel et l'exception
            // interne sont indispensables pour diagnostiquer, contrairement au label UI ci-dessous
            // qui reste court et lisible pour l'utilisateur.
            Log.Error(ex, "Échec de connexion pour {Login}", login);
            _lblError.Text = $"Erreur de connexion : {ex.Message}";
        }
        finally
        {
            _btnLogin.Enabled = true;
        }
    }
}
