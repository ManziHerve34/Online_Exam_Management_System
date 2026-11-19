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
    public class CandidateDashboardControl : UserControl
    {
        private FlowLayoutPanel kpiFlow;
        private DataGridView dgvUpcoming;
        private Label lblWelcome;
        private Panel pnlNextExam;
        private readonly BookingService bookingService = new BookingService();
        private readonly Action<string> navigateCallback;

        public CandidateDashboardControl(Action<string> navigateCallback = null)
        {
            this.navigateCallback = navigateCallback;
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
                RowCount = 4,
                AutoScroll = true,
                Padding = new Padding(24, 20, 24, 20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // KPI Cards
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Next Exam Highlight
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Table

            // Header Banner
            var pnlHeader = new Panel { AutoSize = true, Margin = new Padding(0, 0, 0, 16) };
            lblWelcome = new Label
            {
                Text = $"Welcome back, {SessionManager.CurrentUserName}!",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Manage your certification journey, schedule new exams, and review admission clearance status.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };
            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(lblSub);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // KPI Flow
            kpiFlow = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 16)
            };
            mainLayout.Controls.Add(kpiFlow, 0, 1);

            // Next Exam Highlight Panel
            pnlNextExam = new Panel
            {
                Height = 110,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 20)
            };
            pnlNextExam.Paint += UiTheme.PaintCardBorder;
            mainLayout.Controls.Add(pnlNextExam, 0, 2);

            // Upcoming Bookings Section
            var pnlTableSection = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16)
            };
            pnlTableSection.Paint += UiTheme.PaintCardBorder;

            var tableHeader = new Panel { Dock = DockStyle.Top, Height = 45 };
            var lblTableTitle = new Label
            {
                Text = "Upcoming Examination Bookings",
                Font = UiTheme.SubtitleFont,
                ForeColor = UiTheme.TextDark,
                Location = new Point(0, 8),
                AutoSize = true
            };

            var btnNewBooking = new Button
            {
                Text = "+ Book New Exam",
                Width = 150,
                Height = 32,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(tableHeader.Width - 160, 4)
            };
            UiTheme.StyleButton(btnNewBooking, isPrimary: true);
            btnNewBooking.Click += (s, e) => navigateCallback?.Invoke("BookExam");

            var btnViewAll = new Button
            {
                Text = "View All Bookings",
                Width = 140,
                Height = 32,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(tableHeader.Width - 310, 4)
            };
            UiTheme.StyleButton(btnViewAll, isPrimary: false);
            btnViewAll.Click += (s, e) => navigateCallback?.Invoke("MyBookings");

            tableHeader.Controls.Add(lblTableTitle);
            tableHeader.Controls.Add(btnViewAll);
            tableHeader.Controls.Add(btnNewBooking);

            dgvUpcoming = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvUpcoming);
            dgvUpcoming.CellDoubleClick += DgvUpcoming_CellDoubleClick;

            pnlTableSection.Controls.Add(dgvUpcoming);
            pnlTableSection.Controls.Add(tableHeader);
            mainLayout.Controls.Add(pnlTableSection, 0, 3);

            this.Controls.Add(mainLayout);
            this.Load += (s, e) => LoadDashboardData();
        }

        public void LoadDashboardData()
        {
            lblWelcome.Text = $"Welcome back, {SessionManager.CurrentUserName}!";

            var bookings = bookingService.GetCandidateBookings(SessionManager.CurrentUserId);
            int upcomingCount = 0;
            int clearedCount = 0;
            int pendingCount = 0;
            int completedCount = 0;

            Booking nextExam = null;

            foreach (var b in bookings)
            {
                if (b.IsConfirmed) clearedCount++;
                if (b.IsPendingPayment) pendingCount++;
                if (b.IsCompleted) completedCount++;

                if (b.ExamDate >= DateTime.Today && !b.IsCancelled)
                {
                    upcomingCount++;
                    if (nextExam == null || (b.ExamDate + b.StartTime) < (nextExam.ExamDate + nextExam.StartTime))
                    {
                        nextExam = b;
                    }
                }
            }

            // Update KPI cards
            kpiFlow.Controls.Clear();
            kpiFlow.Controls.Add(UiTheme.CreateKpiCard("Upcoming Exams", upcomingCount.ToString(), "Scheduled sessions", UiTheme.Primary));
            kpiFlow.Controls.Add(UiTheme.CreateKpiCard("Cleared for Exam", clearedCount.ToString(), "Payment verified", UiTheme.Success));
            kpiFlow.Controls.Add(UiTheme.CreateKpiCard("Pending Clearance", pendingCount.ToString(), "Awaiting on-site payment", UiTheme.Warning));
            kpiFlow.Controls.Add(UiTheme.CreateKpiCard("Completed Exams", completedCount.ToString(), "Certification attempts", UiTheme.Secondary));

            // Populate Next Exam Highlight
            pnlNextExam.Controls.Clear();
            if (nextExam != null)
            {
                var strip = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = nextExam.IsConfirmed ? UiTheme.Success : UiTheme.Warning };
                var lblBadge = new Label
                {
                    Text = nextExam.IsConfirmed ? "CLEARED FOR EXAM" : "PENDING ON-SITE PAYMENT",
                    BackColor = nextExam.IsConfirmed ? UiTheme.SuccessLight : UiTheme.WarningLight,
                    ForeColor = nextExam.IsConfirmed ? UiTheme.Success : UiTheme.Warning,
                    Font = UiTheme.SmallFont,
                    Location = new Point(16, 12),
                    AutoSize = true,
                    Padding = new Padding(4, 2, 4, 2)
                };

                var lblNextTitle = new Label
                {
                    Text = $"Next Exam: {nextExam.ExamCode} - {nextExam.ExamTitle}",
                    Font = UiTheme.SubtitleFont,
                    ForeColor = UiTheme.TextDark,
                    Location = new Point(16, 36),
                    AutoSize = true
                };

                var lblNextDetails = new Label
                {
                    Text = $"Date: {nextExam.ExamDate:yyyy-MM-dd} ({nextExam.TimeWindow})   |   Venue: {nextExam.VenueName} ({nextExam.RoomNumber})   |   Ref: {nextExam.BookingReference}",
                    Font = UiTheme.BodyFont,
                    ForeColor = UiTheme.TextMuted,
                    Location = new Point(16, 66),
                    AutoSize = true
                };

                var btnTicket = new Button
                {
                    Text = "🎟️ View Admission Ticket",
                    Width = 190,
                    Height = 36,
                    Location = new Point(pnlNextExam.Width - 210, 36),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                UiTheme.StyleButton(btnTicket, isPrimary: true);
                btnTicket.Click += (s, e) =>
                {
                    var ticketForm = new AdmissionTicketForm(nextExam.BookingReference);
                    ticketForm.ShowDialog(this);
                };

                pnlNextExam.Controls.Add(strip);
                pnlNextExam.Controls.Add(lblBadge);
                pnlNextExam.Controls.Add(lblNextTitle);
                pnlNextExam.Controls.Add(lblNextDetails);
                pnlNextExam.Controls.Add(btnTicket);
            }
            else
            {
                var lblNoExam = new Label
                {
                    Text = "You have no upcoming examinations scheduled. Click 'Book New Exam' to reserve an exam slot.",
                    Font = UiTheme.BodyFont,
                    ForeColor = UiTheme.TextMuted,
                    Location = new Point(20, 42),
                    AutoSize = true
                };
                pnlNextExam.Controls.Add(lblNoExam);
            }

            // Populate Grid
            dgvUpcoming.Columns.Clear();
            dgvUpcoming.Columns.Add("Reference", "Booking Ref");
            dgvUpcoming.Columns.Add("ExamCode", "Exam Code");
            dgvUpcoming.Columns.Add("Title", "Exam Title");
            dgvUpcoming.Columns.Add("Date", "Exam Date");
            dgvUpcoming.Columns.Add("Time", "Time Window");
            dgvUpcoming.Columns.Add("Venue", "Venue / Center");
            dgvUpcoming.Columns.Add("Status", "Clearance Status");
            dgvUpcoming.Columns.Add("Fee", "Fee");

            foreach (var b in bookings)
            {
                if (b.ExamDate >= DateTime.Today && !b.IsCancelled)
                {
                    int idx = dgvUpcoming.Rows.Add(
                        b.BookingReference,
                        b.ExamCode,
                        b.ExamTitle,
                        b.ExamDate.ToString("yyyy-MM-dd"),
                        b.TimeWindow,
                        $"{b.VenueName} ({b.RoomNumber})",
                        b.BookingStatus,
                        $"{b.ExamFee:C}"
                    );

                    var row = dgvUpcoming.Rows[idx];
                    row.Tag = b;
                    if (b.IsConfirmed)
                        row.Cells["Status"].Style.ForeColor = UiTheme.Success;
                    else if (b.IsPendingPayment)
                        row.Cells["Status"].Style.ForeColor = UiTheme.Warning;
                    else
                        row.Cells["Status"].Style.ForeColor = UiTheme.Danger;
                }
            }
        }

        private void DgvUpcoming_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvUpcoming.Rows[e.RowIndex].Tag is Booking b)
            {
                var ticketForm = new AdmissionTicketForm(b.BookingReference);
                ticketForm.ShowDialog(this);
            }
        }
    }
}
