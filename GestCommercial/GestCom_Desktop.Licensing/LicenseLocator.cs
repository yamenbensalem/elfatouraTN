namespace GestCom_Desktop.Licensing;

/// <summary>
/// Resolves where the license file lives on disk. <see cref="ProgramDataPath"/> is the real
/// install location; <see cref="ExeDirectoryPath"/> is a convenience fallback so Yamen can drop
/// <c>license.lic</c> straight next to <c>GestCom_Desktop.exe</c> during a first install,
/// before ProgramData is set up — see deploy/DEPLOY.md, section 7.
/// </summary>
public static class LicenseLocator
{
    private const string CompanyFolderName = "GestCom";
    private const string LicenseFileName = "license.lic";

    public static string ProgramDataPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), CompanyFolderName, LicenseFileName);

    public static string ExeDirectoryPath => Path.Combine(AppContext.BaseDirectory, LicenseFileName);

    /// <summary>The ProgramData path if a license exists there, otherwise the exe-directory fallback, otherwise null.</summary>
    public static string? Resolve()
    {
        if (File.Exists(ProgramDataPath))
            return ProgramDataPath;

        if (File.Exists(ExeDirectoryPath))
            return ExeDirectoryPath;

        return null;
    }

    public static LicenseFile? LoadCurrent()
    {
        var path = Resolve();
        return path is null ? null : LicenseFile.TryLoad(path);
    }
}
