using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Web_T4C_GestCom.Components.Shared;
using Xunit;

namespace Web_T4C_GestCom.Tests.Components.Shared;

public sealed class ErrorDialogTests : TestContext
{
    // ── Visibility ────────────────────────────────────────────────────────

    [Fact]
    public void Visible_False_DoesNotRenderModal()
    {
        var cut = RenderComponent<ErrorDialog>(p => p
            .Add(x => x.Visible, false));

        Assert.DoesNotContain("modal-content", cut.Markup);
    }

    [Fact]
    public void Visible_True_RendersErrorModal()
    {
        var cut = RenderComponent<ErrorDialog>(p => p
            .Add(x => x.Visible, true));

        cut.Find(".modal-content");
    }

    // ── Default Parameters ────────────────────────────────────────────────

    [Fact]
    public void DefaultTitle_IsErreur()
    {
        var cut = RenderComponent<ErrorDialog>(p => p
            .Add(x => x.Visible, true));

        Assert.Contains("Erreur", cut.Find(".modal-title").TextContent);
    }

    [Fact]
    public void DefaultMessage_IsGenericError()
    {
        var cut = RenderComponent<ErrorDialog>(p => p
            .Add(x => x.Visible, true));

        Assert.Contains("erreur est survenue", cut.Find(".modal-body").TextContent);
    }

    [Fact]
    public void ErrorHeader_HasDangerBackground()
    {
        var cut = RenderComponent<ErrorDialog>(p => p
            .Add(x => x.Visible, true));

        Assert.Contains("bg-danger", cut.Find(".modal-header").ClassName);
    }

    // ── Custom Parameters ─────────────────────────────────────────────────

    [Fact]
    public void CustomTitleAndMessage_AreRendered()
    {
        var cut = RenderComponent<ErrorDialog>(p => p
            .Add(x => x.Visible, true)
            .Add(x => x.Title, "Suppression impossible")
            .Add(x => x.Message, "Ce client est lié à des factures."));

        Assert.Contains("Suppression impossible", cut.Find(".modal-title").TextContent);
        Assert.Contains("Ce client est lié à des factures.", cut.Find(".modal-body").TextContent);
    }

    // ── Close Callback ────────────────────────────────────────────────────

    [Fact]
    public async Task FermerButton_InvokesOnClose()
    {
        var closed = false;
        var cut = RenderComponent<ErrorDialog>(p => p
            .Add(x => x.Visible, true)
            .Add(x => x.OnClose, EventCallback.Factory.Create(this, () => closed = true)));

        await cut.Find(".btn-danger").ClickAsync(new MouseEventArgs());

        Assert.True(closed);
    }

    [Fact]
    public async Task CloseIcon_InvokesOnClose()
    {
        var closed = false;
        var cut = RenderComponent<ErrorDialog>(p => p
            .Add(x => x.Visible, true)
            .Add(x => x.OnClose, EventCallback.Factory.Create(this, () => closed = true)));

        await cut.Find(".btn-close").ClickAsync(new MouseEventArgs());

        Assert.True(closed);
    }
}
