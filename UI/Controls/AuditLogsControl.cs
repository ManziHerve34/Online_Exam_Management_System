using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Controls
{
    public class AuditLogsControl : UserControl
    {
        private DataGridView dgvLogs;
        private TextBox txtSearch;
        private Button btnRefresh;

        public AuditLogsControl()
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Header & Filter
            var pnlHeader = new Panel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 16) };
            var lblTitle = new Label
            {
                Text = "Security & Operational Audit Trail",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Immutable chronological audit log of all security authentication events, scheduling transactions, and payment verifications.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };

            var pnlFilter = new Panel { Dock = DockStyle.Bottom, Height = 45, Location = new Point(0, 65), BackColor = Color.White, Padding = new Padding(12) };
            pnlFilter.Paint += UiTheme.PaintCardBorder;

            var lblS = new Label { Text = "Filter Action / Keyword:", Font = UiTheme.BodyBold, Location = new Point(12, 14), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(165, 12), Width = 260, Font = UiTheme.BodyFont };
            txtSearch.TextChanged += (s, e) => LoadLogs();

            btnRefresh = new Button { Text = "Refresh Log", Location = new Point(440, 10), Width = 110, Height = 28 };
            UiTheme.StyleButton(btnRefresh, isPrimary: false);
            btnRefresh.Click += (s, e) => LoadLogs();

            pnlFilter.Controls.Add(lblS);
            pnlFilter.Controls.Add(txtSearch);
            pnlFilter.Controls.Add(btnRefresh);

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Controls.Add(pnlFilter);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Table
            var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            pnlGrid.Paint += UiTheme.PaintCardBorder;
            dgvLogs = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvLogs);

            pnlGrid.Controls.Add(dgvLogs);
            mainLayout.Controls.Add(pnlGrid, 0, 1);

            this.Controls.Add(mainLayout);
            this.Load += (s, e) =>
            {
                SetupGridColumns();
                LoadLogs();
            };
        }

        private void SetupGridColumns()
        {
            dgvLogs.Columns.Clear();
            dgvLogs.Columns.Add("Timestamp", "Timestamp");
            dgvLogs.Columns.Add("Action", "Audit Action");
            dgvLogs.Columns.Add("User", "Initiating Account");
            dgvLogs.Columns.Add("Details", "Event Details & Parameters");
            dgvLogs.Columns.Add("Ip", "Client IP");

            dgvLogs.Columns["Timestamp"].Width = 150;
            dgvLogs.Columns["Action"].Width = 180;
            dgvLogs.Columns["User"].Width = 190;
            dgvLogs.Columns["Details"].Width = 350;
            dgvLogs.Columns["Ip"].Width = 90;
        }

        public void LoadLogs()
        {
            var list = AuditService.GetAuditLogs(null, txtSearch.Text, 150);
            dgvLogs.Rows.Clear();

            foreach (var log in list)
            {
                int idx = dgvLogs.Rows.Add(
                    log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    log.Action,
                    log.UserEmail,
                    log.Details,
                    log.IpAddress
                );

                var r = dgvLogs.Rows[idx];
                if (log.Action.Contains("PAYMENT"))
                    r.Cells["Action"].Style.ForeColor = UiTheme.Success;
                else if (log.Action.Contains("CANCEL") || log.Action.Contains("DELET"))
                    r.Cells["Action"].Style.ForeColor = UiTheme.Danger;
                else if (log.Action.Contains("LOGIN"))
                    r.Cells["Action"].Style.ForeColor = UiTheme.Primary;
            }
        }
    }
}
