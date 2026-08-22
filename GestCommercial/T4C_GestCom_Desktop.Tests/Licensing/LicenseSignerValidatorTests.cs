using System.Security.Cryptography;
using System.Text;
using T4C_GestCom_Desktop.Licensing;
using Xunit;

namespace T4C_GestCom_Desktop.Tests.Licensing;

public class LicenseSignerValidatorTests
{
    private const string MatchingFingerprintHash = "TEST-FINGERPRINT-HASH-0001";
    private const string OtherFingerprintHash = "TEST-FINGERPRINT-HASH-9999";

    // Generated fresh per test run — never the real embedded production key from LicenseValidator.
    private static RSA CreateTestKeyPair() => RSA.Create(2048);

    private static LicensePayload CreatePayload(string fingerprintHash, DateTime? expiresUtc = null) =>
        new(
            LicenseId: Guid.NewGuid(),
            ClientName: "Client Test",
            FingerprintHash: fingerprintHash,
            IssuedUtc: DateTime.UtcNow,
            ExpiresUtc: expiresUtc);

    [Fact]
    public void Validate_SignedLicenseWithMatchingFingerprint_ReturnsValid()
    {
        // Arrange
        using var keyPair = CreateTestKeyPair();
        var payload = CreatePayload(MatchingFingerprintHash);
        var licenseFile = LicenseSigner.Sign(payload, keyPair);

        // Act
        var result = LicenseValidator.Validate(licenseFile, MatchingFingerprintHash, keyPair);

        // Assert
        Assert.Equal(LicenseValidationStatus.Valid, result.Status);
        Assert.True(result.IsValid);
        Assert.Equal(payload.LicenseId, result.Payload?.LicenseId);
    }

    [Fact]
    public void Validate_SignedLicenseWithDifferentFingerprint_ReturnsFingerprintMismatch()
    {
        // Arrange
        using var keyPair = CreateTestKeyPair();
        var payload = CreatePayload(MatchingFingerprintHash);
        var licenseFile = LicenseSigner.Sign(payload, keyPair);

        // Act
        var result = LicenseValidator.Validate(licenseFile, OtherFingerprintHash, keyPair);

        // Assert
        Assert.Equal(LicenseValidationStatus.FingerprintMismatch, result.Status);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_TamperedPayloadAfterSigning_ReturnsInvalidSignature()
    {
        // Arrange — sign, then rewrite the client name inside the stored payload without re-signing
        using var keyPair = CreateTestKeyPair();
        var payload = CreatePayload(MatchingFingerprintHash);
        var signedLicenseFile = LicenseSigner.Sign(payload, keyPair);

        var payloadJson = Encoding.UTF8.GetString(signedLicenseFile.GetPayloadBytes());
        var tamperedJson = payloadJson.Replace("Client Test", "Client Pirate");
        var tamperedPayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(tamperedJson));
        var tamperedLicenseFile = signedLicenseFile with { PayloadBase64 = tamperedPayloadBase64 };

        // Act
        var result = LicenseValidator.Validate(tamperedLicenseFile, MatchingFingerprintHash, keyPair);

        // Assert
        Assert.Equal(LicenseValidationStatus.InvalidSignature, result.Status);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ExpiredLicense_ReturnsExpired()
    {
        // Arrange
        using var keyPair = CreateTestKeyPair();
        var payload = CreatePayload(MatchingFingerprintHash, expiresUtc: DateTime.UtcNow.AddDays(-1));
        var licenseFile = LicenseSigner.Sign(payload, keyPair);

        // Act
        var result = LicenseValidator.Validate(licenseFile, MatchingFingerprintHash, keyPair);

        // Assert
        Assert.Equal(LicenseValidationStatus.Expired, result.Status);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_FutureExpiryLicense_ReturnsValid()
    {
        // Arrange
        using var keyPair = CreateTestKeyPair();
        var payload = CreatePayload(MatchingFingerprintHash, expiresUtc: DateTime.UtcNow.AddDays(30));
        var licenseFile = LicenseSigner.Sign(payload, keyPair);

        // Act
        var result = LicenseValidator.Validate(licenseFile, MatchingFingerprintHash, keyPair);

        // Assert
        Assert.Equal(LicenseValidationStatus.Valid, result.Status);
    }

    [Fact]
    public void Validate_NullLicenseFile_ReturnsMissingFile()
    {
        // Arrange
        using var keyPair = CreateTestKeyPair();

        // Act
        var result = LicenseValidator.Validate(null, MatchingFingerprintHash, keyPair);

        // Assert
        Assert.Equal(LicenseValidationStatus.MissingFile, result.Status);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_SignatureFromDifferentKeyPair_ReturnsInvalidSignature()
    {
        // Arrange — the license was signed by a key other than the one being validated against
        using var signingKeyPair = CreateTestKeyPair();
        using var otherKeyPair = CreateTestKeyPair();
        var payload = CreatePayload(MatchingFingerprintHash);
        var licenseFile = LicenseSigner.Sign(payload, signingKeyPair);

        // Act
        var result = LicenseValidator.Validate(licenseFile, MatchingFingerprintHash, otherKeyPair);

        // Assert
        Assert.Equal(LicenseValidationStatus.InvalidSignature, result.Status);
    }
}
