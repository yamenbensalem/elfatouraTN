namespace GestCom.Shared.Constants;

/// <summary>
/// Constantes de l'application
/// </summary>
public static class AppConstants
{
    public const string DateFormat = "yyyy-MM-dd";
    public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";
    }

    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireManagerRole = "RequireManagerRole";
    }

    public static class StatutFacture
    {
        public const string Brouillon = "Brouillon";
        public const string Validee = "Validée";
        public const string Payee = "Payée";
        public const string Annulee = "Annulée";
    }

    public static class StatutCommande
    {
        public const string EnCours = "En cours";
        public const string Validee = "Validée";
        public const string Livree = "Livrée";
        public const string Annulee = "Annulée";
    }

    public static class TypePersonne
    {
        public const string PersonnePhysique = "Personne Physique";
        public const string PersonneMorale = "Personne Morale";
    }
}
