namespace Web_GestCom.Services;

/// <summary>
/// Abstraction d'envoi d'email — permet de brancher un vrai fournisseur (Brevo) en production
/// et un transport sans effet de bord (log uniquement) tant qu'aucune clé API n'est configurée,
/// sans jamais faire échouer l'appelant si l'email ne part pas (voir <see cref="NoOpEmailTransport"/>
/// et l'usage dans AbonnementService — un échec d'email ne doit jamais bloquer la demande elle-même).
/// </summary>
public interface IEmailTransport
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default);
}

public sealed class EmailOptions
{
    /// <summary>"Brevo" ou vide/inconnu → transport no-op (log uniquement, utile en dev sans clé).</summary>
    public string Provider { get; set; } = string.Empty;

    public string BrevoApiKey { get; set; } = string.Empty;

    public string FromEmail { get; set; } = "noreply@tijaraflow.fr";
    public string FromName { get; set; } = "GestCom";

    /// <summary>Adresse notifiée à chaque nouvelle demande d'abonnement.</summary>
    public string AdminNotificationEmail { get; set; } = "admin@tijaraflow.fr";
}
