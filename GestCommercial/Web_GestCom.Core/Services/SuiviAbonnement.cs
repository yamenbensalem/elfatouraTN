using Web_GestCom.Data.Models;

namespace Web_GestCom.Services;

/// <summary>Où en est un client par rapport à l'échéance de son abonnement ou de sa maintenance.</summary>
public enum EtatSuivi
{
    /// <summary>Demande pas encore activée, refusée, ou licence sans échéance : rien à suivre.</summary>
    SansObjet,

    /// <summary>Client actif dont les dates n'ont pas été renseignées : impossible de savoir quand le relancer.</summary>
    DatesManquantes,

    EnCours,

    /// <summary>L'échéance approche (voir <see cref="SuiviAbonnement.SeuilRelanceJours"/>).</summary>
    ARelancer,

    /// <summary>Échéance dépassée, accès encore ouvert (délai de grâce) — ou fin de maintenance d'une licence Desktop.</summary>
    Expire,

    /// <summary>Délai de grâce écoulé : l'accès du client est bloqué (voir <see cref="AccesEntreprise"/>).</summary>
    Suspendu
}

/// <summary>
/// Suivi des échéances pour le SuperAdmin : quand un client a commencé, quand ça se termine, et
/// s'il faut le relancer. Aucun email ne part tout seul : ces règles font remonter les clients à
/// traiter dans l'écran des abonnements et sur le tableau de bord, la relance étant déclenchée à la
/// main. La coupure d'accès après le délai de grâce, elle, est automatique (voir AccesEntreprise) ;
/// l'état Suspendu ci-dessous en est le reflet côté suivi.
/// </summary>
public sealed record SuiviAbonnement(EtatSuivi Etat, int? JoursRestants)
{
    /// <summary>
    /// Nombre de jours avant l'échéance à partir duquel le client est "à relancer" : un mois pour
    /// un engagement d'un an (le temps d'un virement et d'une facture pro forma), une semaine pour
    /// une période courte (essai gratuit, abonnement mensuel).
    /// </summary>
    public static int SeuilRelanceJours(Abonnement a)
        => a.Statut == "Essai" || (!TarifsOptions.EstAchatUnique(a.Plan) && a.CycleFacturation == CycleFacturation.Mensuel)
            ? 7
            : 30;

    public static SuiviAbonnement Evaluer(Abonnement a, DateTime aujourdHui)
    {
        if (!AbonnementActivationService.EstActif(a.Statut))
            return new(EtatSuivi.SansObjet, null);

        if (a.DateEcheance is not DateTime echeance)
        {
            // Une licence Desktop sans maintenance incluse n'a pas d'échéance : elle ne se termine pas.
            var sansEcheance = TarifsOptions.EstAchatUnique(a.Plan) && a.DateDebut.HasValue;
            return new(sansEcheance ? EtatSuivi.SansObjet : EtatSuivi.DatesManquantes, null);
        }

        var jours = (echeance.Date - aujourdHui.Date).Days;
        // Une licence Desktop n'est jamais suspendue : seule sa maintenance prend fin.
        var suspendu = !TarifsOptions.EstAchatUnique(a.Plan) && -jours > AccesEntreprise.JoursGracePour(a);
        var etat = suspendu ? EtatSuivi.Suspendu
            : jours < 0 ? EtatSuivi.Expire
            : jours <= SeuilRelanceJours(a) ? EtatSuivi.ARelancer
            : EtatSuivi.EnCours;
        return new(etat, jours);
    }

    /// <summary>
    /// Dates proposées quand le SuperAdmin active une demande : début aujourd'hui, échéance selon
    /// l'offre — durée de l'essai, un mois ou un an d'abonnement, fin de la maintenance incluse (ou,
    /// à défaut, de la garantie) pour une licence Desktop.
    /// </summary>
    public static (DateTime Debut, DateTime Echeance) DatesProposees(Abonnement a, TarifsOptions tarifs, DateTime aujourdHui)
    {
        var debut = aujourdHui.Date;

        if (tarifs.GetFormuleDesktop(a.Plan) is { } formule)
            return (debut, debut.AddMonths(formule.MaintenanceIncluseMois > 0 ? formule.MaintenanceIncluseMois : formule.GarantieMois));
        if (a.Statut == "Essai")
            return (debut, debut.AddDays(tarifs.JoursEssai));
        if (a.CycleFacturation == CycleFacturation.Mensuel)
            return (debut, debut.AddMonths(1));
        return (debut, debut.AddYears(1));
    }
}
