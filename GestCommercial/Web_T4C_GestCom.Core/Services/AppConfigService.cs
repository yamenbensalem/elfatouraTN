using Microsoft.Extensions.Configuration;

namespace Web_T4C_GestCom.Services;

public class AppConfigService
{
    public double TimbreFiscal { get; set; } = 0.6;
    public double TauxRetenue { get; set; } = 1.5;
    public bool DisplayRemise { get; set; } = true;
    public bool DisplayTVA { get; set; } = true;
    public string PathLogo { get; set; } = "./logoApp.png";
    public string FormatPrix { get; set; } = "0.###";
    public string FormatQuantite { get; set; } = "0.###";

    public AppConfigService(IConfiguration configuration)
    {
        TimbreFiscal = double.TryParse(configuration["AppConfig:TimbreFiscal"], out var t) ? t : 0.6;
        TauxRetenue = double.TryParse(configuration["AppConfig:TauxRetenue"], out var r) ? r : 1.5;
        DisplayRemise = configuration["AppConfig:DisplayRemise"] != "No";
        DisplayTVA = configuration["AppConfig:DisplayTVA"] != "No";
        PathLogo = configuration["AppConfig:PathLogo"] ?? "./logoApp.png";
    }

    public string FormatMontant(double value) => value.ToString(FormatPrix);
    public string FormatQte(double value) => value.ToString(FormatQuantite);
}
