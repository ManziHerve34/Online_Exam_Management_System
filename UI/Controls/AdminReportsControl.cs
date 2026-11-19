using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Controls
{
    public class AdminReportsControl : UserControl
    {
        private TabControl tabReports;
        private TabPage tabDaily;
        private TabPage tabFees;
        private TabPage tabOccupancy;

        // Daily Registrations Controls
        private DateTimePicker dtpDailyFrom;
        private DateTimePicker dtpDailyTo;
        private DataGridView dgvDaily;
        private Label lblDailySummary;

        // Fee Collections Controls
        private DateTimePicker dtpFeesFrom;
        private DateTimePicker dtpFeesTo;
        private DataGridView dgvFees;
        private Label lblFeesSummary;

        // Venue Occupancy Controls
        private DataGridView dgvOccupancy;
        private Label lblOccupancySummary;

        private readonly ReportService reportService = new ReportService();

        public AdminReportsControl()
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
                RowCount = 2,
                Padding = new Padding(24, 20, 24, 20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Tabs

            // Header
            var pnlHeader = new Panel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 16) };
            var lblTitle = new Label
            {
                Text = "Administrative Reports & Statistical Intelligence",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Comprehensive operational reporting for daily registrations, on-site fee collections, and test center seating occupancy rates (FR-4.4).",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Tab Control
            tabReports = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.BodyBold
            };

            // 1. Daily Registrations Tab
            tabDaily = new TabPage("📅 Daily Registrations Report");
            tabDaily.BackColor = Color.White;
            SetupDailyTab(tabDaily);
            tabReports.TabPages.Add(tabDaily);

            // 2. Fee Collections Tab
            tabFees = new TabPage("💵 Fee Collections & Revenue Report");
            tabFees.BackColor = Color.White;
            SetupFeesTab(tabFees);
            tabReports.TabPages.Add(tabFees);

            // 3. Venue Occupancy Tab
            tabOccupancy = new TabPage("🏛️ Venue Occupancy & Seating Utilization");
            tabOccupancy.BackColor = Color.White;
            SetupOccupancyTab(tabOccupancy);
            tabReports.TabPages.Add(tabOccupancy);

            mainLayout.Controls.Add(tabReports, 0, 1);
            this.Controls.Add(mainLayout);

            this.Load += (s, e) =>
            {
                LoadDailyRegistrations();
                LoadFeeCollections();
                LoadVenueOccupancy();
            };
        }

        private void SetupDailyTab(TabPage tab)
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(12) };

            var lblFrom = new Label { Text = "From Date:", Font = UiTheme.BodyBold, Location = new Point(12, 16), AutoSize = true };
            dtpDailyFrom = new DateTimePicker { Location = new Point(85, 14), Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30), Font = UiTheme.BodyFont };

            var lblTo = new Label { Text = "To Date:", Font = UiTheme.BodyBold, Location = new Point(220, 16), AutoSize = true };
            dtpDailyTo = new DateTimePicker { Location = new Point(280, 14), Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = UiTheme.BodyFont };

            var btnFilter = new Button { Text = "Generate Report", Location = new Point(415, 12), Width = 140, Height = 30 };
            UiTheme.StyleButton(btnFilter, isPrimary: true);
            btnFilter.Click += (s, e) => LoadDailyRegistrations();

            var btnExport = new Button { Text = "Export CSV", Location = new Point(565, 12), Width = 110, Height = 30 };
            UiTheme.StyleButton(btnExport, isPrimary: false);
            btnExport.Click += (s, e) => ExportDataGridToCsv(dgvDaily, "Daily_Registrations_Report.csv");

            pnlTop.Controls.Add(lblFrom);
            pnlTop.Controls.Add(dtpDailyFrom);
            pnlTop.Controls.Add(lblTo);
            pnlTop.Controls.Add(dtpDailyTo);
            pnlTop.Controls.Add(btnFilter);
            pnlTop.Controls.Add(btnExport);

            dgvDaily = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvDaily);

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(12) };
            lblDailySummary = new Label { Text = "Summary: 0 registrations", Font = UiTheme.BodyBold, ForeColor = UiTheme.Primary, Location = new Point(12, 10), AutoSize = true };
            pnlBottom.Controls.Add(lblDailySummary);

            tab.Controls.Add(dgvDaily);
            tab.Controls.Add(pnlBottom);
            tab.Controls.Add(pnlTop);
        }

        private void SetupFeesTab(TabPage tab)
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(12) };

            var lblFrom = new Label { Text = "From Date:", Font = UiTheme.BodyBold, Location = new Point(12, 16), AutoSize = true };
            dtpFeesFrom = new DateTimePicker { Location = new Point(85, 14), Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30), Font = UiTheme.BodyFont };

            var lblTo = new Label { Text = "To Date:", Font = UiTheme.BodyBold, Location = new Point(220, 16), AutoSize = true };
            dtpFeesTo = new DateTimePicker { Location = new Point(280, 14), Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = UiTheme.BodyFont };

            var btnFilter = new Button { Text = "Generate Report", Location = new Point(415, 12), Width = 140, Height = 30 };
            UiTheme.StyleButton(btnFilter, isPrimary: true);
            btnFilter.Click += (s, e) => LoadFeeCollections();

            var btnExport = new Button { Text = "Export CSV", Location = new Point(565, 12), Width = 110, Height = 30 };
            UiTheme.StyleButton(btnExport, isPrimary: false);
            btnExport.Click += (s, e) => ExportDataGridToCsv(dgvFees, "Fee_Collections_Report.csv");

            pnlTop.Controls.Add(lblFrom);
            pnlTop.Controls.Add(dtpFeesFrom);
            pnlTop.Controls.Add(lblTo);
            pnlTop.Controls.Add(dtpFeesTo);
            pnlTop.Controls.Add(btnFilter);
            pnlTop.Controls.Add(btnExport);

            dgvFees = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvFees);

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(12) };
            lblFeesSummary = new Label { Text = "Summary: Total Collected: $0.00", Font = UiTheme.BodyBold, ForeColor = UiTheme.Success, Location = new Point(12, 10), AutoSize = true };
            pnlBottom.Controls.Add(lblFeesSummary);

            tab.Controls.Add(dgvFees);
            tab.Controls.Add(pnlBottom);
            tab.Controls.Add(pnlTop);
        }

        private void SetupOccupancyTab(TabPage tab)
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(12) };

            var btnRefresh = new Button { Text = "Refresh Occupancy Analysis", Location = new Point(12, 12), Width = 210, Height = 30 };
            UiTheme.StyleButton(btnRefresh, isPrimary: true);
            btnRefresh.Click += (s, e) => LoadVenueOccupancy();

            var btnExport = new Button { Text = "Export CSV", Location = new Point(232, 12), Width = 110, Height = 30 };
            UiTheme.StyleButton(btnExport, isPrimary: false);
            btnExport.Click += (s, e) => ExportDataGridToCsv(dgvOccupancy, "Venue_Occupancy_Report.csv");

            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Controls.Add(btnExport);

            dgvOccupancy = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvOccupancy);

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(12) };
            lblOccupancySummary = new Label { Text = "Seating capacity analysis across authorized centers", Font = UiTheme.BodyBold, ForeColor = UiTheme.Primary, Location = new Point(12, 10), AutoSize = true };
            pnlBottom.Controls.Add(lblOccupancySummary);

            tab.Controls.Add(dgvOccupancy);
            tab.Controls.Add(pnlBottom);
            tab.Controls.Add(pnlTop);
        }

        private void LoadDailyRegistrations()
        {
            DataTable dt = reportService.GetDailyRegistrationsReport(dtpDailyFrom.Value.Date, dtpDailyTo.Value.Date);
            dgvDaily.Columns.Clear();
            dgvDaily.Columns.Add("Date", "Registration Date");
            dgvDaily.Columns.Add("Total", "Total Bookings");
            dgvDaily.Columns.Add("Confirmed", "Confirmed / Cleared");
            dgvDaily.Columns.Add("Pending", "Pending Payment");
            dgvDaily.Columns.Add("Cancelled", "Cancelled");
            dgvDaily.Columns.Add("Revenue", "Cleared Revenue");

            int grandTotal = 0;
            decimal grandRevenue = 0;

            foreach (DataRow row in dt.Rows)
            {
                DateTime d = Convert.ToDateTime(row["RegistrationDate"]);
                int total = Convert.ToInt32(row["TotalBookings"]);
                int conf = Convert.ToInt32(row["ConfirmedCount"]);
                int pend = Convert.ToInt32(row["PendingCount"]);
                int canc = Convert.ToInt32(row["CancelledCount"]);
                decimal rev = Convert.ToDecimal(row["ConfirmedRevenue"]);

                grandTotal += total;
                grandRevenue += rev;

                int idx = dgvDaily.Rows.Add(
                    d.ToString("yyyy-MM-dd (ddd)"),
                    total,
                    conf,
                    pend,
                    canc,
                    $"{rev:C}"
                );
            }

            lblDailySummary.Text = $"Period Totals: {grandTotal} registrations across selected date range | Cleared Revenue: {grandRevenue:C}";
        }

        private void LoadFeeCollections()
        {
            DataTable dt = reportService.GetFeeCollectionsReport(dtpFeesFrom.Value.Date, dtpFeesTo.Value.Date);
            dgvFees.Columns.Clear();
            dgvFees.Columns.Add("Receipt", "Receipt #");
            dgvFees.Columns.Add("Date", "Payment Date");
            dgvFees.Columns.Add("Amount", "Amount Paid");
            dgvFees.Columns.Add("Method", "Payment Method");
            dgvFees.Columns.Add("Ref", "Booking Ref");
            dgvFees.Columns.Add("Candidate", "Candidate");
            dgvFees.Columns.Add("Exam", "Exam");
            dgvFees.Columns.Add("Verifier", "Verified By Staff");

            decimal totalCollected = 0;
            foreach (DataRow row in dt.Rows)
            {
                decimal amt = Convert.ToDecimal(row["Amount"]);
                totalCollected += amt;

                dgvFees.Rows.Add(
                    row["ReceiptNumber"].ToString(),
                    Convert.ToDateTime(row["PaymentDate"]).ToString("yyyy-MM-dd"),
                    $"{amt:C}",
                    row["PaymentMethod"].ToString(),
                    row["BookingReference"].ToString(),
                    row["CandidateName"].ToString(),
                    $"{row["ExamCode"]} - {row["ExamTitle"]}",
                    row["VerifiedBy"].ToString()
                );
            }

            lblFeesSummary.Text = $"Total Verified Fee Collections: {totalCollected:C} ({dt.Rows.Count} transaction receipts recorded)";
        }

        private void LoadVenueOccupancy()
        {
            DataTable dt = reportService.GetVenueOccupancyReport();
            dgvOccupancy.Columns.Clear();
            dgvOccupancy.Columns.Add("Venue", "Testing Center Name");
            dgvOccupancy.Columns.Add("City", "City");
            dgvOccupancy.Columns.Add("Room", "Room / Suite");
            dgvOccupancy.Columns.Add("Capacity", "Room Capacity");
            dgvOccupancy.Columns.Add("Sessions", "Scheduled Sessions");
            dgvOccupancy.Columns.Add("Offered", "Total Offered Seats");
            dgvOccupancy.Columns.Add("Booked", "Total Booked Seats");
            dgvOccupancy.Columns.Add("Rate", "Occupancy Rate (%)");

            int totalOffered = 0;
            int totalBooked = 0;

            foreach (DataRow row in dt.Rows)
            {
                int offered = Convert.ToInt32(row["TotalOfferedSeats"]);
                int booked = Convert.ToInt32(row["TotalBookedSeats"]);
                decimal rate = Convert.ToDecimal(row["OccupancyRatePercent"]);

                totalOffered += offered;
                totalBooked += booked;

                int idx = dgvOccupancy.Rows.Add(
                    row["VenueName"].ToString(),
                    row["City"].ToString(),
                    row["Room"]?.ToString() ?? "General",
                    $"{row["VenueRoomCapacity"]} Seats",
                    row["TotalSessionsScheduled"],
                    offered,
                    booked,
                    $"{rate:F1}%"
                );

                var r = dgvOccupancy.Rows[idx];
                if (rate >= 75)
                    r.Cells["Rate"].Style.ForeColor = UiTheme.Success;
                else if (rate >= 40)
                    r.Cells["Rate"].Style.ForeColor = UiTheme.Warning;
                else
                    r.Cells["Rate"].Style.ForeColor = UiTheme.Secondary;
            }

            decimal overallRate = totalOffered > 0 ? (totalBooked * 100m / totalOffered) : 0;
            lblOccupancySummary.Text = $"System-Wide Seating Capacity: {totalBooked} booked of {totalOffered} offered seats ({overallRate:F1}% Overall Capacity Utilization)";
        }

        private void ExportDataGridToCsv(DataGridView dgv, string defaultFileName)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = defaultFileName,
                    Title = "Export Report to CSV"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        sb.Append($"\"{dgv.Columns[i].HeaderText}\"");
                        if (i < dgv.Columns.Count - 1) sb.Append(",");
                    }
                    sb.AppendLine();

                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        for (int i = 0; i < dgv.Columns.Count; i++)
                        {
                            string val = row.Cells[i].Value?.ToString() ?? "";
                            sb.Append($"\"{val.Replace("\"", "\"\"")}\"");
                            if (i < dgv.Columns.Count - 1) sb.Append(",");
                        }
                        sb.AppendLine();
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Report successfully exported to:\n{sfd.FileName}", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
