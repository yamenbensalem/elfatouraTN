using Bunit;
using Web_T4C_GestCom.Components.Shared;
using Xunit;

namespace Web_T4C_GestCom.Tests.Components.Shared;

public sealed class NotificationTests : TestContext
{
    // ── Initial State ─────────────────────────────────────────────────────

    [Fact]
    public void Initially_RendersNothing()
    {
        var cut = RenderComponent<Notification>();
        Assert.Empty(cut.Markup.Trim());
    }

    // ── ShowSuccess ───────────────────────────────────────────────────────

    [Fact]
    public async Task ShowSuccess_RendersSuccessAlert()
    {
        var cut = RenderComponent<Notification>();

        await cut.InvokeAsync(() => cut.Instance.ShowSuccess("Opération réussie"));

        Assert.Contains("alert-success", cut.Markup);
        Assert.Contains("Opération réussie", cut.Markup);
    }

    // ── ShowError ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ShowError_RendersDangerAlert()
    {
        var cut = RenderComponent<Notification>();

        await cut.InvokeAsync(() => cut.Instance.ShowError("Erreur critique"));

        Assert.Contains("alert-danger", cut.Markup);
        Assert.Contains("Erreur critique", cut.Markup);
    }

    // ── ShowWarning ───────────────────────────────────────────────────────

    [Fact]
    public async Task ShowWarning_RendersWarningAlert()
    {
        var cut = RenderComponent<Notification>();

        await cut.InvokeAsync(() => cut.Instance.ShowWarning("Attention"));

        Assert.Contains("alert-warning", cut.Markup);
        Assert.Contains("Attention", cut.Markup);
    }

    // ── ShowInfo ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ShowInfo_RendersInfoAlert()
    {
        var cut = RenderComponent<Notification>();

        await cut.InvokeAsync(() => cut.Instance.ShowInfo("Information importante"));

        Assert.Contains("alert-info", cut.Markup);
        Assert.Contains("Information importante", cut.Markup);
    }

    // ── Dismiss ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Dismiss_HidesAlert()
    {
        var cut = RenderComponent<Notification>();
        await cut.InvokeAsync(() => cut.Instance.ShowSuccess("Visible"));
        Assert.Contains("alert-success", cut.Markup);

        cut.Find(".btn-close").Click();

        Assert.DoesNotContain("alert", cut.Markup);
    }

    [Fact]
    public async Task ShowingNewMessage_OverwritesPreviousType()
    {
        var cut = RenderComponent<Notification>();

        await cut.InvokeAsync(() => cut.Instance.ShowSuccess("Première"));
        Assert.Contains("alert-success", cut.Markup);

        await cut.InvokeAsync(() => cut.Instance.ShowError("Deuxième"));
        Assert.DoesNotContain("alert-success", cut.Markup);
        Assert.Contains("alert-danger", cut.Markup);
    }
}
