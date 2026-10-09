using Microsoft.Extensions.Logging;

namespace Web_GestCom.Services;

/// <summary>Utilisé tant qu'aucun fournisseur email n'est configuré (dev sans clé API) — logue au lieu d'envoyer.</summary>
public sealed class NoOpEmailTransport(ILogger<NoOpEmailTransport> logger) : IEmailTransport
{
    public Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogWarning("Email non envoyé (aucun fournisseur configuré) — À: {ToEmail}, Sujet: {Subject}", toEmail, subject);
        return Task.FromResult(false);
    }
}
