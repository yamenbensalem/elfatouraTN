using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;

namespace Web_GestCom.Services;

/// <summary>
/// CRUD pour les demandes d'abonnement — soumises anonymement depuis la page publique des
/// tarifs, suivies manuellement par le SuperAdmin. Aucun paiement réel n'est traité ici (voir
/// TODO.md, section Paiement) : ce service ne fait que stocker la demande et son suivi.
/// </summary>
public interface IAbonnementService
{
    Task<List<Abonnement>> GetAllAsync();
    Task<Abonnement> CreateDemandeAsync(Abonnement demande);
    Task UpdateAsync(Abonnement abonnement);
    Task DeleteAsync(Abonnement abonnement);
}

public class AbonnementService(AppDbContext db) : IAbonnementService
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
        return demande;
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
