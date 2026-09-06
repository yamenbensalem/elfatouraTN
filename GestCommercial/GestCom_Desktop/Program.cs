using Serilog;
using GestCom_Desktop.Forms;

namespace GestCom_Desktop;

internal static class Program
{
    /// <summary>
    /// %ProgramData%\GestCom\logs by default — shared regardless of which Windows account runs
    /// the app. Falls back to %LOCALAPPDATA% if that folder isn't writable by the current account
    /// (e.g. a ProgramData subfolder created by a different account than the service account the
    /// app is later run under — see deploy/DEPLOY.md section 8.2 for that scenario).
    /// </summary>
    private static readonly string LogDirectory = ResolveWritableLogDirectory();

    [STAThread]
    static void Main()
    {
        // Debug : les écrans (Forms/**) tracent leurs opérations (chargement, enregistrement,
        // suppression) à ce niveau — il doit rester activé pour que ces logs remontent au fichier.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(LogDirectory, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // Sans ces deux handlers, une exception non gérée tue le process .NET en silence (aucune
        // fenêtre, aucun log) dès lors que l'app n'est pas lancée depuis une console — c'est
        // exactement ce qui s'est produit lors du déploiement chez un client avant leur ajout.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Log.Fatal(e.ExceptionObject as Exception, "Exception non gérée (hors thread UI) — l'application s'arrête.");

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
        {
            Log.Error(e.Exception, "Exception non gérée sur le thread UI.");
            MessageBox.Show(
                $"Une erreur inattendue est survenue. Détails dans le journal :\n{LogDirectory}",
                "GestCom — Erreur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        };

        try
        {
            Log.Information("Démarrage de GestCom Desktop.");
            ApplicationConfiguration.Initialize();
            Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Échec au démarrage de l'application.");
            MessageBox.Show(
                $"L'application n'a pas pu démarrer. Détails dans le journal :\n{LogDirectory}",
                "GestCom — Erreur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void Run()
    {
        // Gate on license validity BEFORE AppHost.Initialize() — an invalid/missing/tampered
        // license must never reach the database or show the login form.
        var licenseResult = LicenseGate.Validate();
        if (!licenseResult.IsValid)
        {
            Log.Warning("Licence invalide au démarrage : {Status}", licenseResult.Status);
            MessageBox.Show(LicenseGate.DescribeFailure(licenseResult.Status), LicenseGate.DialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        AppHost.Initialize();

        using var login = new LoginForm();
        if (login.ShowDialog() == DialogResult.OK)
        {
            Log.Information("Connexion réussie : {Login}", AppHost.Session.Login);
            Application.Run(new MainForm());
        }
    }

    private static string ResolveWritableLogDirectory()
    {
        var programDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GestCom", "logs");
        try
        {
            Directory.CreateDirectory(programDataDir);
            return programDataDir;
        }
        catch (UnauthorizedAccessException)
        {
            var fallbackDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestCom", "logs");
            Directory.CreateDirectory(fallbackDir);
            return fallbackDir;
        }
    }
}
