using System.Text.Json.Serialization;
using GestCom_Desktop.Licensing;

namespace GestCom_LicenseGenerator;

/// <summary>
/// The JSON produced by <c>collect</c> and sent back to Yamen by the client. Carries the four raw
/// signals (for a human to sanity-check) plus the already-computed hash that <c>issue</c> actually
/// uses — recomputing the hash from the raw fields at issue time would work too, but shipping it
/// pre-computed keeps the two tools from ever disagreeing about the canonicalization rules.
/// </summary>
public sealed record FingerprintReport(
    [property: JsonPropertyName("machineGuid")] string MachineGuid,
    [property: JsonPropertyName("macAddress")] string MacAddress,
    [property: JsonPropertyName("machineName")] string MachineName,
    [property: JsonPropertyName("processorId")] string ProcessorId,
    [property: JsonPropertyName("fingerprintHash")] string FingerprintHash,
    [property: JsonPropertyName("collectedUtc")] DateTime CollectedUtc)
{
    public static FingerprintReport FromCurrentMachine()
    {
        var info = MachineFingerprint.Collect();
        return new FingerprintReport(
            info.MachineGuid,
            info.MacAddress,
            info.MachineName,
            info.ProcessorId,
            MachineFingerprint.ComputeHash(info),
            DateTime.UtcNow);
    }
}
