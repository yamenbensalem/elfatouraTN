using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("fabriquantproduit")]
public class FabriquantProduit
{
    [Key]
    [Column("code_fabriquantproduit")]
    public int CodeFabriquantProduit { get; set; }

    [Required, MaxLength(100)]
    [Column("nom_fabriquantproduit")]
    public string NomFabriquantProduit { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("pays_fabriquantproduit")]
    public string? PaysFabriquantProduit { get; set; }
}
