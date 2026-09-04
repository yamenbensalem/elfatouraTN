using Web_T4C_GestCom.Services;

namespace Web_T4C_GestCom.Tests.Helpers;

/// <summary>
/// Fixed-identity ICurrentUserService for tests that need a real login (e.g. JournalActiviteService
/// tracing) without the async-initialization dance of production implementations.
/// </summary>
public sealed class StubCurrentUserService(string login) : ICurrentUserService
{
    public string Login { get; } = login;
    public string Role => "Employé";
    public bool IsAdmin => false;
    public bool IsSuperAdmin => false;
    public bool IsAuthenticated => true;
    public Task EnsureInitializedAsync() => Task.CompletedTask;
    public void SetCurrentUser(string login, string role) { }
    public void Clear() { }
}
