using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Controls
{
    public class ScheduleManagementControl : UserControl
    {
        private DataGridView dgvSchedules;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private CheckBox chkUpcomingOnly;

        private readonly ScheduleService scheduleService = new ScheduleService();
        private readonly ExamService examService = new ExamService();
        private readonly VenueService venueService = new VenueService();

        public ScheduleManagementControl()
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
                Text = "Examination Schedules & Session Windows",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Create and schedule examination windows, assign proctors, configure seat capacities, and monitor real-time bookings.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };

            var pnlFilter = new Panel { Dock = DockStyle.Bottom, Height = 40, Location = new Point(0, 65) };
            chkUpcomingOnly = new CheckBox
            {
                Text = "Show upcoming scheduled sessions only",
                Checked = false,
                Location = new Point(0, 10),
                AutoSize = true,
                Font = UiTheme.BodyBold
            };
            chkUpcomingOnly.CheckedChanged += (s, e) => LoadSchedules();
            pnlFilter.Controls.Add(chkUpcomingOnly);

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Controls.Add(pnlFilter);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Table Panel
            var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            pnlGrid.Paint += UiTheme.PaintCardBorder;
            dgvSchedules = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvSchedules);
            dgvSchedules.SelectionChanged += (s, e) =>
            {
                bool has = dgvSchedules.CurrentRow?.Tag is ExamSchedule;
                btnEdit.Enabled = has;
                btnDelete.Enabled = has;
            };
            dgvSchedules.CellDoubleClick += (s, e) => EditSelectedSchedule();

            pnlGrid.Controls.Add(dgvSchedules);
            mainLayout.Controls.Add(pnlGrid, 0, 1);

            // Actions Bar
            var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(0, 16, 0, 0) };
            pnlActions.Paint += UiTheme.PaintCardBorder;

            btnAdd = new Button { Text = "+ Schedule New Exam Session", Location = new Point(12, 12), Width = 220, Height = 36 };
            UiTheme.StyleButton(btnAdd, isPrimary: true);
            btnAdd.Click += (s, e) => ShowScheduleEditor(null);

            btnEdit = new Button { Text = "✏️ Edit Schedule", Location = new Point(242, 12), Width = 140, Height = 36, Enabled = false };
            UiTheme.StyleButton(btnEdit, isPrimary: false);
            btnEdit.Click += (s, e) => EditSelectedSchedule();

            btnDelete = new Button { Text = "❌ Cancel / Delete Session", Location = new Point(392, 12), Width = 190, Height = 36, Enabled = false };
            UiTheme.StyleButton(btnDelete, isPrimary: false, isDanger: true);
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button { Text = "Refresh", Location = new Point(592, 12), Width = 90, Height = 36 };
            UiTheme.StyleButton(btnRefresh, isPrimary: false);
            btnRefresh.Click += (s, e) => LoadSchedules();

            pnlActions.Controls.Add(btnAdd);
            pnlActions.Controls.Add(btnEdit);
            pnlActions.Controls.Add(btnDelete);
            pnlActions.Controls.Add(btnRefresh);
            mainLayout.Controls.Add(pnlActions, 0, 2);

            this.Controls.Add(mainLayout);
            this.Load += (s, e) =>
            {
                SetupGridColumns();
                LoadSchedules();
            };
        }

        private void SetupGridColumns()
        {
            dgvSchedules.Columns.Clear();
            dgvSchedules.Columns.Add("Code", "Exam Code");
            dgvSchedules.Columns.Add("Title", "Exam Title");
            dgvSchedules.Columns.Add("Date", "Exam Date");
            dgvSchedules.Columns.Add("Time", "Time Window");
            dgvSchedules.Columns.Add("Venue", "Test Center");
            dgvSchedules.Columns.Add("Room", "Room");
            dgvSchedules.Columns.Add("Seats", "Seat Utilization");
            dgvSchedules.Columns.Add("Proctor", "Assigned Proctor");
            dgvSchedules.Columns.Add("Status", "Status");

            dgvSchedules.Columns["Code"].Width = 110;
            dgvSchedules.Columns["Title"].Width = 200;
            dgvSchedules.Columns["Date"].Width = 95;
            dgvSchedules.Columns["Time"].Width = 135;
            dgvSchedules.Columns["Venue"].Width = 170;
            dgvSchedules.Columns["Room"].Width = 90;
            dgvSchedules.Columns["Seats"].Width = 120;
            dgvSchedules.Columns["Proctor"].Width = 140;
            dgvSchedules.Columns["Status"].Width = 90;
        }

        public void LoadSchedules()
        {
            var list = scheduleService.GetAllSchedules(chkUpcomingOnly.Checked);
            dgvSchedules.Rows.Clear();
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;

            foreach (var s in list)
            {
                int idx = dgvSchedules.Rows.Add(
                    s.ExamCode,
                    s.ExamTitle,
                    s.ExamDate.ToString("yyyy-MM-dd"),
                    s.TimeWindow,
                    s.VenueName,
                    s.RoomNumber ?? "General",
                    $"{s.BookedSeats} / {s.MaxSeats} (Left: {s.AvailableSeats})",
                    s.AssignedProctor ?? "Unassigned",
                    s.Status
                );
                dgvSchedules.Rows[idx].Tag = s;

                if (s.Status == "Scheduled")
                    dgvSchedules.Rows[idx].Cells["Status"].Style.ForeColor = UiTheme.Success;
                else if (s.Status == "Cancelled")
                    dgvSchedules.Rows[idx].Cells["Status"].Style.ForeColor = UiTheme.Danger;
                else
                    dgvSchedules.Rows[idx].Cells["Status"].Style.ForeColor = UiTheme.Secondary;
            }

            if (dgvSchedules.Rows.Count > 0)
            {
                dgvSchedules.Rows[0].Selected = true;
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
            }
        }

        private void EditSelectedSchedule()
        {
            if (dgvSchedules.CurrentRow?.Tag is ExamSchedule s)
                ShowScheduleEditor(s);
        }

        private void ShowScheduleEditor(ExamSchedule existing)
        {
            using (var dlg = new Form())
            {
                bool isEdit = existing != null;
                dlg.Text = isEdit ? $"Edit Schedule Session #{existing.ScheduleId}" : "Schedule New Examination Window";
                dlg.Size = new Size(540, 560);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.White;

                var pnlH = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UiTheme.SidebarDark, Padding = new Padding(16, 12, 16, 12) };
                var lblH = new Label { Text = isEdit ? "UPDATE SCHEDULE SESSION" : "CONFIGURE NEW EXAM SESSION", Font = UiTheme.SectionFont, ForeColor = Color.White, Location = new Point(16, 16), AutoSize = true };
                pnlH.Controls.Add(lblH);
                dlg.Controls.Add(pnlH);

                int y = 75;

                // Exam Dropdown
                var lblExamP = new Label { Text = "Certification Exam Offering:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var cmbEx = new ComboBox { Location = new Point(24, y + 20), Width = 470, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
                var exams = examService.GetAllExams(activeOnly: true);
                int exSelect = 0;
                for (int i = 0; i < exams.Count; i++)
                {
                    cmbEx.Items.Add(new ComboBoxItem { Text = $"{exams[i].ExamCode} - {exams[i].Title}", Value = exams[i].ExamId });
                    if (existing != null && exams[i].ExamId == existing.ExamId) exSelect = i;
                }
                if (cmbEx.Items.Count > 0) cmbEx.SelectedIndex = exSelect;
                dlg.Controls.Add(lblExamP);
                dlg.Controls.Add(cmbEx);

                // Venue Dropdown
                y += 55;
                var lblVenueP = new Label { Text = "Authorized Testing Center & Room:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var cmbVen = new ComboBox { Location = new Point(24, y + 20), Width = 470, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
                var venues = venueService.GetAllVenues(activeOnly: true);
                int venSelect = 0;
                for (int i = 0; i < venues.Count; i++)
                {
                    cmbVen.Items.Add(new ComboBoxItem { Text = $"{venues[i].VenueName} ({venues[i].City} - {venues[i].RoomNumber}) - Max {venues[i].TotalCapacity} seats", Value = venues[i].VenueId });
                    if (existing != null && venues[i].VenueId == existing.VenueId) venSelect = i;
                }
                if (cmbVen.Items.Count > 0) cmbVen.SelectedIndex = venSelect;
                dlg.Controls.Add(lblVenueP);
                dlg.Controls.Add(cmbVen);

                // Date & Time
                y += 55;
                var lblDateP = new Label { Text = "Exam Date:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var dtpDate = new DateTimePicker { Location = new Point(24, y + 20), Width = 140, Format = DateTimePickerFormat.Short, Value = existing?.ExamDate ?? DateTime.Today.AddDays(7), Font = UiTheme.BodyFont };

                var lblStartP = new Label { Text = "Start Time:", Font = UiTheme.BodyBold, Location = new Point(180, y), AutoSize = true };
                var dtpStart = new DateTimePicker { Location = new Point(180, y + 20), Width = 140, Format = DateTimePickerFormat.Time, ShowUpDown = true, Value = DateTime.Today.Add(existing?.StartTime ?? new TimeSpan(9, 0, 0)), Font = UiTheme.BodyFont };

                var lblEndP = new Label { Text = "End Time:", Font = UiTheme.BodyBold, Location = new Point(340, y), AutoSize = true };
                var dtpEnd = new DateTimePicker { Location = new Point(340, y + 20), Width = 154, Format = DateTimePickerFormat.Time, ShowUpDown = true, Value = DateTime.Today.Add(existing?.EndTime ?? new TimeSpan(11, 30, 0)), Font = UiTheme.BodyFont };

                dlg.Controls.Add(lblDateP);
                dlg.Controls.Add(dtpDate);
                dlg.Controls.Add(lblStartP);
                dlg.Controls.Add(dtpStart);
                dlg.Controls.Add(lblEndP);
                dlg.Controls.Add(dtpEnd);

                // Max Seats & Proctor
                y += 55;
                var lblSeatsP = new Label { Text = "Max Seats Available:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var numSeats = new NumericUpDown { Location = new Point(24, y + 20), Width = 140, Minimum = 1, Maximum = 500, Value = existing?.MaxSeats ?? 25, Font = UiTheme.BodyFont };

                var lblProcP = new Label { Text = "Assigned Proctor / Invigilator:", Font = UiTheme.BodyBold, Location = new Point(180, y), AutoSize = true };
                var txtProc = new TextBox { Text = existing?.AssignedProctor ?? "Lead Proctor", Location = new Point(180, y + 20), Width = 314, Font = UiTheme.BodyFont };

                dlg.Controls.Add(lblSeatsP);
                dlg.Controls.Add(numSeats);
                dlg.Controls.Add(lblProcP);
                dlg.Controls.Add(txtProc);

                // Status
                y += 55;
                var lblStatusP = new Label { Text = "Schedule Status:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var cmbStat = new ComboBox { Location = new Point(24, y + 20), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
                cmbStat.Items.AddRange(new object[] { "Scheduled", "Completed", "Cancelled" });
                cmbStat.SelectedItem = existing?.Status ?? "Scheduled";
                dlg.Controls.Add(lblStatusP);
                dlg.Controls.Add(cmbStat);

                // Buttons
                y += 65;
                var btnSave = new Button { Text = isEdit ? "Update Schedule" : "Save Schedule", Location = new Point(24, y), Width = 150, Height = 36 };
                UiTheme.StyleButton(btnSave, isPrimary: true);

                var btnCancel = new Button { Text = "Cancel", Location = new Point(184, y), Width = 90, Height = 36 };
                UiTheme.StyleButton(btnCancel, isPrimary: false);
                btnCancel.Click += (s, ev) => dlg.DialogResult = DialogResult.Cancel;

                btnSave.Click += (s, ev) =>
                {
                    if (cmbEx.SelectedItem == null || cmbVen.SelectedItem == null)
                    {
                        MessageBox.Show("Please select both an exam and a testing venue.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int exId = (int)((ComboBoxItem)cmbEx.SelectedItem).Value;
                    int venId = (int)((ComboBoxItem)cmbVen.SelectedItem).Value;

                    TimeSpan startTime = dtpStart.Value.TimeOfDay;
                    TimeSpan endTime = dtpEnd.Value.TimeOfDay;

                    if (endTime <= startTime)
                    {
                        MessageBox.Show("End time must be later than start time.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var sched = new ExamSchedule
                    {
                        ScheduleId = existing?.ScheduleId ?? 0,
                        ExamId = exId,
                        VenueId = venId,
                        ExamDate = dtpDate.Value.Date,
                        StartTime = startTime,
                        EndTime = endTime,
                        MaxSeats = (int)numSeats.Value,
                        AssignedProctor = txtProc.Text.Trim(),
                        Status = cmbStat.SelectedItem.ToString()
                    };

                    bool success;
                    string err;
                    if (isEdit)
                        success = scheduleService.UpdateSchedule(sched, out err);
                    else
                        success = scheduleService.CreateSchedule(sched, out err);

                    if (success)
                    {
                        MessageBox.Show("Schedule session saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    LoadSchedules();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvSchedules.CurrentRow?.Tag is ExamSchedule s)
            {
                var confirm = MessageBox.Show($"Are you sure you want to cancel or delete session #{s.ScheduleId} on {s.ExamDate:yyyy-MM-dd} ({s.TimeWindow})?", "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    bool ok = scheduleService.DeleteSchedule(s.ScheduleId, out string error);
                    if (ok)
                    {
                        MessageBox.Show("Schedule updated.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSchedules();
                    }
                    else
                    {
                        MessageBox.Show(error, "Action Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
