using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité ParametresDecimales - Configuration des décimales
/// </summary>
public class ParametresDecimales : BaseEntity
{
    public int Id { get; set; }
    public int NombreDecimalesQuantite { get; set; } = 2;
    public int NombreDecimalesPrix { get; set; } = 3;
    public int NombreDecimalesMontant { get; set; } = 3;
}
