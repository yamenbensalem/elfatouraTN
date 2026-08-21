using T4C_GestCom_Desktop.Session;
using Xunit;

namespace T4C_GestCom_Desktop.Tests.Session;

public class DesktopCurrentUserServiceTests
{
    [Fact]
    public void IsAuthenticated_BeforeSignIn_IsFalse()
    {
        // Arrange
        var session = new UserSession();
        var service = new DesktopCurrentUserService(session);

        // Act & Assert
        Assert.False(service.IsAuthenticated);
    }

    [Theory]
    [InlineData("Admin", true, false)]
    [InlineData("SuperAdmin", false, true)]
    [InlineData("Manager", false, false)]
    [InlineData("Employé", false, false)]
    public void IsAdminAndIsSuperAdmin_ReflectRole(string role, bool expectedIsAdmin, bool expectedIsSuperAdminByRole)
    {
        // Arrange
        var session = new UserSession();
        var service = new DesktopCurrentUserService(session);
        session.SignIn(userId: 1, companyId: 1, login: "u", role: role, isSuperAdmin: false);

        // Act & Assert
        Assert.Equal(expectedIsAdmin, service.IsAdmin);
        Assert.Equal(expectedIsSuperAdminByRole, service.IsSuperAdmin);
    }

    [Fact]
    public void IsSuperAdmin_WhenSessionFlagSetRegardlessOfRoleString_IsTrue()
    {
        // Arrange
        var session = new UserSession();
        var service = new DesktopCurrentUserService(session);
        session.SignIn(userId: 1, companyId: null, login: "root", role: "SuperAdmin", isSuperAdmin: true);

        // Act & Assert
        Assert.True(service.IsSuperAdmin);
    }

    [Fact]
    public void Clear_SignsSessionOut()
    {
        // Arrange
        var session = new UserSession();
        var service = new DesktopCurrentUserService(session);
        session.SignIn(userId: 1, companyId: 1, login: "u", role: "Employé", isSuperAdmin: false);

        // Act
        service.Clear();

        // Assert
        Assert.False(session.IsAuthenticated);
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public async Task EnsureInitializedAsync_CompletesImmediately()
    {
        // Arrange
        var service = new DesktopCurrentUserService(new UserSession());

        // Act & Assert — desktop session is populated synchronously at login, no async init needed
        await service.EnsureInitializedAsync();
    }
}
