using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Commandes.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Devis.Commands.ConvertDevisToCommande;

public class ConvertDevisToCommandeCommandHandler : IRequestHandler<ConvertDevisToCommandeCommand, CommandeVenteDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public ConvertDevisToCommandeCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<CommandeVenteDto> Handle(ConvertDevisToCommandeCommand request, CancellationToken cancellationToken)
    {
        // Récupérer le devis
        var devis = await _unitOfWork.DevisClients.GetByNumeroAsync(request.NumeroDevis, _currentUserService.CodeEntreprise);
        if (devis == null)
        {
            throw new InvalidOperationException($"Devis '{request.NumeroDevis}' non trouvé.");
        }

        // Vérifier que le devis n'est pas déjà converti
        if (devis.Statut == "Converti" || !string.IsNullOrEmpty(devis.NumeroCommande))
        {
            throw new InvalidOperationException(
                $"Le devis '{request.NumeroDevis}' a déjà été converti en commande '{devis.NumeroCommande}'.");
        }

        // Vérifier que le devis n'est pas expiré
        if (devis.DateValidite < DateTime.Today)
        {
            throw new InvalidOperationException(
                $"Le devis '{request.NumeroDevis}' a expiré le {devis.DateValidite:dd/MM/yyyy}.");
        }

        var dateCommande = request.DateCommande ?? DateTime.Today;

        // Générer le numéro de commande
        var annee = dateCommande.Year;
        var commandes = await _unitOfWork.CommandesVente.GetAllAsync();
        var dernierNumero = commandes
            .Where(c => c.NumeroCommande.StartsWith($"CV{annee}"))
            .Select(c => c.NumeroCommande)
            .OrderByDescending(n => n)
            .FirstOrDefault();

        int sequence = 1;
        if (!string.IsNullOrEmpty(dernierNumero))
        {
            var parts = dernierNumero.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        var numeroCommande = $"CV{annee}-{sequence:D6}";

        // Créer la commande à partir du devis
        var commande = new CommandeVente
        {
            NumeroCommande = numeroCommande,
            DateCommande = dateCommande,
            CodeClient = devis.CodeClient,
            NumeroDevis = devis.NumeroDevis,
            TauxRemiseGlobale = devis.TauxRemiseGlobale,
            MontantHT = devis.MontantHT,
            MontantTVA = devis.MontantTVA,
            MontantRemise = devis.MontantRemise,
            MontantTTC = devis.MontantTTC,
            Observation = request.Observation ?? devis.Observation,
            Statut = "En attente",
            LignesCommande = new List<LigneCommandeVente>()
        };

        // Copier les lignes du devis
        if (devis.LignesDevis != null)
        {
            foreach (var ligneDevis in devis.LignesDevis)
            {
                var ligneCommande = new LigneCommandeVente
                {
                    NumeroCommande = numeroCommande,
                    CodeProduit = ligneDevis.CodeProduit,
                    Quantite = ligneDevis.Quantite,
                    PrixUnitaireHT = ligneDevis.PrixUnitaireHT,
                    TauxTVA = ligneDevis.TauxTVA,
                    TauxRemise = ligneDevis.TauxRemise,
                    TauxFodec = ligneDevis.TauxFodec,
                    MontantHT = ligneDevis.MontantHT,
                    MontantTVA = ligneDevis.MontantTVA,
                    MontantFodec = ligneDevis.MontantFodec,
                    MontantTTC = ligneDevis.MontantTTC
                };
                commande.LignesCommande.Add(ligneCommande);
            }
        }

        // Mettre à jour le devis
        devis.Statut = "Converti";
        devis.NumeroCommande = numeroCommande;

        await _unitOfWork.CommandesVente.AddAsync(commande);
        await _unitOfWork.DevisClients.UpdateAsync(devis);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CommandeVenteDto>(commande);
    }
}
