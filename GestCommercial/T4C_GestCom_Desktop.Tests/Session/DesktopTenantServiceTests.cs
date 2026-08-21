using T4C_GestCom_Desktop.Session;
using Xunit;

namespace T4C_GestCom_Desktop.Tests.Session;

public class DesktopTenantServiceTests
{
    [Fact]
    public void Properties_BeforeSignIn_AreEmpty()
    {
        // Arrange
        var session = new UserSession();
        var service = new DesktopTenantService(session);

        // Act & Assert
        Assert.Null(service.CurrentCompanyId);
        Assert.Null(service.CurrentUserId);
        Assert.Equal(string.Empty, service.CurrentUserLogin);
    }

    [Fact]
    public void Properties_AfterSignIn_ReflectSession()
    {
        // Arrange
        var session = new UserSession();
        var service = new DesktopTenantService(session);

        // Act
        session.SignIn(userId: 42, companyId: 9, login: "jdupont", role: "Employé", isSuperAdmin: false);

        // Assert
        Assert.Equal(9, service.CurrentCompanyId);
        Assert.Equal(42, service.CurrentUserId);
        Assert.Equal("jdupont", service.CurrentUserLogin);
    }
}
