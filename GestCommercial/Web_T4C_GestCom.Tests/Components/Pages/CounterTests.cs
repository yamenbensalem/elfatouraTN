using Bunit;
using Web_T4C_GestCom.Components.Pages;
using Xunit;

namespace Web_T4C_GestCom.Tests.Components.Pages;

public sealed class CounterTests : TestContext
{
    // ── Initial State ─────────────────────────────────────────────────────

    [Fact]
    public void InitialCount_IsZero()
    {
        var cut = RenderComponent<Counter>();

        Assert.Contains("Current count: 0", cut.Find("[role=status]").TextContent);
    }

    // ── Increment ─────────────────────────────────────────────────────────

    [Fact]
    public void ClickButton_IncrementsCountByOne()
    {
        var cut = RenderComponent<Counter>();

        cut.Find("button").Click();

        Assert.Contains("Current count: 1", cut.Find("[role=status]").TextContent);
    }

    [Fact]
    public void MultipleClicks_AccumulatesCount()
    {
        var cut = RenderComponent<Counter>();

        cut.Find("button").Click();
        cut.Find("button").Click();
        cut.Find("button").Click();

        Assert.Contains("Current count: 3", cut.Find("[role=status]").TextContent);
    }

    // ── UI Structure ──────────────────────────────────────────────────────

    [Fact]
    public void RendersClickMeButton()
    {
        var cut = RenderComponent<Counter>();

        var btn = cut.Find("button");
        Assert.Contains("Click me", btn.TextContent);
    }
}
