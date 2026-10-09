using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Web_GestCom.Services;

/// <summary>
/// Envoie via l'API transactionnelle Brevo (https://api.brevo.com/v3/smtp/email). N'expose jamais
/// la clé API en dehors de l'en-tête HTTP — configurée via EmailOptions:BrevoApiKey (user-secrets
/// en dev, variable d'environnement en prod, jamais dans un fichier suivi par git).
/// </summary>
public sealed class BrevoEmailTransport(HttpClient http, IOptions<EmailOptions> options, ILogger<BrevoEmailTransport> logger) : IEmailTransport
{
    private readonly EmailOptions _options = options.Value;

    public async Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", _options.BrevoApiKey);
        request.Headers.Add("Accept", "application/json");
        request.Content = JsonContent.Create(new
        {
            sender = new { name = _options.FromName, email = _options.FromEmail },
            to = new[] { new { email = toEmail, name = toName } },
            subject,
            htmlContent = htmlBody
        });

        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Échec d'envoi email via Brevo ({Status}) à {ToEmail} : {Body}", response.StatusCode, toEmail, body);
            return false;
        }

        return true;
    }
}
