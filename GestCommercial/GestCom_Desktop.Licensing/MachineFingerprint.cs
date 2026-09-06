using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace GestCom_Desktop.Licensing;

/// <summary>
/// The four raw signals collected on a machine before hashing. Shipped as-is inside the JSON
/// report produced by <c>collect</c> so a human (Yamen) can eyeball what was read.
/// </summary>
public sealed record MachineFingerprintInfo(
    string MachineGuid,
    string MacAddress,
    string MachineName,
    string ProcessorId);

/// <summary>
/// Collects a machine's identity signals and reduces them to one stable hash used to lock a
/// license to a single machine. Every signal individually can drift (a NIC gets replaced, a
/// machine gets renamed) — combining four raises the bar for spoofing without requiring any of
/// them to be perfectly stable on their own; see <see cref="ComputeHash"/> for how it tolerates
/// nothing, by design: any drift means the license stops matching and must be reissued.
/// </summary>
public static class MachineFingerprint
{
    private const string Delimiter = "|";

    private static readonly string[] VirtualAdapterNameMarkers =
    [
        "virtual", "vmware", "vbox", "virtualbox", "hyper-v", "tap-", "tap ", "tunnel", "pseudo", "loopback", "wan miniport",
    ];

    public static MachineFingerprintInfo Collect()
    {
        return new MachineFingerprintInfo(
            MachineGuid: ReadMachineGuid(),
            MacAddress: ReadPrimaryMacAddress(),
            MachineName: Environment.MachineName,
            ProcessorId: ReadProcessorId());
    }

    /// <summary>
    /// SHA-256 hex digest of the four fields, each trimmed and upper-cased for stability, joined
    /// with a fixed delimiter. This is the value embedded in a <see cref="LicensePayload"/> and
    /// compared against at validation time — never the raw fields themselves.
    /// </summary>
    public static string ComputeHash(MachineFingerprintInfo info)
    {
        var canonical = string.Join(
            Delimiter,
            Normalize(info.MachineGuid),
            Normalize(info.MacAddress),
            Normalize(info.MachineName),
            Normalize(info.ProcessorId));

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hashBytes);
    }

    public static string ComputeCurrent() => ComputeHash(Collect());

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static string ReadMachineGuid()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
        var value = key?.GetValue("MachineGuid") as string;

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Impossible de lire MachineGuid dans le registre.");

        return value;
    }

    private static string ReadPrimaryMacAddress()
    {
        var primaryAdapter = NetworkInterface.GetAllNetworkInterfaces()
            .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
            .Where(adapter => adapter.NetworkInterfaceType is NetworkInterfaceType.Ethernet or NetworkInterfaceType.Wireless80211)
            .Where(adapter => !IsVirtualAdapter(adapter))
            .OrderBy(adapter => adapter.Id, StringComparer.Ordinal)
            .FirstOrDefault();

        if (primaryAdapter is null)
            throw new InvalidOperationException("Aucune carte réseau physique active trouvée.");

        return primaryAdapter.GetPhysicalAddress().ToString();
    }

    private static bool IsVirtualAdapter(NetworkInterface adapter)
    {
        var description = $"{adapter.Name} {adapter.Description}".ToLowerInvariant();
        return VirtualAdapterNameMarkers.Any(description.Contains);
    }

    private static string ReadProcessorId()
    {
        using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
        using var results = searcher.Get();

        foreach (ManagementObject processor in results.Cast<ManagementObject>())
        {
            var processorId = processor["ProcessorId"] as string;
            if (!string.IsNullOrWhiteSpace(processorId))
                return processorId;
        }

        throw new InvalidOperationException("Impossible de lire ProcessorId via WMI.");
    }
}
