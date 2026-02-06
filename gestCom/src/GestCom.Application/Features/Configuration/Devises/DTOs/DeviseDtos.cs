using System;

namespace GestCom.Application.Features.Configuration.Devises.DTOs;

/// <summary>
/// DTO pour une devise
/// </summary>
public class DeviseDto
{
    public string CodeDevise { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Symbole { get; set; }
    public decimal TauxChange { get; set; }
    public bool EstDeviseParDefaut { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour la création d'une devise
/// </summary>
public class CreateDeviseDto
{
    public string CodeDevise { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Symbole { get; set; }
    public decimal TauxChange { get; set; }
    public bool EstDeviseParDefaut { get; set; }
}

/// <summary>
/// DTO pour la mise à jour d'une devise
/// </summary>
public class UpdateDeviseDto
{
    public string CodeDevise { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public string? Symbole { get; set; }
    public decimal TauxChange { get; set; }
    public bool EstDeviseParDefaut { get; set; }
}
