using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("devise")]
public class Devise
{
    [Key]
    [Column("code_devise")]
    public int CodeDevise { get; set; }

    [Required, MaxLength(50)]
    [Column("nom_devise")]
    public string NomDevise { get; set; } = string.Empty;

    [MaxLength(10)]
    [Column("symbole_devise")]
    public string? SymboleDevise { get; set; }

    [Column("taux_devise")]
    public double TauxDevise { get; set; } = 1.0;
}
