using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestCom_Desktop.Licensing;

/// <summary>
/// Options used to serialize a <see cref="LicensePayload"/> into the exact bytes that get signed
/// and later re-verified. Shared between <see cref="LicenseSigner"/> and
/// <see cref="LicenseValidator"/> so both sides agree byte-for-byte.
/// </summary>
internal static class LicenseJson
{
    public static readonly JsonSerializerOptions PayloadOptions = new()
    {
        WriteIndented = false,
    };

    public static readonly JsonSerializerOptions FileOptions = new()
    {
        WriteIndented = true,
    };
}

/// <summary>
/// On-disk shape of a <c>.lic</c> file: the signed payload plus its RSA signature, both base64.
/// <see cref="FormatVersion"/> exists so a future breaking change to the payload shape (a new
/// field, a different signature scheme) can be detected before attempting to parse or verify.
/// </summary>
public sealed record LicenseFile(
    [property: JsonPropertyName("formatVersion")] int FormatVersion,
    [property: JsonPropertyName("payload")] string PayloadBase64,
    [property: JsonPropertyName("signature")] string SignatureBase64)
{
    public const int CurrentFormatVersion = 1;

    public byte[] GetPayloadBytes() => Convert.FromBase64String(PayloadBase64);

    public byte[] GetSignatureBytes() => Convert.FromBase64String(SignatureBase64);

    public LicensePayload DeserializePayload() =>
        JsonSerializer.Deserialize<LicensePayload>(GetPayloadBytes(), LicenseJson.PayloadOptions)
        ?? throw new InvalidDataException("Le contenu de la licence est illisible.");

    /// <summary>
    /// The canonical bytes for a payload — call this once, sign the result, then pass the very
    /// same bytes to <see cref="Create"/> so the stored payload and the signed bytes never drift
    /// apart through two independent serialization calls.
    /// </summary>
    public static byte[] SerializePayload(LicensePayload payload) =>
        JsonSerializer.SerializeToUtf8Bytes(payload, LicenseJson.PayloadOptions);

    public static LicenseFile Create(byte[] payloadBytes, byte[] signature) =>
        new(CurrentFormatVersion, Convert.ToBase64String(payloadBytes), Convert.ToBase64String(signature));

    public void Save(string path)
    {
        var json = JsonSerializer.Serialize(this, LicenseJson.FileOptions);
        File.WriteAllText(path, json);
    }

    public static LicenseFile? TryLoad(string path)
    {
        if (!File.Exists(path))
            return null;

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<LicenseFile>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
