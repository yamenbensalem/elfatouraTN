using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class ServicePermissionGuardTests
{
    [Fact]
    public async Task AddClient_WaitsUntilCurrentUserInitializationCompletes()
    {
        var db = DbContextFactory.Create();
        db.Utilisateurs.Add(new Utilisateur
        {
            Login = "admin",
            Prenom = "Admin",
            Nom = "Systeme",
            Role = "Admin",
            Actif = true
        });
        await db.SaveChangesAsync();

        var currentUser = new DelayedCurrentUserService();
        var permissionService = new TrackingPermissionService(_ => true);
        var service = new ClientService(db, new NoOpJournalActiviteService(), currentUser, permissionService);

        var addTask = service.AddAsync(new Client
        {
            CodeClient = "CL00001",
            NomClient = "Client Test",
            CodeDevise = 1
        });

        await Task.Delay(50);
        Assert.False(addTask.IsCompleted);

        currentUser.CompleteInitialization("admin", "Admin");

        var code = await addTask;
        Assert.Equal("CL00001", code);
        Assert.Equal(1, await db.Clients.CountAsync());
        Assert.Equal(0, permissionService.CallCount);
    }

    [Fact]
    public async Task AddClient_UsesInitializedLoginForPermissionEvaluation()
    {
        var db = DbContextFactory.Create();
        var user = new Utilisateur
        {
            Login = "manager1",
            Prenom = "Manager",
            Nom = "User",
            Role = "Manager",
            Actif = true
        };
        db.Utilisateurs.Add(user);
        await db.SaveChangesAsync();

        var currentUser = new DelayedCurrentUserService();
        var permissionService = new TrackingPermissionService(id => id == user.Id);
        var service = new ClientService(db, new NoOpJournalActiviteService(), currentUser, permissionService);

        var addTask = service.AddAsync(new Client
        {
            CodeClient = "CL00002",
            NomClient = "Client Manager",
            CodeDevise = 1
        });

        await Task.Delay(50);
        Assert.False(addTask.IsCompleted);

        currentUser.CompleteInitialization("manager1", "Manager");

        var code = await addTask;
        Assert.Equal("CL00002", code);
        Assert.Equal(1, permissionService.CallCount);
        Assert.Equal("clients.create", permissionService.LastPermission);
        Assert.Equal(user.Id, permissionService.LastUserId);
    }

    private sealed class DelayedCurrentUserService : ICurrentUserService
    {
        private readonly TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string Login { get; private set; } = "système";
        public string Role { get; private set; } = "Employé";
        public bool IsAdmin => string.Equals(Role, RoleNameMapper.Admin, StringComparison.OrdinalIgnoreCase);
        public bool IsSuperAdmin => string.Equals(Role, RoleNameMapper.SuperAdmin, StringComparison.OrdinalIgnoreCase);
        public bool IsAuthenticated => !string.Equals(Login, "système", StringComparison.OrdinalIgnoreCase);

        public Task EnsureInitializedAsync() => _ready.Task;

        public void SetCurrentUser(string login, string role)
        {
            Login = string.IsNullOrWhiteSpace(login) ? "système" : login;
            Role = RoleNameMapper.NormalizeKnownRoleName(role);
        }

        public void Clear()
        {
            Login = "système";
            Role = "Employé";
        }

        public void CompleteInitialization(string login, string role)
        {
            SetCurrentUser(login, role);
            _ready.TrySetResult(true);
        }
    }

    private sealed class TrackingPermissionService(Func<int, bool> rule) : IPermissionService
    {
        private readonly Func<int, bool> _rule = rule;

        public int CallCount { get; private set; }
        public int? LastUserId { get; private set; }
        public string? LastPermission { get; private set; }

        public Task<bool> HasPermissionAsync(int userId, string permission)
        {
            CallCount++;
            LastUserId = userId;
            LastPermission = permission;
            return Task.FromResult(_rule(userId));
        }

        public Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
            => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());

        public void InvalidateUser(int userId)
        {
        }
    }
}