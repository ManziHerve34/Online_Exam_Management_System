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
    public class PaymentVerificationControl : UserControl
    {
        private ComboBox cmbVenues;
        private CheckBox chkFilterDate;
        private DateTimePicker dtpDate;
        private ComboBox cmbStatus;
        private TextBox txtSearch;
        private DataGridView dgvBookings;
        private Button btnVerifyPayment;
        private Button btnViewReceipt;
        private Button btnViewTicket;
        private Button btnRefresh;
        private Label lblTotalResults;

        private readonly PaymentService paymentService = new PaymentService();
        private readonly VenueService venueService = new VenueService();

        public PaymentVerificationControl()
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header & Filters
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Table
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Actions Bar

            // Header & Filter Panel
            var pnlTop = new Panel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 16) };
            var lblTitle = new Label
            {
                Text = "On-Site Payment Verification & Clearance Desk",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Review pending exam registrations by venue and date, verify on-site candidate payments, and finalize examination clearance.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };

            // Filter Bar (FR-4.2)
            var pnlFilters = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Location = new Point(0, 65),
                BackColor = Color.White,
                Padding = new Padding(12)
            };
            pnlFilters.Paint += UiTheme.PaintCardBorder;

            var lblSearch = new Label { Text = "Search:", Font = UiTheme.BodyBold, Location = new Point(12, 16), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(68, 14), Width = 140, Font = UiTheme.BodyFont };
            txtSearch.TextChanged += (s, e) => ApplyFilters();

            var lblVenue = new Label { Text = "Venue:", Font = UiTheme.BodyBold, Location = new Point(220, 16), AutoSize = true };
            cmbVenues = new ComboBox { Location = new Point(275, 14), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
            cmbVenues.SelectedIndexChanged += (s, e) => ApplyFilters();

            chkFilterDate = new CheckBox { Text = "Filter Date:", Font = UiTheme.BodyBold, Location = new Point(470, 16), Width = 95, AutoSize = true };
            chkFilterDate.CheckedChanged += (s, e) =>
            {
                dtpDate.Enabled = chkFilterDate.Checked;
                ApplyFilters();
            };

            dtpDate = new DateTimePicker { Location = new Point(570, 14), Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Enabled = false, Font = UiTheme.BodyFont };
            dtpDate.ValueChanged += (s, e) => { if (chkFilterDate.Checked) ApplyFilters(); };

            var lblStatus = new Label { Text = "Status:", Font = UiTheme.BodyBold, Location = new Point(705, 16), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(758, 14), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
            cmbStatus.Items.AddRange(new object[] { "Pending Payment", "Confirmed / Cleared for Exam", "All Statuses" });
            cmbStatus.SelectedIndex = 0; // Default to Pending Payment
            cmbStatus.SelectedIndexChanged += (s, e) => ApplyFilters();

            var btnReset = new Button { Text = "Reset", Location = new Point(930, 12), Width = 65, Height = 28 };
            UiTheme.StyleButton(btnReset, isPrimary: false);
            btnReset.Click += (s, e) => ResetFilters();

            pnlFilters.Controls.Add(lblSearch);
            pnlFilters.Controls.Add(txtSearch);
            pnlFilters.Controls.Add(lblVenue);
            pnlFilters.Controls.Add(cmbVenues);
            pnlFilters.Controls.Add(chkFilterDate);
            pnlFilters.Controls.Add(dtpDate);
            pnlFilters.Controls.Add(lblStatus);
            pnlFilters.Controls.Add(cmbStatus);
            pnlFilters.Controls.Add(btnReset);

            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblSub);
            pnlTop.Controls.Add(pnlFilters);
            mainLayout.Controls.Add(pnlTop, 0, 0);

            // Grid Panel
            var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            pnlGrid.Paint += UiTheme.PaintCardBorder;

            dgvBookings = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvBookings);
            dgvBookings.SelectionChanged += DgvBookings_SelectionChanged;

            pnlGrid.Controls.Add(dgvBookings);
            mainLayout.Controls.Add(pnlGrid, 0, 1);

            // Actions Bar
            var pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(12),
                Margin = new Padding(0, 16, 0, 0)
            };
            pnlActions.Paint += UiTheme.PaintCardBorder;

            btnVerifyPayment = new Button
            {
                Text = "✓ Record & Clear Payment",
                Location = new Point(12, 12),
                Width = 210,
                Height = 36,
                Enabled = false
            };
            UiTheme.StyleButton(btnVerifyPayment, isPrimary: true, isSuccess: true);
            btnVerifyPayment.Click += BtnVerifyPayment_Click;

            btnViewReceipt = new Button
            {
                Text = "🧾 View Payment Receipt",
                Location = new Point(232, 12),
                Width = 190,
                Height = 36,
                Enabled = false
            };
            UiTheme.StyleButton(btnViewReceipt, isPrimary: false);
            btnViewReceipt.Click += BtnViewReceipt_Click;

            btnViewTicket = new Button
            {
                Text = "🎟️ View Admission Ticket",
                Location = new Point(432, 12),
                Width = 190,
                Height = 36,
                Enabled = false
            };
            UiTheme.StyleButton(btnViewTicket, isPrimary: false);
            btnViewTicket.Click += BtnViewTicket_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(632, 12),
                Width = 90,
                Height = 36
            };
            UiTheme.StyleButton(btnRefresh, isPrimary: false);
            btnRefresh.Click += (s, e) => ApplyFilters();

            lblTotalResults = new Label
            {
                Text = "Showing 0 registrations",
                Font = UiTheme.BodyBold,
                ForeColor = UiTheme.TextMuted,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlActions.Width - 220, 20),
                AutoSize = true
            };

            pnlActions.Controls.Add(btnVerifyPayment);
            pnlActions.Controls.Add(btnViewReceipt);
            pnlActions.Controls.Add(btnViewTicket);
            pnlActions.Controls.Add(btnRefresh);
            pnlActions.Controls.Add(lblTotalResults);

            mainLayout.Controls.Add(pnlActions, 0, 2);
            this.Controls.Add(mainLayout);

            this.Load += (s, e) =>
            {
                SetupGridColumns();
                LoadVenues();
                ApplyFilters();
            };
        }

        private void SetupGridColumns()
        {
            dgvBookings.Columns.Clear();
            dgvBookings.Columns.Add("Ref", "Booking Ref");
            dgvBookings.Columns.Add("Candidate", "Candidate Name");
            dgvBookings.Columns.Add("Email", "Candidate Email");
            dgvBookings.Columns.Add("Phone", "Phone Number");
            dgvBookings.Columns.Add("Exam", "Exam Code");
            dgvBookings.Columns.Add("Date", "Exam Date");
            dgvBookings.Columns.Add("Time", "Time Window");
            dgvBookings.Columns.Add("Venue", "Test Center");
            dgvBookings.Columns.Add("Fee", "Fee Due");
            dgvBookings.Columns.Add("Status", "Clearance Status");

            dgvBookings.Columns["Ref"].Width = 130;
            dgvBookings.Columns["Candidate"].Width = 150;
            dgvBookings.Columns["Exam"].Width = 110;
            dgvBookings.Columns["Date"].Width = 95;
            dgvBookings.Columns["Time"].Width = 125;
            dgvBookings.Columns["Venue"].Width = 160;
            dgvBookings.Columns["Fee"].Width = 80;
            dgvBookings.Columns["Status"].Width = 150;
        }

        private void LoadVenues()
        {
            cmbVenues.Items.Clear();
            cmbVenues.Items.Add(new ComboBoxItem { Text = "All Venues", Value = 0 });
            var venues = venueService.GetAllVenues();
            foreach (var v in venues)
                cmbVenues.Items.Add(new ComboBoxItem { Text = $"{v.VenueName} ({v.City})", Value = v.VenueId });
            cmbVenues.SelectedIndex = 0;
        }

        public void ApplyFilters()
        {
            int? venueId = null;
            if (cmbVenues.SelectedItem is ComboBoxItem vItem && (int)vItem.Value > 0)
                venueId = (int)vItem.Value;

            DateTime? examDate = chkFilterDate.Checked ? dtpDate.Value.Date : (DateTime?)null;
            string status = cmbStatus.SelectedItem?.ToString();
            string search = txtSearch.Text?.Trim().ToLowerInvariant();

            var list = paymentService.GetBookingsForOversight(venueId, examDate, status);
            dgvBookings.Rows.Clear();

            btnVerifyPayment.Enabled = false;
            btnViewReceipt.Enabled = false;
            btnViewTicket.Enabled = false;

            int displayedCount = 0;
            foreach (var b in list)
            {
                if (!string.IsNullOrEmpty(search))
                {
                    bool match = b.BookingReference.ToLowerInvariant().Contains(search) ||
                                 b.CandidateName.ToLowerInvariant().Contains(search) ||
                                 b.CandidateEmail.ToLowerInvariant().Contains(search) ||
                                 b.ExamCode.ToLowerInvariant().Contains(search);
                    if (!match) continue;
                }

                displayedCount++;
                int idx = dgvBookings.Rows.Add(
                    b.BookingReference,
                    b.CandidateName,
                    b.CandidateEmail,
                    b.CandidatePhone ?? "N/A",
                    b.ExamCode,
                    b.ExamDate.ToString("yyyy-MM-dd"),
                    b.TimeWindow,
                    $"{b.VenueName} ({b.RoomNumber})",
                    $"{b.ExamFee:C}",
                    b.BookingStatus
                );

                var row = dgvBookings.Rows[idx];
                row.Tag = b;

                if (b.IsConfirmed)
                    row.Cells["Status"].Style.ForeColor = UiTheme.Success;
                else if (b.IsPendingPayment)
                    row.Cells["Status"].Style.ForeColor = UiTheme.Warning;
                else
                    row.Cells["Status"].Style.ForeColor = UiTheme.Danger;
            }

            lblTotalResults.Text = $"Showing {displayedCount} registration(s)";

            if (dgvBookings.Rows.Count > 0)
            {
                dgvBookings.Rows[0].Selected = true;
                UpdateActionButtons(dgvBookings.Rows[0].Tag as Booking);
            }
        }

        private void ResetFilters()
        {
            txtSearch.Clear();
            cmbVenues.SelectedIndex = 0;
            chkFilterDate.Checked = false;
            cmbStatus.SelectedIndex = 0;
            ApplyFilters();
        }

        private void DgvBookings_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow?.Tag is Booking b)
            {
                UpdateActionButtons(b);
            }
            else
            {
                btnVerifyPayment.Enabled = false;
                btnViewReceipt.Enabled = false;
                btnViewTicket.Enabled = false;
            }
        }

        private void UpdateActionButtons(Booking b)
        {
            if (b == null) return;
            btnViewTicket.Enabled = true;
            btnVerifyPayment.Enabled = b.IsPendingPayment;
            btnViewReceipt.Enabled = b.IsConfirmed;
        }

        // FR-4.3: Manual On-site Payment Verification Dialog
        private void BtnVerifyPayment_Click(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow?.Tag is Booking b)
            {
                using (var modal = new Form())
                {
                    modal.Text = "On-Site Payment Verification";
                    modal.Size = new Size(500, 420);
                    modal.StartPosition = FormStartPosition.CenterParent;
                    modal.FormBorderStyle = FormBorderStyle.FixedDialog;
                    modal.MaximizeBox = false;
                    modal.MinimizeBox = false;
                    modal.BackColor = Color.White;

                    var pnlH = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UiTheme.SidebarDark, Padding = new Padding(16, 12, 16, 12) };
                    var lblMTitle = new Label { Text = "RECORD ON-SITE PAYMENT CLEARANCE", Font = UiTheme.SectionFont, ForeColor = Color.White, Location = new Point(16, 16), AutoSize = true };
                    pnlH.Controls.Add(lblMTitle);
                    modal.Controls.Add(pnlH);

                    int y = 75;
                    var lblInfo = new Label
                    {
                        Text = $"Candidate: {b.CandidateName}\n" +
                               $"Exam: {b.ExamCode} - {b.ExamTitle}\n" +
                               $"Booking Ref: {b.BookingReference}\n" +
                               $"Scheduled: {b.ExamDate:yyyy-MM-dd} ({b.TimeWindow})",
                        Font = UiTheme.BodyFont,
                        ForeColor = UiTheme.TextDark,
                        Location = new Point(24, y),
                        AutoSize = true
                    };
                    modal.Controls.Add(lblInfo);

                    y += 85;
                    var lblAmtP = new Label { Text = "Amount Received ($):", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                    var numAmount = new NumericUpDown
                    {
                        Location = new Point(24, y + 20),
                        Width = 200,
                        DecimalPlaces = 2,
                        Maximum = 10000,
                        Value = b.ExamFee,
                        Font = UiTheme.BodyFont
                    };
                    modal.Controls.Add(lblAmtP);
                    modal.Controls.Add(numAmount);

                    var lblMethodP = new Label { Text = "Payment Method:", Font = UiTheme.BodyBold, Location = new Point(250, y), AutoSize = true };
                    var cmbMethod = new ComboBox
                    {
                        Location = new Point(250, y + 20),
                        Width = 200,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Font = UiTheme.BodyFont
                    };
                    cmbMethod.Items.AddRange(new object[] { "Cash at Center", "Bank Slip", "Mobile Money (MoMo)", "Credit Card" });
                    cmbMethod.SelectedIndex = 0;
                    modal.Controls.Add(lblMethodP);
                    modal.Controls.Add(cmbMethod);

                    y += 60;
                    var lblRemarksP = new Label { Text = "Staff Notes / Transaction Remarks:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                    var txtRemarks = new TextBox { Location = new Point(24, y + 20), Width = 426, Height = 40, Multiline = true, Font = UiTheme.BodyFont };
                    modal.Controls.Add(lblRemarksP);
                    modal.Controls.Add(txtRemarks);

                    y += 75;
                    var btnConfirm = new Button { Text = "Confirm & Clear Candidate", Location = new Point(24, y), Width = 230, Height = 36 };
                    UiTheme.StyleButton(btnConfirm, isPrimary: true, isSuccess: true);

                    var btnCancel = new Button { Text = "Cancel", Location = new Point(270, y), Width = 100, Height = 36 };
                    UiTheme.StyleButton(btnCancel, isPrimary: false);
                    btnCancel.Click += (s, ev) => modal.DialogResult = DialogResult.Cancel;

                    btnConfirm.Click += (s, ev) =>
                    {
                        bool ok = paymentService.RecordAndVerifyPayment(
                            b.BookingId,
                            numAmount.Value,
                            cmbMethod.SelectedItem.ToString(),
                            txtRemarks.Text,
                            SessionManager.CurrentUserId,
                            out string receiptNumber,
                            out string error);

                        if (ok)
                        {
                            MessageBox.Show(
                                $"Payment recorded and verified successfully!\n\n" +
                                $"Receipt Number: {receiptNumber}\n" +
                                $"Candidate Status changed to: Confirmed / Cleared for Exam",
                                "Clearance Successful",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            modal.DialogResult = DialogResult.OK;
                        }
                        else
                        {
                            MessageBox.Show(error, "Payment Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    modal.Controls.Add(btnConfirm);
                    modal.Controls.Add(btnCancel);

                    if (modal.ShowDialog(this) == DialogResult.OK)
                    {
                        ApplyFilters();
                    }
                }
            }
        }

        private void BtnViewReceipt_Click(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow?.Tag is Booking b)
            {
                var payment = paymentService.GetPaymentByBookingId(b.BookingId);
                if (payment != null)
                {
                    MessageBox.Show(
                        $"OFFICIAL ON-SITE PAYMENT CLEARANCE RECEIPT\n" +
                        $"========================================\n\n" +
                        $"Receipt Number:   {payment.ReceiptNumber}\n" +
                        $"Payment Date:     {payment.VerifiedAt:yyyy-MM-dd HH:mm}\n" +
                        $"Candidate Name:   {payment.CandidateName}\n" +
                        $"Candidate Email:  {payment.CandidateEmail}\n" +
                        $"Exam Offering:    {payment.ExamCode} - {payment.ExamTitle}\n" +
                        $"Booking Ref:      {payment.BookingReference}\n" +
                        $"Amount Cleared:   {payment.AmountPaid:C}\n" +
                        $"Payment Method:   {payment.PaymentMethod}\n" +
                        $"Payment Status:   {payment.PaymentStatus}\n" +
                        $"Authorized Staff: {payment.VerifierName}\n" +
                        $"Staff Remarks:    {(string.IsNullOrEmpty(payment.Remarks) ? "Verified on-site" : payment.Remarks)}\n\n" +
                        $"Candidate is cleared to enter the testing hall.",
                        "Payment Receipt",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No payment record found for this booking.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnViewTicket_Click(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow?.Tag is Booking b)
            {
                var form = new AdmissionTicketForm(b.BookingReference);
                form.ShowDialog(this);
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
