using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace Web_GestCom.Pages.Compte;

/// <summary>
/// Page publique de demande d'abonnement (liée depuis la section Tarifs de la page d'accueil).
/// Pas de paiement en ligne à ce stade — la demande est stockée pour suivi manuel par le
/// SuperAdmin (voir TODO.md, section Paiement).
/// La validation antiforgery est faite à la main (voir <see cref="OnPostAsync"/>) : le jeton est lié
/// à l'identité de l'utilisateur au moment du GET, donc un formulaire chargé déconnecté puis envoyé
/// après une connexion dans un autre onglet échouait avec un 400 vide (bug réel en prod, 2026-09-27).
/// </summary>
[IgnoreAntiforgeryToken]
public class DemandeAbonnementModel(
    IAbonnementService abonnementService,
    IAntiforgery antiforgery,
    IOptions<TarifsOptions> tarifsOptions) : PageModel
{
    private static readonly string[] PlansValides =
    [
        "Standard", "Pro", "Enterprise",
        TarifsOptions.PlanDesktopEssentiel, TarifsOptions.PlanDesktopSerenite, TarifsOptions.PlanDesktopEquipe
    ];

    public const string SessionChangeeMessage =
        "Votre session a changé depuis l'ouverture de cette page. Vérifiez vos informations puis renvoyez le formulaire.";

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool DemandeEnvoyee { get; private set; }

    /// <summary>Demande telle qu'enregistrée (prix et réduction calculés côté serveur) — pour la page de succès.</summary>
    public Abonnement? Demande { get; private set; }

    public TarifsOptions Tarifs => tarifsOptions.Value;

    public void OnGet(string? plan, string? cycle)
    {
        if (!string.IsNullOrWhiteSpace(plan) && PlansValides.Contains(plan))
            Input.Plan = plan;
        Input.CycleFacturation = CycleFacturation.Normaliser(cycle);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Jeton invalide → on réaffiche le formulaire (saisie conservée, nouveau jeton émis pour
        // l'utilisateur courant) au lieu du 400 vide par défaut. La protection CSRF est conservée :
        // rien n'est enregistré tant que le jeton n'est pas valide.
        if (!await antiforgery.IsRequestValidAsync(HttpContext))
        {
            ModelState.AddModelError(string.Empty, SessionChangeeMessage);
            return Page();
        }

        if (!PlansValides.Contains(Input.Plan))
            ModelState.AddModelError("Input.Plan", "Plan inconnu.");

        if (!ModelState.IsValid) return Page();

        Demande = await abonnementService.CreateDemandeAsync(new Abonnement
        {
            NomEntreprise = Input.NomEntreprise.Trim(),
            NomContact = Input.NomContact.Trim(),
            EmailContact = Input.EmailContact.Trim().ToLower(),
            TelephoneContact = string.IsNullOrWhiteSpace(Input.TelephoneContact) ? null : Input.TelephoneContact.Trim(),
            Plan = Input.Plan,
            CycleFacturation = CycleFacturation.Normaliser(Input.CycleFacturation),
            ModePaiementSouhaite = Input.ModePaiementSouhaite,
            Message = string.IsNullOrWhiteSpace(Input.Message) ? null : Input.Message.Trim()
        });

        DemandeEnvoyee = true;
        return Page();
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Le nom de l'entreprise est obligatoire.")]
        [MaxLength(200)]
        [Display(Name = "Nom de l'entreprise")]
        public string NomEntreprise { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom du contact est obligatoire.")]
        [MaxLength(150)]
        [Display(Name = "Nom du contact")]
        public string NomContact { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse email invalide.")]
        [MaxLength(150)]
        [Display(Name = "Email")]
        public string EmailContact { get; set; } = string.Empty;

        [MaxLength(30)]
        [Display(Name = "Téléphone")]
        public string? TelephoneContact { get; set; }

        [Required]
        public string Plan { get; set; } = "Standard";

        public string CycleFacturation { get; set; } = "Annuel";

        [Display(Name = "Mode de paiement souhaité")]
        public string? ModePaiementSouhaite { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Message")]
        public string? Message { get; set; }
    }
}
