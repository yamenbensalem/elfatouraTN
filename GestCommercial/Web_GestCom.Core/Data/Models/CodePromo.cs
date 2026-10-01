using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

/// <summary>
/// Code promotionnel saisi sur le formulaire public de demande d'abonnement. Comme Abonnement,
/// volontairement PAS ITenantOwned : c'est une donnée de plateforme, lue par des visiteurs anonymes.
/// Le nombre d'utilisations n'est pas stocké ici — il est compté sur les abonnements réellement
/// activés (statut Essai/Active), pour qu'une simple demande ne consomme pas une place.
/// </summary>
[Table("code_promo")]
public class CodePromo
{
    [Key]
    [Column("id_codepromo")]
    public int Id { get; set; }

    /// <summary>Toujours stocké en majuscules.</summary>
    [Required, MaxLength(50)]
    [Column("code_codepromo")]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Column("libelle_codepromo")]
    public string Libelle { get; set; } = string.Empty;

    /// <summary>Réduction en pourcentage du prix catalogue (ex. 35 = -35 %).</summary>
    [Column("pourcentage_codepromo")]
    public double PourcentageReduction { get; set; }

    [Column("max_utilisations_codepromo")]
    public int MaxUtilisations { get; set; }

    [Column("date_expiration_codepromo")]
    public DateTime? DateExpiration { get; set; }

    /// <summary>Indicatif pour le suivi manuel : la réduction s'applique aussi aux renouvellements.</summary>
    [Column("permanent_codepromo")]
    public bool EstPermanent { get; set; }
}
