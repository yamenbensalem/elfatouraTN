using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Web_GestCom.Components.Pages.Entreprise;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Components.Pages;

public sealed class EntrepriseFormTests : TestContext
{
    private readonly AppDbContext _db = DbContextFactory.Create();

    public EntrepriseFormTests()
    {
        Services.AddSingleton(_db);
        Services.AddSingleton<ITenantService>(new StubTenantService());
    }

    private IRenderedComponent<EntrepriseForm> RenderWithFiche(string nom, string? matricule)
    {
        _db.Entreprises.Add(new Entreprise { CodeEntreprise = "ENT001", NomEntreprise = nom, MatriculeFiscale = matricule, CompanyId = 1 });
        _db.SaveChanges();
        var cut = RenderComponent<EntrepriseForm>();
        cut.WaitForElement("form");
        return cut;
    }

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    [Fact]
    public void Submit_WhenMatriculeFiscalMissing_ShouldShowFieldErrorAndNotSave()
    {
        // Arrange — état d'une nouvelle entreprise : fiche pré-créée à l'activation, sans matricule.
        var cut = RenderWithFiche("Société Exemple", matricule: null);
        cut.Find("textarea").Change("note qui ne doit pas être enregistrée");

        // Act
        cut.Find("form").Submit();

        // Assert
        Assert.Contains("Le matricule fiscal est obligatoire.", cut.Markup);
        Assert.DoesNotContain("Fiche entreprise enregistrée", cut.Markup);
        Assert.Null(_db.Entreprises.AsNoTracking().Single().Note);
    }

    [Fact]
    public void Submit_WhenNameMissing_ShouldShowFieldError()
    {
        // Arrange
        var cut = RenderWithFiche("Société Exemple", "1234567A");
        cut.FindAll("input.form-control")[0].Change("   ");

        // Act
        cut.Find("form").Submit();

        // Assert
        Assert.Contains("Le nom de l'entreprise est obligatoire.", cut.Markup);
        Assert.Equal("Société Exemple", _db.Entreprises.AsNoTracking().Single().NomEntreprise);
    }

    [Fact]
    public void Submit_WhenNameAndMatriculeFilled_ShouldSaveTrimmedValues()
    {
        // Arrange
        var cut = RenderWithFiche("Société Exemple", matricule: null);
        cut.FindAll("input.form-control")[1].Change("  1234567A  ");

        // Act
        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Fiche entreprise enregistrée", cut.Markup));
        Assert.Equal("1234567A", _db.Entreprises.AsNoTracking().Single().MatriculeFiscale);
    }

    [Fact]
    public void Logo_WhenValidImageChosenThenSaved_ShouldBeStoredAsDataUri()
    {
        // Arrange
        var cut = RenderWithFiche("Société Exemple", "1234567A");

        // Act
        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(Png, "logo.png", contentType: "image/png"));
        cut.WaitForAssertion(() => Assert.Contains("data:image/png;base64,", cut.Markup));
        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Fiche entreprise enregistrée", cut.Markup));
        Assert.StartsWith("data:image/png;base64,", _db.Entreprises.AsNoTracking().Single().LogoImage);
    }

    [Fact]
    public void Logo_WhenFileIsNotAnImage_ShouldShowErrorAndKeepCurrentLogo()
    {
        // Arrange
        var cut = RenderWithFiche("Société Exemple", "1234567A");

        // Act — un PDF renommé en .png, annoncé comme image par le navigateur.
        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromBinary("%PDF-1.7 contenu"u8.ToArray(), "logo.png", contentType: "image/png"));

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("n'est pas une image reconnue", cut.Markup));
        Assert.DoesNotContain("data:image", cut.Markup);
    }

    [Fact]
    public void Logo_WhenTooBig_ShouldShowSizeError()
    {
        // Arrange
        var cut = RenderWithFiche("Société Exemple", "1234567A");
        var gros = new byte[LogoImage.MaxBytes + 1];
        Png.CopyTo(gros, 0);

        // Act
        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(gros, "logo.png", contentType: "image/png"));

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("dépasse la taille maximale de 200 Ko", cut.Markup));
        Assert.DoesNotContain("data:image", cut.Markup);
    }

    [Fact]
    public void Logo_WhenRemovedThenSaved_ShouldClearUploadedLogoAndLegacyPath()
    {
        // Arrange
        _db.Entreprises.Add(new Entreprise
        {
            CodeEntreprise = "ENT001", NomEntreprise = "Société Exemple", MatriculeFiscale = "1234567A", CompanyId = 1,
            LogoImage = "data:image/png;base64,AAAA", PathLogo = "./ancien.png"
        });
        _db.SaveChanges();
        var cut = RenderComponent<EntrepriseForm>();
        cut.WaitForElement("form");

        // Act
        cut.FindAll("button").Single(b => b.TextContent.Contains("Retirer le logo")).Click();
        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Fiche entreprise enregistrée", cut.Markup));
        var fiche = _db.Entreprises.AsNoTracking().Single();
        Assert.Null(fiche.LogoImage);
        Assert.Null(fiche.PathLogo);
    }
}
