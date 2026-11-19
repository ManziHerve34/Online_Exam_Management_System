using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;
using OnlineExamsSystem.UI.Forms;

namespace OnlineExamsSystem.UI.Controls
{
    public class AdminDashboardControl : UserControl
    {
        private FlowLayoutPanel kpiFlow;
        private DataGridView dgvRecentPending;
        private readonly ReportService reportService = new ReportService();
        private readonly PaymentService paymentService = new PaymentService();
        private readonly Action<string> navigateCallback;

        public AdminDashboardControl(Action<string> navigateCallback = null)
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Quick actions bar
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // KPI Cards
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Table

            // Header
            var pnlHeader = new Panel { AutoSize = true, Margin = new Padding(0, 0, 0, 16) };
            var lblTitle = new Label
            {
                Text = "Administrative & Executive Dashboard",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Comprehensive oversight of certification exam offerings, testing venues, slot capacities, and manual on-site fee verification.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Quick Actions Bar
            var pnlActions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.White,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 16)
            };
            pnlActions.Paint += UiTheme.PaintCardBorder;

            var btnVerifyPayments = new Button { Text = "💳 Verify On-Site Payments", Width = 180, Height = 32, Location = new Point(12, 8) };
            UiTheme.StyleButton(btnVerifyPayments, isPrimary: true);
            btnVerifyPayments.Click += (s, e) => navigateCallback?.Invoke("Payments");

            var btnManageSchedules = new Button { Text = "📅 Exam Schedules", Width = 150, Height = 32, Location = new Point(200, 8) };
            UiTheme.StyleButton(btnManageSchedules, isPrimary: false);
            btnManageSchedules.Click += (s, e) => navigateCallback?.Invoke("Schedules");

            var btnManageExams = new Button { Text = "📚 Exam Offerings", Width = 140, Height = 32, Location = new Point(360, 8) };
            UiTheme.StyleButton(btnManageExams, isPrimary: false);
            btnManageExams.Click += (s, e) => navigateCallback?.Invoke("Exams");

            var btnManageVenues = new Button { Text = "🏛️ Test Centers", Width = 130, Height = 32, Location = new Point(510, 8) };
            UiTheme.StyleButton(btnManageVenues, isPrimary: false);
            btnManageVenues.Click += (s, e) => navigateCallback?.Invoke("Venues");

            var btnReports = new Button { Text = "📊 System Reports", Width = 140, Height = 32, Location = new Point(650, 8) };
            UiTheme.StyleButton(btnReports, isPrimary: false);
            btnReports.Click += (s, e) => navigateCallback?.Invoke("Reports");

            pnlActions.Controls.Add(btnVerifyPayments);
            pnlActions.Controls.Add(btnManageSchedules);
            pnlActions.Controls.Add(btnManageExams);
            pnlActions.Controls.Add(btnManageVenues);
            pnlActions.Controls.Add(btnReports);
            mainLayout.Controls.Add(pnlActions, 0, 1);

            // KPI Flow
            kpiFlow = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            mainLayout.Controls.Add(kpiFlow, 0, 2);

            // Pending Payments Table
            var pnlTableSection = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16)
            };
            pnlTableSection.Paint += UiTheme.PaintCardBorder;

            var tableHeader = new Panel { Dock = DockStyle.Top, Height = 40 };
            var lblTableTitle = new Label
            {
                Text = "Action Required: Candidates Awaiting On-Site Payment Clearance",
                Font = UiTheme.SubtitleFont,
                ForeColor = UiTheme.TextDark,
                Location = new Point(0, 8),
                AutoSize = true
            };

            var btnGoToClearance = new Button
            {
                Text = "Open Payment Verification Desk →",
                Width = 240,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(tableHeader.Width - 250, 4)
            };
            UiTheme.StyleButton(btnGoToClearance, isPrimary: true);
            btnGoToClearance.Click += (s, e) => navigateCallback?.Invoke("Payments");

            tableHeader.Controls.Add(lblTableTitle);
            tableHeader.Controls.Add(btnGoToClearance);

            dgvRecentPending = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvRecentPending);
            dgvRecentPending.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && dgvRecentPending.Rows[e.RowIndex].Tag is Booking b)
                {
                    navigateCallback?.Invoke("Payments");
                }
            };

            pnlTableSection.Controls.Add(dgvRecentPending);
            pnlTableSection.Controls.Add(tableHeader);
            mainLayout.Controls.Add(pnlTableSection, 0, 3);

            this.Controls.Add(mainLayout);
            this.Load += (s, e) => LoadDashboardData();
        }

        public void LoadDashboardData()
        {
            var stats = reportService.GetDashboardStats();

            kpiFlow.Controls.Clear();
            kpiFlow.Controls.Add(UiTheme.CreateKpiCard("Total Revenue", $"{stats.TotalRevenueCollected:C}", "Verified collections", UiTheme.Success));
            kpiFlow.Controls.Add(UiTheme.CreateKpiCard("Pending Clearance", stats.PendingClearanceCount.ToString(), $"Pending: {stats.PendingRevenueAmount:C}", UiTheme.Warning));
            kpiFlow.Controls.Add(UiTheme.CreateKpiCard("Cleared Bookings", stats.ConfirmedBookingsCount.ToString(), "Ready for exams", UiTheme.Primary));
            kpiFlow.Controls.Add(UiTheme.CreateKpiCard("Total Candidates", stats.TotalCandidates.ToString(), "Registered users", UiTheme.Secondary));
            kpiFlow.Controls.Add(UiTheme.CreateKpiCard("Upcoming Sessions", stats.TotalSchedules.ToString(), "Scheduled exam windows", UiTheme.Primary));
            kpiFlow.Controls.Add(UiTheme.CreateKpiCard("Active Test Centers", stats.TotalVenues.ToString(), "Across all regions", UiTheme.Secondary));

            // Populate Pending Table
            dgvRecentPending.Columns.Clear();
            dgvRecentPending.Columns.Add("Ref", "Booking Ref");
            dgvRecentPending.Columns.Add("Candidate", "Candidate Name");
            dgvRecentPending.Columns.Add("Email", "Email");
            dgvRecentPending.Columns.Add("Exam", "Exam");
            dgvRecentPending.Columns.Add("Date", "Exam Date");
            dgvRecentPending.Columns.Add("Venue", "Test Center");
            dgvRecentPending.Columns.Add("Fee", "Fee Due");

            var pendingList = paymentService.GetBookingsForOversight(null, null, "Pending Payment");
            foreach (var b in pendingList)
            {
                int idx = dgvRecentPending.Rows.Add(
                    b.BookingReference,
                    b.CandidateName,
                    b.CandidateEmail,
                    $"{b.ExamCode} - {b.ExamTitle}",
                    $"{b.ExamDate:yyyy-MM-dd} ({b.TimeWindow})",
                    $"{b.VenueName} ({b.RoomNumber})",
                    $"{b.ExamFee:C}"
                );
                dgvRecentPending.Rows[idx].Tag = b;
                dgvRecentPending.Rows[idx].Cells["Fee"].Style.ForeColor = UiTheme.Warning;
            }
        }
    }
}
