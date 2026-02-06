namespace GestCom.Application.Features.Ventes.Clients.DTOs;

/// <summary>
/// DTO pour les données du client
/// </summary>
public class ClientDto
{
    public string CodeClient { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string MatriculeFiscale { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string TypePersonne { get; set; } = string.Empty;
    public string? TypeEntreprise { get; set; }
    public string? RIB { get; set; }
    
    public string Adresse { get; set; } = string.Empty;
    public string? CodePostal { get; set; }
    public string Ville { get; set; } = string.Empty;
    public string Pays { get; set; } = string.Empty;
    
    public string? Tel { get; set; }
    public string? TelMobile { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    
    public string Etat { get; set; } = string.Empty;
    public int NombreTransactions { get; set; }
    public string? Note { get; set; }
    public bool Etranger { get; set; }
    public bool Exonore { get; set; }
    public decimal MaxCredit { get; set; }
    public int CodeDevise { get; set; }
    public string? Responsable { get; set; }
    
    // Relations
    public string? DeviseNom { get; set; }
    public decimal TotalCreances { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes
/// </summary>
public class ClientListDto
{
    public string CodeClient { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string MatriculeFiscale { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string? Tel { get; set; }
    public string? Email { get; set; }
    public string Etat { get; set; } = string.Empty;
    public decimal TotalCreances { get; set; }
}

/// <summary>
/// DTO pour création de client
/// </summary>
public class CreateClientDto
{
    public string CodeClient { get; set; } = string.Empty;
    public string MatriculeFiscale { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string TypePersonne { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string? Tel { get; set; }
    public string? Email { get; set; }
    public int CodeDevise { get; set; }
}

/// <summary>
/// DTO pour mise à jour de client
/// </summary>
public class UpdateClientDto : CreateClientDto
{
    public string? CodePostal { get; set; }
    public string? TelMobile { get; set; }
    public string? Fax { get; set; }
    public string? SiteWeb { get; set; }
    public string Etat { get; set; } = "Actif";
    public decimal MaxCredit { get; set; }
    public string? Responsable { get; set; }
}
