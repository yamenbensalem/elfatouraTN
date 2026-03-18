using GestCom.Application.Features.Ventes.Commandes.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Commandes.Queries.GetCommandeVenteByNumero;

public class GetCommandeVenteByNumeroQuery : IRequest<CommandeVenteDto>
{
    public string NumeroCommande { get; set; }

    public GetCommandeVenteByNumeroQuery(string numeroCommande)
    {
        NumeroCommande = numeroCommande;
    }
}
