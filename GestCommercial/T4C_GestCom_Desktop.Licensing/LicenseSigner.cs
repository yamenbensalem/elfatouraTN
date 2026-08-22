using System.Security.Cryptography;

namespace T4C_GestCom_Desktop.Licensing;

/// <summary>
/// Signs a <see cref="LicensePayload"/> with the RSA private key. Only ever runs on Yamen's own
/// machine, using the private key at <c>deploy/keys/t4c-license-private.pem</c> — that file never
/// ships with the app and is gitignored (see <see cref="LicenseValidator"/> for the public half
/// embedded in the shipped app).
/// </summary>
public static class LicenseSigner
{
    public static LicenseFile Sign(LicensePayload payload, RSA privateKey)
    {
        var payloadBytes = LicenseFile.SerializePayload(payload);
        var signature = privateKey.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return LicenseFile.Create(payloadBytes, signature);
    }

    public static LicenseFile SignWithPrivateKeyFile(LicensePayload payload, string privateKeyPemPath)
    {
        if (!File.Exists(privateKeyPemPath))
            throw new FileNotFoundException("Clé privée introuvable.", privateKeyPemPath);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(privateKeyPemPath));
        return Sign(payload, rsa);
    }
}
