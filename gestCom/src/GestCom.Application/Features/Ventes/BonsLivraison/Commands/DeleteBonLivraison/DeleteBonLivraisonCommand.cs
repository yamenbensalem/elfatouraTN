using MediatR;

namespace GestCom.Application.Features.Ventes.BonsLivraison.Commands.DeleteBonLivraison;

/// <summary>
/// Command pour supprimer un bon de livraison
/// </summary>
public class DeleteBonLivraisonCommand : IRequest<bool>
{
    public string NumeroBonLivraison { get; set; } = string.Empty;
    
    /// <summary>
    /// Si true, restaure le stock des produits
    /// </summary>
    public bool RestaurerStock { get; set; } = true;
}
