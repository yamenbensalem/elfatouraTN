using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;

namespace Web_GestCom.Services;

public enum EtatAcces
{
    /// <summary>Entreprise sans abonnement suivi, ou client actif dont l'échéance n'est pas renseignée : jamais bloquée.</summary>
    Libre,

    /// <summary>Abonnement en cours, échéance non dépassée.</summary>
    AJour,

    /// <summary>Échéance dépassée, mais encore dans le délai de grâce : accès maintenu, avec un avertissement.</summary>
    EnGrace,

    /// <summary>Délai de grâce écoulé (ou essai gratuit terminé) : accès bloqué jusqu'au règlement.</summary>
    Suspendu
}

/// <summary>
/// Droit d'accès d'une entreprise cliente d'après son abonnement. Règle (décidée le 2026-10-09) :
/// passé l'échéance, le client garde l'accès <see cref="JoursGrace"/> jours, puis il est bloqué
/// entièrement — pas de lecture seule — jusqu'à ce que le SuperAdmin enregistre le règlement en
/// repoussant la date d'échéance. Un essai gratuit n'a pas de délai de grâce : il est bloqué dès sa
/// fin. Ne sont jamais bloqués : une entreprise sans abonnement actif, un client actif sans date
/// d'échéance (sinon tous les clients existants seraient coupés au déploiement), et les licences
/// Desktop (le logiciel tourne chez le client, la fin de maintenance ne bloque rien).
///
/// Cette règle est appliquée à trois endroits, tous via cette classe : le middleware
/// SuspensionAccesMiddleware (toute requête HTTP), ServicePermissionGuard (toute écriture, y compris
/// depuis un circuit Blazor resté ouvert) et le bandeau d'avertissement de MainLayout.
/// </summary>
public sealed record AccesEntreprise(EtatAcces Etat, DateTime? DateEcheance, DateTime? DernierJourAcces, bool EstEssai)
{
    /// <summary>Jours d'accès maintenus après l'échéance d'un abonnement payant.</summary>
    public const int JoursGrace = 14;

    public static readonly AccesEntreprise SansSuivi = new(EtatAcces.Libre, null, null, false);

    /// <summary>Jours restants avant suspension, pendant le délai de grâce (0 = dernier jour).</summary>
    public int JoursAvantSuspension(DateTime aujourdHui)
        => DernierJourAcces is DateTime dernier ? Math.Max(0, (dernier.Date - aujourdHui.Date).Days) : 0;

    public bool EstSuspendu => Etat == EtatAcces.Suspendu;

    /// <summary>Délai de grâce d'un abonnement : aucun pour un essai gratuit.</summary>
    public static int JoursGracePour(Abonnement a) => a.Statut == "Essai" ? 0 : JoursGrace;

    public static AccesEntreprise Evaluer(IEnumerable<Abonnement> abonnementsDeLEntreprise, DateTime aujourdHui)
    {
        var actifs = abonnementsDeLEntreprise
            .Where(a => AbonnementActivationService.EstActif(a.Statut) && !TarifsOptions.EstAchatUnique(a.Plan))
            .ToList();

        if (actifs.Count == 0 || actifs.Any(a => a.DateEcheance is null))
            return SansSuivi;

        // Plusieurs abonnements liés à la même entreprise : c'est le plus favorable qui compte.
        var reference = actifs.MaxBy(a => a.DateEcheance!.Value.Date.AddDays(JoursGracePour(a)))!;
        var echeance = reference.DateEcheance!.Value.Date;
        var dernierJour = echeance.AddDays(JoursGracePour(reference));
        var jour = aujourdHui.Date;

        var etat = jour <= echeance ? EtatAcces.AJour
            : jour <= dernierJour ? EtatAcces.EnGrace
            : EtatAcces.Suspendu;
        return new(etat, echeance, dernierJour, reference.Statut == "Essai");
    }

    public static async Task<AccesEntreprise> ChargerAsync(AppDbContext db, int companyId, DateTime aujourdHui)
    {
        var abonnements = await db.Abonnements.AsNoTracking()
            .Where(a => a.CompanyId == companyId && (a.Statut == "Essai" || a.Statut == "Active"))
            .ToListAsync();
        return Evaluer(abonnements, aujourdHui);
    }

    /// <summary>Message affiché à un utilisateur d'une entreprise suspendue qui tente une action.</summary>
    public string MessageSuspension => EstEssai
        ? $"Accès suspendu : votre essai gratuit s'est terminé le {DateEcheance:dd/MM/yyyy}. Confirmez votre abonnement pour retrouver l'accès."
        : $"Accès suspendu : votre abonnement est arrivé à échéance le {DateEcheance:dd/MM/yyyy}. Réglez-le pour retrouver l'accès.";
}

public interface IAccesEntrepriseService
{
    /// <summary>Droit d'accès de l'entreprise de l'utilisateur connecté ; <see cref="AccesEntreprise.SansSuivi"/> pour un SuperAdmin ou un visiteur.</summary>
    Task<AccesEntreprise> GetCourantAsync();
}

/// <summary>
/// Utilise son propre DbContext (fabrique), pas celui de la requête : le bandeau de MainLayout
/// appelle ce service pendant que la page charge ses données en parallèle, et deux requêtes
/// simultanées sur un même DbContext lèvent "A second operation was started on this context
/// instance" (erreur 500 sur toutes les pages, rencontrée en mettant ce bandeau en place).
/// </summary>
public class AccesEntrepriseService(IDbContextFactory<AppDbContext> dbFactory, ITenantService tenant) : IAccesEntrepriseService
{
    public async Task<AccesEntreprise> GetCourantAsync()
    {
        if (tenant.CurrentCompanyId is not int companyId)
            return AccesEntreprise.SansSuivi;

        await using var db = await dbFactory.CreateDbContextAsync();
        return await AccesEntreprise.ChargerAsync(db, companyId, DateTime.Today);
    }
}
