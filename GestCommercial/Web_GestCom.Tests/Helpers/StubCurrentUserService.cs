using Web_GestCom.Services;

namespace Web_GestCom.Tests.Helpers;

/// <summary>
/// Fixed-identity ICurrentUserService for tests that need a real login (e.g. JournalActiviteService
/// tracing) without the async-initialization dance of production implementations.
/// </summary>
public sealed class StubCurrentUserService(string login, bool isSuperAdmin = false, bool isAdmin = false) : ICurrentUserService
{
    public string Login { get; } = login;
    public string Role => "Employé";
    public bool IsAdmin => isAdmin;
    public bool IsSuperAdmin => isSuperAdmin;
    public bool IsAuthenticated => true;
    public Task EnsureInitializedAsync() => Task.CompletedTask;
    public void SetCurrentUser(string login, string role) { }
    public void Clear() { }
}
