using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Controls
{
    public class UserProfileControl : UserControl
    {
        private TextBox txtFullName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtEducation;
        private TextBox txtCredentials;
        private Label lblRole;
        private Label lblEmailVerified;
        private Button btnSaveProfile;

        private TextBox txtCurrentPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnChangePassword;

        private readonly AuthService authService = new AuthService();

        public UserProfileControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = UiTheme.Background;
            this.Dock = DockStyle.Fill;
            this.AutoScroll = true;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                AutoScroll = true,
                Padding = new Padding(24, 20, 24, 20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Profile Card
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Password Card

            // Header
            var pnlHeader = new Panel { AutoSize = true, Margin = new Padding(0, 0, 0, 16) };
            var lblTitle = new Label
            {
                Text = "Candidate Profile & Professional Credentials",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Manage your contact details, educational background, professional certifications, and security credentials.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Profile Card (FR-1.5)
            var pnlProfile = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = Color.White,
                Padding = new Padding(24),
                Margin = new Padding(0, 0, 0, 20)
            };
            pnlProfile.Paint += UiTheme.PaintCardBorder;

            var lblSection1 = new Label
            {
                Text = "PERSONAL & PROFESSIONAL INFORMATION",
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.Primary,
                Location = new Point(24, 20),
                AutoSize = true
            };

            var lblNameP = new Label { Text = "Full Legal Name:", Font = UiTheme.BodyBold, Location = new Point(24, 56), AutoSize = true };
            txtFullName = new TextBox { Location = new Point(24, 78), Width = 380, Font = UiTheme.BodyFont };

            var lblEmailP = new Label { Text = "Email Address (Read-only):", Font = UiTheme.BodyBold, Location = new Point(440, 56), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(440, 78), Width = 380, Font = UiTheme.BodyFont, ReadOnly = true, BackColor = Color.FromArgb(248, 250, 252) };

            var lblPhoneP = new Label { Text = "Phone / WhatsApp:", Font = UiTheme.BodyBold, Location = new Point(24, 116), AutoSize = true };
            txtPhone = new TextBox { Location = new Point(24, 138), Width = 380, Font = UiTheme.BodyFont };

            var lblRoleP = new Label { Text = "Account Role & Verification:", Font = UiTheme.BodyBold, Location = new Point(440, 116), AutoSize = true };
            lblRole = new Label { Text = "Role: Candidate", Font = UiTheme.BodyFont, ForeColor = UiTheme.TextDark, Location = new Point(440, 140), AutoSize = true };
            lblEmailVerified = new Label { Text = "✓ Email Verified", Font = UiTheme.BodyBold, ForeColor = UiTheme.Success, Location = new Point(580, 140), AutoSize = true };

            var lblEduP = new Label { Text = "Educational Background (Degree, University, Graduation Year):", Font = UiTheme.BodyBold, Location = new Point(24, 176), AutoSize = true };
            txtEducation = new TextBox { Location = new Point(24, 198), Width = 796, Font = UiTheme.BodyFont };

            var lblCredP = new Label { Text = "Existing Professional Credentials & Certifications:", Font = UiTheme.BodyBold, Location = new Point(24, 236), AutoSize = true };
            txtCredentials = new TextBox { Location = new Point(24, 258), Width = 796, Font = UiTheme.BodyFont };

            btnSaveProfile = new Button
            {
                Text = "Save Profile Changes",
                Location = new Point(24, 300),
                Width = 200,
                Height = 38
            };
            UiTheme.StyleButton(btnSaveProfile, isPrimary: true);
            btnSaveProfile.Click += BtnSaveProfile_Click;

            pnlProfile.Controls.Add(lblSection1);
            pnlProfile.Controls.Add(lblNameP);
            pnlProfile.Controls.Add(txtFullName);
            pnlProfile.Controls.Add(lblEmailP);
            pnlProfile.Controls.Add(txtEmail);
            pnlProfile.Controls.Add(lblPhoneP);
            pnlProfile.Controls.Add(txtPhone);
            pnlProfile.Controls.Add(lblRoleP);
            pnlProfile.Controls.Add(lblRole);
            pnlProfile.Controls.Add(lblEmailVerified);
            pnlProfile.Controls.Add(lblEduP);
            pnlProfile.Controls.Add(txtEducation);
            pnlProfile.Controls.Add(lblCredP);
            pnlProfile.Controls.Add(txtCredentials);
            pnlProfile.Controls.Add(btnSaveProfile);
            mainLayout.Controls.Add(pnlProfile, 0, 1);

            // Change Password Card
            var pnlPassword = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = Color.White,
                Padding = new Padding(24),
                Margin = new Padding(0, 0, 0, 20)
            };
            pnlPassword.Paint += UiTheme.PaintCardBorder;

            var lblSection2 = new Label
            {
                Text = "ACCOUNT SECURITY & PASSWORD CHANGE",
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.Primary,
                Location = new Point(24, 20),
                AutoSize = true
            };

            var lblCurrentP = new Label { Text = "Current Password:", Font = UiTheme.BodyBold, Location = new Point(24, 56), AutoSize = true };
            txtCurrentPassword = new TextBox { Location = new Point(24, 78), Width = 260, Font = UiTheme.BodyFont, UseSystemPasswordChar = true };

            var lblNewP = new Label { Text = "New Password (min 6 chars):", Font = UiTheme.BodyBold, Location = new Point(300, 56), AutoSize = true };
            txtNewPassword = new TextBox { Location = new Point(300, 78), Width = 260, Font = UiTheme.BodyFont, UseSystemPasswordChar = true };

            var lblConfirmP = new Label { Text = "Confirm New Password:", Font = UiTheme.BodyBold, Location = new Point(576, 56), AutoSize = true };
            txtConfirmPassword = new TextBox { Location = new Point(576, 78), Width = 244, Font = UiTheme.BodyFont, UseSystemPasswordChar = true };

            btnChangePassword = new Button
            {
                Text = "Update Password",
                Location = new Point(24, 120),
                Width = 180,
                Height = 36
            };
            UiTheme.StyleButton(btnChangePassword, isPrimary: false);
            btnChangePassword.Click += BtnChangePassword_Click;

            pnlPassword.Controls.Add(lblSection2);
            pnlPassword.Controls.Add(lblCurrentP);
            pnlPassword.Controls.Add(txtCurrentPassword);
            pnlPassword.Controls.Add(lblNewP);
            pnlPassword.Controls.Add(txtNewPassword);
            pnlPassword.Controls.Add(lblConfirmP);
            pnlPassword.Controls.Add(txtConfirmPassword);
            pnlPassword.Controls.Add(btnChangePassword);
            mainLayout.Controls.Add(pnlPassword, 0, 2);

            this.Controls.Add(mainLayout);
            this.Load += (s, e) => LoadUserData();
        }

        private void LoadUserData()
        {
            var user = authService.GetUserById(SessionManager.CurrentUserId);
            if (user != null)
            {
                txtFullName.Text = user.FullName;
                txtEmail.Text = user.Email;
                txtPhone.Text = user.PhoneNumber ?? "";
                txtEducation.Text = user.EducationalBackground ?? "";
                txtCredentials.Text = user.ProfessionalCredentials ?? "";
                lblRole.Text = $"Role: {user.RoleName}";
                lblEmailVerified.Text = user.IsEmailVerified ? "✓ Verified Email" : "⚠ Unverified Email";
                lblEmailVerified.ForeColor = user.IsEmailVerified ? UiTheme.Success : UiTheme.Warning;
            }
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Full Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = authService.UpdateProfile(
                SessionManager.CurrentUserId,
                txtFullName.Text,
                txtPhone.Text,
                txtEducation.Text,
                txtCredentials.Text,
                out string error);

            if (success)
            {
                MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SessionManager.SetSession(SessionManager.CurrentUserId, txtFullName.Text.Trim(), SessionManager.CurrentEmail, SessionManager.CurrentRoleId, SessionManager.CurrentRoleName);
            }
            else
            {
                MessageBox.Show(error, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCurrentPassword.Text))
            {
                MessageBox.Show("Please enter your current password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txtNewPassword.Text) || txtNewPassword.Text.Length < 6)
            {
                MessageBox.Show("New password must be at least 6 characters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("New password and confirmation do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = authService.ChangePassword(
                SessionManager.CurrentUserId,
                txtCurrentPassword.Text,
                txtNewPassword.Text,
                out string error);

            if (success)
            {
                MessageBox.Show("Password updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCurrentPassword.Clear();
                txtNewPassword.Clear();
                txtConfirmPassword.Clear();
            }
            else
            {
                MessageBox.Show(error, "Password Change Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
