using System;

namespace GestCom.Application.Features.Configuration.ModesPaiement.DTOs;

/// <summary>
/// DTO pour un mode de paiement
/// </summary>
public class ModePaiementDto
{
    public string CodeModePaiement { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public bool EstParDefaut { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour la création d'un mode de paiement
/// </summary>
public class CreateModePaiementDto
{
    public string CodeModePaiement { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public bool EstParDefaut { get; set; }
}

/// <summary>
/// DTO pour la mise à jour d'un mode de paiement
/// </summary>
public class UpdateModePaiementDto
{
    public string CodeModePaiement { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public bool EstParDefaut { get; set; }
}
