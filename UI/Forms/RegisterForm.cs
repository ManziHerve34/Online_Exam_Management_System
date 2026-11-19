using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Forms
{
    public class RegisterForm : Form
    {
        private TextBox txtFullName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtEducation;
        private TextBox txtCredentials;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private Button btnRegister;
        private Button btnCancel;
        private Label lblError;

        private readonly AuthService authService = new AuthService();
        public string RegisteredEmail { get; private set; }

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Candidate Registration - Online Professional Exams System";
            this.Size = new Size(580, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            var pnlH = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = UiTheme.SidebarDark, Padding = new Padding(24, 16, 24, 16) };
            var lblH = new Label { Text = "CANDIDATE ACCOUNT REGISTRATION (FR-1.1)", Font = UiTheme.SectionFont, ForeColor = Color.White, Location = new Point(24, 14), AutoSize = true };
            var lblHSub = new Label { Text = "Provide your personal and educational profile to register for certification exams.", Font = UiTheme.SmallFont, ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(24, 38), AutoSize = true };
            pnlH.Controls.Add(lblH);
            pnlH.Controls.Add(lblHSub);
            this.Controls.Add(pnlH);

            lblError = new Label { Text = "", Font = UiTheme.SmallFont, ForeColor = UiTheme.Danger, Location = new Point(32, 78), AutoSize = true };
            this.Controls.Add(lblError);

            int y = 98;
            var lblNameP = new Label { Text = "Full Legal Name *:", Font = UiTheme.BodyBold, Location = new Point(32, y), AutoSize = true };
            txtFullName = new TextBox { Location = new Point(32, y + 20), Width = 500, Font = UiTheme.BodyFont };
            this.Controls.Add(lblNameP);
            this.Controls.Add(txtFullName);

            y += 55;
            var lblEmailP = new Label { Text = "Email Address *:", Font = UiTheme.BodyBold, Location = new Point(32, y), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(32, y + 20), Width = 500, Font = UiTheme.BodyFont };
            this.Controls.Add(lblEmailP);
            this.Controls.Add(txtEmail);

            y += 55;
            var lblPhoneP = new Label { Text = "Phone / Mobile Number *:", Font = UiTheme.BodyBold, Location = new Point(32, y), AutoSize = true };
            txtPhone = new TextBox { Location = new Point(32, y + 20), Width = 500, Font = UiTheme.BodyFont };
            this.Controls.Add(lblPhoneP);
            this.Controls.Add(txtPhone);

            y += 55;
            var lblEduP = new Label { Text = "Educational Background (e.g. B.Sc. IT, AUCA):", Font = UiTheme.BodyBold, Location = new Point(32, y), AutoSize = true };
            txtEducation = new TextBox { Location = new Point(32, y + 20), Width = 500, Font = UiTheme.BodyFont };
            this.Controls.Add(lblEduP);
            this.Controls.Add(txtEducation);

            y += 55;
            var lblCredP = new Label { Text = "Professional Credentials / Prior Certifications (Optional):", Font = UiTheme.BodyBold, Location = new Point(32, y), AutoSize = true };
            txtCredentials = new TextBox { Location = new Point(32, y + 20), Width = 500, Font = UiTheme.BodyFont };
            this.Controls.Add(lblCredP);
            this.Controls.Add(txtCredentials);

            y += 55;
            var lblPwdP = new Label { Text = "Secure Password (min 6 chars) *:", Font = UiTheme.BodyBold, Location = new Point(32, y), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(32, y + 20), Width = 240, Font = UiTheme.BodyFont, UseSystemPasswordChar = true };

            var lblConfP = new Label { Text = "Confirm Password *:", Font = UiTheme.BodyBold, Location = new Point(290, y), AutoSize = true };
            txtConfirmPassword = new TextBox { Location = new Point(290, y + 20), Width = 242, Font = UiTheme.BodyFont, UseSystemPasswordChar = true };
            this.Controls.Add(lblPwdP);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblConfP);
            this.Controls.Add(txtConfirmPassword);

            y += 65;
            btnRegister = new Button { Text = "Register Account", Location = new Point(32, y), Width = 200, Height = 40, Font = UiTheme.BodyBold };
            UiTheme.StyleButton(btnRegister, isPrimary: true);
            btnRegister.Click += BtnRegister_Click;

            btnCancel = new Button { Text = "Cancel", Location = new Point(245, y), Width = 110, Height = 40 };
            UiTheme.StyleButton(btnCancel, isPrimary: false);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnRegister);
            this.Controls.Add(btnCancel);
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Please fill in all required fields.";
                return;
            }
            if (txtPassword.Text.Length < 6)
            {
                lblError.Text = "Password must be at least 6 characters long.";
                return;
            }
            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                lblError.Text = "Password and confirmation do not match.";
                return;
            }

            btnRegister.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            bool success = authService.Register(
                txtFullName.Text,
                txtEmail.Text,
                txtPhone.Text,
                txtEducation.Text,
                txtCredentials.Text,
                txtPassword.Text,
                out string verificationCode,
                out string error);

            Cursor.Current = Cursors.Default;
            btnRegister.Enabled = true;

            if (success)
            {
                RegisteredEmail = txtEmail.Text.Trim().ToLowerInvariant();

                // FR-1.2: Trigger Email verification prompt
                var verifyForm = new EmailVerificationForm(RegisteredEmail, verificationCode);
                var verifyResult = verifyForm.ShowDialog(this);

                if (verifyResult == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    // Allow logging in later even if verification was postponed
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                lblError.Text = error;
            }
        }
    }
}
