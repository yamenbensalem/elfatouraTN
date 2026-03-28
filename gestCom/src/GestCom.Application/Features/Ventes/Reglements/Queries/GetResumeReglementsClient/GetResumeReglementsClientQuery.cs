using GestCom.Application.Features.Ventes.Reglements.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Reglements.Queries.GetResumeReglementsClient;

public class GetResumeReglementsClientQuery : IRequest<ResumeReglementsClientDto?>
{
    public string CodeClient { get; set; } = string.Empty;
}