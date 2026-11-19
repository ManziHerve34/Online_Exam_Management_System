using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Controls
{
    public class VenueManagementControl : UserControl
    {
        private DataGridView dgvVenues;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;

        private readonly VenueService venueService = new VenueService();

        public VenueManagementControl()
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
                Text = "Testing Centers & Venue Management",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Manage authorized examination testing centers, physical room allocations, maximum capacities, and proctor contact points.",
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
            dgvVenues = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvVenues);
            dgvVenues.SelectionChanged += (s, e) =>
            {
                bool has = dgvVenues.CurrentRow?.Tag is Venue;
                btnEdit.Enabled = has;
                btnDelete.Enabled = has;
            };
            dgvVenues.CellDoubleClick += (s, e) => EditSelectedVenue();

            pnlGrid.Controls.Add(dgvVenues);
            mainLayout.Controls.Add(pnlGrid, 0, 1);

            // Actions Toolbar
            var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(0, 16, 0, 0) };
            pnlActions.Paint += UiTheme.PaintCardBorder;

            btnAdd = new Button { Text = "+ Add Test Center", Location = new Point(12, 12), Width = 170, Height = 36 };
            UiTheme.StyleButton(btnAdd, isPrimary: true);
            btnAdd.Click += (s, e) => ShowVenueEditor(null);

            btnEdit = new Button { Text = "✏️ Edit Center", Location = new Point(192, 12), Width = 130, Height = 36, Enabled = false };
            UiTheme.StyleButton(btnEdit, isPrimary: false);
            btnEdit.Click += (s, e) => EditSelectedVenue();

            btnDelete = new Button { Text = "🗑️ Delete / Deactivate", Location = new Point(332, 12), Width = 160, Height = 36, Enabled = false };
            UiTheme.StyleButton(btnDelete, isPrimary: false, isDanger: true);
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button { Text = "Refresh", Location = new Point(502, 12), Width = 90, Height = 36 };
            UiTheme.StyleButton(btnRefresh, isPrimary: false);
            btnRefresh.Click += (s, e) => LoadVenues();

            pnlActions.Controls.Add(btnAdd);
            pnlActions.Controls.Add(btnEdit);
            pnlActions.Controls.Add(btnDelete);
            pnlActions.Controls.Add(btnRefresh);
            mainLayout.Controls.Add(pnlActions, 0, 2);

            this.Controls.Add(mainLayout);
            this.Load += (s, e) =>
            {
                SetupGridColumns();
                LoadVenues();
            };
        }

        private void SetupGridColumns()
        {
            dgvVenues.Columns.Clear();
            dgvVenues.Columns.Add("Name", "Testing Center Name");
            dgvVenues.Columns.Add("City", "City");
            dgvVenues.Columns.Add("Room", "Room / Suite");
            dgvVenues.Columns.Add("Address", "Physical Address");
            dgvVenues.Columns.Add("Capacity", "Total Capacity");
            dgvVenues.Columns.Add("Contact", "Contact Person");
            dgvVenues.Columns.Add("Phone", "Phone Number");
            dgvVenues.Columns.Add("Status", "Status");

            dgvVenues.Columns["Name"].Width = 220;
            dgvVenues.Columns["City"].Width = 100;
            dgvVenues.Columns["Room"].Width = 130;
            dgvVenues.Columns["Capacity"].Width = 110;
            dgvVenues.Columns["Contact"].Width = 150;
            dgvVenues.Columns["Phone"].Width = 130;
            dgvVenues.Columns["Status"].Width = 80;
        }

        public void LoadVenues()
        {
            var list = venueService.GetAllVenues(activeOnly: false);
            dgvVenues.Rows.Clear();
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;

            foreach (var v in list)
            {
                int idx = dgvVenues.Rows.Add(
                    v.VenueName,
                    v.City,
                    v.RoomNumber ?? "General Hall",
                    v.Address,
                    $"{v.TotalCapacity} Seats",
                    v.ContactPerson ?? "Operations Desk",
                    v.ContactPhone ?? "N/A",
                    v.IsActive ? "Active" : "Inactive"
                );
                dgvVenues.Rows[idx].Tag = v;
                if (!v.IsActive)
                    dgvVenues.Rows[idx].Cells["Status"].Style.ForeColor = UiTheme.Danger;
                else
                    dgvVenues.Rows[idx].Cells["Status"].Style.ForeColor = UiTheme.Success;
            }

            if (dgvVenues.Rows.Count > 0)
            {
                dgvVenues.Rows[0].Selected = true;
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
            }
        }

        private void EditSelectedVenue()
        {
            if (dgvVenues.CurrentRow?.Tag is Venue v)
                ShowVenueEditor(v);
        }

        private void ShowVenueEditor(Venue existing)
        {
            using (var dlg = new Form())
            {
                bool isEdit = existing != null;
                dlg.Text = isEdit ? $"Edit Test Center: {existing.VenueName}" : "Add New Test Center";
                dlg.Size = new Size(520, 520);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.White;

                var pnlH = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UiTheme.SidebarDark, Padding = new Padding(16, 12, 16, 12) };
                var lblH = new Label { Text = isEdit ? "UPDATE TESTING CENTER INFORMATION" : "REGISTER TESTING CENTER & ROOM", Font = UiTheme.SectionFont, ForeColor = Color.White, Location = new Point(16, 16), AutoSize = true };
                pnlH.Controls.Add(lblH);
                dlg.Controls.Add(pnlH);

                int y = 75;
                var lblNameP = new Label { Text = "Venue / Center Name:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtName = new TextBox { Text = existing?.VenueName ?? "", Location = new Point(24, y + 20), Width = 440, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblNameP);
                dlg.Controls.Add(txtName);

                y += 55;
                var lblCityP = new Label { Text = "City / Region:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtCity = new TextBox { Text = existing?.City ?? "", Location = new Point(24, y + 20), Width = 210, Font = UiTheme.BodyFont };

                var lblRoomP = new Label { Text = "Room / Hall / Suite:", Font = UiTheme.BodyBold, Location = new Point(254, y), AutoSize = true };
                var txtRoom = new TextBox { Text = existing?.RoomNumber ?? "", Location = new Point(254, y + 20), Width = 210, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblCityP);
                dlg.Controls.Add(txtCity);
                dlg.Controls.Add(lblRoomP);
                dlg.Controls.Add(txtRoom);

                y += 55;
                var lblAddrP = new Label { Text = "Physical Street Address:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtAddr = new TextBox { Text = existing?.Address ?? "", Location = new Point(24, y + 20), Width = 440, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblAddrP);
                dlg.Controls.Add(txtAddr);

                y += 55;
                var lblCapP = new Label { Text = "Maximum Seating Capacity:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var numCap = new NumericUpDown { Location = new Point(24, y + 20), Width = 210, Minimum = 1, Maximum = 1000, Value = existing?.TotalCapacity ?? 30, Font = UiTheme.BodyFont };

                var lblPhoneP = new Label { Text = "Contact Phone:", Font = UiTheme.BodyBold, Location = new Point(254, y), AutoSize = true };
                var txtPhone = new TextBox { Text = existing?.ContactPhone ?? "", Location = new Point(254, y + 20), Width = 210, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblCapP);
                dlg.Controls.Add(numCap);
                dlg.Controls.Add(lblPhoneP);
                dlg.Controls.Add(txtPhone);

                y += 55;
                var lblContactP = new Label { Text = "Center Manager / Contact Person:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtContact = new TextBox { Text = existing?.ContactPerson ?? "", Location = new Point(24, y + 20), Width = 440, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblContactP);
                dlg.Controls.Add(txtContact);

                y += 55;
                var chkActive = new CheckBox { Text = "Active (Accepting Sessions)", Checked = existing?.IsActive ?? true, Location = new Point(24, y), AutoSize = true, Font = UiTheme.BodyBold };
                dlg.Controls.Add(chkActive);

                y += 40;
                var btnSave = new Button { Text = isEdit ? "Update Venue" : "Save Venue", Location = new Point(24, y), Width = 140, Height = 36 };
                UiTheme.StyleButton(btnSave, isPrimary: true);

                var btnCancel = new Button { Text = "Cancel", Location = new Point(174, y), Width = 90, Height = 36 };
                UiTheme.StyleButton(btnCancel, isPrimary: false);
                btnCancel.Click += (s, ev) => dlg.DialogResult = DialogResult.Cancel;

                btnSave.Click += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCity.Text) || string.IsNullOrWhiteSpace(txtAddr.Text))
                    {
                        MessageBox.Show("Venue Name, City, and Address are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var venue = new Venue
                    {
                        VenueId = existing?.VenueId ?? 0,
                        VenueName = txtName.Text.Trim(),
                        City = txtCity.Text.Trim(),
                        RoomNumber = txtRoom.Text.Trim(),
                        Address = txtAddr.Text.Trim(),
                        TotalCapacity = (int)numCap.Value,
                        ContactPerson = txtContact.Text.Trim(),
                        ContactPhone = txtPhone.Text.Trim(),
                        IsActive = chkActive.Checked
                    };

                    bool success;
                    string err;
                    if (isEdit)
                        success = venueService.UpdateVenue(venue, out err);
                    else
                        success = venueService.CreateVenue(venue, out err);

                    if (success)
                    {
                        MessageBox.Show($"Venue {venue.VenueName} saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show(err, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                dlg.Controls.Add(btnSave);
                dlg.Controls.Add(btnCancel);

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadVenues();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvVenues.CurrentRow?.Tag is Venue v)
            {
                var confirm = MessageBox.Show($"Are you sure you want to delete or deactivate venue '{v.VenueName}'?", "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    bool ok = venueService.DeleteVenue(v.VenueId, out string error);
                    if (ok)
                    {
                        MessageBox.Show("Venue status updated successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadVenues();
                    }
                    else
                    {
                        MessageBox.Show(error, "Action Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
