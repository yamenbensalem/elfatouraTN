using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Forms.Shared;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;

namespace GestCom_Desktop.Forms.Clients;

/// <summary>Desktop equivalent of Components/Pages/Clients/ClientForm.razor.</summary>
public class ClientEditForm : Form
{
    private static readonly ILogger Logger = Log.ForContext<ClientEditForm>();

    private readonly string? _code;
    private readonly bool _isNew;

    private readonly TextBox _txtCode = new() { Left = 150, Top = 20, Width = 150 };
    private readonly TextBox _txtNom = new() { Left = 150, Top = 50, Width = 300 };
    private readonly TextBox _txtMatriculeFiscale = new() { Left = 150, Top = 80, Width = 200 };
    private readonly ComboBox _cmbTypePersonne = new() { Left = 150, Top = 110, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbDevise = new() { Left = 150, Top = 140, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "NomDevise", ValueMember = "CodeDevise" };
    private readonly ComboBox _cmbEtranger = new() { Left = 150, Top = 170, Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbExonore = new() { Left = 150, Top = 200, Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };

    private readonly TextBox _txtAdresse = new() { Left = 150, Top = 230, Width = 300 };
    private readonly TextBox _txtCodePostal = new() { Left = 150, Top = 260, Width = 100 };
    private readonly TextBox _txtVille = new() { Left = 150, Top = 290, Width = 200 };
    private readonly TextBox _txtPays = new() { Left = 150, Top = 320, Width = 200 };
    private readonly TextBox _txtTel = new() { Left = 150, Top = 350, Width = 150 };
    private readonly TextBox _txtMobile = new() { Left = 150, Top = 380, Width = 150 };
    private readonly TextBox _txtFax = new() { Left = 150, Top = 410, Width = 150 };
    private readonly TextBox _txtEmail = new() { Left = 150, Top = 440, Width = 250 };

    private readonly NumericUpDown _numMaxCredit = new() { Left = 150, Top = 470, Width = 120, DecimalPlaces = 3, Maximum = 999_999_999 };
    private readonly TextBox _txtRib = new() { Left = 150, Top = 500, Width = 250 };
    private readonly TextBox _txtResponsable = new() { Left = 150, Top = 530, Width = 200 };
    private readonly ComboBox _cmbEtat = new() { Left = 150, Top = 560, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtNote = new() { Left = 150, Top = 590, Width = 300, Height = 50, Multiline = true };

    private readonly Label _lblError = new() { Left = 20, Top = 650, Width = 460, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _btnSave = new() { Left = 150, Top = 680, Width = 100, Text = "Enregistrer" };
    private readonly Button _btnCancel = new() { Left = 260, Top = 680, Width = 100, Text = "Annuler" };

    public ClientEditForm(string? code)
    {
        _code = code;
        _isNew = code is null;

        Text = _isNew ? "Nouveau Client" : $"Client : {code}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 730);
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        AddField("Code Client", _txtCode, 20);
        AddField("Nom *", _txtNom, 50);
        AddField("Matricule Fiscale", _txtMatriculeFiscale, 80);
        AddField("Type Personne", _cmbTypePersonne, 110);
        AddField("Devise", _cmbDevise, 140);
        AddField("Étranger", _cmbEtranger, 170);
        AddField("Exonéré TVA", _cmbExonore, 200);
        AddField("Adresse", _txtAdresse, 230);
        AddField("Code Postal", _txtCodePostal, 260);
        AddField("Ville", _txtVille, 290);
        AddField("Pays", _txtPays, 320);
        AddField("Téléphone", _txtTel, 350);
        AddField("Mobile", _txtMobile, 380);
        AddField("Fax", _txtFax, 410);
        AddField("Email", _txtEmail, 440);
        AddField("Crédit Maximum", _numMaxCredit, 470);
        AddField("RIB", _txtRib, 500);
        AddField("Responsable", _txtResponsable, 530);
        AddField("État", _cmbEtat, 560);
        AddField("Note", _txtNote, 590);

        Controls.Add(_lblError);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);

        _txtCode.Enabled = _isNew;
        _txtCode.PlaceholderText = "Auto-généré";

        _cmbTypePersonne.Items.AddRange(["Physique", "Morale"]);
        _cmbEtranger.Items.AddRange(["NON", "OUI"]);
        _cmbExonore.Items.AddRange(["NON", "OUI"]);
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

        var devises = await db.Devises.ToListAsync();
        _cmbDevise.DataSource = devises;

        if (!_isNew)
        {
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();
            var client = await clientService.GetByCodeAsync(_code!);
            if (client is null)
            {
                Logger.WarningNotFound("Client", _code);
                MessageBox.Show(this, "Client introuvable.", "Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            Populate(client);
        }
        else
        {
            _cmbTypePersonne.SelectedItem = "Physique";
            _cmbEtranger.SelectedItem = "NON";
            _cmbExonore.SelectedItem = "NON";
            _cmbEtat.SelectedItem = "Actif";
        }
    }

    private void Populate(Client c)
    {
        _txtCode.Text = c.CodeClient;
        _txtNom.Text = c.NomClient;
        _txtMatriculeFiscale.Text = c.MatriculeFiscale;
        _cmbTypePersonne.SelectedItem = c.TypePersonne;
        _cmbDevise.SelectedValue = c.CodeDevise;
        _cmbEtranger.SelectedItem = c.Etranger;
        _cmbExonore.SelectedItem = c.Exonore;
        _txtAdresse.Text = c.Adresse;
        _txtCodePostal.Text = c.CodePostal;
        _txtVille.Text = c.Ville;
        _txtPays.Text = c.Pays;
        _txtTel.Text = c.Tel;
        _txtMobile.Text = c.TelMobile;
        _txtFax.Text = c.Fax;
        _txtEmail.Text = c.Email;
        _numMaxCredit.Value = (decimal)c.MaxCredit;
        _txtRib.Text = c.Rib;
        _txtResponsable.Text = c.Responsable;
        _cmbEtat.SelectedItem = c.EtatClient;
        _txtNote.Text = c.Note;
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
            Logger.DebugSaving("client", _code, _isNew);
            using var scope = AppHost.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            // UpdateAsync does db.Clients.Update(client) — a full entity replace. For an existing
            // client this must be the loaded entity, mutated, not a freshly built one: a fresh object
            // would carry CompanyId = null (not exposed in this form) and either fail tenant-ownership
            // validation or, for a SuperAdmin session where that check is skipped, silently orphan the
            // client from its tenant. Mirrors ClientForm.razor, which binds _client = the loaded entity.
            var client = _isNew ? new Client() : await clientService.GetByCodeAsync(_code!)
                ?? throw new InvalidOperationException("Client introuvable.");

            client.CodeClient = _txtCode.Text.Trim();
            client.NomClient = _txtNom.Text.Trim();
            client.MatriculeFiscale = string.IsNullOrWhiteSpace(_txtMatriculeFiscale.Text) ? null : _txtMatriculeFiscale.Text.Trim();
            client.TypePersonne = (string)(_cmbTypePersonne.SelectedItem ?? "Physique");
            client.CodeDevise = (int)(_cmbDevise.SelectedValue ?? 1);
            client.Etranger = (string)(_cmbEtranger.SelectedItem ?? "NON");
            client.Exonore = (string)(_cmbExonore.SelectedItem ?? "NON");
            client.Adresse = string.IsNullOrWhiteSpace(_txtAdresse.Text) ? null : _txtAdresse.Text.Trim();
            client.CodePostal = string.IsNullOrWhiteSpace(_txtCodePostal.Text) ? null : _txtCodePostal.Text.Trim();
            client.Ville = string.IsNullOrWhiteSpace(_txtVille.Text) ? null : _txtVille.Text.Trim();
            client.Pays = string.IsNullOrWhiteSpace(_txtPays.Text) ? null : _txtPays.Text.Trim();
            client.Tel = string.IsNullOrWhiteSpace(_txtTel.Text) ? null : _txtTel.Text.Trim();
            client.TelMobile = string.IsNullOrWhiteSpace(_txtMobile.Text) ? null : _txtMobile.Text.Trim();
            client.Fax = string.IsNullOrWhiteSpace(_txtFax.Text) ? null : _txtFax.Text.Trim();
            client.Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim();
            client.MaxCredit = (double)_numMaxCredit.Value;
            client.Rib = string.IsNullOrWhiteSpace(_txtRib.Text) ? null : _txtRib.Text.Trim();
            client.Responsable = string.IsNullOrWhiteSpace(_txtResponsable.Text) ? null : _txtResponsable.Text.Trim();
            client.EtatClient = (string)(_cmbEtat.SelectedItem ?? "Actif");
            client.Note = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim();

            if (_isNew)
                await clientService.AddAsync(client);
            else
                await clientService.UpdateAsync(client);

            Logger.DebugSaved("Client", client.CodeClient);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Logger.ErrorSaveFailed(ex, "client", _code);
            _lblError.Text = $"Erreur : {ex.Message}";
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }
}
