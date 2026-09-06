using System.Text.Json.Serialization;

namespace GestCom_Desktop.Licensing;

/// <summary>
/// The data a license grants, before signing. Property order and names are fixed via
/// <see cref="JsonPropertyNameAttribute"/> so that serializing the same payload twice always
/// produces the same bytes — the signature is over these exact bytes, so any drift in
/// serialization would make an otherwise-valid license fail to verify.
/// </summary>
public sealed record LicensePayload(
    [property: JsonPropertyName("licenseId")] Guid LicenseId,
    [property: JsonPropertyName("clientName")] string ClientName,
    [property: JsonPropertyName("fingerprintHash")] string FingerprintHash,
    [property: JsonPropertyName("issuedUtc")] DateTime IssuedUtc,
    [property: JsonPropertyName("expiresUtc")] DateTime? ExpiresUtc);
