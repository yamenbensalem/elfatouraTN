using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("tvaproduit")]
public class TvaProduit
{
    [Key]
    [Column("code_tvaproduit")]
    public int CodeTvaProduit { get; set; }

    [Required, MaxLength(50)]
    [Column("nom_tvaproduit")]
    public string NomTvaProduit { get; set; } = string.Empty;

    [Column("taux_tvaproduit")]
    public double TauxTvaProduit { get; set; }
}
