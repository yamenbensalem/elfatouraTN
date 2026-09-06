namespace Web_GestCom.Services;

/// <summary>
/// Abstracts tenant/user resolution for services that need it outside of a request pipeline.
/// Only the interface lives here — the HTTP-backed implementation (TenantService, resolving
/// claims via IHttpContextAccessor) stays in the web project. Desktop hosts provide their own
/// implementation backed by the logged-in session instead.
/// </summary>
public interface ITenantService
{
    int?   CurrentCompanyId { get; }
    int?   CurrentUserId    { get; }
    string CurrentUserLogin { get; }
}
