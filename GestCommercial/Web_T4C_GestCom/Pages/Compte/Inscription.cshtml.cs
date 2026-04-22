using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace Web_T4C_GestCom.Pages.Compte;

public class InscriptionModel(
    IUtilisateurService utilisateurService,
    IConfiguration configuration) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }
    public bool Success { get; private set; }
    private bool AllowPublicSignup { get; } = configuration.GetValue<bool>("Security:AllowPublicSignup");

    public IActionResult OnGet()
    {
        if (!AllowPublicSignup)
            return NotFound();

        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect("/");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!AllowPublicSignup)
            return NotFound();

        if (!ModelState.IsValid) return Page();

        if (Input.Password != Input.ConfirmPassword)
        {
            ErrorMessage = "Les mots de passe ne correspondent pas.";
            return Page();
        }

        var loginExists = await utilisateurService.LoginExistsAsync(Input.Email);
        if (loginExists)
        {
            ErrorMessage = "Cette adresse email est déjà utilisée.";
            return Page();
        }

        var utilisateur = new Utilisateur
        {
            Login  = Input.Email.Trim().ToLower(),
            Prenom = Input.Prenom.Trim(),
            Nom    = Input.Nom.Trim(),
            Email  = Input.Email.Trim().ToLower(),
            Role   = "Employé",
            Actif  = true
        };

        await utilisateurService.AddAsync(utilisateur, Input.Password);
        return RedirectToPage("/Compte/Connexion", new { message = "inscription-ok" });
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [MaxLength(50)]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [MaxLength(50)]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse email invalide.")]
        [MaxLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit comporter au moins 6 caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
