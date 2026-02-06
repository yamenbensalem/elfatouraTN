using MediatR;

namespace GestCom.Application.Features.Ventes.Devis.Commands.DeleteDevisClient;

/// <summary>
/// Command pour supprimer un devis client
/// </summary>
public class DeleteDevisClientCommand : IRequest<bool>
{
    public string NumeroDevis { get; set; } = string.Empty;
}
