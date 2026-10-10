using Web_GestCom.Data;
using Web_GestCom.Services;

namespace Web_GestCom.Auth;

/// <summary>
/// Bloque toute requête d'un utilisateur dont l'entreprise est suspendue (abonnement échu au-delà
/// du délai de grâce — voir <see cref="AccesEntreprise"/>) : il est renvoyé vers la page « Accès
/// suspendu ». Blocage complet, sans lecture seule. Placé après UseAuthentication.
///
/// Les pages étant chacune leur propre circuit Blazor, chaque navigation repasse par ici. La
/// connexion SignalR (/_blazor) est refusée aussi, pour qu'un circuit resté ouvert ne puisse pas se
/// reconnecter ; ServicePermissionGuard couvre les écritures tentées depuis un tel circuit.
/// </summary>
public sealed class SuspensionAccesMiddleware(RequestDelegate next)
{
    public const string PageSuspension = "/compte/acces-suspendu";

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true
            && !user.IsSuperAdmin()
            && user.GetCompanyId() is int companyId
            && !EstCheminLibre(context.Request.Path))
        {
            var acces = await AccesEntreprise.ChargerAsync(db, companyId, DateTime.Today);
            if (acces.EstSuspendu)
            {
                // Une page : on explique. Tout le reste (SignalR, envoi de formulaire) : refus sec.
                if (HttpMethods.IsGet(context.Request.Method) && !context.Request.Path.StartsWithSegments("/_blazor"))
                    context.Response.Redirect(PageSuspension);
                else
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        await next(context);
    }

    /// <summary>
    /// Ce qui reste accessible à une entreprise suspendue : se connecter, se déconnecter, lire la
    /// page de suspension, la page publique de demande d'abonnement, et les fichiers statiques.
    /// </summary>
    public static bool EstCheminLibre(PathString path)
        => path.StartsWithSegments("/compte")
           || path.StartsWithSegments("/demande-abonnement")
           || path.StartsWithSegments("/_framework")
           || path.StartsWithSegments("/_content")
           || Path.HasExtension(path.Value);
}
