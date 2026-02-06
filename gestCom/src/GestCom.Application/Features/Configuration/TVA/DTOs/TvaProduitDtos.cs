using System;

namespace GestCom.Application.Features.Configuration.TVA.DTOs;

/// <summary>
/// DTO pour un taux de TVA
/// </summary>
public class TvaProduitDto
{
    public string CodeTVA { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public decimal Taux { get; set; }
    public bool EstParDefaut { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour la création d'un taux de TVA
/// </summary>
public class CreateTvaProduitDto
{
    public string CodeTVA { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public decimal Taux { get; set; }
    public bool EstParDefaut { get; set; }
}

/// <summary>
/// DTO pour la mise à jour d'un taux de TVA
/// </summary>
public class UpdateTvaProduitDto
{
    public string CodeTVA { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public decimal Taux { get; set; }
    public bool EstParDefaut { get; set; }
}
