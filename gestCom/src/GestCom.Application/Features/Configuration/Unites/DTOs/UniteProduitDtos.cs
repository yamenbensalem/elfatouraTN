using System;

namespace GestCom.Application.Features.Configuration.Unites.DTOs;

/// <summary>
/// DTO pour une unité de produit
/// </summary>
public class UniteProduitDto
{
    public string CodeUnite { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Abreviation { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour la création d'une unité
/// </summary>
public class CreateUniteProduitDto
{
    public string CodeUnite { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Abreviation { get; set; }
}

/// <summary>
/// DTO pour la mise à jour d'une unité
/// </summary>
public class UpdateUniteProduitDto
{
    public string CodeUnite { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Abreviation { get; set; }
}
