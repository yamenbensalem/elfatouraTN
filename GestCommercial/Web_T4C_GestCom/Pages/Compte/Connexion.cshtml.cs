using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web_T4C_GestCom.Services;

namespace Web_T4C_GestCom.Pages.Compte;

public class ConnexionModel(IUtilisateurService utilisateurService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

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

        var user = await utilisateurService.AuthentifierAsync(Input.Login, Input.Password);
        if (user is null)
        {
            ErrorMessage = "Login ou mot de passe incorrect.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,      user.Login),
            new(ClaimTypes.GivenName, user.NomComplet),
            new(ClaimTypes.Role,      RoleNameMapper.NormalizeKnownRoleName(user.Role)),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("UserId",             user.Id.ToString())
        };

        if (user.CompanyId.HasValue)
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
        return LocalRedirect(Input.ReturnUrl ?? "/");
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
