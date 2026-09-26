using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Forms.Shared;
using Web_GestCom.Data;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Entreprise;

/// <summary>Desktop equivalent of Components/Pages/Entreprise/EntrepriseForm.razor.</summary>
public class EntrepriseForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<EntrepriseForm>();

    private const string DefaultCode = "ENT001";

    private readonly TextBox _txtNom = new() { Left = 170, Top = 20, Width = 300 };
    private readonly TextBox _txtMatriculeFiscale = new() { Left = 170, Top = 50, Width = 200 };
    private readonly TextBox _txtAdresse = new() { Left = 170, Top = 80, Width = 300 };
    private readonly TextBox _txtCodePostal = new() { Left = 170, Top = 110, Width = 100 };
    private readonly TextBox _txtVille = new() { Left = 170, Top = 140, Width = 200 };
    private readonly TextBox _txtPays = new() { Left = 170, Top = 170, Width = 200 };
    private readonly TextBox _txtTel = new() { Left = 170, Top = 200, Width = 150 };
    private readonly TextBox _txtFax = new() { Left = 170, Top = 230, Width = 150 };
    private readonly TextBox _txtEmail = new() { Left = 170, Top = 260, Width = 250 };
    private readonly TextBox _txtSite = new() { Left = 170, Top = 290, Width = 250 };
    private readonly TextBox _txtRib = new() { Left = 170, Top = 320, Width = 250 };
    private readonly TextBox _txtPathLogo = new() { Left = 170, Top = 350, Width = 300, PlaceholderText = "./logoApp.png" };
    private readonly TextBox _txtNote = new() { Left = 170, Top = 380, Width = 300, Height = 50, Multiline = true };

    private readonly Label _lblError = new() { Left = 20, Top = 445, Width = 460, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnSave = new() { Left = 170, Top = 475, Width = 120, Text = "Enregistrer" };

    private string _codeEntreprise = DefaultCode;
    private bool _exists;

    public EntrepriseForm()
    {
        Text = "Fiche Entreprise";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(500, 520);
        AcceptButton = _btnSave;

        AddField("Nom *", _txtNom, 20);
        AddField("Matricule Fiscale", _txtMatriculeFiscale, 50);
        AddField("Adresse", _txtAdresse, 80);
        AddField("Code Postal", _txtCodePostal, 110);
        AddField("Ville", _txtVille, 140);
        AddField("Pays", _txtPays, 170);
        AddField("Téléphone", _txtTel, 200);
        AddField("Fax", _txtFax, 230);
        AddField("Email", _txtEmail, 260);
        AddField("Site Web", _txtSite, 290);
        AddField("RIB", _txtRib, 320);
        AddField("Chemin du Logo", _txtPathLogo, 350);
        AddField("Note", _txtNote, 380);

        Controls.Add(_lblError);
        Controls.Add(_btnSave);

        _btnSave.Click += async (_, _) => await SaveAsync();

        Load += async (_, _) => await LoadAsync();
    }

    private void AddField(string label, Control input, int top)
    {
        Controls.Add(new Label { Left = 20, Top = top + 3, Width = 140, Text = label });
        Controls.Add(input);
    }

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existing = await db.Entreprises.AsNoTracking().FirstOrDefaultAsync();
        _exists = existing is not null;
        _codeEntreprise = existing?.CodeEntreprise ?? DefaultCode;

        if (existing is not null)
            Populate(existing);
    }

    private void Populate(Web_GestCom.Data.Models.Entreprise e)
    {
        _txtNom.Text = e.NomEntreprise;
        _txtMatriculeFiscale.Text = e.MatriculeFiscale;
        _txtAdresse.Text = e.Adresse;
        _txtCodePostal.Text = e.CodePostal;
        _txtVille.Text = e.Ville;
        _txtPays.Text = e.Pays;
        _txtTel.Text = e.Tel;
        _txtFax.Text = e.Fax;
        _txtEmail.Text = e.Email;
        _txtSite.Text = e.Site;
        _txtRib.Text = e.Rib;
        _txtPathLogo.Text = e.PathLogo;
        _txtNote.Text = e.Note;
    }

    private async Task SaveAsync()
    {
        _lblError.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(_txtNom.Text))
        {
            _lblError.Text = "Le nom est obligatoire.";
            return;
        }

        _btnSave.Enabled = false;
        try
        {
            Logger.DebugSaving("entreprise", _codeEntreprise, !_exists);
            using var scope = AppHost.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var entreprise = new Web_GestCom.Data.Models.Entreprise
            {
                CodeEntreprise = _codeEntreprise,
                NomEntreprise = _txtNom.Text.Trim(),
                MatriculeFiscale = string.IsNullOrWhiteSpace(_txtMatriculeFiscale.Text) ? null : _txtMatriculeFiscale.Text.Trim(),
                Adresse = string.IsNullOrWhiteSpace(_txtAdresse.Text) ? null : _txtAdresse.Text.Trim(),
                CodePostal = string.IsNullOrWhiteSpace(_txtCodePostal.Text) ? null : _txtCodePostal.Text.Trim(),
                Ville = string.IsNullOrWhiteSpace(_txtVille.Text) ? null : _txtVille.Text.Trim(),
                Pays = string.IsNullOrWhiteSpace(_txtPays.Text) ? null : _txtPays.Text.Trim(),
                Tel = string.IsNullOrWhiteSpace(_txtTel.Text) ? null : _txtTel.Text.Trim(),
                Fax = string.IsNullOrWhiteSpace(_txtFax.Text) ? null : _txtFax.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim(),
                Site = string.IsNullOrWhiteSpace(_txtSite.Text) ? null : _txtSite.Text.Trim(),
                Rib = string.IsNullOrWhiteSpace(_txtRib.Text) ? null : _txtRib.Text.Trim(),
                PathLogo = string.IsNullOrWhiteSpace(_txtPathLogo.Text) ? null : _txtPathLogo.Text.Trim(),
                Note = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim(),
            };

            if (_exists)
                db.Entreprises.Update(entreprise);
            else
                db.Entreprises.Add(entreprise);
            await db.SaveChangesGuardedAsync();

            _exists = true;
            Logger.DebugSaved("Entreprise", _codeEntreprise);
            MessageBox.Show(this, "Fiche entreprise enregistrée.", "Entreprise", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Logger.ErrorSaveFailed(ex, "entreprise", _codeEntreprise);
            _lblError.Text = $"Erreur : {ex.Message}";
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }
}
