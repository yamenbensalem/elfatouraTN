using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("categorieproduit")]
public class CategorieProduit
{
    [Key]
    [Column("code_categorieproduit")]
    public int CodeCategorieProduit { get; set; }

    [Required, MaxLength(100)]
    [Column("nom_categorieproduit")]
    public string NomCategorieProduit { get; set; } = string.Empty;
}
