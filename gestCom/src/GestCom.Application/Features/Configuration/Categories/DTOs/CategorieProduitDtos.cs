using System;

namespace GestCom.Application.Features.Configuration.Categories.DTOs;

/// <summary>
/// DTO pour une catégorie de produit
/// </summary>
public class CategorieProduitDto
{
    public string CodeCategorie { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Description { get; set; }
    public int NombreProduits { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes
/// </summary>
public class CategorieProduitListDto
{
    public string CodeCategorie { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public int NombreProduits { get; set; }
}

/// <summary>
/// DTO pour la création d'une catégorie
/// </summary>
public class CreateCategorieProduitDto
{
    public string CodeCategorie { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO pour la mise à jour d'une catégorie
/// </summary>
public class UpdateCategorieProduitDto
{
    public string CodeCategorie { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Description { get; set; }
}
