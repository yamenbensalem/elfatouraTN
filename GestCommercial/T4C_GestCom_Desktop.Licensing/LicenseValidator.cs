using System.Security.Cryptography;

namespace T4C_GestCom_Desktop.Licensing;

public enum LicenseValidationStatus
{
    Valid,
    MissingFile,
    Corrupt,
    InvalidSignature,
    FingerprintMismatch,
    Expired,
}

public sealed record LicenseValidationResult(LicenseValidationStatus Status, LicensePayload? Payload)
{
    public bool IsValid => Status == LicenseValidationStatus.Valid;

    public static LicenseValidationResult Of(LicenseValidationStatus status, LicensePayload? payload = null) => new(status, payload);
}

/// <summary>
/// Validates a <see cref="LicenseFile"/> against the RSA public key embedded below. The matching
/// private key lives only at <c>deploy/keys/t4c-license-private.pem</c> on Yamen's machine — never
/// committed, never shipped. Regenerating the keypair invalidates every license issued with the
/// previous one, so this constant only ever changes on a deliberate key rotation.
/// </summary>
public static class LicenseValidator
{
    private const string PublicKeyPem = """
        -----BEGIN PUBLIC KEY-----
        MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAwWV23vsUKNl60DqoTSfo
        UrFeUMuc3DmOWSqJ2Gv9uJvlMuI5pZhONbzzX+Dv5+2u+uzuWs2nh7/jxnkJ0M+E
        V4UprqB48cWu7uYUet8HEQiphVHuzHruHOu7ARclYwTPZXUf1JpAPqpddOf7PR2+
        +Z4l9xoQ/qLaYQ916U++QHib7KEh3lMxjhxnQx/ZEX4ub1CnxLJJUDVp2SnNmJDW
        6GftLbJGPUXS9jfqw11/3TEJsIGL5Af2T0ssC+S5/hHvH54nqXAAiVgmL3MGAqds
        8a0Ruvbr7EZqZvD/8nchI8yP0HQ4Sq88OE9hsYSBqwccQSlHOdjIZHVNxu6U3J6Z
        PXJ4pJV1IchFR00Y6FARGMumvNQa92JlJLUsevne0CsID2YZVpA/Pq32tX0hGx8l
        Ai4kTQC9awI1itx5ydb8HY4gEcPI/ANbK1AG16kb4tN1o9DJkZH8pE1HP9vYyqUW
        0DvstiS9vJP+nfC+6/l0893kWRgRZP5AjcrryTsy2iifAgMBAAE=
        -----END PUBLIC KEY-----
        """;

    public static LicenseValidationResult Validate(LicenseFile? licenseFile) =>
        Validate(licenseFile, MachineFingerprint.ComputeCurrent());

    /// <summary>Overload taking an explicit fingerprint hash — used by tests to avoid depending on the real machine.</summary>
    public static LicenseValidationResult Validate(LicenseFile? licenseFile, string currentFingerprintHash)
    {
        using var publicKey = RSA.Create();
        publicKey.ImportFromPem(PublicKeyPem);
        return Validate(licenseFile, currentFingerprintHash, publicKey);
    }

    /// <summary>
    /// Overload taking an explicit public key — lets tests verify against a throwaway keypair
    /// instead of the real embedded production key, without changing any validation logic.
    /// </summary>
    public static LicenseValidationResult Validate(LicenseFile? licenseFile, string currentFingerprintHash, RSA publicKey)
    {
        if (licenseFile is null)
            return LicenseValidationResult.Of(LicenseValidationStatus.MissingFile);

        LicensePayload payload;
        byte[] payloadBytes, signatureBytes;
        try
        {
            payloadBytes = licenseFile.GetPayloadBytes();
            signatureBytes = licenseFile.GetSignatureBytes();
            payload = licenseFile.DeserializePayload();
        }
        catch (Exception ex) when (ex is FormatException or InvalidDataException)
        {
            return LicenseValidationResult.Of(LicenseValidationStatus.Corrupt);
        }

        var signatureValid = publicKey.VerifyData(payloadBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        if (!signatureValid)
            return LicenseValidationResult.Of(LicenseValidationStatus.InvalidSignature);

        if (!string.Equals(payload.FingerprintHash, currentFingerprintHash, StringComparison.OrdinalIgnoreCase))
            return LicenseValidationResult.Of(LicenseValidationStatus.FingerprintMismatch, payload);

        if (payload.ExpiresUtc is { } expiresUtc && expiresUtc < DateTime.UtcNow)
            return LicenseValidationResult.Of(LicenseValidationStatus.Expired, payload);

        return LicenseValidationResult.Of(LicenseValidationStatus.Valid, payload);
    }
}
