using Microsoft.AspNetCore.Authorization;

namespace Web_T4C_GestCom.Auth;

/// <summary>Requirement portant un code de permission (ex. "factures.view").</summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}
