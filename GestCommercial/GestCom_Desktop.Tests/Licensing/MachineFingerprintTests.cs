using GestCom_Desktop.Licensing;
using Xunit;

namespace GestCom_Desktop.Tests.Licensing;

public class MachineFingerprintTests
{
    [Fact]
    public void ComputeHash_CalledTwiceWithSameInfo_ReturnsSameHash()
    {
        // Arrange
        var info = new MachineFingerprintInfo("guid-1234", "AA-BB-CC-DD-EE-FF", "POSTE-CLIENT", "cpu-id-9876");

        // Act
        var firstHash = MachineFingerprint.ComputeHash(info);
        var secondHash = MachineFingerprint.ComputeHash(info);

        // Assert
        Assert.Equal(firstHash, secondHash);
    }

    [Fact]
    public void ComputeHash_FieldsDifferOnlyByCaseAndWhitespace_ReturnsSameHash()
    {
        // Arrange
        var canonicalInfo = new MachineFingerprintInfo("guid-1234", "AA-BB-CC-DD-EE-FF", "POSTE-CLIENT", "cpu-id-9876");
        var messyInfo = new MachineFingerprintInfo("  Guid-1234 ", "aa-bb-cc-dd-ee-ff", " poste-client", "CPU-ID-9876  ");

        // Act
        var canonicalHash = MachineFingerprint.ComputeHash(canonicalInfo);
        var messyHash = MachineFingerprint.ComputeHash(messyInfo);

        // Assert
        Assert.Equal(canonicalHash, messyHash);
    }

    [Fact]
    public void ComputeHash_DifferentInfo_ReturnsDifferentHash()
    {
        // Arrange
        var infoA = new MachineFingerprintInfo("guid-1234", "AA-BB-CC-DD-EE-FF", "POSTE-CLIENT", "cpu-id-9876");
        var infoB = new MachineFingerprintInfo("guid-5678", "AA-BB-CC-DD-EE-FF", "POSTE-CLIENT", "cpu-id-9876");

        // Act
        var hashA = MachineFingerprint.ComputeHash(infoA);
        var hashB = MachineFingerprint.ComputeHash(infoB);

        // Assert
        Assert.NotEqual(hashA, hashB);
    }

    [Fact]
    public void ComputeCurrent_CalledTwice_ReturnsSameHash()
    {
        // Arrange & Act — reads the real machine's registry/NIC/WMI signals twice in a row
        var firstHash = MachineFingerprint.ComputeCurrent();
        var secondHash = MachineFingerprint.ComputeCurrent();

        // Assert
        Assert.Equal(firstHash, secondHash);
    }
}
