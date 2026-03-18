using GestCom.Application.Features.Achats.CommandesAchat.DTOs;
using MediatR;

namespace GestCom.Application.Features.Achats.CommandesAchat.Queries.GetCommandeAchatByNumero;

public class GetCommandeAchatByNumeroQuery : IRequest<CommandeAchatDto>
{
    public string NumeroCommande { get; set; }

    public GetCommandeAchatByNumeroQuery(string numeroCommande)
    {
        NumeroCommande = numeroCommande;
    }
}
