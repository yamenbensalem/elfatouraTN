using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Commands.DeleteFactureClient;

/// <summary>
/// Command pour supprimer une facture client
/// Note: Les factures ne peuvent être supprimées que si elles n'ont pas de règlements
/// </summary>
public class DeleteFactureClientCommand : IRequest<bool>
{
    public string NumeroFacture { get; set; } = string.Empty;
    
    /// <summary>
    /// Si true, restaure également le stock des produits
    /// </summary>
    public bool RestaurerStock { get; set; } = true;
}
