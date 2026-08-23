using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Forms.Fournisseurs;

/// <summary>Desktop equivalent of Components/Pages/Fournisseurs/FournisseurForm.razor.</summary>
public class FournisseurEditForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<FournisseurEditForm>();

    private readonly string? _code;
    private readonly bool _isNew;

    private readonly TextBox _txtCode = new() { Left = 150, Top = 20, Width = 150 };
    private readonly TextBox _txtNom = new() { Left = 150, Top = 50, Width = 300 };
    private readonly TextBox _txtMatriculeFiscale = new() { Left = 150, Top = 80, Width = 200 };
    private readonly ComboBox _cmbDevise = new() { Left = 150, Top = 110, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomDevise", ValueMember = "CodeDevise" };
    private readonly ComboBox _cmbEtat = new() { Left = 150, Top = 140, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };

    private readonly TextBox _txtAdresse = new() { Left = 150, Top = 170, Width = 300 };
    private readonly TextBox _txtCodePostal = new() { Left = 150, Top = 200, Width = 100 };
    private readonly TextBox _txtVille = new() { Left = 150, Top = 230, Width = 200 };
    private readonly TextBox _txtPays = new() { Left = 150, Top = 260, Width = 200 };
    private readonly TextBox _txtTel = new() { Left = 150, Top = 290, Width = 150 };
    private readonly TextBox _txtMobile = new() { Left = 150, Top = 320, Width = 150 };
    private readonly TextBox _txtFax = new() { Left = 150, Top = 350, Width = 150 };
    private readonly TextBox _txtEmail = new() { Left = 150, Top = 380, Width = 250 };

    private readonly TextBox _txtRib = new() { Left = 150, Top = 410, Width = 250 };
    private readonly TextBox _txtNote = new() { Left = 150, Top = 440, Width = 300, Height = 50, Multiline = true };

    private readonly Label _lblError = new() { Left = 20, Top = 500, Width = 460, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnSave = new() { Left = 150, Top = 530, Width = 100, Text = "Enregistrer" };
    private readonly Button _btnCancel = new() { Left = 260, Top = 530, Width = 100, Text = "Annuler" };

    public FournisseurEditForm(string? code)
    {
        _code = code;
        _isNew = code is null;

        Text = _isNew ? "Nouveau Fournisseur" : $"Fournisseur : {code}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 580);
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        AddField("Code Fournisseur", _txtCode, 20);
        AddField("Nom *", _txtNom, 50);
        AddField("Matricule Fiscale", _txtMatriculeFiscale, 80);
        AddField("Devise", _cmbDevise, 110);
        AddField("État", _cmbEtat, 140);
        AddField("Adresse", _txtAdresse, 170);
        AddField("Code Postal", _txtCodePostal, 200);
        AddField("Ville", _txtVille, 230);
        AddField("Pays", _txtPays, 260);
        AddField("Téléphone", _txtTel, 290);
        AddField("Mobile", _txtMobile, 320);
        AddField("Fax", _txtFax, 350);
        AddField("Email", _txtEmail, 380);
        AddField("RIB", _txtRib, 410);
        AddField("Note", _txtNote, 440);

        Controls.Add(_lblError);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);

        _txtCode.Enabled = _isNew;
        _txtCode.PlaceholderText = "Auto-généré";

        _cmbEtat.Items.AddRange(["Actif", "Inactif"]);

        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Load += async (_, _) => await LoadAsync();
    }

    private void AddField(string label, Control input, int top)
    {
        Controls.Add(new Label { Left = 20, Top = top + 3, Width = 120, Text = label });
        Controls.Add(input);
    }

    private async Task LoadAsync()
    {
        using var scope = AppHost.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _cmbDevise.DataSource = await db.Devises.ToListAsync();

        if (!_isNew)
        {
            var fournisseurService = scope.ServiceProvider.GetRequiredService<IFournisseurService>();
            var fournisseur = await fournisseurService.GetByCodeAsync(_code!);
            if (fournisseur is null)
            {
                Logger.Warning("Fournisseur {Code} introuvable à l'ouverture de l'éditeur.", _code);
                MessageBox.Show(this, "Fournisseur introuvable.", "Fournisseur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            Populate(fournisseur);
        }
        else
        {
            _cmbEtat.SelectedItem = "Actif";
        }
    }

    private void Populate(Fournisseur f)
    {
        _txtCode.Text = f.CodeFournisseur;
        _txtNom.Text = f.NomFournisseur;
        _txtMatriculeFiscale.Text = f.MatriculeFiscale;
        _cmbDevise.SelectedValue = f.CodeDevise;
        _cmbEtat.SelectedItem = f.EtatFournisseur;
        _txtAdresse.Text = f.Adresse;
        _txtCodePostal.Text = f.CodePostal;
        _txtVille.Text = f.Ville;
        _txtPays.Text = f.Pays;
        _txtTel.Text = f.Tel;
        _txtMobile.Text = f.TelMobile;
        _txtFax.Text = f.Fax;
        _txtEmail.Text = f.Email;
        _txtRib.Text = f.Rib;
        _txtNote.Text = f.Note;
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
            Logger.Debug("Enregistrement du fournisseur {Code} (nouveau={IsNew}).", _code, _isNew);
            using var scope = AppHost.CreateScope();
            var fournisseurService = scope.ServiceProvider.GetRequiredService<IFournisseurService>();

            // UpdateAsync does db.Fournisseurs.Update(fournisseur) — a full entity replace. For an
            // existing fournisseur this must be the loaded entity, mutated, not a freshly built one:
            // a fresh object would carry CompanyId = null (not exposed in this form) and either fail
            // tenant-ownership validation or, for a SuperAdmin session where that check is skipped,
            // silently orphan the fournisseur from its tenant. Mirrors FournisseurForm.razor, which
            // binds _fournisseur = the loaded entity.
            var fournisseur = _isNew ? new Fournisseur() : await fournisseurService.GetByCodeAsync(_code!)
                ?? throw new InvalidOperationException("Fournisseur introuvable.");

            fournisseur.CodeFournisseur = _txtCode.Text.Trim();
            fournisseur.NomFournisseur = _txtNom.Text.Trim();
            fournisseur.MatriculeFiscale = string.IsNullOrWhiteSpace(_txtMatriculeFiscale.Text) ? null : _txtMatriculeFiscale.Text.Trim();
            fournisseur.CodeDevise = (int)(_cmbDevise.SelectedValue ?? 1);
            fournisseur.EtatFournisseur = (string)(_cmbEtat.SelectedItem ?? "Actif");
            fournisseur.Adresse = string.IsNullOrWhiteSpace(_txtAdresse.Text) ? null : _txtAdresse.Text.Trim();
            fournisseur.CodePostal = string.IsNullOrWhiteSpace(_txtCodePostal.Text) ? null : _txtCodePostal.Text.Trim();
            fournisseur.Ville = string.IsNullOrWhiteSpace(_txtVille.Text) ? null : _txtVille.Text.Trim();
            fournisseur.Pays = string.IsNullOrWhiteSpace(_txtPays.Text) ? null : _txtPays.Text.Trim();
            fournisseur.Tel = string.IsNullOrWhiteSpace(_txtTel.Text) ? null : _txtTel.Text.Trim();
            fournisseur.TelMobile = string.IsNullOrWhiteSpace(_txtMobile.Text) ? null : _txtMobile.Text.Trim();
            fournisseur.Fax = string.IsNullOrWhiteSpace(_txtFax.Text) ? null : _txtFax.Text.Trim();
            fournisseur.Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim();
            fournisseur.Rib = string.IsNullOrWhiteSpace(_txtRib.Text) ? null : _txtRib.Text.Trim();
            fournisseur.Note = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim();

            if (_isNew)
                await fournisseurService.AddAsync(fournisseur);
            else
                await fournisseurService.UpdateAsync(fournisseur);

            Logger.Debug("Fournisseur {Code} enregistré.", fournisseur.CodeFournisseur);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Échec de l'enregistrement du fournisseur {Code}.", _code);
            _lblError.Text = $"Erreur : {ex.Message}";
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }
}
