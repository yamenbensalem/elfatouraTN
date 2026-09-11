using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace Web_GestCom.Pages.Compte;

/// <summary>
/// Page publique de demande d'abonnement (liée depuis la section Tarifs de la page d'accueil).
/// Pas de paiement en ligne à ce stade — la demande est stockée pour suivi manuel par le
/// SuperAdmin (voir TODO.md, section Paiement).
/// </summary>
public class DemandeAbonnementModel(IAbonnementService abonnementService) : PageModel
{
    private static readonly string[] PlansValides = ["Standard", "Pro", "Enterprise"];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool DemandeEnvoyee { get; private set; }

    public void OnGet(string? plan)
    {
        if (!string.IsNullOrWhiteSpace(plan) && PlansValides.Contains(plan))
            Input.Plan = plan;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        await abonnementService.CreateDemandeAsync(new Abonnement
        {
            NomEntreprise = Input.NomEntreprise.Trim(),
            NomContact = Input.NomContact.Trim(),
            EmailContact = Input.EmailContact.Trim().ToLower(),
            TelephoneContact = string.IsNullOrWhiteSpace(Input.TelephoneContact) ? null : Input.TelephoneContact.Trim(),
            Plan = Input.Plan,
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

        [Display(Name = "Mode de paiement souhaité")]
        public string? ModePaiementSouhaite { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Message")]
        public string? Message { get; set; }
    }
}
