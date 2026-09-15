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
    Task UpdateAsync(Abonnement abonnement);
    Task DeleteAsync(Abonnement abonnement);
}

public class AbonnementService(
    AppDbContext db,
    IEmailTransport emailTransport,
    IOptions<EmailOptions> emailOptions,
    ILogger<AbonnementService> logger) : IAbonnementService
{
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

        db.Abonnements.Add(demande);
        await db.SaveChangesGuardedAsync();

        await NotifyNewDemandeAsync(demande);
        return demande;
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
