using System;
using System.Collections.Generic;

namespace GestCom.Application.Common.Exceptions;

/// <summary>
/// Exception levée lors de la validation des commandes
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("Une ou plusieurs erreurs de validation se sont produites.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : this()
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}

/// <summary>
/// Exception levée quand une entité n'est pas trouvée
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException()
        : base()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"L'entité \"{name}\" avec la clé ({key}) n'a pas été trouvée.")
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception levée pour les erreurs métier
/// </summary>
public class BusinessException : Exception
{
    public string? ErrorCode { get; }

    public BusinessException()
        : base()
    {
    }

    public BusinessException(string message)
        : base(message)
    {
    }

    public BusinessException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception levée quand le crédit client est dépassé
/// </summary>
public class CreditLimitExceededException : BusinessException
{
    public string CodeClient { get; }
    public decimal LimiteCredit { get; }
    public decimal SoldeActuel { get; }
    public decimal MontantDemande { get; }

    public CreditLimitExceededException(string codeClient, decimal limiteCredit, decimal soldeActuel, decimal montantDemande)
        : base($"La limite de crédit du client '{codeClient}' serait dépassée. " +
               $"Limite: {limiteCredit:N3} TND, Solde actuel: {soldeActuel:N3} TND, Montant demandé: {montantDemande:N3} TND.",
               "CREDIT_LIMIT_EXCEEDED")
    {
        CodeClient = codeClient;
        LimiteCredit = limiteCredit;
        SoldeActuel = soldeActuel;
        MontantDemande = montantDemande;
    }
}

/// <summary>
/// Exception levée quand le stock est insuffisant
/// </summary>
public class InsufficientStockException : BusinessException
{
    public string CodeProduit { get; }
    public decimal StockDisponible { get; }
    public decimal QuantiteDemandee { get; }

    public InsufficientStockException(string codeProduit, decimal stockDisponible, decimal quantiteDemandee)
        : base($"Stock insuffisant pour le produit '{codeProduit}'. " +
               $"Stock disponible: {stockDisponible:N3}, Quantité demandée: {quantiteDemandee:N3}.",
               "INSUFFICIENT_STOCK")
    {
        CodeProduit = codeProduit;
        StockDisponible = stockDisponible;
        QuantiteDemandee = quantiteDemandee;
    }
}

/// <summary>
/// Exception levée quand un document est déjà traité
/// </summary>
public class DocumentAlreadyProcessedException : BusinessException
{
    public string NumeroDocument { get; }
    public string TypeDocument { get; }
    public string Statut { get; }

    public DocumentAlreadyProcessedException(string numeroDocument, string typeDocument, string statut)
        : base($"Le {typeDocument} '{numeroDocument}' est déjà traité avec le statut '{statut}'.",
               "DOCUMENT_ALREADY_PROCESSED")
    {
        NumeroDocument = numeroDocument;
        TypeDocument = typeDocument;
        Statut = statut;
    }
}

/// <summary>
/// Exception levée quand une entité existe déjà
/// </summary>
public class DuplicateEntityException : BusinessException
{
    public string EntityName { get; }
    public string DuplicateField { get; }
    public string DuplicateValue { get; }

    public DuplicateEntityException(string entityName, string duplicateField, string duplicateValue)
        : base($"Une entité '{entityName}' avec la valeur '{duplicateValue}' pour le champ '{duplicateField}' existe déjà.",
               "DUPLICATE_ENTITY")
    {
        EntityName = entityName;
        DuplicateField = duplicateField;
        DuplicateValue = duplicateValue;
    }
}
