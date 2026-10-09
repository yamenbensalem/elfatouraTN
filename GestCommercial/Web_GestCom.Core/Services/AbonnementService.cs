using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;

namespace Web_GestCom.Services;

/// <summary>
/// CRUD pour les demandes d'abonnement — soumises anonymement depuis la page publique des
/// tarifs, suivies manuellement par le SuperAdmin. Aucun paiement réel n'est traité ici (voir
/// TODO.md, section Paiement) : ce service ne fait que stocker la demande et son suivi, et
/// notifier par email (demandeur + admin) — l'envoi ne doit jamais faire échouer la demande
/// elle-même (même principe que JournalActiviteService : un échec de notification est loggé,
/// pas remonté à l'appelant).
/// </summary>
public interface IAbonnementService
{
    Task<List<Abonnement>> GetAllAsync();
    Task<Abonnement> CreateDemandeAsync(Abonnement demande);

    /// <summary>
    /// Retourne l'offre si le code existe, n'est pas expiré et qu'il reste au moins une place ;
    /// null sinon (y compris pour un code vide). Sert à la fois à la validation du formulaire et à
    /// la bannière de la page tarifs.
    /// </summary>
    Task<OffrePromo?> GetOffrePromoAsync(string? code);

    /// <summary>
    /// Envoie au client un email de rappel d'échéance. Retourne false, sans rien enregistrer, si
    /// l'email n'a pas pu partir ; sinon enregistre la date de relance.
    /// </summary>
    Task<bool> RelancerAsync(int abonnementId);
    Task UpdateAsync(Abonnement abonnement);
    Task DeleteAsync(Abonnement abonnement);
}

public class AbonnementService(
    AppDbContext db,
    IEmailTransport emailTransport,
    IOptions<EmailOptions> emailOptions,
    IOptions<TarifsOptions> tarifsOptions,
    ILogger<AbonnementService> logger) : IAbonnementService
{
    public const string CodePromoInvalideMessage = "Code promotionnel invalide, expiré ou épuisé.";

    // Une place n'est consommée que par un abonnement réellement démarré — une simple demande
    // (EnAttente/Contactee) ou une demande refusée ne bloque pas le code pour les suivants.
    private static readonly string[] StatutsConsommantUnePlace = ["Essai", "Active"];

    public async Task<List<Abonnement>> GetAllAsync()
        => await db.Abonnements.AsNoTracking()
            .Include(a => a.Company)
            .OrderByDescending(a => a.DateDemande)
            .ToListAsync();

    public async Task<Abonnement> CreateDemandeAsync(Abonnement demande)
    {
        demande.DateDemande = DateTime.UtcNow;
        demande.Statut = "EnAttente";
        demande.DateDebut = null;
        demande.DateEcheance = null;
        demande.DateTraitement = null;
        demande.CompanyId = null;

        // Prix et réduction recalculés ici : jamais repris de ce que le formulaire a envoyé.
        demande.CycleFacturation = CycleFacturation.Normaliser(demande.CycleFacturation);
        demande.PrixCatalogue = tarifsOptions.Value.GetPrix(demande.Plan, demande.CycleFacturation);
        demande.PrixApplique = demande.PrixCatalogue;
        demande.CodePromo = string.IsNullOrWhiteSpace(demande.CodePromo) ? null : demande.CodePromo.Trim().ToUpperInvariant();
        if (TarifsOptions.EstAchatUnique(demande.Plan))
            demande.CodePromo = null; // les codes promo ne concernent que les abonnements
        if (demande.CodePromo is not null)
        {
            var offre = await GetOffrePromoAsync(demande.CodePromo)
                ?? throw new InvalidOperationException(CodePromoInvalideMessage);
            if (demande.PrixCatalogue is double prix)
                demande.PrixApplique = offre.Appliquer(prix);
        }

        db.Abonnements.Add(demande);
        await db.SaveChangesGuardedAsync();

        await NotifyNewDemandeAsync(demande);
        return demande;
    }

    public async Task<OffrePromo?> GetOffrePromoAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var normalise = code.Trim().ToUpperInvariant();

        var promo = await db.CodesPromo.AsNoTracking().FirstOrDefaultAsync(p => p.Code == normalise);
        if (promo is null) return null;
        if (promo.DateExpiration is DateTime expiration && expiration < DateTime.UtcNow) return null;

        var utilisations = await db.Abonnements.CountAsync(a =>
            a.CodePromo == normalise && StatutsConsommantUnePlace.Contains(a.Statut));
        var placesRestantes = promo.MaxUtilisations - utilisations;
        if (placesRestantes <= 0) return null;

        return new OffrePromo(promo.Code, promo.Libelle, promo.PourcentageReduction, promo.MaxUtilisations, placesRestantes);
    }

    public async Task<bool> RelancerAsync(int abonnementId)
    {
        var abonnement = await db.Abonnements.FirstOrDefaultAsync(a => a.Id == abonnementId)
            ?? throw new InvalidOperationException("Demande introuvable.");
        if (abonnement.DateEcheance is not DateTime echeance || !AbonnementActivationService.EstActif(abonnement.Statut))
            throw new InvalidOperationException("Ce client n'a pas d'échéance à rappeler : activez la demande et renseignez ses dates.");

        static string html(string? valeur) => System.Net.WebUtility.HtmlEncode(valeur) ?? "";
        var expire = echeance.Date < DateTime.Today;
        var desktop = TarifsOptions.EstAchatUnique(abonnement.Plan);
        var nomPlan = TarifsOptions.NomPlan(abonnement.Plan);
        var contact = emailOptions.Value.AdminNotificationEmail;

        // Abonnement : dire au client quand son accès sera coupé (ou qu'il l'est déjà).
        var dernierJour = echeance.Date.AddDays(AccesEntreprise.JoursGracePour(abonnement));
        var suspension = desktop ? ""
            : dernierJour < DateTime.Today
                ? "<p><strong>L'accès à votre espace est suspendu.</strong> Il est rétabli dès réception du règlement, vos données sont conservées.</p>"
                : $"<p>Sans règlement, l'accès à votre espace sera suspendu après le <strong>{dernierJour:dd/MM/yyyy}</strong>. Vos données sont conservées.</p>";

        string sujet, corps;
        if (desktop)
        {
            var maintenance = tarifsOptions.Value.GetFormuleDesktop(abonnement.Plan)?.MaintenanceAnnuelle;
            sujet = "GestCom : fin de votre période de maintenance";
            corps = $"""
                <p>La période de garantie et de maintenance de votre licence <strong>GestCom {html(nomPlan)}</strong>
                {(expire ? "s'est terminée" : "se termine")} le <strong>{echeance:dd/MM/yyyy}</strong>.</p>
                <p>Votre licence continue de fonctionner. Sans maintenance, les mises à jour et les interventions
                sont facturées sur devis.</p>
                {(maintenance is double prix ? $"<p>Vous pouvez la prolonger d'un an pour <strong>{prix:0.###} DT HT</strong>.</p>" : "")}
                """;
        }
        else if (abonnement.Statut == "Essai")
        {
            sujet = "GestCom : votre essai gratuit arrive à son terme";
            corps = $"""
                <p>Votre essai gratuit de GestCom {(expire ? "s'est terminé" : "se termine")} le <strong>{echeance:dd/MM/yyyy}</strong>.</p>
                <p>Pour continuer à utiliser votre espace <strong>{html(abonnement.NomEntreprise)}</strong> sans interruption,
                il suffit de confirmer votre abonnement {html(nomPlan)} ({DecrireTarif(abonnement)}).</p>
                {suspension}
                """;
        }
        else
        {
            sujet = "GestCom : votre abonnement arrive à échéance";
            corps = $"""
                <p>Votre abonnement <strong>GestCom {html(nomPlan)}</strong> pour <strong>{html(abonnement.NomEntreprise)}</strong>
                {(expire ? "est arrivé" : "arrive")} à échéance le <strong>{echeance:dd/MM/yyyy}</strong>.</p>
                <p>Montant du renouvellement : <strong>{DecrireTarif(abonnement)}</strong>, TVA en sus.</p>
                {suspension}
                """;
        }

        var envoye = await emailTransport.SendAsync(
            abonnement.EmailContact, abonnement.NomContact, sujet,
            $"""
            <p>Bonjour {html(abonnement.NomContact)},</p>
            {corps}
            <p>Pour le règlement par virement, écrivez-nous à <a href="mailto:{contact}">{contact}</a> :
            nous vous envoyons le RIB et la facture pro forma.</p>
            <p>— L'équipe GestCom</p>
            """);
        if (!envoye) return false;

        abonnement.DateDerniereRelance = DateTime.UtcNow;
        await db.SaveChangesGuardedAsync();
        return true;
    }

    private static string DecrireTarif(Abonnement demande)
    {
        if (demande.PrixApplique is not double prix) return "sur devis";
        if (TarifsOptions.EstAchatUnique(demande.Plan)) return $"{prix:0.###} DT HT, paiement unique";
        var unite = demande.CycleFacturation == CycleFacturation.Mensuel ? "mois" : "an";
        var promo = demande.CodePromo is null ? "" : $" (code {demande.CodePromo}, prix catalogue {demande.PrixCatalogue:0.###} DT)";
        return $"{prix:0.###} DT HT / {unite}{promo}";
    }

    private async Task NotifyNewDemandeAsync(Abonnement demande)
    {
        try
        {
            await emailTransport.SendAsync(
                demande.EmailContact, demande.NomContact,
                "Votre demande d'abonnement GestCom a bien été reçue",
                $"""
                <p>Bonjour {demande.NomContact},</p>
                <p>Nous avons bien reçu votre demande d'abonnement au plan <strong>{TarifsOptions.NomPlan(demande.Plan)}</strong>
                pour <strong>{demande.NomEntreprise}</strong>.</p>
                <p>Tarif retenu : <strong>{DecrireTarif(demande)}</strong>.</p>
                <p>Notre équipe vous recontactera sous 24 à 48h pour finaliser l'activation.</p>
                <p>— L'équipe GestCom</p>
                """);

            await emailTransport.SendAsync(
                emailOptions.Value.AdminNotificationEmail, "Admin",
                $"Nouvelle demande d'abonnement — {demande.NomEntreprise}",
                $"""
                <p>Nouvelle demande d'abonnement reçue :</p>
                <ul>
                  <li><strong>Entreprise :</strong> {demande.NomEntreprise}</li>
                  <li><strong>Contact :</strong> {demande.NomContact} ({demande.EmailContact})</li>
                  <li><strong>Plan :</strong> {TarifsOptions.NomPlan(demande.Plan)}</li>
                  <li><strong>Tarif :</strong> {DecrireTarif(demande)}</li>
                  <li><strong>Téléphone :</strong> {demande.TelephoneContact}</li>
                </ul>
                <p>À traiter depuis /admin/demandes-abonnement.</p>
                """);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Échec de notification email pour la demande d'abonnement {Id}", demande.Id);
        }
    }

    public async Task UpdateAsync(Abonnement abonnement)
    {
        db.DetachStaleTrackedEntry(abonnement);
        db.Abonnements.Update(abonnement);
        await db.SaveChangesGuardedAsync();
    }

    public async Task DeleteAsync(Abonnement abonnement)
    {
        db.DetachStaleTrackedEntry(abonnement);
        db.Abonnements.Remove(abonnement);
        await db.SaveChangesGuardedAsync();
    }
}
