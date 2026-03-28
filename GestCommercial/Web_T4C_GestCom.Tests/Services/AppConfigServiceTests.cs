using Microsoft.Extensions.Configuration;
using Web_T4C_GestCom.Services;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class AppConfigServiceTests
{
    private static AppConfigService Build(Dictionary<string, string?> values)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        return new AppConfigService(config);
    }

    [Fact]
    public void Constructor_WithEmptyConfig_UsesDefaults()
    {
        var svc = Build([]);

        Assert.Equal(0.6, svc.TimbreFiscal);
        Assert.Equal(1.5, svc.TauxRetenue);
        Assert.True(svc.DisplayRemise);
        Assert.True(svc.DisplayTVA);
        Assert.Equal("./logoApp.png", svc.PathLogo);
        Assert.Equal("0.###", svc.FormatPrix);
        Assert.Equal("0.###", svc.FormatQuantite);
    }

    [Fact]
    public void Constructor_LoadsAllConfigValues()
    {
        // Use whole-number strings to avoid decimal-separator locale ambiguity
        // (AppConfigService uses double.TryParse with CurrentCulture)
        var svc = Build(new Dictionary<string, string?>
        {
            ["AppConfig:TimbreFiscal"] = "2",
            ["AppConfig:TauxRetenue"] = "3",
            ["AppConfig:DisplayRemise"] = "No",
            ["AppConfig:DisplayTVA"] = "No",
            ["AppConfig:PathLogo"] = "./custom.png"
        });

        Assert.Equal(2.0, svc.TimbreFiscal);
        Assert.Equal(3.0, svc.TauxRetenue);
        Assert.False(svc.DisplayRemise);
        Assert.False(svc.DisplayTVA);
        Assert.Equal("./custom.png", svc.PathLogo);
    }

    [Fact]
    public void DisplayRemise_WhenNotNo_IsTrue()
    {
        var svc = Build(new Dictionary<string, string?> { ["AppConfig:DisplayRemise"] = "Yes" });
        Assert.True(svc.DisplayRemise);
    }

    [Fact]
    public void DisplayTVA_WhenNo_IsFalse()
    {
        var svc = Build(new Dictionary<string, string?> { ["AppConfig:DisplayTVA"] = "No" });
        Assert.False(svc.DisplayTVA);
    }

    [Fact]
    public void FormatMontant_FormatsWithPrixFormat()
    {
        var svc = Build([]);
        Assert.Equal((1234.5).ToString("0.###"), svc.FormatMontant(1234.5));
    }

    [Fact]
    public void FormatQte_FormatsWithQuantiteFormat()
    {
        var svc = Build([]);
        Assert.Equal((42.75).ToString("0.###"), svc.FormatQte(42.75));
    }

    [Fact]
    public void FormatMontant_ZeroValue_ReturnsZeroFormatted()
    {
        var svc = Build([]);
        Assert.Equal((0.0).ToString("0.###"), svc.FormatMontant(0));
    }
}
