using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

[Table("modepayement")]
public class ModePayement
{
    [Key]
    [Column("code_modepayement")]
    public int CodeModePayement { get; set; }

    [Required, MaxLength(50)]
    [Column("nom_modepayement")]
    public string NomModePayement { get; set; } = string.Empty;
}
