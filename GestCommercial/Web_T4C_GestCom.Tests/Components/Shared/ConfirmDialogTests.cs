using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Web_T4C_GestCom.Components.Shared;
using Xunit;

namespace Web_T4C_GestCom.Tests.Components.Shared;

public sealed class ConfirmDialogTests : TestContext
{
    // ── Visibility ────────────────────────────────────────────────────────

    [Fact]
    public void Visible_False_DoesNotRenderModal()
    {
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(x => x.Visible, false));

        Assert.DoesNotContain("modal-content", cut.Markup);
    }

    [Fact]
    public void Visible_True_RendersModal()
    {
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(x => x.Visible, true));

        cut.Find(".modal-content");
    }

    // ── Default Parameters ────────────────────────────────────────────────

    [Fact]
    public void DefaultTitle_IsConfirmation()
    {
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(x => x.Visible, true));

        Assert.Contains("Confirmation", cut.Find(".modal-title").TextContent);
    }

    [Fact]
    public void DefaultMessage_HasFrenchText()
    {
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(x => x.Visible, true));

        Assert.Contains("Êtes-vous sûr", cut.Find(".modal-body").TextContent);
    }

    // ── Custom Parameters ─────────────────────────────────────────────────

    [Fact]
    public void CustomTitleAndMessage_AreRendered()
    {
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(x => x.Visible, true)
            .Add(x => x.Title, "Supprimer l'élément")
            .Add(x => x.Message, "Cette action est irréversible."));

        Assert.Contains("Supprimer l'élément", cut.Find(".modal-title").TextContent);
        Assert.Contains("Cette action est irréversible.", cut.Find(".modal-body").TextContent);
    }

    // ── Callbacks ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ConfirmButton_InvokesOnConfirm()
    {
        var confirmed = false;
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(x => x.Visible, true)
            .Add(x => x.OnConfirm, EventCallback.Factory.Create(this, () => confirmed = true)));

        await cut.Find(".btn-danger").ClickAsync(new MouseEventArgs());

        Assert.True(confirmed);
    }

    [Fact]
    public async Task AnnulerButton_InvokesOnCancel()
    {
        var cancelled = false;
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(x => x.Visible, true)
            .Add(x => x.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true)));

        await cut.Find(".btn-secondary").ClickAsync(new MouseEventArgs());

        Assert.True(cancelled);
    }

    [Fact]
    public async Task CloseIcon_InvokesOnCancel()
    {
        var cancelled = false;
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(x => x.Visible, true)
            .Add(x => x.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true)));

        await cut.Find(".btn-close").ClickAsync(new MouseEventArgs());

        Assert.True(cancelled);
    }
}
