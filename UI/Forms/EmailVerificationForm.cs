using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Forms
{
    public class EmailVerificationForm : Form
    {
        private readonly string email;
        private TextBox txtCode;
        private Button btnVerify;
        private Button btnResend;
        private Button btnSkip;
        private Label lblMessage;

        private readonly AuthService authService = new AuthService();

        public EmailVerificationForm(string email, string generatedCode = "")
        {
            this.email = email;
            // generatedCode is NOT displayed — code has been sent to the actual inbox
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Email Address Verification (FR-1.2)";
            this.Size = new Size(480, 390);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            var pnlH = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UiTheme.SidebarDark, Padding = new Padding(20, 14, 20, 14) };
            var lblH = new Label { Text = "VALIDATE YOUR EMAIL ADDRESS", Font = UiTheme.SectionFont, ForeColor = Color.White, Location = new Point(20, 18), AutoSize = true };
            pnlH.Controls.Add(lblH);
            this.Controls.Add(pnlH);

            var lblPrompt = new Label
            {
                Text = $"A 6-digit verification code has been sent to your registered\nemail inbox. Please check your inbox (and spam folder) for:\n\n  {email}",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextDark,
                Location = new Point(24, 75),
                AutoSize = true
            };

            // Email dispatch status notice
            var lblDispatchNote = new Label
            {
                Text = "✉  Verification code dispatched. Enter it below to activate your account.",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Success,
                BackColor = Color.FromArgb(240, 253, 244),
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(24, 155),
                AutoSize = true
            };

            var lblInputP = new Label { Text = "Enter 6-Digit Verification Code:", Font = UiTheme.BodyBold, Location = new Point(24, 190), AutoSize = true };
            txtCode = new TextBox
            {
                Location = new Point(24, 215),
                Width = 240,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center
            };

            lblMessage = new Label { Text = "", Font = UiTheme.SmallFont, ForeColor = UiTheme.Danger, Location = new Point(24, 260), AutoSize = true };

            btnVerify = new Button { Text = "Verify Email Now", Location = new Point(24, 285), Width = 180, Height = 38 };
            UiTheme.StyleButton(btnVerify, isPrimary: true, isSuccess: true);
            btnVerify.Click += BtnVerify_Click;

            btnResend = new Button { Text = "Resend Code", Location = new Point(215, 285), Width = 120, Height = 38 };
            UiTheme.StyleButton(btnResend, isPrimary: false);
            btnResend.Click += BtnResend_Click;

            btnSkip = new Button { Text = "Verify Later", Location = new Point(345, 285), Width = 100, Height = 38 };
            UiTheme.StyleButton(btnSkip, isPrimary: false);
            btnSkip.Click += (s, e) => { this.DialogResult = DialogResult.Ignore; this.Close(); };

            this.Controls.Add(lblPrompt);
            this.Controls.Add(lblDispatchNote);
            this.Controls.Add(lblInputP);
            this.Controls.Add(txtCode);
            this.Controls.Add(lblMessage);
            this.Controls.Add(btnVerify);
            this.Controls.Add(btnResend);
            this.Controls.Add(btnSkip);
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                lblMessage.Text = "Please enter the verification code.";
                return;
            }

            bool success = authService.VerifyEmail(email, txtCode.Text, out string err);
            if (success)
            {
                MessageBox.Show("Email verified successfully! You are now eligible to schedule exams.", "Email Verified", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblMessage.Text = err;
            }
        }

        private void BtnResend_Click(object sender, EventArgs e)
        {
            btnResend.Enabled = false;
            lblMessage.Text = "Sending new code...";
            lblMessage.ForeColor = UiTheme.TextMuted;

            bool ok = authService.ResendVerificationCode(email, out string _, out string err);
            btnResend.Enabled = true;

            if (ok)
            {
                txtCode.Text = "";
                txtCode.Focus();
                lblMessage.Text = "✉  A new code has been dispatched to your inbox. Check your email.";
                lblMessage.ForeColor = UiTheme.Success;
            }
            else
            {
                lblMessage.Text = err;
                lblMessage.ForeColor = UiTheme.Danger;
            }
        }
    }
}
