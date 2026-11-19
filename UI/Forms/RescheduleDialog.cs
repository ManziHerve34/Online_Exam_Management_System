using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Forms
{
    public class RescheduleDialog : Form
    {
        private readonly Booking currentBooking;
        private DataGridView dgvSlots;
        private Button btnConfirm;
        private Button btnCancel;
        private Label lblCurrentInfo;
        private ExamSchedule selectedSchedule;

        private readonly ScheduleService scheduleService = new ScheduleService();
        private readonly BookingService bookingService = new BookingService();

        public RescheduleDialog(Booking booking)
        {
            this.currentBooking = booking;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Reschedule Examination Booking";
            this.Size = new Size(780, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UiTheme.Background;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 110, Padding = new Padding(20, 16, 20, 16), BackColor = Color.White };
            pnlTop.Paint += UiTheme.PaintCardBorder;

            var lblTitle = new Label
            {
                Text = $"Reschedule: {currentBooking.ExamCode} - {currentBooking.ExamTitle}",
                Font = UiTheme.SubtitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(20, 16)
            };

            lblCurrentInfo = new Label
            {
                Text = $"Current Session: {currentBooking.ExamDate:yyyy-MM-dd} ({currentBooking.TimeWindow}) at {currentBooking.VenueName}\n" +
                       $"Booking Reference: {currentBooking.BookingReference}   |   Status: {currentBooking.BookingStatus}",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(20, 48)
            };

            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblCurrentInfo);
            this.Controls.Add(pnlTop);

            var pnlCenter = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16) };
            var lblPrompt = new Label
            {
                Text = "Choose an alternate available examination session:",
                Font = UiTheme.BodyBold,
                ForeColor = UiTheme.TextDark,
                Location = new Point(20, 120),
                AutoSize = true
            };

            dgvSlots = new DataGridView
            {
                Location = new Point(20, 145),
                Size = new Size(725, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            UiTheme.StyleDataGrid(dgvSlots);
            dgvSlots.SelectionChanged += DgvSlots_SelectionChanged;

            this.Controls.Add(dgvSlots);
            this.Controls.Add(lblPrompt);

            // Bottom Buttons
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White, Padding = new Padding(16) };
            pnlBottom.Paint += UiTheme.PaintCardBorder;

            btnConfirm = new Button
            {
                Text = "Confirm Reschedule",
                Location = new Point(pnlBottom.Width - 280, 12),
                Width = 160,
                Height = 36,
                Enabled = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            UiTheme.StyleButton(btnConfirm, isPrimary: true);
            btnConfirm.Click += BtnConfirm_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(pnlBottom.Width - 110, 12),
                Width = 90,
                Height = 36,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            UiTheme.StyleButton(btnCancel, isPrimary: false);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            pnlBottom.Controls.Add(btnConfirm);
            pnlBottom.Controls.Add(btnCancel);
            this.Controls.Add(pnlBottom);

            this.Load += (s, e) => LoadAvailableSlots();
        }

        private void LoadAvailableSlots()
        {
            dgvSlots.Columns.Clear();
            dgvSlots.Columns.Add("Date", "Exam Date");
            dgvSlots.Columns.Add("Time", "Time Window");
            dgvSlots.Columns.Add("Venue", "Test Center");
            dgvSlots.Columns.Add("City", "City");
            dgvSlots.Columns.Add("Available", "Seats Available");

            var slots = scheduleService.GetAvailableSchedulesForExam(currentBooking.ExamId, DateTime.Today);
            foreach (var s in slots)
            {
                if (s.ScheduleId == currentBooking.ScheduleId)
                    continue; // Skip current session

                int idx = dgvSlots.Rows.Add(
                    s.ExamDate.ToString("yyyy-MM-dd (ddd)"),
                    s.TimeWindow,
                    $"{s.VenueName} ({s.RoomNumber})",
                    s.VenueCity,
                    $"{s.AvailableSeats} / {s.MaxSeats} Seats"
                );
                dgvSlots.Rows[idx].Tag = s;
            }

            if (dgvSlots.Rows.Count > 0)
            {
                dgvSlots.Rows[0].Selected = true;
                selectedSchedule = dgvSlots.Rows[0].Tag as ExamSchedule;
                btnConfirm.Enabled = true;
            }
            else
            {
                btnConfirm.Enabled = false;
                MessageBox.Show("No alternate sessions are currently available for this exam.", "No Slots Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DgvSlots_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSlots.CurrentRow?.Tag is ExamSchedule s)
            {
                selectedSchedule = s;
                btnConfirm.Enabled = true;
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (selectedSchedule == null) return;

            var confirm = MessageBox.Show(
                $"Reschedule booking {currentBooking.BookingReference} to:\n\n" +
                $"Date: {selectedSchedule.ExamDate:yyyy-MM-dd}\n" +
                $"Time: {selectedSchedule.TimeWindow}\n" +
                $"Venue: {selectedSchedule.VenueName}\n\nProceed?",
                "Confirm Reschedule",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                bool success = bookingService.RescheduleBooking(currentBooking.BookingId, selectedSchedule.ScheduleId, SessionManager.CurrentUserId, out string error);
                if (success)
                {
                    MessageBox.Show("Booking successfully rescheduled! A revised admission ticket is available.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(error, "Reschedule Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
