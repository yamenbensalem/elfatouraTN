using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.Magasins.Commands.CreateMagasin;

/// <summary>
/// Commande pour créer un nouveau magasin
/// </summary>
public class CreateMagasinCommand : IRequest<MagasinProduitDto>
{
    public string CodeMagasin { get; set; } = string.Empty;
    public string LibelleMagasin { get; set; } = string.Empty;
    public string? Adresse { get; set; }
    public string? Responsable { get; set; }
    public bool EstDefaut { get; set; }
}
