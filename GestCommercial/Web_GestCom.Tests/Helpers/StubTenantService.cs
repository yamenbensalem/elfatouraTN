using Web_GestCom.Services;

namespace Web_GestCom.Tests.Helpers;

/// <summary>
/// Fixed-tenant ITenantService for tests that need UtilisateurService.EnsureTenantDefaults to
/// resolve a real company (the common "acting admin creates a user in their own company" case)
/// without wiring up the full HTTP claims pipeline.
/// </summary>
public sealed class StubTenantService(int? companyId = 1) : ITenantService
{
    public int?   CurrentCompanyId { get; } = companyId;
    public int?   CurrentUserId    => null;
    public string CurrentUserLogin => string.Empty;
}
