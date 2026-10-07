using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;

namespace Web_GestCom.Services;

public interface IEntrepriseService
{
    /// <summary>
    /// Vrai si la fiche de l'entreprise courante n'existe pas encore, ou s'il lui manque le nom ou
    /// le matricule fiscal — les deux mentions que tout document imprimé doit porter. Une nouvelle
    /// entreprise démarre dans cet état : sa fiche est pré-créée à l'activation, sans matricule.
    /// </summary>
    Task<bool> IsFicheIncompleteAsync();
}

public class EntrepriseService(AppDbContext db) : IEntrepriseService
{
    public async Task<bool> IsFicheIncompleteAsync()
    {
        var fiche = await db.Entreprises.AsNoTracking()
            .Select(e => new { e.NomEntreprise, e.MatriculeFiscale })
            .FirstOrDefaultAsync();

        return fiche is null
            || string.IsNullOrWhiteSpace(fiche.NomEntreprise)
            || string.IsNullOrWhiteSpace(fiche.MatriculeFiscale);
    }
}
