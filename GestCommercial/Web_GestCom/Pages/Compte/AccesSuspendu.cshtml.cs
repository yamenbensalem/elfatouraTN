using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Web_GestCom.Services;

namespace Web_GestCom.Pages.Compte;

/// <summary>
/// Page vers laquelle SuspensionAccesMiddleware renvoie les utilisateurs d'une entreprise dont
/// l'abonnement est échu au-delà du délai de grâce. Elle explique pourquoi et comment régler ;
/// l'accès revient tout seul dès que le SuperAdmin repousse la date d'échéance.
/// </summary>
public class AccesSuspenduModel(IAccesEntrepriseService accesService, IOptions<EmailOptions> emailOptions) : PageModel
{
    public AccesEntreprise Acces { get; private set; } = AccesEntreprise.SansSuivi;

    public string EmailContact => emailOptions.Value.AdminNotificationEmail;

    public async Task<IActionResult> OnGetAsync()
    {
        Acces = await accesService.GetCourantAsync();

        // Page ouverte directement, ou accès rétabli entre-temps : rien à expliquer.
        return Acces.EstSuspendu ? Page() : Redirect("/");
    }
}
