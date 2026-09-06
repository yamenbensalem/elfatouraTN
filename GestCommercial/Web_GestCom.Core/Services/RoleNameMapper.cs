namespace Web_GestCom.Services;

public static class RoleNameMapper
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employe = "Employé";

    public static string NormalizeKnownRoleName(string? roleName)
    {
        var raw = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
            return Employe;

        return raw.ToLowerInvariant() switch
        {
            "superadmin" or "super-admin" or "super_admin" or "globaladmin" => SuperAdmin,
            "admin" => Admin,
            "manager" or "gestionnaire" => Manager,
            "employe" or "employé" or "employee" or "user" or "utilisateur" => Employe,
            _ => raw
        };
    }
}