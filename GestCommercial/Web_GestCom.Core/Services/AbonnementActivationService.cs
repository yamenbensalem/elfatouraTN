using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;

namespace Web_GestCom.Services;

/// <summary>Identifiants du compte Admin à créer pour le client lors de l'activation.</summary>
public sealed record AccesCompte(string Login, string MotDePasse);

/// <summary>Ce que le traitement d'une demande a réellement fait — pour le message affiché au SuperAdmin.</summary>
public sealed record TraitementAbonnementResult(bool EntrepriseCreee, string? LoginCree, bool EmailDemande);

/// <summary>
/// Traitement d'une demande d'abonnement par le SuperAdmin. Quand la demande passe (ou reste) en
/// Essai/Active, garantit que le client peut réellement se connecter : entreprise créée si aucune
/// n'est liée, compte Admin créé si l'entreprise n'a encore aucun utilisateur, puis email de
/// confirmation. Avant ce service, « Active » n'était qu'un libellé : le SuperAdmin devait créer
/// l'entreprise et le compte à la main ailleurs, et le client n'était jamais prévenu.
/// </summary>
public interface IAbonnementActivationService
{
    /// <summary>Vrai si l'enregistrement de cette demande créera un compte client (donc exige des identifiants).</summary>
    Task<bool> CompteRequisAsync(Abonnement abonnement);

    /// <summary>Login libre dérivé du nom du contact ("BEN SALEM" → "ben.salem", puis "ben.salem2"... si pris).</summary>
    Task<string> SuggererLoginAsync(string nomContact);

    string GenererMotDePasse();

    Task<TraitementAbonnementResult> EnregistrerAsync(Abonnement abonnement, AccesCompte? acces, bool envoyerEmail);
}

public class AbonnementActivationService(
    AppDbContext db,
    IAbonnementService abonnementService,
    ICompanyService companyService,
    IUtilisateurService utilisateurService,
    IEmailTransport emailTransport,
    IOptions<EmailOptions> emailOptions,
    ILogger<AbonnementActivationService> logger) : IAbonnementActivationService
{
    public const int LongueurMinMotDePasse = 8;

    // Sans caractères ambigus (0/O, 1/l/I) : le mot de passe est recopié depuis un email.
    private const string AlphabetMotDePasse = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";

    public static bool EstActif(string statut) => statut is "Essai" or "Active";

    public async Task<bool> CompteRequisAsync(Abonnement abonnement)
    {
        if (!EstActif(abonnement.Statut)) return false;
        if (!abonnement.CompanyId.HasValue) return true;
        return !await db.Utilisateurs.AnyAsync(u => u.CompanyId == abonnement.CompanyId && !u.IsSuperAdmin);
    }

    public async Task<string> SuggererLoginAsync(string nomContact)
    {
        var racine = LoginDepuisNom(nomContact);
        var login = racine;
        for (var suffixe = 2; await utilisateurService.LoginExistsAsync(login); suffixe++)
            login = $"{racine}{suffixe}";
        return login;
    }

    /// <summary>Minuscules, sans accents, mots séparés par un point ; "client" si le nom ne contient rien d'exploitable.</summary>
    public static string LoginDepuisNom(string nom)
    {
        var sansAccents = new string(nom.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());
        var mots = new string(sansAccents.ToLowerInvariant().Select(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' ? c : ' ').ToArray())
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var login = string.Join('.', mots);
        if (login.Length > 40) login = login[..40].TrimEnd('.');
        return login.Length == 0 ? "client" : login;
    }

    public string GenererMotDePasse()
        => new(Enumerable.Range(0, 12)
            .Select(_ => AlphabetMotDePasse[RandomNumberGenerator.GetInt32(AlphabetMotDePasse.Length)])
            .ToArray());

    public async Task<TraitementAbonnementResult> EnregistrerAsync(Abonnement abonnement, AccesCompte? acces, bool envoyerEmail)
    {
        var actif = EstActif(abonnement.Statut);
        var compteRequis = await CompteRequisAsync(abonnement);
        var creerEntreprise = actif && !abonnement.CompanyId.HasValue;

        // Toutes les vérifications avant la première écriture.
        if (compteRequis)
        {
            if (acces is null || string.IsNullOrWhiteSpace(acces.Login))
                throw new InvalidOperationException("Renseignez le login du compte client.");
            if (acces.MotDePasse.Length < LongueurMinMotDePasse)
                throw new InvalidOperationException($"Le mot de passe du compte client doit faire au moins {LongueurMinMotDePasse} caractères.");
            if (await utilisateurService.LoginExistsAsync(acces.Login.Trim()))
                throw new InvalidOperationException($"Le login « {acces.Login.Trim()} » est déjà utilisé. Choisissez-en un autre.");
        }
        if (creerEntreprise && await db.Companies.AnyAsync(c => c.Name == abonnement.NomEntreprise))
            throw new InvalidOperationException(
                $"Une entreprise nommée « {abonnement.NomEntreprise} » existe déjà. Sélectionnez-la dans « Entreprise liée » au lieu d'en créer une nouvelle.");

        Company? entrepriseCreee = null;
        Utilisateur? compteCree = null;
        Entreprise? ficheCreee = null;
        var companyIdInitial = abonnement.CompanyId;

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            if (creerEntreprise)
            {
                entrepriseCreee = new Company { Name = abonnement.NomEntreprise, Plan = abonnement.Plan };
                await companyService.AddAsync(entrepriseCreee);
                abonnement.CompanyId = entrepriseCreee.Id;

                // Fiche entreprise (en-tête des documents) pré-remplie : sans elle, les premières
                // factures du client sortiraient sans nom. Il la complète ensuite dans Paramètres.
                ficheCreee = new Entreprise
                {
                    CodeEntreprise = Entreprise.CodePourCompany(entrepriseCreee.Id),
                    NomEntreprise = abonnement.NomEntreprise,
                    Email = abonnement.EmailContact,
                    Tel = abonnement.TelephoneContact,
                    CompanyId = entrepriseCreee.Id
                };
                db.Entreprises.Add(ficheCreee);
                await db.SaveChangesGuardedAsync();
            }

            if (compteRequis)
            {
                var (prenom, nom) = DecouperNom(abonnement.NomContact);
                compteCree = new Utilisateur
                {
                    Login = acces!.Login.Trim(),
                    Prenom = prenom,
                    Nom = nom,
                    Email = abonnement.EmailContact,
                    Role = RoleNameMapper.Admin,
                    CompanyId = abonnement.CompanyId,
                    Actif = true
                };
                await utilisateurService.AddAsync(compteCree, acces.MotDePasse);
            }

            abonnement.DateTraitement = DateTime.UtcNow;
            await abonnementService.UpdateAsync(abonnement);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            // Le DbContext vit pour tout le circuit Blazor : ne pas y laisser des entités que la base n'a plus.
            if (compteCree is not null) db.Entry(compteCree).State = EntityState.Detached;
            if (ficheCreee is not null) db.Entry(ficheCreee).State = EntityState.Detached;
            if (entrepriseCreee is not null) db.Entry(entrepriseCreee).State = EntityState.Detached;
            abonnement.CompanyId = companyIdInitial;
            throw;
        }

        var emailDemande = envoyerEmail && actif;
        if (emailDemande)
            await NotifierClientAsync(abonnement, compteCree is null ? null : acces);

        return new TraitementAbonnementResult(entrepriseCreee is not null, compteCree?.Login, emailDemande);
    }

    private static (string Prenom, string Nom) DecouperNom(string nomContact)
    {
        var mots = nomContact.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return mots.Length switch
        {
            0 => ("Admin", "Admin"),
            1 => (mots[0], mots[0]),
            _ => (mots[0], string.Join(' ', mots.Skip(1)))
        };
    }

    // Comme pour la demande initiale : un échec d'email est loggé, jamais remonté — l'abonnement est déjà enregistré.
    private async Task NotifierClientAsync(Abonnement abonnement, AccesCompte? acces)
    {
        try
        {
            static string html(string? valeur) => System.Net.WebUtility.HtmlEncode(valeur) ?? "";
            var url = emailOptions.Value.AppUrl.TrimEnd('/') + "/compte/connexion";
            var statut = abonnement.Statut == "Essai" ? "Essai gratuit" : "Actif";
            var dates = abonnement.DateEcheance is DateTime echeance
                ? $"<li><strong>Valable jusqu'au :</strong> {echeance:dd/MM/yyyy}</li>"
                : "";
            var identifiants = acces is null
                ? $"<p>Connectez-vous avec vos identifiants habituels : <a href=\"{url}\">{url}</a></p>"
                : $"""
                   <p>Voici vos identifiants de connexion :</p>
                   <ul>
                     <li><strong>Adresse :</strong> <a href="{url}">{url}</a></li>
                     <li><strong>Login :</strong> {html(acces.Login.Trim())}</li>
                     <li><strong>Mot de passe :</strong> {html(acces.MotDePasse)}</li>
                   </ul>
                   <p>Conservez cet email en lieu sûr et ne le transférez pas.</p>
                   """;

            await emailTransport.SendAsync(
                abonnement.EmailContact, abonnement.NomContact,
                acces is null ? "Votre abonnement GestCom a été mis à jour" : "Votre compte GestCom est activé",
                $"""
                <p>Bonjour {html(abonnement.NomContact)},</p>
                <p>{(acces is null ? "Votre abonnement GestCom a été mis à jour." : "Bonne nouvelle : votre compte GestCom est prêt.")}</p>
                <ul>
                  <li><strong>Entreprise :</strong> {html(abonnement.NomEntreprise)}</li>
                  <li><strong>Plan :</strong> {html(abonnement.Plan)}</li>
                  <li><strong>Statut :</strong> {statut}</li>
                  {dates}
                </ul>
                {identifiants}
                <p>— L'équipe GestCom</p>
                """);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Échec de l'email de confirmation pour l'abonnement {Id}", abonnement.Id);
        }
    }
}
