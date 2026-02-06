namespace GestCom.Application.Common.Interfaces;

/// <summary>
/// Interface pour le service d'envoi d'emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envoie un email simple
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Envoie un email avec pièce jointe
    /// </summary>
    Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string attachmentName);

    /// <summary>
    /// Envoie une facture par email
    /// </summary>
    Task SendFactureAsync(string numeroFacture, string emailClient);

    /// <summary>
    /// Envoie un devis par email
    /// </summary>
    Task SendDevisAsync(string numeroDevis, string emailClient);

    /// <summary>
    /// Envoie un rappel de paiement
    /// </summary>
    Task SendRappelPaiementAsync(string codeClient, string emailClient, List<string> numerosFactures);
}
