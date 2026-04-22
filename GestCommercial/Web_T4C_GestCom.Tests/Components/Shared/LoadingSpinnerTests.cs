using Bunit;
using Web_T4C_GestCom.Components.Shared;
using Xunit;

namespace Web_T4C_GestCom.Tests.Components.Shared;

public sealed class LoadingSpinnerTests : TestContext
{
    // ── Loading = false ───────────────────────────────────────────────────

    [Fact]
    public void Loading_False_RendersNothing()
    {
        var cut = RenderComponent<LoadingSpinner>(p => p
            .Add(x => x.Loading, false));

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void Loading_False_DefaultValue_RendersNothing()
    {
        var cut = RenderComponent<LoadingSpinner>();

        Assert.Empty(cut.Markup.Trim());
    }

    // ── Loading = true ────────────────────────────────────────────────────

    [Fact]
    public void Loading_True_RendersSpinner()
    {
        var cut = RenderComponent<LoadingSpinner>(p => p
            .Add(x => x.Loading, true));

        cut.Find(".spinner-border");
    }

    [Fact]
    public void Loading_True_RendersDefaultMessage()
    {
        var cut = RenderComponent<LoadingSpinner>(p => p
            .Add(x => x.Loading, true));

        Assert.Contains("Chargement", cut.Markup);
    }

    [Fact]
    public void CustomMessage_IsRendered()
    {
        var cut = RenderComponent<LoadingSpinner>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.Message, "Traitement en cours..."));

        Assert.Contains("Traitement en cours...", cut.Markup);
    }

    [Fact]
    public void Loading_True_SpinnerHasAccessibleRole()
    {
        var cut = RenderComponent<LoadingSpinner>(p => p
            .Add(x => x.Loading, true));

        Assert.Contains("role=\"status\"", cut.Markup);
    }
}
