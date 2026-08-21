namespace Web_T4C_GestCom.Services;

/// <summary>
/// Abstracts "who is the current user" so that business services work identically whether
/// the host is a Blazor Server circuit (CurrentUserService in Web_T4C_GestCom, HTTP-backed)
/// or a WinForms desktop session (DesktopCurrentUserService in T4C_GestCom_Desktop, set once
/// at login via SetCurrentUser). Only the interface lives here — HTTP-specific implementations
/// stay in the web project, not in this shared library.
/// </summary>
public interface ICurrentUserService
{
    string Login { get; }
    string Role { get; }
    bool IsAdmin { get; }
    bool IsSuperAdmin { get; }
    bool IsAuthenticated { get; }
    Task EnsureInitializedAsync();
    void SetCurrentUser(string login, string role);
    void Clear();
}
