using T4C_GestCom_Desktop.Forms;

namespace T4C_GestCom_Desktop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Gate on license validity BEFORE AppHost.Initialize() — an invalid/missing/tampered
        // license must never reach the database or show the login form.
        var licenseResult = LicenseGate.Validate();
        if (!licenseResult.IsValid)
        {
            MessageBox.Show(LicenseGate.DescribeFailure(licenseResult.Status), LicenseGate.DialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        AppHost.Initialize();

        using var login = new LoginForm();
        if (login.ShowDialog() == DialogResult.OK)
        {
            Application.Run(new MainForm());
        }
    }
}
