using System;

namespace GestCom.Application.Features.Achats.ReglementsFournisseur.DTOs;

/// <summary>
/// DTO complet pour un règlement fournisseur
/// </summary>
public class ReglementFournisseurDto
{
    public int Id { get; set; }
    public string NumeroReglement { get; set; } = string.Empty;
    public DateTime DateReglement { get; set; }
    public string NumeroFacture { get; set; } = string.Empty;
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NomFournisseur { get; set; }
    public decimal Montant { get; set; }
    public string? ModePayement { get; set; }
    public string? NumeroTransaction { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO pour la liste des règlements
/// </summary>
public class ReglementFournisseurListDto
{
    public int Id { get; set; }
    public DateTime DateReglement { get; set; }
    public string NumeroFacture { get; set; } = string.Empty;
    public string? NomFournisseur { get; set; }
    public decimal Montant { get; set; }
    public string? ModePayement { get; set; }
}

/// <summary>
/// DTO pour créer un règlement
/// </summary>
public class CreateReglementFournisseurDto
{
    public DateTime DateReglement { get; set; }
    public string NumeroFacture { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public string? ModePayement { get; set; }
    public string? NumeroTransaction { get; set; }
    public string? Notes { get; set; }
}
