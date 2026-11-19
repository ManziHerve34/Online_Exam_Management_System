using System;
using System.Windows.Forms;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.UI.Forms;

namespace OnlineExamsSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Test and ensure XAMPP MySQL database connectivity
            if (!DatabaseHelper.TestConnection(out string dbError))
            {
                var retryResult = MessageBox.Show(
                    "Warning: Could not connect to MySQL server in XAMPP.\n\n" +
                    $"Error Details: {dbError}\n\n" +
                    "Please ensure that XAMPP Control Panel is open and the MySQL service is started (port 3306).\n\n" +
                    "Would you like to proceed anyway?",
                    "Database Connectivity Check",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (retryResult != DialogResult.Yes)
                {
                    return;
                }
            }
            else
            {
                // Verify schema & initial tables exist
                DatabaseHelper.EnsureDatabaseAndTables();
            }

            // Launch Authentication
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new MainForm());
                }
            }
        }
    }
}
