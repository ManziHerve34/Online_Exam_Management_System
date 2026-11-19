using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtEmail;
        private TextBox txtPassword;
        private CheckBox chkShowPassword;
        private Button btnLogin;
        private LinkLabel lnkForgotPassword;
        private LinkLabel lnkRegister;
        private Label lblError;
        private Label lblDbStatus;

        private readonly AuthService authService = new AuthService();

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Login - Online Professional Exams Scheduling and Management System";
            this.Size = new Size(980, 640);
            this.MinimumSize = new Size(920, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            // Root 2-column TableLayoutPanel ensures left and right panels CANNOT overlap
            var rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 410F)); // Left sidebar
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));    // Right login form
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // ==========================================
            // LEFT PANEL (Brand, Info, Quick Accounts)
            // ==========================================
            var pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.SidebarDark,
                Padding = new Padding(28, 30, 28, 30),
                Margin = new Padding(0)
            };

            var lblBadge = new Label
            {
                Text = "AUCA • DOTNET GROUP A",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Primary,
                BackColor = Color.FromArgb(20, 30, 50),
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(28, 25),
                AutoSize = true
            };

            var lblBrandTitle = new Label
            {
                Text = "ONLINE PROFESSIONAL\nEXAMS SYSTEM",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(26, 60),
                AutoSize = true
            };

            var lblBrandSub = new Label
            {
                Text = "Certification Scheduling & Administrative Portal",
                Font = UiTheme.BodyFont,
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(28, 118),
                AutoSize = true
            };

            // Project Specification Card
            var pnlDevInfo = new Panel
            {
                Location = new Point(28, 155),
                Size = new Size(354, 130),
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(16)
            };
            pnlDevInfo.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(51, 65, 85), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlDevInfo.Width - 1, pnlDevInfo.Height - 1);
                }
            };

            var lblDevTitle = new Label
            {
                Text = "PROJECT INFORMATION",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Primary,
                Location = new Point(14, 12),
                AutoSize = true
            };

            var lblDevDetails = new Label
            {
                Text = "Developer:  Manzi Herve\n" +
                       "Student ID: 24605\n" +
                       "Course:     DotNet (Group A)\n" +
                       "Database:   XAMPP MySQL (Port 3306)",
                Font = new Font("Consolas", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(226, 232, 240),
                Location = new Point(14, 34),
                AutoSize = true
            };
            pnlDevInfo.Controls.Add(lblDevTitle);
            pnlDevInfo.Controls.Add(lblDevDetails);

            // Quick Demo Credential Section
            var lblQuick = new Label
            {
                Text = "QUICK LOGIN TEST ACCOUNTS:",
                Font = UiTheme.SmallFont,
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(28, 305),
                AutoSize = true
            };

            var btnQuickAdmin = new Button
            {
                Text = "👑 System Admin (admin@exams.com)",
                Location = new Point(28, 330),
                Width = 354,
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            UiTheme.StyleButton(btnQuickAdmin, isPrimary: false);
            btnQuickAdmin.BackColor = Color.FromArgb(30, 41, 59);
            btnQuickAdmin.ForeColor = Color.White;
            btnQuickAdmin.Click += (s, e) => FillCredentials("admin@exams.com", "Admin@123");

            var btnQuickStaff = new Button
            {
                Text = "👔 Admin Staff (staff@exams.com)",
                Location = new Point(28, 374),
                Width = 354,
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            UiTheme.StyleButton(btnQuickStaff, isPrimary: false);
            btnQuickStaff.BackColor = Color.FromArgb(30, 41, 59);
            btnQuickStaff.ForeColor = Color.White;
            btnQuickStaff.Click += (s, e) => FillCredentials("staff@exams.com", "Staff@123");

            var btnQuickCandidate = new Button
            {
                Text = "🎓 Candidate (manzi.herve@auca.ac.rw)",
                Location = new Point(28, 418),
                Width = 354,
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            UiTheme.StyleButton(btnQuickCandidate, isPrimary: false);
            btnQuickCandidate.BackColor = Color.FromArgb(30, 41, 59);
            btnQuickCandidate.ForeColor = Color.White;
            btnQuickCandidate.Click += (s, e) => FillCredentials("manzi.herve@auca.ac.rw", "Herve@123");

            // Feature bullet tags
            var lblFeatures = new Label
            {
                Text = "✓ SHA-256 Encrypted   ✓ RBAC Access   ✓ Real-Time Capacity",
                Font = UiTheme.SmallFont,
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(28, 480),
                AutoSize = true
            };

            pnlLeft.Controls.Add(lblBadge);
            pnlLeft.Controls.Add(lblBrandTitle);
            pnlLeft.Controls.Add(lblBrandSub);
            pnlLeft.Controls.Add(pnlDevInfo);
            pnlLeft.Controls.Add(lblQuick);
            pnlLeft.Controls.Add(btnQuickAdmin);
            pnlLeft.Controls.Add(btnQuickStaff);
            pnlLeft.Controls.Add(btnQuickCandidate);
            pnlLeft.Controls.Add(lblFeatures);

            rootLayout.Controls.Add(pnlLeft, 0, 0);

            // ==========================================
            // RIGHT PANEL (Sign In Form)
            // ==========================================
            var pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40, 20, 40, 20),
                Margin = new Padding(0)
            };

            // Centered card container for the inputs
            var cardContainer = new Panel
            {
                Width = 440,
                Height = 520,
                BackColor = Color.White
            };

            // Center card container inside right panel
            pnlRight.Resize += (s, e) =>
            {
                cardContainer.Left = Math.Max(20, (pnlRight.ClientSize.Width - cardContainer.Width) / 2);
                cardContainer.Top = Math.Max(10, (pnlRight.ClientSize.Height - cardContainer.Height) / 2);
            };

            var lblSignIn = new Label
            {
                Text = "Sign In to Your Account",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = UiTheme.TextDark,
                Location = new Point(0, 15),
                AutoSize = true
            };

            var lblSignInSub = new Label
            {
                Text = "Enter your registered credentials to access your exam dashboard.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                Location = new Point(0, 55),
                AutoSize = true
            };

            lblError = new Label
            {
                Text = "",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.Danger,
                BackColor = Color.FromArgb(254, 242, 242),
                Location = new Point(0, 85),
                Size = new Size(440, 32),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 8, 0),
                Visible = false
            };

            // Email Label & Input
            var lblEmail = new Label
            {
                Text = "Email Address",
                Font = UiTheme.BodyBold,
                ForeColor = UiTheme.TextDark,
                Location = new Point(0, 125),
                AutoSize = true
            };

            txtEmail = new TextBox
            {
                Location = new Point(0, 150),
                Width = 440,
                Font = new Font("Segoe UI", 11F),
                Text = "manzi.herve@auca.ac.rw"
            };

            // Password Label & Input
            var lblPassword = new Label
            {
                Text = "Password",
                Font = UiTheme.BodyBold,
                ForeColor = UiTheme.TextDark,
                Location = new Point(0, 200),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(0, 225),
                Width = 440,
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true,
                Text = "Herve@123"
            };
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };

            // Options Row (Show Password + Forgot Password)
            chkShowPassword = new CheckBox
            {
                Text = "Show Password",
                Location = new Point(0, 265),
                AutoSize = true,
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.TextMuted
            };
            chkShowPassword.CheckedChanged += (s, e) => txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;

            lnkForgotPassword = new LinkLabel
            {
                Text = "Forgot Password? (FR-1.4)",
                Location = new Point(265, 265),
                AutoSize = true,
                Font = UiTheme.SmallFont,
                LinkColor = UiTheme.Primary
            };
            lnkForgotPassword.LinkClicked += (s, e) =>
            {
                var forgotDlg = new ForgotPasswordForm(txtEmail.Text.Trim());
                forgotDlg.ShowDialog(this);
            };

            // Sign In Button
            btnLogin = new Button
            {
                Text = "Sign In  ➔",
                Location = new Point(0, 305),
                Width = 440,
                Height = 44,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            UiTheme.StyleButton(btnLogin, isPrimary: true);
            btnLogin.Click += (s, e) => DoLogin();

            // Registration Prompt
            var pnlRegisterLink = new FlowLayoutPanel
            {
                Location = new Point(0, 365),
                Width = 440,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight
            };
            var lblNoAccount = new Label
            {
                Text = "Don't have an account yet? ",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true
            };
            lnkRegister = new LinkLabel
            {
                Text = "Register as Candidate (FR-1.1)",
                Font = UiTheme.BodyBold,
                LinkColor = UiTheme.Primary,
                AutoSize = true
            };
            lnkRegister.LinkClicked += (s, e) =>
            {
                var regDlg = new RegisterForm();
                if (regDlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtEmail.Text = regDlg.RegisteredEmail;
                    txtPassword.Clear();
                    lblError.Text = "Registration successful! You may now sign in.";
                    lblError.ForeColor = UiTheme.Success;
                    lblError.BackColor = Color.FromArgb(236, 253, 245);
                    lblError.Visible = true;
                }
            };
            pnlRegisterLink.Controls.Add(lblNoAccount);
            pnlRegisterLink.Controls.Add(lnkRegister);

            // Database Connectivity Status Strip
            lblDbStatus = new Label
            {
                Text = "● Database: XAMPP MySQL (online_exams_db) Connected",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Success,
                Location = new Point(0, 420),
                AutoSize = true
            };

            cardContainer.Controls.Add(lblSignIn);
            cardContainer.Controls.Add(lblSignInSub);
            cardContainer.Controls.Add(lblError);
            cardContainer.Controls.Add(lblEmail);
            cardContainer.Controls.Add(txtEmail);
            cardContainer.Controls.Add(lblPassword);
            cardContainer.Controls.Add(txtPassword);
            cardContainer.Controls.Add(chkShowPassword);
            cardContainer.Controls.Add(lnkForgotPassword);
            cardContainer.Controls.Add(btnLogin);
            cardContainer.Controls.Add(pnlRegisterLink);
            cardContainer.Controls.Add(lblDbStatus);

            pnlRight.Controls.Add(cardContainer);
            rootLayout.Controls.Add(pnlRight, 1, 0);

            this.Controls.Add(rootLayout);
        }

        private void FillCredentials(string email, string password)
        {
            txtEmail.Text = email;
            txtPassword.Text = password;
            lblError.Visible = false;
        }

        private void DoLogin()
        {
            lblError.Visible = false;
            btnLogin.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            bool success = authService.Login(txtEmail.Text, txtPassword.Text, out User user, out string error);

            Cursor.Current = Cursors.Default;
            btnLogin.Enabled = true;

            if (success && user != null)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblError.Text = error;
                lblError.ForeColor = UiTheme.Danger;
                lblError.BackColor = Color.FromArgb(254, 242, 242);
                lblError.Visible = true;
            }
        }
    }
}
