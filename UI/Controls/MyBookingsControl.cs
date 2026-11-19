using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;
using OnlineExamsSystem.UI.Forms;

namespace OnlineExamsSystem.UI.Controls
{
    public class MyBookingsControl : UserControl
    {
        private RadioButton rbAll;
        private RadioButton rbUpcoming;
        private RadioButton rbPast;
        private RadioButton rbCancelled;
        private DataGridView dgvBookings;
        private Button btnViewTicket;
        private Button btnReschedule;
        private Button btnCancel;
        private Button btnRefresh;
        private Label lblCutoffNotice;

        private readonly BookingService bookingService = new BookingService();

        public MyBookingsControl()
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Actions Bar

            // Header Panel
            var pnlHeader = new Panel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 16) };
            var lblTitle = new Label
            {
                Text = "My Examination Schedules & Bookings",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Review your registered certification exams, track on-site clearance status, download admission tickets, or reschedule.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };

            // Filter Tabs (Radio buttons styled as tabs)
            var pnlTabs = new Panel
            {
                Location = new Point(0, 65),
                Height = 36,
                Dock = DockStyle.Bottom
            };

            rbUpcoming = new RadioButton { Text = "Upcoming Exams", Checked = true, Location = new Point(0, 6), Width = 140, Font = UiTheme.BodyBold, AutoSize = true };
            rbUpcoming.CheckedChanged += (s, e) => { if (rbUpcoming.Checked) LoadBookings("Upcoming"); };

            rbPast = new RadioButton { Text = "Past / Completed", Location = new Point(160, 6), Width = 140, Font = UiTheme.BodyBold, AutoSize = true };
            rbPast.CheckedChanged += (s, e) => { if (rbPast.Checked) LoadBookings("Past"); };

            rbCancelled = new RadioButton { Text = "Cancelled Bookings", Location = new Point(320, 6), Width = 150, Font = UiTheme.BodyBold, AutoSize = true };
            rbCancelled.CheckedChanged += (s, e) => { if (rbCancelled.Checked) LoadBookings("Cancelled"); };

            rbAll = new RadioButton { Text = "All Bookings", Location = new Point(490, 6), Width = 120, Font = UiTheme.BodyBold, AutoSize = true };
            rbAll.CheckedChanged += (s, e) => { if (rbAll.Checked) LoadBookings("All"); };

            pnlTabs.Controls.Add(rbUpcoming);
            pnlTabs.Controls.Add(rbPast);
            pnlTabs.Controls.Add(rbCancelled);
            pnlTabs.Controls.Add(rbAll);

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Controls.Add(pnlTabs);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Table Panel
            var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            pnlGrid.Paint += UiTheme.PaintCardBorder;

            dgvBookings = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvBookings);
            dgvBookings.SelectionChanged += DgvBookings_SelectionChanged;
            dgvBookings.CellDoubleClick += (s, e) => OpenSelectedTicket();

            pnlGrid.Controls.Add(dgvBookings);
            mainLayout.Controls.Add(pnlGrid, 0, 1);

            // Action Buttons Bar
            var pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(12),
                Margin = new Padding(0, 16, 0, 0)
            };
            pnlActions.Paint += UiTheme.PaintCardBorder;

            btnViewTicket = new Button
            {
                Text = "🎟️ View & Print Admission Ticket",
                Location = new Point(12, 12),
                Width = 230,
                Height = 36,
                Enabled = false
            };
            UiTheme.StyleButton(btnViewTicket, isPrimary: true);
            btnViewTicket.Click += (s, e) => OpenSelectedTicket();

            btnReschedule = new Button
            {
                Text = "🔄 Reschedule Exam",
                Location = new Point(255, 12),
                Width = 160,
                Height = 36,
                Enabled = false
            };
            UiTheme.StyleButton(btnReschedule, isPrimary: false);
            btnReschedule.Click += BtnReschedule_Click;

            btnCancel = new Button
            {
                Text = "❌ Cancel Booking",
                Location = new Point(425, 12),
                Width = 150,
                Height = 36,
                Enabled = false
            };
            UiTheme.StyleButton(btnCancel, isPrimary: false, isDanger: true);
            btnCancel.Click += BtnCancel_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(585, 12),
                Width = 90,
                Height = 36
            };
            UiTheme.StyleButton(btnRefresh, isPrimary: false);
            btnRefresh.Click += (s, e) => RefreshCurrentFilter();

            lblCutoffNotice = new Label
            {
                Text = $"* Rescheduling & cancellation are allowed up to {DbConfig.RescheduleCutoffHours} hours prior to scheduled exam window.",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.TextMuted,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlActions.Width - 460, 20),
                AutoSize = true
            };

            pnlActions.Controls.Add(btnViewTicket);
            pnlActions.Controls.Add(btnReschedule);
            pnlActions.Controls.Add(btnCancel);
            pnlActions.Controls.Add(btnRefresh);
            pnlActions.Controls.Add(lblCutoffNotice);

            mainLayout.Controls.Add(pnlActions, 0, 2);
            this.Controls.Add(mainLayout);

            this.Load += (s, e) =>
            {
                SetupGridColumns();
                LoadBookings("Upcoming");
            };
        }

        private void SetupGridColumns()
        {
            dgvBookings.Columns.Clear();
            dgvBookings.Columns.Add("Ref", "Booking Ref");
            dgvBookings.Columns.Add("ExamCode", "Exam Code");
            dgvBookings.Columns.Add("Title", "Certification Title");
            dgvBookings.Columns.Add("Date", "Exam Date");
            dgvBookings.Columns.Add("Time", "Time Window");
            dgvBookings.Columns.Add("Venue", "Testing Center");
            dgvBookings.Columns.Add("Status", "Clearance Status");
            dgvBookings.Columns.Add("Fee", "Fee");
            dgvBookings.Columns.Add("BookedOn", "Booked On");

            dgvBookings.Columns["Ref"].Width = 130;
            dgvBookings.Columns["ExamCode"].Width = 110;
            dgvBookings.Columns["Title"].Width = 220;
            dgvBookings.Columns["Date"].Width = 100;
            dgvBookings.Columns["Time"].Width = 130;
            dgvBookings.Columns["Venue"].Width = 180;
            dgvBookings.Columns["Status"].Width = 160;
            dgvBookings.Columns["Fee"].Width = 80;
        }

        public void LoadBookings(string filter)
        {
            var list = bookingService.GetCandidateBookings(SessionManager.CurrentUserId, filter);
            dgvBookings.Rows.Clear();

            btnViewTicket.Enabled = false;
            btnReschedule.Enabled = false;
            btnCancel.Enabled = false;

            foreach (var b in list)
            {
                int idx = dgvBookings.Rows.Add(
                    b.BookingReference,
                    b.ExamCode,
                    b.ExamTitle,
                    b.ExamDate.ToString("yyyy-MM-dd"),
                    b.TimeWindow,
                    $"{b.VenueName} ({b.RoomNumber})",
                    b.BookingStatus,
                    $"{b.ExamFee:C}",
                    b.BookingDate.ToString("yyyy-MM-dd HH:mm")
                );

                var row = dgvBookings.Rows[idx];
                row.Tag = b;

                if (b.IsConfirmed)
                    row.Cells["Status"].Style.ForeColor = UiTheme.Success;
                else if (b.IsPendingPayment)
                    row.Cells["Status"].Style.ForeColor = UiTheme.Warning;
                else if (b.IsCancelled)
                    row.Cells["Status"].Style.ForeColor = UiTheme.Danger;
                else
                    row.Cells["Status"].Style.ForeColor = UiTheme.Secondary;
            }

            if (dgvBookings.Rows.Count > 0)
            {
                dgvBookings.Rows[0].Selected = true;
                UpdateActionButtons(dgvBookings.Rows[0].Tag as Booking);
            }
        }

        private void RefreshCurrentFilter()
        {
            string f = "All";
            if (rbUpcoming.Checked) f = "Upcoming";
            else if (rbPast.Checked) f = "Past";
            else if (rbCancelled.Checked) f = "Cancelled";
            LoadBookings(f);
        }

        private void DgvBookings_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow?.Tag is Booking b)
            {
                UpdateActionButtons(b);
            }
            else
            {
                btnViewTicket.Enabled = false;
                btnReschedule.Enabled = false;
                btnCancel.Enabled = false;
            }
        }

        private void UpdateActionButtons(Booking b)
        {
            if (b == null) return;
            btnViewTicket.Enabled = true;

            int cutoffHours = DbConfig.RescheduleCutoffHours;
            bool canModify = b.CanModify(cutoffHours);

            btnReschedule.Enabled = canModify;
            btnCancel.Enabled = canModify;
        }

        private void OpenSelectedTicket()
        {
            if (dgvBookings.CurrentRow?.Tag is Booking b)
            {
                var form = new AdmissionTicketForm(b.BookingReference);
                form.ShowDialog(this);
            }
        }

        private void BtnReschedule_Click(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow?.Tag is Booking b)
            {
                var dialog = new RescheduleDialog(b);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshCurrentFilter();
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow?.Tag is Booking b)
            {
                var confirm = MessageBox.Show(
                    $"Are you sure you want to cancel your booking for '{b.ExamCode} - {b.ExamTitle}' on {b.ExamDate:yyyy-MM-dd}?\n\n" +
                    $"Reference: {b.BookingReference}\n\n" +
                    $"This action cannot be undone.",
                    "Confirm Cancellation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                {
                    bool success = bookingService.CancelBooking(b.BookingId, SessionManager.CurrentUserId, "Cancelled by candidate from dashboard", out string error);
                    if (success)
                    {
                        MessageBox.Show("Your exam booking has been cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshCurrentFilter();
                    }
                    else
                    {
                        MessageBox.Show(error, "Cancellation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
