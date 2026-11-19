using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Forms
{
    public class ForgotPasswordForm : Form
    {
        private TextBox txtEmail;
        private Button btnSendOtp;
        private Label lblOtpHint;

        private Panel pnlStep2;
        private TextBox txtOtp;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnResetPassword;

        private Label lblError;
        private readonly AuthService authService = new AuthService();
        private string activeOtp = "";

        public ForgotPasswordForm(string initialEmail = "")
        {
            InitializeComponent(initialEmail);
        }

        private void InitializeComponent(string initialEmail)
        {
            this.Text = "Account Password Recovery (FR-1.4)";
            this.Size = new Size(520, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            var pnlH = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UiTheme.SidebarDark, Padding = new Padding(20, 14, 20, 14) };
            var lblH = new Label { Text = "PASSWORD RECOVERY VIA EMAIL OTP", Font = UiTheme.SectionFont, ForeColor = Color.White, Location = new Point(20, 18), AutoSize = true };
            pnlH.Controls.Add(lblH);
            this.Controls.Add(pnlH);

            lblError = new Label { Text = "", Font = UiTheme.SmallFont, ForeColor = UiTheme.Danger, Location = new Point(24, 75), AutoSize = true };
            this.Controls.Add(lblError);

            // Step 1: Request OTP
            var lblEmailP = new Label { Text = "Enter Registered Account Email:", Font = UiTheme.BodyBold, Location = new Point(24, 100), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(24, 125), Width = 310, Font = UiTheme.BodyFont, Text = initialEmail };

            btnSendOtp = new Button { Text = "Send OTP", Location = new Point(345, 123), Width = 130, Height = 30 };
            UiTheme.StyleButton(btnSendOtp, isPrimary: true);
            btnSendOtp.Click += BtnSendOtp_Click;

            lblOtpHint = new Label
            {
                Text = "",
                Font = UiTheme.BodyBold,
                ForeColor = UiTheme.Primary,
                BackColor = UiTheme.PrimaryLight,
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(24, 165),
                AutoSize = true,
                Visible = false
            };

            this.Controls.Add(lblEmailP);
            this.Controls.Add(txtEmail);
            this.Controls.Add(btnSendOtp);
            this.Controls.Add(lblOtpHint);

            // Step 2 Panel: Enter OTP & New Password
            pnlStep2 = new Panel
            {
                Location = new Point(24, 210),
                Size = new Size(460, 240),
                Visible = false
            };

            var lblOtpP = new Label { Text = "Enter 6-Digit OTP Code:", Font = UiTheme.BodyBold, Location = new Point(0, 5), AutoSize = true };
            txtOtp = new TextBox { Location = new Point(0, 28), Width = 200, Font = new Font("Segoe UI", 12F, FontStyle.Bold), TextAlign = HorizontalAlignment.Center };

            var lblNewPwdP = new Label { Text = "New Password (min 6 chars):", Font = UiTheme.BodyBold, Location = new Point(0, 68), AutoSize = true };
            txtNewPassword = new TextBox { Location = new Point(0, 92), Width = 450, Font = UiTheme.BodyFont, UseSystemPasswordChar = true };

            var lblConfPwdP = new Label { Text = "Confirm New Password:", Font = UiTheme.BodyBold, Location = new Point(0, 130), AutoSize = true };
            txtConfirmPassword = new TextBox { Location = new Point(0, 154), Width = 450, Font = UiTheme.BodyFont, UseSystemPasswordChar = true };

            btnResetPassword = new Button { Text = "Reset & Update Password", Location = new Point(0, 195), Width = 230, Height = 38 };
            UiTheme.StyleButton(btnResetPassword, isPrimary: true, isSuccess: true);
            btnResetPassword.Click += BtnResetPassword_Click;

            pnlStep2.Controls.Add(lblOtpP);
            pnlStep2.Controls.Add(txtOtp);
            pnlStep2.Controls.Add(lblNewPwdP);
            pnlStep2.Controls.Add(txtNewPassword);
            pnlStep2.Controls.Add(lblConfPwdP);
            pnlStep2.Controls.Add(txtConfirmPassword);
            pnlStep2.Controls.Add(btnResetPassword);

            this.Controls.Add(pnlStep2);
        }

        private void BtnSendOtp_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                lblError.Text = "Please enter your email address.";
                return;
            }

            btnSendOtp.Enabled = false;
            bool ok = authService.GeneratePasswordResetOtp(txtEmail.Text, out string otp, out string err);
            btnSendOtp.Enabled = true;

            if (ok)
            {
                activeOtp = otp;
                lblOtpHint.Text = $"[Simulated OTP Delivered to Inbox: {otp}]";
                lblOtpHint.Visible = true;
                txtOtp.Text = otp;
                pnlStep2.Visible = true;
                lblError.Text = "OTP sent successfully! Please set your new password.";
                lblError.ForeColor = UiTheme.Success;
            }
            else
            {
                lblError.Text = err;
                lblError.ForeColor = UiTheme.Danger;
            }
        }

        private void BtnResetPassword_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtOtp.Text))
            {
                lblError.Text = "Please enter the OTP code.";
                return;
            }
            if (string.IsNullOrEmpty(txtNewPassword.Text) || txtNewPassword.Text.Length < 6)
            {
                lblError.Text = "Password must be at least 6 characters long.";
                return;
            }
            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                lblError.Text = "Passwords do not match.";
                return;
            }

            bool ok = authService.ResetPasswordWithOtp(txtEmail.Text, txtOtp.Text, txtNewPassword.Text, out string err);
            if (ok)
            {
                MessageBox.Show("Password reset successful! You can now log in with your new password.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblError.Text = err;
                lblError.ForeColor = UiTheme.Danger;
            }
        }
    }
}
