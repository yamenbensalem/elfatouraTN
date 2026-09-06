using GestCom_Desktop.Licensing;

namespace GestCom_Desktop;

/// <summary>
/// The one place that turns a <see cref="LicenseValidationStatus"/> into a French message a user
/// can act on. Shared between the startup check in <see cref="Program"/> (before
/// <see cref="AppHost.Initialize"/> ever touches the database) and the periodic re-check timer in
/// <see cref="Forms.MainForm"/>, so the two never drift into showing different wording for the
/// same failure.
/// </summary>
internal static class LicenseGate
{
    public static LicenseValidationResult Validate() => LicenseValidator.Validate(LicenseLocator.LoadCurrent());

    public static string DescribeFailure(LicenseValidationStatus status) => status switch
    {
        LicenseValidationStatus.MissingFile =>
            "Aucune licence n'a été trouvée sur ce poste.\n\n" +
            "Contactez Yamen pour obtenir votre fichier de licence (license.lic) et placez-le dans :\n" +
            LicenseLocator.ProgramDataPath,

        LicenseValidationStatus.Corrupt =>
            "Le fichier de licence est illisible ou corrompu.\n\n" +
            "Redemandez un fichier de licence à Yamen et remplacez celui présent sur ce poste.",

        LicenseValidationStatus.InvalidSignature =>
            "La signature de la licence est invalide — ce fichier de licence n'est pas reconnu comme authentique.\n\n" +
            "Contactez Yamen : ce fichier a peut-être été altéré ou provient d'une autre application.",

        LicenseValidationStatus.FingerprintMismatch =>
            "Cette licence ne correspond pas à ce poste de travail.\n\n" +
            "Une licence GestCom est verrouillée sur la machine pour laquelle elle a été émise. " +
            "Contactez Yamen pour obtenir une licence pour ce poste.",

        LicenseValidationStatus.Expired =>
            "La licence de ce poste a expiré.\n\n" +
            "Contactez Yamen pour renouveler votre licence.",

        _ => "La licence de l'application est invalide. Contactez Yamen.",
    };

    public const string DialogTitle = "Licence GestCom";
}
