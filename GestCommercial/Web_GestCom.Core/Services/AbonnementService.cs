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

    private static string DecrireTarif(Abonnement demande)
    {
        if (demande.PrixApplique is not double prix) return "sur devis";
        var unite = demande.CycleFacturation == CycleFacturation.Mensuel ? "mois" : "an";
        var promo = demande.CodePromo is null ? "" : $" (code {demande.CodePromo}, prix catalogue {demande.PrixCatalogue:0.###} DT)";
        return $"{prix:0.###} DT / {unite}{promo}";
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
                <p>Nous avons bien reçu votre demande d'abonnement au plan <strong>{demande.Plan}</strong>
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
                  <li><strong>Plan :</strong> {demande.Plan}</li>
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
