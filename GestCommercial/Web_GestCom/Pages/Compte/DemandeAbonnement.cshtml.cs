using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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
/// Page anonyme exposée à Internet : protégée contre les robots par un champ piège invisible, un délai
/// minimal de remplissage signé côté serveur, un plafond d'envois par IP et un refus des doublons
/// (voir <see cref="OnPostAsync"/>) — suite à un envoi en masse de ~800 fausses demandes le 2026-11-04.
/// </summary>
[IgnoreAntiforgeryToken]
public class DemandeAbonnementModel(
    IAbonnementService abonnementService,
    IAntiforgery antiforgery,
    IOptions<TarifsOptions> tarifsOptions,
    IDataProtectionProvider dataProtection,
    IPublicFormRateLimiter rateLimiter,
    ILogger<DemandeAbonnementModel> logger) : PageModel
{
    public const string FormTokenPurpose = "GestCom.DemandeAbonnement.FormToken.v1";
    public const int DelaiMinimalSecondes = 3;
    public const int AgeMaxHeures = 24;
    public const int MaxEnvoisParIpParHeure = 8;

    public const string FormulaireExpireMessage =
        "Le formulaire a expiré. Vérifiez vos informations puis renvoyez-le.";
    public const string EnvoiTropRapideMessage =
        "Envoi trop rapide. Patientez quelques secondes puis cliquez de nouveau sur « Envoyer la demande ».";
    public const string TropDeDemandesMessage =
        "Trop de demandes depuis votre connexion. Réessayez dans une heure ou contactez-nous directement.";
    public const string DemandeDejaEnCoursMessage =
        "Une demande avec cet email et ce plan est déjà en cours de traitement. Nous vous recontactons sous 24 à 48h.";

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
        IssueFormToken();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Jeton invalide → on réaffiche le formulaire (saisie conservée, nouveau jeton émis pour
        // l'utilisateur courant) au lieu du 400 vide par défaut. La protection CSRF est conservée :
        // rien n'est enregistré tant que le jeton n'est pas valide.
        if (!await antiforgery.IsRequestValidAsync(HttpContext))
        {
            ModelState.AddModelError(string.Empty, SessionChangeeMessage);
            IssueFormToken();
            return Page();
        }

        var ip = ClientIp();

        // Champ piège : invisible pour un humain, rempli par les robots qui complètent tous les champs.
        // On répond comme si tout s'était bien passé (le robot n'apprend rien) mais rien n'est enregistré
        // ni envoyé.
        if (!string.IsNullOrWhiteSpace(Input.Commentaire))
        {
            logger.LogWarning("Demande d'abonnement ignorée (champ piège rempli) depuis {Ip}.", ip);
            DemandeEnvoyee = true;
            return Page();
        }

        // Jeton de temps signé côté serveur : un robot qui poste instantanément, sans jeton, ou avec un
        // jeton falsifié/expiré est refusé. Message neutre et réessayable : un vrai visiteur retente.
        var age = LireAgeDuFormulaire();
        if (age is null || age > TimeSpan.FromHours(AgeMaxHeures))
        {
            ModelState.AddModelError(string.Empty, FormulaireExpireMessage);
            IssueFormToken();
            return Page();
        }
        if (age < TimeSpan.FromSeconds(DelaiMinimalSecondes))
        {
            logger.LogWarning("Demande d'abonnement refusée (envoi en {Age:0.0}s) depuis {Ip}.", age.Value.TotalSeconds, ip);
            ModelState.AddModelError(string.Empty, EnvoiTropRapideMessage);
            return Page();
        }

        if (!rateLimiter.TryAcquire($"demande-abonnement:{ip}", MaxEnvoisParIpParHeure, TimeSpan.FromHours(1), DateTimeOffset.UtcNow))
        {
            logger.LogWarning("Demande d'abonnement refusée (plafond {Max}/h atteint) depuis {Ip}.", MaxEnvoisParIpParHeure, ip);
            ModelState.AddModelError(string.Empty, TropDeDemandesMessage);
            return Page();
        }

        if (!PlansValides.Contains(Input.Plan))
            ModelState.AddModelError("Input.Plan", "Plan inconnu.");

        if (!ModelState.IsValid) return Page();

        if (await abonnementService.ExisteDemandeRecenteAsync(Input.EmailContact, Input.Plan))
        {
            ModelState.AddModelError(string.Empty, DemandeDejaEnCoursMessage);
            return Page();
        }

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

    private void IssueFormToken()
    {
        // Sans ce Remove, le tag helper réaffiche l'ancienne valeur postée (ModelState prime sur le modèle).
        ModelState.Remove("Input.FormToken");
        Input.FormToken = dataProtection.CreateProtector(FormTokenPurpose)
            .Protect(DateTimeOffset.UtcNow.UtcTicks.ToString(CultureInfo.InvariantCulture));
    }

    private TimeSpan? LireAgeDuFormulaire()
    {
        if (string.IsNullOrEmpty(Input.FormToken)) return null;
        try
        {
            var ticks = long.Parse(dataProtection.CreateProtector(FormTokenPurpose).Unprotect(Input.FormToken), CultureInfo.InvariantCulture);
            return DateTimeOffset.UtcNow - new DateTimeOffset(ticks, TimeSpan.Zero);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException or OverflowException or ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    /// <summary>
    /// Dernier saut de X-Forwarded-For : c'est celui ajouté par le proxy de confiance (nginx-proxy). Le
    /// premier saut est fourni par le client et donc falsifiable à volonté — le prendre permettrait de
    /// contourner le plafond par IP en changeant simplement l'en-tête à chaque requête.
    /// </summary>
    private string ClientIp()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var dernier = forwardedFor.Split(',')[^1].Trim();
            if (dernier.Length > 0) return dernier;
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "inconnue";
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

        /// <summary>Jeton signé contenant l'heure d'affichage du formulaire (voir <see cref="DemandeAbonnementModel"/>).</summary>
        public string? FormToken { get; set; }

        /// <summary>Champ piège anti-robot : doit rester vide.</summary>
        public string? Commentaire { get; set; }
    }
}
