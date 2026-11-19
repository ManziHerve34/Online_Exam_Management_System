using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Controls
{
    public class UserManagementControl : UserControl
    {
        private DataGridView dgvUsers;
        private Button btnAddStaff;
        private Button btnToggleActive;
        private Button btnRefresh;

        private readonly AuthService authService = new AuthService();

        public UserManagementControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = UiTheme.Background;
            this.Dock = DockStyle.Fill;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(24, 20, 24, 20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Header
            var pnlHeader = new Panel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 16) };
            var lblTitle = new Label
            {
                Text = "User Roster & Access Control Management",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Enforce Role-Based Access Control (RBAC), provision authorized staff accounts, and monitor candidate verification status.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Table Panel
            var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            pnlGrid.Paint += UiTheme.PaintCardBorder;
            dgvUsers = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvUsers);
            dgvUsers.SelectionChanged += (s, e) =>
            {
                bool has = dgvUsers.CurrentRow?.Tag is User;
                btnToggleActive.Enabled = has;
            };

            pnlGrid.Controls.Add(dgvUsers);
            mainLayout.Controls.Add(pnlGrid, 0, 1);

            // Actions Toolbar
            var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(0, 16, 0, 0) };
            pnlActions.Paint += UiTheme.PaintCardBorder;

            btnAddStaff = new Button { Text = "+ Create Staff / Admin Account", Location = new Point(12, 12), Width = 230, Height = 36 };
            UiTheme.StyleButton(btnAddStaff, isPrimary: true);
            btnAddStaff.Click += (s, e) => ShowAddUserModal();

            btnToggleActive = new Button { Text = "🔒 Toggle Active / Deactivate", Location = new Point(252, 12), Width = 200, Height = 36, Enabled = false };
            UiTheme.StyleButton(btnToggleActive, isPrimary: false);
            btnToggleActive.Click += BtnToggleActive_Click;

            btnRefresh = new Button { Text = "Refresh", Location = new Point(462, 12), Width = 90, Height = 36 };
            UiTheme.StyleButton(btnRefresh, isPrimary: false);
            btnRefresh.Click += (s, e) => LoadUsers();

            pnlActions.Controls.Add(btnAddStaff);
            pnlActions.Controls.Add(btnToggleActive);
            pnlActions.Controls.Add(btnRefresh);
            mainLayout.Controls.Add(pnlActions, 0, 2);

            this.Controls.Add(mainLayout);
            this.Load += (s, e) =>
            {
                SetupGridColumns();
                LoadUsers();
            };
        }

        private void SetupGridColumns()
        {
            dgvUsers.Columns.Clear();
            dgvUsers.Columns.Add("Id", "User ID");
            dgvUsers.Columns.Add("Name", "Full Name");
            dgvUsers.Columns.Add("Email", "Email");
            dgvUsers.Columns.Add("Role", "Role (RBAC)");
            dgvUsers.Columns.Add("Phone", "Phone Number");
            dgvUsers.Columns.Add("Verified", "Email Verified");
            dgvUsers.Columns.Add("Status", "Account Status");
            dgvUsers.Columns.Add("Created", "Joined Date");

            dgvUsers.Columns["Id"].Width = 70;
            dgvUsers.Columns["Name"].Width = 180;
            dgvUsers.Columns["Email"].Width = 200;
            dgvUsers.Columns["Role"].Width = 150;
            dgvUsers.Columns["Phone"].Width = 130;
            dgvUsers.Columns["Verified"].Width = 120;
            dgvUsers.Columns["Status"].Width = 110;
        }

        public void LoadUsers()
        {
            var list = authService.GetAllUsers();
            dgvUsers.Rows.Clear();
            btnToggleActive.Enabled = false;

            foreach (var u in list)
            {
                int idx = dgvUsers.Rows.Add(
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.RoleName,
                    u.PhoneNumber ?? "N/A",
                    u.IsEmailVerified ? "✓ Verified" : "⚠ Pending",
                    u.IsActive ? "Active" : "Deactivated",
                    u.CreatedAt.ToString("yyyy-MM-dd")
                );

                var row = dgvUsers.Rows[idx];
                row.Tag = u;

                if (u.RoleName == "System Administrator")
                    row.Cells["Role"].Style.ForeColor = UiTheme.Primary;
                else if (u.RoleName == "Admin Staff")
                    row.Cells["Role"].Style.ForeColor = UiTheme.Secondary;

                if (!u.IsActive)
                    row.Cells["Status"].Style.ForeColor = UiTheme.Danger;
                else
                    row.Cells["Status"].Style.ForeColor = UiTheme.Success;
            }

            if (dgvUsers.Rows.Count > 0)
            {
                dgvUsers.Rows[0].Selected = true;
                btnToggleActive.Enabled = true;
            }
        }

        private void ShowAddUserModal()
        {
            using (var dlg = new Form())
            {
                dlg.Text = "Provision New System Account";
                dlg.Size = new Size(500, 480);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.White;

                var pnlH = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UiTheme.SidebarDark, Padding = new Padding(16, 12, 16, 12) };
                var lblH = new Label { Text = "CREATE NEW STAFF / ADMINISTRATIVE ACCOUNT", Font = UiTheme.SectionFont, ForeColor = Color.White, Location = new Point(16, 16), AutoSize = true };
                pnlH.Controls.Add(lblH);
                dlg.Controls.Add(pnlH);

                int y = 75;
                var lblNameP = new Label { Text = "Full Legal Name:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtName = new TextBox { Location = new Point(24, y + 20), Width = 430, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblNameP);
                dlg.Controls.Add(txtName);

                y += 55;
                var lblEmailP = new Label { Text = "Corporate Email Address:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtEmail = new TextBox { Location = new Point(24, y + 20), Width = 430, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblEmailP);
                dlg.Controls.Add(txtEmail);

                y += 55;
                var lblRoleP = new Label { Text = "System Role (RBAC):", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var cmbRole = new ComboBox { Location = new Point(24, y + 20), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
                cmbRole.Items.Add(new ComboBoxItem { Text = "Admin Staff", Value = 2 });
                cmbRole.Items.Add(new ComboBoxItem { Text = "System Administrator", Value = 1 });
                cmbRole.SelectedIndex = 0;

                var lblPhoneP = new Label { Text = "Phone Number:", Font = UiTheme.BodyBold, Location = new Point(244, y), AutoSize = true };
                var txtPhone = new TextBox { Location = new Point(244, y + 20), Width = 210, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblRoleP);
                dlg.Controls.Add(cmbRole);
                dlg.Controls.Add(lblPhoneP);
                dlg.Controls.Add(txtPhone);

                y += 55;
                var lblPwdP = new Label { Text = "Temporary Initial Password (min 6 chars):", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtPwd = new TextBox { Text = "Exams@2026", Location = new Point(24, y + 20), Width = 430, Font = UiTheme.BodyFont, UseSystemPasswordChar = true };
                dlg.Controls.Add(lblPwdP);
                dlg.Controls.Add(txtPwd);

                y += 65;
                var btnSave = new Button { Text = "Create Account", Location = new Point(24, y), Width = 150, Height = 36 };
                UiTheme.StyleButton(btnSave, isPrimary: true);

                var btnCancel = new Button { Text = "Cancel", Location = new Point(184, y), Width = 90, Height = 36 };
                UiTheme.StyleButton(btnCancel, isPrimary: false);
                btnCancel.Click += (s, ev) => dlg.DialogResult = DialogResult.Cancel;

                btnSave.Click += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPwd.Text))
                    {
                        MessageBox.Show("Name, Email, and Password are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int roleId = (int)((ComboBoxItem)cmbRole.SelectedItem).Value;
                    var newUser = new User
                    {
                        RoleId = roleId,
                        FullName = txtName.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        PhoneNumber = txtPhone.Text.Trim()
                    };

                    bool success = authService.CreateUserByAdmin(newUser, txtPwd.Text, out string err);
                    if (success)
                    {
                        MessageBox.Show($"Account created for {newUser.Email}!", "Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show(err, "Creation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                dlg.Controls.Add(btnSave);
                dlg.Controls.Add(btnCancel);

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadUsers();
                }
            }
        }

        private void BtnToggleActive_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow?.Tag is User u)
            {
                if (u.UserId == SessionManager.CurrentUserId)
                {
                    MessageBox.Show("You cannot deactivate your own administrative account.", "Action Prohibited", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool newState = !u.IsActive;
                string action = newState ? "activate" : "deactivate";

                var confirm = MessageBox.Show($"Are you sure you want to {action} account '{u.FullName}' ({u.Email})?", "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    bool ok = authService.ToggleUserActive(u.UserId, newState, out string err);
                    if (ok)
                    {
                        MessageBox.Show($"User account {action}d successfully.", "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
