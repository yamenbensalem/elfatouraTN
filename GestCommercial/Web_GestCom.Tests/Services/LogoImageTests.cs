using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class LogoImageTests
{
    private static byte[] Fichier(byte[] signature, int taille = 512)
    {
        var contenu = new byte[taille];
        signature.CopyTo(contenu, 0);
        return contenu;
    }

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0];
    private static readonly byte[] Gif = "GIF89a"u8.ToArray();
    private static readonly byte[] Webp = [.. "RIFF"u8, 0, 0, 0, 0, .. "WEBP"u8];

    public static TheoryData<byte[], string> ImagesValides => new()
    {
        { Png, "image/png" }, { Jpeg, "image/jpeg" }, { Gif, "image/gif" }, { Webp, "image/webp" }
    };

    [Theory]
    [MemberData(nameof(ImagesValides))]
    public void TryBuildDataUri_WhenRealImage_ShouldReturnDataUriWithDetectedType(byte[] signature, string typeAttendu)
    {
        // Arrange
        var contenu = Fichier(signature);

        // Act
        var ok = LogoImage.TryBuildDataUri(contenu, out var dataUri, out var erreur);

        // Assert
        Assert.True(ok);
        Assert.Empty(erreur);
        Assert.StartsWith($"data:{typeAttendu};base64,", dataUri);
        Assert.Equal(contenu, Convert.FromBase64String(dataUri[(dataUri.IndexOf(',') + 1)..]));
    }

    [Fact]
    public void TryBuildDataUri_WhenFileIsNotAnImage_ShouldRefuseEvenIfItWouldBeNamedPng()
    {
        // Arrange — un exécutable ou un PDF renommé en logo.png : seul le contenu compte.
        var contenu = Fichier("%PDF-1.7"u8.ToArray());

        // Act
        var ok = LogoImage.TryBuildDataUri(contenu, out var dataUri, out var erreur);

        // Assert
        Assert.False(ok);
        Assert.Empty(dataUri);
        Assert.Contains("n'est pas une image", erreur);
    }

    [Fact]
    public void TryBuildDataUri_WhenSvg_ShouldRefuse()
    {
        // Arrange
        var contenu = "<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>"u8.ToArray();

        // Act
        var ok = LogoImage.TryBuildDataUri(contenu, out _, out _);

        // Assert
        Assert.False(ok);
    }

    [Fact]
    public void TryBuildDataUri_WhenTooBig_ShouldRefuseWithSizeInMessage()
    {
        // Arrange
        var contenu = Fichier(Png, LogoImage.MaxBytes + 1);

        // Act
        var ok = LogoImage.TryBuildDataUri(contenu, out _, out var erreur);

        // Assert
        Assert.False(ok);
        Assert.Contains("200 Ko", erreur);
    }

    [Fact]
    public void TryBuildDataUri_WhenExactlyAtLimit_ShouldAccept()
    {
        // Arrange
        var contenu = Fichier(Png, LogoImage.MaxBytes);

        // Act
        var ok = LogoImage.TryBuildDataUri(contenu, out _, out _);

        // Assert
        Assert.True(ok);
    }

    [Fact]
    public void TryBuildDataUri_WhenEmpty_ShouldRefuse()
    {
        // Act
        var ok = LogoImage.TryBuildDataUri([], out _, out var erreur);

        // Assert
        Assert.False(ok);
        Assert.Contains("vide", erreur);
    }

    // ── Source affichée : logo téléversé > ancien chemin > logo par défaut ──

    [Theory]
    [InlineData("data:image/png;base64,AAAA", "./ancien.png", "data:image/png;base64,AAAA")]
    [InlineData(null, "./images/logo.png", "images/logo.png")]
    [InlineData(null, "~/logo.png", "logo.png")]
    [InlineData(null, null, "logoApp.png")]
    [InlineData("  ", " ", "logoApp.png")]
    public void LogoSource_ShouldPreferUploadedLogoThenLegacyPathThenDefault(string? logoImage, string? pathLogo, string attendu)
    {
        // Arrange
        var fiche = new Entreprise { LogoImage = logoImage, PathLogo = pathLogo };

        // Act + Assert
        Assert.Equal(attendu, fiche.LogoSource);
    }

    // ── Fiche incomplète ──

    [Theory]
    [InlineData("Société A", "1234567A", false)]
    [InlineData("Société A", null, true)]
    [InlineData("Société A", "  ", true)]
    [InlineData("", "1234567A", true)]
    public async Task IsFicheIncompleteAsync_ShouldRequireNameAndMatricule(string nom, string? matricule, bool attendu)
    {
        // Arrange
        await using var db = DbContextFactory.Create();
        db.Entreprises.Add(new Entreprise { CodeEntreprise = "ENT001", NomEntreprise = nom, MatriculeFiscale = matricule, CompanyId = 1 });
        await db.SaveChangesAsync();

        // Act
        var incomplete = await new EntrepriseService(db).IsFicheIncompleteAsync();

        // Assert
        Assert.Equal(attendu, incomplete);
    }

    [Fact]
    public async Task IsFicheIncompleteAsync_WhenNoFicheYet_ShouldBeTrue()
    {
        // Arrange
        await using var db = DbContextFactory.Create();

        // Act + Assert
        Assert.True(await new EntrepriseService(db).IsFicheIncompleteAsync());
    }
}
