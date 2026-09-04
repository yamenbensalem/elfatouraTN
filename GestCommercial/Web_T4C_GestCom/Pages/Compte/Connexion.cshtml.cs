using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web_T4C_GestCom.Services;

namespace Web_T4C_GestCom.Pages.Compte;

public class ConnexionModel(
    IUtilisateurService utilisateurService,
    ILoginProtectionService loginProtectionService,
    IJournalActiviteService journalActiviteService,
    IConfiguration configuration,
    ILogger<ConnexionModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }
    public bool AllowPublicSignup { get; } = configuration.GetValue<bool>("Security:AllowPublicSignup");

    public IActionResult OnGet(string? returnUrl = null)
    {
        Input.ReturnUrl = returnUrl;
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect(returnUrl ?? "/");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var nowUtc = DateTimeOffset.UtcNow;
        var login = (Input.Login ?? string.Empty).Trim();
        var ipAddress = ResolveClientIpAddress();

        var preCheck = loginProtectionService.Evaluate(login, ipAddress, nowUtc);
        if (preCheck.IsBlocked)
        {
            await EmitSecurityAlertsAsync(preCheck.Alerts, login, ipAddress, "blocked-precheck");
            ErrorMessage = BuildBlockedErrorMessage(preCheck, nowUtc);
            return Page();
        }

        var user = await utilisateurService.AuthentifierAsync(login, Input.Password);
        if (user is null)
        {
            var failure = loginProtectionService.RegisterFailure(login, ipAddress, nowUtc);
            await EmitSecurityAlertsAsync(failure.Alerts, login, ipAddress, "invalid-credentials");
            ErrorMessage = failure.IsBlocked
                ? BuildBlockedErrorMessage(failure, nowUtc)
                : "Login ou mot de passe incorrect.";
            return Page();
        }

        loginProtectionService.RegisterSuccess(login, ipAddress);

        var role = await utilisateurService.GetPrimaryRoleNameAsync(user.Id, user.IsSuperAdmin);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,      user.Login),
            new(ClaimTypes.GivenName, user.NomComplet),
            new(ClaimTypes.Role,      role),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("UserId",             user.Id.ToString()),
            new("IsSuperAdmin",       user.IsSuperAdmin ? "1" : "0"),
            new("SecurityStamp",      user.SecurityStamp),
            new("PermissionsVersion", user.PermissionsVersion.ToString())
        };

        if (!user.IsSuperAdmin && user.CompanyId.HasValue)
            claims.Add(new Claim("CompanyId", user.CompanyId.Value.ToString()));

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props     = new AuthenticationProperties
        {
            IsPersistent  = Input.RememberMe,
            ExpiresUtc    = Input.RememberMe
                            ? DateTimeOffset.UtcNow.AddDays(30)
                            : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        // SignInAsync pose le cookie pour la PROCHAINE requête mais ne met pas à jour
        // HttpContext.User pour le reste de CETTE requête — sans cette ligne, le prochain appel
        // qui dépend du tenant courant (ex. journalActiviteService.EnregistrerAsync, qui écrit une
        // entité liée à l'entreprise) verrait encore un contexte non authentifié et
        // AppDbContext.ApplyTenantOwnershipRules() lèverait "Aucun tenant actif".
        HttpContext.User = principal;

        await journalActiviteService.EnregistrerAsync("Connexion", "Authentification", user.Login, $"ip={ipAddress}");

        return LocalRedirect(Input.ReturnUrl ?? "/");
    }

    private async Task EmitSecurityAlertsAsync(
        IReadOnlyList<string> alerts,
        string login,
        string ipAddress,
        string stage)
    {
        if (alerts.Count == 0)
            return;

        foreach (var alert in alerts.Distinct(StringComparer.Ordinal))
        {
            logger.LogWarning(
                "Alerte de securite login: {AlertCode} | stage={Stage} | login={Login} | ip={IP}",
                alert,
                stage,
                login,
                ipAddress);

            await journalActiviteService.EnregistrerAsync(
                "AlerteSecurite",
                "Authentification",
                login,
                $"{alert}; stage={stage}; ip={ipAddress}");
        }
    }

    private static string BuildBlockedErrorMessage(LoginProtectionDecision decision, DateTimeOffset nowUtc)
    {
        var retryAfterSeconds = decision.GetRetryAfterSeconds(nowUtc);
        if (retryAfterSeconds <= 0)
            return "Trop de tentatives de connexion. Reessayez plus tard.";

        if (retryAfterSeconds < 60)
            return $"Trop de tentatives de connexion. Reessayez dans {retryAfterSeconds} seconde(s).";

        var retryAfterMinutes = (int)Math.Ceiling(retryAfterSeconds / 60.0);
        return $"Trop de tentatives de connexion. Reessayez dans {retryAfterMinutes} minute(s).";
    }

    private string ResolveClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstHop = forwardedFor.Split(',')[0].Trim();
            if (!string.IsNullOrWhiteSpace(firstHop))
                return firstHop;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Le login est obligatoire.")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }

        public bool RememberMe { get; set; }
    }
}
