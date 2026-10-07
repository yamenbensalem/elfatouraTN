namespace Web_GestCom.Services;

/// <summary>
/// Logo d'entreprise téléversé depuis la fiche Entreprise. Il est stocké dans la base (colonne
/// logo_image_entreprise) sous forme de "data URI", pas dans un dossier du serveur : le conteneur
/// est recréé à chaque déploiement (un fichier déposé serait perdu), et la fiche étant cloisonnée
/// par entreprise, le logo l'est aussi sans rien faire de plus.
/// </summary>
public static class LogoImage
{
    /// <summary>Largement suffisant pour un logo d'en-tête ; garde la fiche légère (elle est lue à chaque impression).</summary>
    public const int MaxBytes = 200 * 1024;

    /// <summary>Types proposés par le sélecteur de fichier (attribut accept).</summary>
    public const string Accept = "image/png,image/jpeg,image/gif,image/webp";

    public const string FormatsAcceptes = "PNG, JPEG, GIF ou WebP";

    /// <summary>
    /// Construit le data URI à stocker. Le type est déduit des premiers octets du fichier, jamais
    /// de son nom ni du type annoncé par le navigateur — un fichier renommé en .png est refusé.
    /// SVG est volontairement exclu (c'est du XML pouvant contenir du script).
    /// </summary>
    public static bool TryBuildDataUri(byte[] contenu, out string dataUri, out string erreur)
    {
        dataUri = string.Empty;

        if (contenu.Length == 0)
        {
            erreur = "Le fichier est vide.";
            return false;
        }
        if (contenu.Length > MaxBytes)
        {
            erreur = $"Le logo dépasse la taille maximale de {MaxBytes / 1024} Ko ({contenu.Length / 1024} Ko). Réduisez l'image puis réessayez.";
            return false;
        }

        var type = DetecterType(contenu);
        if (type is null)
        {
            erreur = $"Ce fichier n'est pas une image reconnue. Formats acceptés : {FormatsAcceptes}.";
            return false;
        }

        dataUri = $"data:{type};base64,{Convert.ToBase64String(contenu)}";
        erreur = string.Empty;
        return true;
    }

    private static string? DetecterType(ReadOnlySpan<byte> b)
    {
        if (b.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return "image/png";
        if (b.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF })) return "image/jpeg";
        if (b.StartsWith("GIF87a"u8) || b.StartsWith("GIF89a"u8)) return "image/gif";
        if (b.Length >= 12 && b[..4].SequenceEqual("RIFF"u8) && b.Slice(8, 4).SequenceEqual("WEBP"u8)) return "image/webp";
        return null;
    }
}
