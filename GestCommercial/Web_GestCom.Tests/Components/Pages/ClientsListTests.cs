using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Web_GestCom.Components.Pages.Clients;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Components.Pages;

public sealed class ClientsListTests : TestContext
{
    private readonly Mock<IClientService>     _clientService = new();
    private readonly Mock<IPermissionService> _permService   = new();

    public ClientsListTests()
    {
        Services.AddScoped(_ => _clientService.Object);
        Services.AddSingleton(_permService.Object);
    }

    private void AuthorizeAdmin()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin");
        auth.SetRoles("Admin");
        auth.SetPolicies("perm:clients.view");
    }

    // ── Empty State ───────────────────────────────────────────────────────

    [Fact]
    public void EmptyList_ShowsNoClientsFoundMessage()
    {
        _clientService.Setup(s => s.GetAllAsync(null)).ReturnsAsync([]);
        AuthorizeAdmin();

        var cut = RenderComponent<ClientsList>();
        cut.WaitForState(
            () => cut.Markup.Contains("Aucun client"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("Aucun client trouvé", cut.Markup);
    }

    [Fact]
    public void EmptyList_FooterShowsZeroClients()
    {
        _clientService.Setup(s => s.GetAllAsync(null)).ReturnsAsync([]);
        AuthorizeAdmin();

        var cut = RenderComponent<ClientsList>();
        cut.WaitForState(
            () => cut.Markup.Contains("client(s)"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("0 client(s)", cut.Markup);
    }

    // ── List Display ──────────────────────────────────────────────────────

    [Fact]
    public void Clients_AreRenderedInTable()
    {
        _clientService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(
        [
            new Client { CodeClient = "CL00001", NomClient = "Alpha SARL", CodeDevise = 1, EtatClient = "Actif" },
            new Client { CodeClient = "CL00002", NomClient = "Beta Corp",  CodeDevise = 1, EtatClient = "Actif" }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<ClientsList>();
        cut.WaitForState(
            () => cut.Markup.Contains("Alpha SARL"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("Alpha SARL", cut.Markup);
        Assert.Contains("Beta Corp",  cut.Markup);
        Assert.Contains("CL00001",    cut.Markup);
    }

    [Fact]
    public void ClientCount_ShownInFooter()
    {
        _clientService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(
        [
            new Client { CodeClient = "CL00001", NomClient = "Alpha SARL", CodeDevise = 1 },
            new Client { CodeClient = "CL00002", NomClient = "Beta Corp",  CodeDevise = 1 }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<ClientsList>();
        cut.WaitForState(
            () => cut.Markup.Contains("client(s)"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("2 client(s)", cut.Markup);
    }

    [Fact]
    public void ActiveClient_ShowsSuccessBadge()
    {
        _clientService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(
        [
            new Client { CodeClient = "CL00001", NomClient = "Active Co", CodeDevise = 1, EtatClient = "Actif" }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<ClientsList>();
        cut.WaitForState(
            () => cut.Markup.Contains("Active Co"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("bg-success", cut.Markup);
    }

    // ── Delete Flow ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteButton_OpensConfirmDialog()
    {
        _clientService.Setup(s => s.GetAllAsync(It.IsAny<string?>())).ReturnsAsync(
        [
            new Client { CodeClient = "CL00001", NomClient = "À Supprimer", CodeDevise = 1, EtatClient = "Actif" }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<ClientsList>();
        cut.WaitForState(
            () => cut.Markup.Contains("À Supprimer"),
            TimeSpan.FromSeconds(5));

        await cut.Find(".btn-outline-danger").ClickAsync(new MouseEventArgs());

        Assert.Contains("Supprimer le client", cut.Markup);
    }

    [Fact]
    public async Task DeleteConfirmed_CallsServiceDeleteAsync()
    {
        var client = new Client { CodeClient = "CL00001", NomClient = "À Supprimer", CodeDevise = 1, EtatClient = "Actif" };
        _clientService.Setup(s => s.GetAllAsync(It.IsAny<string?>())).ReturnsAsync([client]);
        _clientService.Setup(s => s.DeleteAsync("CL00001")).Returns(Task.CompletedTask);
        AuthorizeAdmin();

        var cut = RenderComponent<ClientsList>();
        cut.WaitForState(
            () => cut.Markup.Contains("À Supprimer"),
            TimeSpan.FromSeconds(5));

        // Open confirm dialog then click confirm button
        await cut.Find(".btn-outline-danger").ClickAsync(new MouseEventArgs());
        _clientService.Setup(s => s.GetAllAsync(It.IsAny<string?>())).ReturnsAsync([]);
        await cut.Find(".btn-danger").ClickAsync(new MouseEventArgs());

        _clientService.Verify(s => s.DeleteAsync("CL00001"), Times.Once);
    }

    // ── Page Title ────────────────────────────────────────────────────────

    [Fact]
    public void PageTitle_IsClients()
    {
        _clientService.Setup(s => s.GetAllAsync(null)).ReturnsAsync([]);
        AuthorizeAdmin();

        var cut = RenderComponent<ClientsList>();
        cut.WaitForState(
            () => cut.Markup.Contains("Clients"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("Clients", cut.Find("h3").TextContent);
    }
}
