using System.Globalization;
using System.Text.Json;
using GestCom_Desktop.Licensing;

namespace GestCom_LicenseGenerator;

public static class Commands
{
    private static readonly JsonSerializerOptions ReportJsonOptions = new() { WriteIndented = true };

    public static int PrintUsage()
    {
        Console.WriteLine("GestCom — Outil de licence");
        Console.WriteLine();
        Console.WriteLine("Utilisation :");
        Console.WriteLine("  collect [cheminSortie]");
        Console.WriteLine("      À exécuter sur la machine du CLIENT. Ne nécessite pas la clé privée.");
        Console.WriteLine("      Produit un rapport d'empreinte machine (fingerprint.json par défaut).");
        Console.WriteLine();
        Console.WriteLine("  issue --fingerprint <chemin> --client \"<nom>\" --key <cheminClePrivee> --out <cheminSortie> [--expires yyyy-MM-dd]");
        Console.WriteLine("      À exécuter par Yamen, avec la clé privée. Produit un fichier .lic signé.");
        Console.WriteLine();
        return 1;
    }

    public static int Collect(string[] args)
    {
        var outputPath = args.Length > 1 ? args[1] : "fingerprint.json";

        var report = FingerprintReport.FromCurrentMachine();
        var json = JsonSerializer.Serialize(report, ReportJsonOptions);
        File.WriteAllText(outputPath, json);

        Console.WriteLine($"Empreinte machine écrite dans : {Path.GetFullPath(outputPath)}");
        Console.WriteLine();
        Console.WriteLine("Envoyez ce fichier à Yamen (par email ou tout autre canal) — il en a besoin");
        Console.WriteLine("pour générer votre licence. Ce fichier ne contient aucune information sensible.");
        return 0;
    }

    public static int Issue(string[] args)
    {
        try
        {
            var arguments = NamedArguments.Parse(args, startIndex: 1);

            var fingerprintPath = arguments.Require("fingerprint");
            var clientName = arguments.Require("client");
            var privateKeyPath = arguments.Require("key");
            var outputPath = arguments.Require("out");
            var expiresText = arguments.GetOrDefault("expires");

            if (!File.Exists(fingerprintPath))
            {
                Console.Error.WriteLine($"Fichier d'empreinte introuvable : {fingerprintPath}");
                return 1;
            }

            var report = JsonSerializer.Deserialize<FingerprintReport>(File.ReadAllText(fingerprintPath))
                ?? throw new InvalidDataException("Le fichier d'empreinte est illisible.");

            DateTime? expiresUtc = null;
            if (!string.IsNullOrWhiteSpace(expiresText))
            {
                if (!DateTime.TryParseExact(expiresText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedExpiry))
                {
                    Console.Error.WriteLine($"Format de date invalide pour --expires : {expiresText} (attendu : yyyy-MM-dd)");
                    return 1;
                }

                expiresUtc = DateTime.SpecifyKind(parsedExpiry, DateTimeKind.Utc);
            }

            var payload = new LicensePayload(
                LicenseId: Guid.NewGuid(),
                ClientName: clientName,
                FingerprintHash: report.FingerprintHash,
                IssuedUtc: DateTime.UtcNow,
                ExpiresUtc: expiresUtc);

            var licenseFile = LicenseSigner.SignWithPrivateKeyFile(payload, privateKeyPath);
            licenseFile.Save(outputPath);

            Console.WriteLine("Licence générée avec succès.");
            Console.WriteLine($"  Client       : {clientName}");
            Console.WriteLine($"  LicenseId    : {payload.LicenseId}");
            Console.WriteLine($"  Expiration   : {(expiresUtc?.ToString("yyyy-MM-dd") ?? "aucune (perpétuelle)")}");
            Console.WriteLine($"  Fichier      : {Path.GetFullPath(outputPath)}");
            Console.WriteLine();
            Console.WriteLine("Envoyez ce fichier .lic au client — à placer à l'emplacement décrit dans INSTALL.md.");
            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
