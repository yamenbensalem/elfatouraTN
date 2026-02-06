using System;

namespace GestCom.Application.Features.Configuration.Magasins.DTOs;

/// <summary>
/// DTO pour un magasin
/// </summary>
public class MagasinProduitDto
{
    public string CodeMagasin { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Adresse { get; set; }
    public string? Responsable { get; set; }
    public string? Telephone { get; set; }
    public int NombreProduits { get; set; }
    public decimal ValeurStock { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes
/// </summary>
public class MagasinProduitListDto
{
    public string CodeMagasin { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Responsable { get; set; }
    public int NombreProduits { get; set; }
}

/// <summary>
/// DTO pour la création d'un magasin
/// </summary>
public class CreateMagasinProduitDto
{
    public string CodeMagasin { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Adresse { get; set; }
    public string? Responsable { get; set; }
    public string? Telephone { get; set; }
}

/// <summary>
/// DTO pour la mise à jour d'un magasin
/// </summary>
public class UpdateMagasinProduitDto
{
    public string CodeMagasin { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Adresse { get; set; }
    public string? Responsable { get; set; }
    public string? Telephone { get; set; }
}
