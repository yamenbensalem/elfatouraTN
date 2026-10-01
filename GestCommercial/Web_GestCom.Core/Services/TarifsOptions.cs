namespace Web_GestCom.Services;

/// <summary>
/// Tarifs publics des abonnements (section "Tarifs" de la configuration) — source unique pour la
/// page d'accueil, le formulaire /demande-abonnement, l'écran SuperAdmin et le prix enregistré sur
/// chaque demande. Aucun paiement en ligne : ces montants servent à l'affichage et au suivi manuel.
/// </summary>
public sealed class TarifsOptions
{
    /// <summary>Durée de l'essai gratuit du plan Standard, en jours.</summary>
    public int JoursEssai { get; set; } = 30;

    public PlanTarif Standard { get; set; } = new() { Annuel = 390, Mensuel = 45 };
    public PlanTarif Pro { get; set; } = new() { Annuel = 690, Mensuel = 79 };

    /// <summary>Code promo affiché dans la bannière de la page tarifs ; vide = pas de bannière.</summary>
    public string CodePromoMisEnAvant { get; set; } = "FONDATEUR2026";

    /// <summary>Prix catalogue en DT pour le cycle donné ; null pour un plan sur devis (Enterprise) ou inconnu.</summary>
    public double? GetPrix(string plan, string cycle)
    {
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
