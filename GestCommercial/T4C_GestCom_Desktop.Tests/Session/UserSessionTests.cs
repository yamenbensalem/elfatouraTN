using T4C_GestCom_Desktop.Session;
using Xunit;

namespace T4C_GestCom_Desktop.Tests.Session;

public class UserSessionTests
{
    [Fact]
    public void IsAuthenticated_BeforeSignIn_IsFalse()
    {
        // Arrange
        var session = new UserSession();

        // Act
        var result = session.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void SignIn_PopulatesAllFields()
    {
        // Arrange
        var session = new UserSession();

        // Act
        session.SignIn(userId: 7, companyId: 3, login: "jdupont", role: "Manager", isSuperAdmin: false);

        // Assert
        Assert.True(session.IsAuthenticated);
        Assert.Equal(7, session.UserId);
        Assert.Equal(3, session.CompanyId);
        Assert.Equal("jdupont", session.Login);
        Assert.Equal("Manager", session.Role);
        Assert.False(session.IsSuperAdmin);
    }

    [Fact]
    public void SignIn_WithNullCompanyId_AllowsSuperAdminSession()
    {
        // Arrange
        var session = new UserSession();

        // Act
        session.SignIn(userId: 1, companyId: null, login: "root", role: "SuperAdmin", isSuperAdmin: true);

        // Assert
        Assert.Null(session.CompanyId);
        Assert.True(session.IsSuperAdmin);
    }

    [Fact]
    public void SignOut_AfterSignIn_ClearsEverything()
    {
        // Arrange
        var session = new UserSession();
        session.SignIn(userId: 7, companyId: 3, login: "jdupont", role: "Manager", isSuperAdmin: false);

        // Act
        session.SignOut();

        // Assert
        Assert.False(session.IsAuthenticated);
        Assert.Null(session.UserId);
        Assert.Null(session.CompanyId);
        Assert.Equal(string.Empty, session.Login);
        Assert.Equal(string.Empty, session.Role);
        Assert.False(session.IsSuperAdmin);
    }
}
