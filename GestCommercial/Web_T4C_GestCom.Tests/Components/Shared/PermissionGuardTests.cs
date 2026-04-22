using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Web_T4C_GestCom.Components.Shared;
using Web_T4C_GestCom.Services;
using Xunit;

namespace Web_T4C_GestCom.Tests.Components.Shared;

public sealed class PermissionGuardTests : TestContext
{
    private readonly Mock<IPermissionService> _permServiceMock = new();

    public PermissionGuardTests()
    {
        Services.AddSingleton(_permServiceMock.Object);
    }

    private static RenderFragment ProtectedContent()
        => b => b.AddContent(0, "Contenu protégé");

    private static RenderFragment FallbackContent()
        => b => b.AddContent(0, "Accès refusé");

    // ── Not Authenticated ─────────────────────────────────────────────────

    [Fact]
    public void NotAuthenticated_HidesChildContent()
    {
        var auth = this.AddTestAuthorization();
        auth.SetNotAuthorized();

        var cut = RenderComponent<PermissionGuard>(p => p
            .Add(x => x.Permission, "clients.view")
            .Add(x => x.ChildContent, ProtectedContent()));

        Assert.DoesNotContain("Contenu protégé", cut.Markup);
    }

    [Fact]
    public void NotAuthenticated_NoFallback_RendersEmpty()
    {
        var auth = this.AddTestAuthorization();
        auth.SetNotAuthorized();

        var cut = RenderComponent<PermissionGuard>(p => p
            .Add(x => x.Permission, "clients.view")
            .Add(x => x.ChildContent, ProtectedContent()));

        Assert.Empty(cut.Markup.Trim());
    }

    // ── Admin Role ────────────────────────────────────────────────────────

    [Fact]
    public void AdminRole_ShowsContent_WithoutQueryingPermissionService()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin");
        auth.SetRoles("Admin");

        var cut = RenderComponent<PermissionGuard>(p => p
            .Add(x => x.Permission, "clients.delete")
            .Add(x => x.ChildContent, ProtectedContent()));

        cut.WaitForState(
            () => cut.Markup.Contains("Contenu protégé"),
            TimeSpan.FromSeconds(3));

        _permServiceMock.Verify(
            s => s.HasPermissionAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    // ── Permission Claim ──────────────────────────────────────────────────

    [Fact]
    public void HasPermissionClaim_ShowsChildContent()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("employe");
        auth.SetClaims(
            new Claim("Permission", "clients.view"),
            new Claim(ClaimTypes.NameIdentifier, "10"));

        var cut = RenderComponent<PermissionGuard>(p => p
            .Add(x => x.Permission, "clients.view")
            .Add(x => x.ChildContent, ProtectedContent()));

        cut.WaitForState(
            () => cut.Markup.Contains("Contenu protégé"),
            TimeSpan.FromSeconds(3));
    }

    // ── DB Permission Lookup ──────────────────────────────────────────────

    [Fact]
    public void NoClaim_ServiceGrantsPermission_ShowsContent()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("user");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "7"));

        _permServiceMock
            .Setup(s => s.HasPermissionAsync(7, "produits.view"))
            .ReturnsAsync(true);

        var cut = RenderComponent<PermissionGuard>(p => p
            .Add(x => x.Permission, "produits.view")
            .Add(x => x.ChildContent, ProtectedContent()));

        cut.WaitForState(
            () => cut.Markup.Contains("Contenu protégé"),
            TimeSpan.FromSeconds(3));

        _permServiceMock.Verify(s => s.HasPermissionAsync(7, "produits.view"), Times.AtLeastOnce);
    }

    // ── No Permission ─────────────────────────────────────────────────────

    [Fact]
    public void NoPermission_HidesContent_ShowsFallback()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("user");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "99"));

        _permServiceMock
            .Setup(s => s.HasPermissionAsync(99, "factures.delete"))
            .ReturnsAsync(false);

        var cut = RenderComponent<PermissionGuard>(p => p
            .Add(x => x.Permission, "factures.delete")
            .Add(x => x.ChildContent, ProtectedContent())
            .Add(x => x.Fallback, FallbackContent()));

        cut.WaitForState(
            () => !cut.Markup.Contains("Contenu protégé"),
            TimeSpan.FromSeconds(3));

        Assert.DoesNotContain("Contenu protégé", cut.Markup);
        Assert.Contains("Accès refusé", cut.Markup);
    }

    [Fact]
    public void NoPermission_NoFallback_RendersEmpty()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("user");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "42"));

        _permServiceMock
            .Setup(s => s.HasPermissionAsync(42, "factures.delete"))
            .ReturnsAsync(false);

        var cut = RenderComponent<PermissionGuard>(p => p
            .Add(x => x.Permission, "factures.delete")
            .Add(x => x.ChildContent, ProtectedContent()));

        cut.WaitForState(
            () => !cut.Markup.Contains("Contenu protégé"),
            TimeSpan.FromSeconds(3));

        Assert.DoesNotContain("Contenu protégé", cut.Markup);
    }
}
