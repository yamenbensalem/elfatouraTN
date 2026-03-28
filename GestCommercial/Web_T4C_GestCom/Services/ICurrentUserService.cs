namespace Web_T4C_GestCom.Services;

/// <summary>
/// Service scopé qui maintient l'identité de l'utilisateur courant pour toute la durée du circuit Blazor.
/// Doit être initialisé depuis MainLayout via SetCurrentUser().
/// </summary>
public interface ICurrentUserService
{
    string Login { get; }
    string Role { get; }
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
    void SetCurrentUser(string login, string role);
    void Clear();
}

public class CurrentUserService : ICurrentUserService
{
    public string Login { get; private set; } = "système";
    public string Role { get; private set; } = "Employé";
    public bool IsAdmin => Role == "Admin";
    public bool IsAuthenticated => Login != "système" && !string.IsNullOrEmpty(Login);

    public void SetCurrentUser(string login, string role)
    {
        Login = login;
        Role = role;
    }

    public void Clear()
    {
        Login = "système";
        Role = "Employé";
    }
}
