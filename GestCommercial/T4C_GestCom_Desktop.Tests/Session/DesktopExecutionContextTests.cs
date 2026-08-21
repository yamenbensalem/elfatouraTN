using T4C_GestCom_Desktop.Session;
using Xunit;

namespace T4C_GestCom_Desktop.Tests.Session;

public class DesktopExecutionContextTests
{
    [Fact]
    public void HasActiveContext_BeforeSignIn_IsFalse()
    {
        // Arrange
        var session = new UserSession();
        var context = new DesktopExecutionContext(session);

        // Act & Assert
        Assert.False(context.HasActiveContext);
    }

    [Fact]
    public void CurrentCompanyId_AfterSignIn_ReflectsSession()
    {
        // Arrange
        var session = new UserSession();
        var context = new DesktopExecutionContext(session);
        session.SignIn(userId: 1, companyId: 5, login: "a", role: "Employé", isSuperAdmin: false);

        // Act & Assert
        Assert.True(context.HasActiveContext);
        Assert.Equal(5, context.CurrentCompanyId);
        Assert.False(context.IsSuperAdmin);
    }

    [Fact]
    public void IsSuperAdmin_AfterSuperAdminSignIn_IsTrue()
    {
        // Arrange
        var session = new UserSession();
        var context = new DesktopExecutionContext(session);
        session.SignIn(userId: 1, companyId: null, login: "root", role: "SuperAdmin", isSuperAdmin: true);

        // Act & Assert
        Assert.True(context.IsSuperAdmin);
        Assert.Null(context.CurrentCompanyId);
    }

    [Fact]
    public void HasActiveContext_AfterSignOut_IsFalseAgain()
    {
        // Arrange
        var session = new UserSession();
        var context = new DesktopExecutionContext(session);
        session.SignIn(userId: 1, companyId: 5, login: "a", role: "Employé", isSuperAdmin: false);

        // Act
        session.SignOut();

        // Assert
        Assert.False(context.HasActiveContext);
    }
}
