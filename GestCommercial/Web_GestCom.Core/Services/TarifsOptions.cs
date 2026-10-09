namespace Web_GestCom.Services;

/// <summary>
/// Tarifs publics (section "Tarifs" de la configuration), tous hors taxes — source unique pour la
/// page d'accueil, le formulaire /demande-abonnement, l'écran SuperAdmin et le prix enregistré sur
/// chaque demande. Aucun paiement en ligne : ces montants servent à l'affichage et au suivi manuel.
/// </summary>
public sealed class TarifsOptions
{
    /// <summary>Durée de l'essai gratuit du plan Standard, en jours.</summary>
    public int JoursEssai { get; set; } = 30;

    /// <summary>Heures d'initiation à l'application incluses dans chaque offre ; au-delà, facturé.</summary>
    public int InitiationHeures { get; set; } = 2;

    public PlanTarif Standard { get; set; } = new() { Annuel = 350, Mensuel = 39 };
    public PlanTarif Pro { get; set; } = new() { Annuel = 590, Mensuel = 69 };

    /// <summary>Application Desktop (Windows) : licences achetées une fois, pas des abonnements.</summary>
    public DesktopTarifs Desktop { get; set; } = new();

    /// <summary>
    /// Code promo affiché dans une bannière de la page tarifs ; vide = pas de bannière. L'offre
    /// "Clients Fondateurs" a été retirée (2026-10) : plus de bannière ni de champ code promo sur le
    /// formulaire. Le mécanisme (table code_promo, AbonnementService.GetOffrePromoAsync) reste en
    /// place pour une éventuelle campagne future.
    /// </summary>
    public string CodePromoMisEnAvant { get; set; } = string.Empty;

    public const string PlanDesktopEssentiel = "DesktopEssentiel";
    public const string PlanDesktopSerenite = "DesktopSerenite";
    public const string PlanDesktopEquipe = "DesktopEquipe";

    /// <summary>
    /// Vrai pour une offre payée une seule fois (les formules Desktop) : ni cycle annuel/mensuel,
    /// ni code promo, ni compte web à créer à l'activation.
    /// </summary>
    public static bool EstAchatUnique(string plan) => plan.StartsWith("Desktop", StringComparison.Ordinal);

    /// <summary>Nom affiché d'un plan ("DesktopSerenite" → "Desktop Sérénité") ; les abonnements gardent leur nom.</summary>
    public static string NomPlan(string plan) => plan switch
    {
        PlanDesktopEssentiel => "Desktop Essentiel",
        PlanDesktopSerenite => "Desktop Sérénité",
        PlanDesktopEquipe => "Desktop Équipe",
        _ => plan
    };

    public FormuleDesktop? GetFormuleDesktop(string plan) => plan switch
    {
        PlanDesktopEssentiel => Desktop.Essentiel,
        PlanDesktopSerenite => Desktop.Serenite,
        PlanDesktopEquipe => Desktop.Equipe,
        _ => null
    };

    /// <summary>Prix catalogue HT en DT pour le cycle donné ; null pour un plan sur devis (Enterprise) ou inconnu.</summary>
    public double? GetPrix(string plan, string cycle)
    {
        // Achat unique : même prix quel que soit le cycle (qui ne s'applique pas).
        if (GetFormuleDesktop(plan) is { } formule) return formule.Prix;

        var tarif = plan switch
        {
            "Standard" => Standard,
            "Pro" => Pro,
            _ => null
        };
        if (tarif is null) return null;
        return cycle == CycleFacturation.Mensuel ? tarif.Mensuel : tarif.Annuel;
    }
}

public sealed class DesktopTarifs
{
    /// <summary>Le logiciel seul : garantie courte, aucune mise à jour, toute demande facturée.</summary>
    public FormuleDesktop Essentiel { get; set; } = new() { Prix = 1000, Postes = 1, GarantieMois = 2 };

    /// <summary>Le logiciel avec un an de mises à jour et d'assistance — la formule mise en avant.</summary>
    public FormuleDesktop Serenite { get; set; } = new() { Prix = 1400, Postes = 1, GarantieMois = 12, MaintenanceIncluseMois = 12, MaintenanceAnnuelle = 350 };

    /// <summary>Comme Sérénité, pour plusieurs postes.</summary>
    public FormuleDesktop Equipe { get; set; } = new() { Prix = 2400, Postes = 3, GarantieMois = 12, MaintenanceIncluseMois = 12, MaintenanceAnnuelle = 600 };

    /// <summary>Prix HT d'un poste ajouté à la formule Équipe.</summary>
    public double PosteSupplementaire { get; set; } = 500;
}

public sealed class FormuleDesktop
{
    /// <summary>Prix HT de la licence en DT, payé une seule fois.</summary>
    public double Prix { get; set; }

    public int Postes { get; set; } = 1;

    /// <summary>Durée, en mois à compter de l'installation, pendant laquelle les anomalies sont corrigées gratuitement.</summary>
    public int GarantieMois { get; set; }

    /// <summary>Mois de mises à jour et d'assistance inclus ; 0 = aucun (tout est alors facturé sur devis).</summary>
    public int MaintenanceIncluseMois { get; set; }

    /// <summary>Prix HT par an du renouvellement facultatif de la maintenance ; null si la formule n'en propose pas.</summary>
    public double? MaintenanceAnnuelle { get; set; }
}

public sealed class PlanTarif
{
    /// <summary>Prix en DT pour un an, payé en une fois.</summary>
    public double Annuel { get; set; }

    /// <summary>Prix en DT par mois, sans engagement.</summary>
    public double Mensuel { get; set; }
}

public static class CycleFacturation
{
    public const string Annuel = "Annuel";
    public const string Mensuel = "Mensuel";

    /// <summary>Toute valeur autre que "Mensuel" (absente, inconnue, casse différente) retombe sur Annuel.</summary>
    public static string Normaliser(string? cycle)
        => string.Equals(cycle, Mensuel, StringComparison.OrdinalIgnoreCase) ? Mensuel : Annuel;
}

/// <summary>Code promo utilisable par un prospect : valide, non expiré, avec au moins une place restante.</summary>
public sealed record OffrePromo(string Code, string Libelle, double PourcentageReduction, int MaxUtilisations, int PlacesRestantes)
{
    public double Appliquer(double prix) => Math.Round(prix * (1 - PourcentageReduction / 100), 3);
}
