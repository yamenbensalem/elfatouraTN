using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("uniteproduit")]
public class UniteProduit
{
    [Key]
    [Column("code_uniteproduit")]
    public int CodeUniteProduit { get; set; }

    [Required, MaxLength(50)]
    [Column("nom_uniteproduit")]
    public string NomUniteProduit { get; set; } = string.Empty;
}
