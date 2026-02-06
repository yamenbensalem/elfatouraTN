using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Commands.DeleteProduit;

public class DeleteProduitCommand : IRequest<bool>
{
    public string CodeProduit { get; set; } = string.Empty;
}
