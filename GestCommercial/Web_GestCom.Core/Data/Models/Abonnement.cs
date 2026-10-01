using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

/// <summary>
/// Demande d'abonnement soumise depuis la page publique des tarifs, puis suivie manuellement
/// par le SuperAdmin (pas de passerelle de paiement réelle ni de compte marchand à ce stade —
/// voir TODO.md, section Paiement). CompanyId est un lien optionnel, pas une appartenance
/// tenant : le SuperAdmin doit voir toutes les demandes de toutes les entreprises (et même
/// avant qu'une entreprise n'existe), donc cette entité n'est volontairement PAS ITenantOwned —
/// même convention que ApiKey/Webhook.
/// </summary>
[Table("abonnement")]
public class Abonnement
{
    [Key]
    [Column("id_abonnement")]
    public int Id { get; set; }

    /// <summary>Nom de l'entreprise saisi dans la demande (peut ne pas encore exister en base).</summary>
    [Required, MaxLength(200)]
    [Column("nom_entreprise_abonnement")]
    public string NomEntreprise { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("nom_contact_abonnement")]
    public string NomContact { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("email_contact_abonnement")]
    public string EmailContact { get; set; } = string.Empty;

    [MaxLength(30)]
    [Column("telephone_contact_abonnement")]
    public string? TelephoneContact { get; set; }

    /// <summary>Plan demandé : Standard, Pro, Enterprise.</summary>
    [Required, MaxLength(50)]
    [Column("plan_abonnement")]
    public string Plan { get; set; } = "Standard";

    /// <summary>Annuel ou Mensuel (voir CycleFacturation) — sans objet pour Enterprise (sur devis).</summary>
    [Required, MaxLength(10)]
    [Column("cycle_facturation_abonnement")]
    public string CycleFacturation { get; set; } = "Annuel";

    /// <summary>Code promo validé à la soumission (majuscules) ; null si aucun.</summary>
    [MaxLength(50)]
    [Column("code_promo_abonnement")]
    public string? CodePromo { get; set; }

    /// <summary>Prix catalogue en DT pour le cycle choisi, figé à la demande ; null pour un plan sur devis.</summary>
    [Column("prix_catalogue_abonnement")]
    public double? PrixCatalogue { get; set; }

    /// <summary>Prix après réduction du code promo, figé à la demande ; null pour un plan sur devis.</summary>
    [Column("prix_applique_abonnement")]
    public double? PrixApplique { get; set; }

    /// <summary>Moyen de paiement souhaité par le demandeur — indicatif, aucun paiement réel n'est traité ici.</summary>
    [MaxLength(50)]
    [Column("mode_paiement_abonnement")]
    public string? ModePaiementSouhaite { get; set; }

    [MaxLength(1000)]
    [Column("message_abonnement")]
    public string? Message { get; set; }

    /// <summary>EnAttente, Contactee, Essai, Active, Refusee.</summary>
    [Required, MaxLength(30)]
    [Column("statut_abonnement")]
    public string Statut { get; set; } = "EnAttente";

    [Column("date_demande_abonnement")]
    public DateTime DateDemande { get; set; } = DateTime.UtcNow;

    [Column("date_debut_abonnement")]
    public DateTime? DateDebut { get; set; }

    [Column("date_echeance_abonnement")]
    public DateTime? DateEcheance { get; set; }

    [Column("date_traitement_abonnement")]
    public DateTime? DateTraitement { get; set; }

    [MaxLength(1000)]
    [Column("notes_admin_abonnement")]
    public string? NotesAdmin { get; set; }

    /// <summary>Entreprise liée une fois la demande traitée/activée ; null tant qu'elle n'existe pas encore.</summary>
    [Column("company_id_abonnement")]
    public int? CompanyId { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }
}
