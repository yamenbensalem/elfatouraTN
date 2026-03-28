using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web_T4C_GestCom.Services;

namespace Web_T4C_GestCom.Pages.Compte;

public class MotDePasseOublieModel(IUtilisateurService utilisateurService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool EmailSent { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Check if user exists (we always show success to avoid email enumeration)
        await utilisateurService.GetByLoginAsync(Input.Email.Trim().ToLower());

        EmailSent = true;
        return Page();
    }

    public class InputModel
    {
        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse email invalide.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
