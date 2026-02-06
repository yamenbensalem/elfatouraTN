using AutoMapper;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Configuration.Magasins.Queries.GetAllMagasins;

public class GetAllMagasinsQueryHandler : IRequestHandler<GetAllMagasinsQuery, IEnumerable<MagasinProduitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllMagasinsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MagasinProduitDto>> Handle(GetAllMagasinsQuery request, CancellationToken cancellationToken)
    {
        var magasins = await _unitOfWork.MagasinsProduit.GetAllAsync();
        var magasinsList = magasins.ToList();

        // Récupérer les produits pour compter par magasin
        var produits = await _unitOfWork.Produits.GetAllAsync();
        var produitsParMagasin = produits
            .Where(p => !string.IsNullOrEmpty(p.CodeMagasin))
            .GroupBy(p => p.CodeMagasin!)
            .ToDictionary(g => g.Key, g => new { Count = g.Count(), Valeur = g.Sum(p => p.Quantite * p.PrixAchatTTC) });

        var result = _mapper.Map<List<MagasinProduitDto>>(magasinsList);

        foreach (var mag in result)
        {
            if (produitsParMagasin.TryGetValue(mag.CodeMagasin, out var stats))
            {
                mag.NombreProduits = stats.Count;
                mag.ValeurStock = stats.Valeur;
            }
        }

        return result.OrderBy(m => m.LibelleMagasin);
    }
}
