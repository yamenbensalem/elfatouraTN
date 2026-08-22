using T4C_GestCom_Desktop.Forms.Admin;
using T4C_GestCom_Desktop.Forms.BonsLivraison;
using T4C_GestCom_Desktop.Forms.BonsReception;
using T4C_GestCom_Desktop.Forms.Clients;
using T4C_GestCom_Desktop.Forms.CommandesAchat;
using T4C_GestCom_Desktop.Forms.CommandesVente;
using T4C_GestCom_Desktop.Forms.Devis;
using T4C_GestCom_Desktop.Forms.FacturesClient;
using T4C_GestCom_Desktop.Forms.FacturesFournisseur;
using T4C_GestCom_Desktop.Forms.Fournisseurs;
using T4C_GestCom_Desktop.Forms.Produits;

namespace T4C_GestCom_Desktop.Forms;

public class MainForm : Form
{
    private static readonly TimeSpan LicenseRecheckInterval = TimeSpan.FromMinutes(15);

    private readonly Dictionary<object, Form> _openChildren = [];
    private readonly System.Windows.Forms.Timer _licenseRecheckTimer;

    public MainForm()
    {
        Text = $"T4C GestCom — {AppHost.Session.Login}";
        IsMdiContainer = true;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 700);
        WindowState = FormWindowState.Maximized;

        var menu = new MenuStrip();

        var menuFichier = new ToolStripMenuItem("&Fichier");
        menuFichier.DropDownItems.Add("Quitter", null, (_, _) => Close());

        var menuVentes = new ToolStripMenuItem("&Ventes");
        menuVentes.DropDownItems.Add("Clients", null, (_, _) => ShowSingleton(() => new ClientsListForm()));
        menuVentes.DropDownItems.Add("Devis", null, (_, _) => ShowSingleton(() => new DevisListForm()));
        menuVentes.DropDownItems.Add("Commandes Vente", null, (_, _) => ShowSingleton(() => new CommandesVenteListForm()));
        menuVentes.DropDownItems.Add("Bons de Livraison", null, (_, _) => ShowSingleton(() => new BonsLivraisonListForm()));
        menuVentes.DropDownItems.Add("Factures Client", null, (_, _) => ShowSingleton(() => new FacturesClientListForm()));
        menuVentes.DropDownItems.Add("Avoirs", null, (_, _) => ShowSingleton(() => new FacturesClientListForm(isAvoir: true), key: "Avoirs"));

        var menuAchats = new ToolStripMenuItem("&Achats");
        menuAchats.DropDownItems.Add("Fournisseurs", null, (_, _) => ShowSingleton(() => new FournisseursListForm()));
        menuAchats.DropDownItems.Add("Commandes Achat", null, (_, _) => ShowSingleton(() => new CommandesAchatListForm()));
        menuAchats.DropDownItems.Add("Bons de Réception", null, (_, _) => ShowSingleton(() => new BonsReceptionListForm()));
        menuAchats.DropDownItems.Add("Factures Fournisseur", null, (_, _) => ShowSingleton(() => new FacturesFournisseurListForm()));

        var menuStock = new ToolStripMenuItem("&Stock");
        menuStock.DropDownItems.Add("Produits", null, (_, _) => ShowSingleton(() => new ProduitsListForm()));

        menu.Items.Add(menuFichier);
        menu.Items.Add(menuVentes);
        menu.Items.Add(menuAchats);
        menu.Items.Add(menuStock);

        if (AppHost.Session.IsSuperAdmin || AppHost.Session.Role is "Admin" or "SuperAdmin")
        {
            var menuAdmin = new ToolStripMenuItem("&Admin");
            menuAdmin.DropDownItems.Add("Utilisateurs", null, (_, _) => ShowSingleton(() => new UtilisateursListForm()));
            menu.Items.Add(menuAdmin);
        }

        MainMenuStrip = menu;
        Controls.Add(menu);

        var status = new StatusStrip();
        status.Items.Add(new ToolStripStatusLabel($"Connecté : {AppHost.Session.Login} ({AppHost.Session.Role})"));
        Controls.Add(status);

        // Re-validates the license during the session — catches a license file deleted or
        // tampered with after startup (the one-time check in Program.cs only covers launch time).
        _licenseRecheckTimer = new System.Windows.Forms.Timer { Interval = (int)LicenseRecheckInterval.TotalMilliseconds };
        _licenseRecheckTimer.Tick += (_, _) => RecheckLicense();
        _licenseRecheckTimer.Start();
    }

    private void RecheckLicense()
    {
        var result = LicenseGate.Validate();
        if (result.IsValid)
            return;

        _licenseRecheckTimer.Stop();
        MessageBox.Show(this, LicenseGate.DescribeFailure(result.Status), LicenseGate.DialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _licenseRecheckTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Opens one MDI child per key, re-activating it if already open — mirrors the legacy Accueil.cs
    /// lazy-child pattern. Defaults the key to the form type, but callers that open the same form
    /// class in two different modes (e.g. FacturesClientListForm for both "Factures Client" and
    /// "Avoirs") must pass a distinct explicit key so the two don't collide on one MDI child.
    /// </summary>
    private void ShowSingleton<TForm>(Func<TForm> factory, object? key = null) where TForm : Form
    {
        key ??= typeof(TForm);

        if (_openChildren.TryGetValue(key, out var existing) && !existing.IsDisposed)
        {
            existing.Activate();
            return;
        }

        var child = factory();
        child.MdiParent = this;
        child.FormClosed += (_, _) => _openChildren.Remove(key);
        _openChildren[key] = child;
        child.Show();
    }
}
