using T4C_GestCom_Desktop.Forms;

namespace T4C_GestCom_Desktop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        AppHost.Initialize();

        using var login = new LoginForm();
        if (login.ShowDialog() == DialogResult.OK)
        {
            Application.Run(new MainForm());
        }
    }
}
