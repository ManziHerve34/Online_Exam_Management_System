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
    public class BookingWizardControl : UserControl
    {
        private ComboBox cmbExams;
        private Label lblExamDetails;
        private Label lblExamFee;
        private Label lblPrereq;
        private DateTimePicker dtpExamDate;
        private ComboBox cmbVenues;
        private DataGridView dgvSlots;
        private TextBox txtNotes;
        private Button btnConfirmBooking;
        private Panel pnlSelectedSlotCard;
        private Label lblSelectedSlotSummary;

        private readonly ExamService examService = new ExamService();
        private readonly VenueService venueService = new VenueService();
        private readonly ScheduleService scheduleService = new ScheduleService();
        private readonly BookingService bookingService = new BookingService();

        private int preselectedExamId = 0;
        private ExamSchedule currentSelectedSchedule = null;
        private bool _isInitializing = true; // Guard flag to suppress events during setup

        public BookingWizardControl(int examId = 0)
        {
            this.preselectedExamId = examId;
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
                RowCount = 3,
                AutoScroll = true,
                Padding = new Padding(24, 20, 24, 20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Form Steps Card
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Slots & Confirmation Card

            // Header
            var pnlHeader = new Panel { AutoSize = true, Margin = new Padding(0, 0, 0, 16) };
            var lblTitle = new Label
            {
                Text = "Schedule & Book Certification Exam",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Select your desired certification exam, choose an authorized test center and available time slot, and confirm your seat.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Step 1 & 2 Card: Exam & Venue Selection
            var pnlSelection = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = Color.White,
                Padding = new Padding(20),
                Margin = new Padding(0, 0, 0, 16)
            };
            pnlSelection.Paint += UiTheme.PaintCardBorder;

            var lblStep1 = new Label
            {
                Text = "STEP 1: Choose Certification Examination",
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.Primary,
                Location = new Point(20, 16),
                AutoSize = true
            };

            var lblExamPrompt = new Label { Text = "Examination:", Font = UiTheme.BodyBold, Location = new Point(20, 48), AutoSize = true };
            cmbExams = new ComboBox
            {
                Location = new Point(130, 44),
                Width = 480,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.BodyFont
            };
            cmbExams.SelectedIndexChanged += CmbExams_SelectedIndexChanged;

            lblExamFee = new Label
            {
                Font = UiTheme.SubtitleFont,
                ForeColor = UiTheme.Success,
                Location = new Point(630, 44),
                AutoSize = true
            };

            lblExamDetails = new Label
            {
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                Location = new Point(130, 76),
                AutoSize = true
            };

            lblPrereq = new Label
            {
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.TextDark,
                Location = new Point(130, 98),
                AutoSize = true
            };

            // Step 2: Venue and Date
            var lblStep2 = new Label
            {
                Text = "STEP 2: Filter Available Sessions by Testing Center & Date",
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.Primary,
                Location = new Point(20, 130),
                AutoSize = true
            };

            var lblVenuePrompt = new Label { Text = "Test Center:", Font = UiTheme.BodyBold, Location = new Point(20, 164), AutoSize = true };
            cmbVenues = new ComboBox
            {
                Location = new Point(130, 160),
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.BodyFont
            };
            cmbVenues.SelectedIndexChanged += (s, e) => { if (!_isInitializing) LoadAvailableSlots(); };

            var lblDatePrompt = new Label { Text = "From Date:", Font = UiTheme.BodyBold, Location = new Point(470, 164), AutoSize = true };
            dtpExamDate = new DateTimePicker
            {
                Location = new Point(560, 160),
                Width = 140,
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today,
                Value = DateTime.Today,
                Font = UiTheme.BodyFont
            };
            dtpExamDate.ValueChanged += (s, e) => { if (!_isInitializing) LoadAvailableSlots(); };

            var btnRefreshSlots = new Button
            {
                Text = "Find Available Slots",
                Location = new Point(720, 158),
                Width = 160,
                Height = 30
            };
            UiTheme.StyleButton(btnRefreshSlots, isPrimary: false);
            btnRefreshSlots.Click += (s, e) => LoadAvailableSlots();

            pnlSelection.Controls.Add(lblStep1);
            pnlSelection.Controls.Add(lblExamPrompt);
            pnlSelection.Controls.Add(cmbExams);
            pnlSelection.Controls.Add(lblExamFee);
            pnlSelection.Controls.Add(lblExamDetails);
            pnlSelection.Controls.Add(lblPrereq);

            pnlSelection.Controls.Add(lblStep2);
            pnlSelection.Controls.Add(lblVenuePrompt);
            pnlSelection.Controls.Add(cmbVenues);
            pnlSelection.Controls.Add(lblDatePrompt);
            pnlSelection.Controls.Add(dtpExamDate);
            pnlSelection.Controls.Add(btnRefreshSlots);

            mainLayout.Controls.Add(pnlSelection, 0, 1);

            // Step 3 & 4 Card: Available Slots & Notes & Booking Confirmation
            var pnlBottomCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            pnlBottomCard.Paint += UiTheme.PaintCardBorder;

            var lblStep3 = new Label
            {
                Text = "STEP 3: Select an Available Examination Time Slot (Real-Time Seat Availability)",
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.Primary,
                Location = new Point(20, 16),
                AutoSize = true
            };

            dgvSlots = new DataGridView
            {
                Location = new Point(20, 45),
                Size = new Size(860, 160),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            UiTheme.StyleDataGrid(dgvSlots);
            dgvSlots.SelectionChanged += DgvSlots_SelectionChanged;

            // Selected Slot summary strip
            pnlSelectedSlotCard = new Panel
            {
                Location = new Point(20, 215),
                Size = new Size(860, 50),
                BackColor = UiTheme.PrimaryLight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlSelectedSlotCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(UiTheme.Primary, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlSelectedSlotCard.Width - 1, pnlSelectedSlotCard.Height - 1);
                }
            };
            lblSelectedSlotSummary = new Label
            {
                Text = "Please select a time slot above to proceed with booking.",
                Font = UiTheme.BodyBold,
                ForeColor = UiTheme.Primary,
                Location = new Point(16, 16),
                AutoSize = true
            };
            pnlSelectedSlotCard.Controls.Add(lblSelectedSlotSummary);

            // Notes and Special Accommodations
            var lblNotesPrompt = new Label
            {
                Text = "Special Accommodations / Candidate Notes (Optional):",
                Font = UiTheme.BodyBold,
                ForeColor = UiTheme.TextDark,
                Location = new Point(20, 275),
                AutoSize = true
            };

            txtNotes = new TextBox
            {
                Location = new Point(20, 300),
                Size = new Size(580, 50),
                Multiline = true,
                Font = UiTheme.BodyFont
            };

            btnConfirmBooking = new Button
            {
                Text = "✓ Confirm & Generate Ticket",
                Location = new Point(620, 300),
                Size = new Size(260, 50),
                Enabled = false
            };
            UiTheme.StyleButton(btnConfirmBooking, isPrimary: true);
            btnConfirmBooking.Click += BtnConfirmBooking_Click;

            pnlBottomCard.Controls.Add(lblStep3);
            pnlBottomCard.Controls.Add(dgvSlots);
            pnlBottomCard.Controls.Add(pnlSelectedSlotCard);
            pnlBottomCard.Controls.Add(lblNotesPrompt);
            pnlBottomCard.Controls.Add(txtNotes);
            pnlBottomCard.Controls.Add(btnConfirmBooking);

            mainLayout.Controls.Add(pnlBottomCard, 0, 2);
            this.Controls.Add(mainLayout);

            this.Load += (s, e) => LoadFormDependencies();
        }

        private void LoadFormDependencies()
        {
            _isInitializing = true;

            // Step 1: Always set up grid columns FIRST before anything can fire LoadAvailableSlots
            SetupSlotsGrid();

            // Step 2: Populate Venues dropdown (must come before exams so grid is ready)
            cmbVenues.Items.Clear();
            cmbVenues.Items.Add(new ComboBoxItem { Text = "— All Test Centers (Any Location) —", Value = 0 });
            var venues = venueService.GetAllVenues(activeOnly: true);
            foreach (var v in venues)
                cmbVenues.Items.Add(new ComboBoxItem { Text = $"{v.VenueName}  •  {v.City} ({v.RoomNumber})", Value = v.VenueId });
            if (cmbVenues.Items.Count > 0)
                cmbVenues.SelectedIndex = 0;

            // Step 3: Populate Exams dropdown
            cmbExams.Items.Clear();
            var exams = examService.GetAllExams(activeOnly: true);
            int selectIdx = 0;
            for (int i = 0; i < exams.Count; i++)
            {
                var ex = exams[i];
                cmbExams.Items.Add(new ComboBoxItem { Text = $"{ex.ExamCode} - {ex.Title}", Value = ex.ExamId, Tag = ex });
                if (ex.ExamId == preselectedExamId)
                    selectIdx = i;
            }

            // Step 4: Lift guard BEFORE setting SelectedIndex so the handler fires cleanly once
            _isInitializing = false;

            if (cmbExams.Items.Count > 0)
                cmbExams.SelectedIndex = selectIdx; // triggers CmbExams_SelectedIndexChanged → LoadAvailableSlots
            else
                LoadAvailableSlots(); // no exams, still try to show empty state
        }

        private void SetupSlotsGrid()
        {
            dgvSlots.Columns.Clear();
            dgvSlots.Columns.Add("Date", "Exam Date");
            dgvSlots.Columns.Add("Time", "Time Window");
            dgvSlots.Columns.Add("Venue", "Test Center & Room");
            dgvSlots.Columns.Add("City", "City");
            dgvSlots.Columns.Add("Available", "Seats Available");
            dgvSlots.Columns.Add("Proctor", "Proctor");

            dgvSlots.Columns["Date"].Width = 110;
            dgvSlots.Columns["Time"].Width = 140;
            dgvSlots.Columns["Venue"].Width = 230;
            dgvSlots.Columns["City"].Width = 100;
            dgvSlots.Columns["Available"].Width = 120;
            dgvSlots.Columns["Proctor"].Width = 150;
        }

        private void CmbExams_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // skip during bulk setup
            if (cmbExams.SelectedItem is ComboBoxItem item && item.Tag is Exam ex)
            {
                lblExamFee.Text = $"Fee: {ex.Fee:C}";
                lblExamDetails.Text = $"Category: {ex.CategoryName}   |   Level: {ex.CertificationLevel}   |   Duration: {ex.DurationMinutes} mins   |   Pass: {ex.PassingScore}%";
                lblPrereq.Text = $"Prerequisites: {(string.IsNullOrEmpty(ex.Prerequisites) ? "None specified" : ex.Prerequisites)}";
                LoadAvailableSlots();
            }
        }

        private void LoadAvailableSlots()
        {
            if (_isInitializing) return;

            // Safety net: ensure grid columns exist before adding rows
            if (dgvSlots.Columns.Count == 0)
                SetupSlotsGrid();

            if (!(cmbExams.SelectedItem is ComboBoxItem item)) return;
            int examId = (int)item.Value;

            // Update exam detail labels if not already set
            if (item.Tag is Exam ex)
            {
                lblExamFee.Text = $"Fee: {ex.Fee:C}";
                lblExamDetails.Text = $"Category: {ex.CategoryName}   |   Level: {ex.CertificationLevel}   |   Duration: {ex.DurationMinutes} mins   |   Pass: {ex.PassingScore}%";
                lblPrereq.Text = $"Prerequisites: {(string.IsNullOrEmpty(ex.Prerequisites) ? "None specified" : ex.Prerequisites)}";
            }

            int selectedVenueId = 0;
            if (cmbVenues.SelectedItem is ComboBoxItem vItem)
                selectedVenueId = (int)vItem.Value;

            // Use the picker date as minimum; never go earlier than today
            DateTime minDate = dtpExamDate.Value.Date < DateTime.Today ? DateTime.Today : dtpExamDate.Value.Date;

            Cursor.Current = Cursors.WaitCursor;
            var availableSlots = scheduleService.GetAvailableSchedulesForExam(examId, minDate);
            Cursor.Current = Cursors.Default;

            dgvSlots.Rows.Clear();
            currentSelectedSchedule = null;
            btnConfirmBooking.Enabled = false;
            lblSelectedSlotSummary.Text = "Please select a time slot above to proceed with booking.";
            lblSelectedSlotSummary.ForeColor = UiTheme.Primary;

            foreach (var s in availableSlots)
            {
                // Apply venue filter if a specific test center was selected
                if (selectedVenueId > 0 && s.VenueId != selectedVenueId)
                    continue;

                int idx = dgvSlots.Rows.Add(
                    s.ExamDate.ToString("yyyy-MM-dd (ddd)"),
                    s.TimeWindow,
                    $"{s.VenueName} ({s.RoomNumber})",
                    s.VenueCity,
                    $"{s.AvailableSeats} / {s.MaxSeats} Seats",
                    s.AssignedProctor ?? "Assigned Staff"
                );

                var row = dgvSlots.Rows[idx];
                row.Tag = s;

                // Color seats indicator: red when ≤5 seats, green otherwise
                if (s.AvailableSeats <= 5)
                    row.Cells["Available"].Style.ForeColor = UiTheme.Danger;
                else
                    row.Cells["Available"].Style.ForeColor = UiTheme.Success;
            }

            if (dgvSlots.Rows.Count > 0)
            {
                dgvSlots.Rows[0].Selected = true;
                SelectSchedule(dgvSlots.Rows[0].Tag as ExamSchedule);
            }
            else
            {
                string venueHint = selectedVenueId > 0 ? " at the selected test center" : "";
                lblSelectedSlotSummary.Text = $"No open exam slots found{venueHint} from {minDate:yyyy-MM-dd}. Try a different date or test center.";
                lblSelectedSlotSummary.ForeColor = UiTheme.Danger;
                btnConfirmBooking.Enabled = false;
            }
        }

        private void DgvSlots_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSlots.CurrentRow?.Tag is ExamSchedule s)
            {
                SelectSchedule(s);
            }
        }

        private void SelectSchedule(ExamSchedule s)
        {
            if (s == null) return;
            currentSelectedSchedule = s;
            lblSelectedSlotSummary.Text = $"SELECTED SESSION: {s.ExamDate:yyyy-MM-dd} ({s.TimeWindow}) at {s.VenueName} ({s.RoomNumber}, {s.VenueCity}) - {s.AvailableSeats} Seats Left";
            lblSelectedSlotSummary.ForeColor = UiTheme.Primary;
            btnConfirmBooking.Enabled = true;
        }

        private void BtnConfirmBooking_Click(object sender, EventArgs e)
        {
            if (currentSelectedSchedule == null)
            {
                MessageBox.Show("Please select an available examination time slot.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Are you sure you want to schedule this exam session?\n\n" +
                $"Exam: {currentSelectedSchedule.ExamCode} - {currentSelectedSchedule.ExamTitle}\n" +
                $"Date: {currentSelectedSchedule.ExamDate:yyyy-MM-dd}\n" +
                $"Time: {currentSelectedSchedule.TimeWindow}\n" +
                $"Venue: {currentSelectedSchedule.VenueName} ({currentSelectedSchedule.RoomNumber})\n" +
                $"Fee: {currentSelectedSchedule.ExamFee:C}\n\n" +
                $"Please note: Your status will be set to 'Pending Payment'. On-site payment must be completed at the test center prior to taking the exam.",
                "Confirm Exam Booking",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            btnConfirmBooking.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            bool success = bookingService.BookExamSlot(
                SessionManager.CurrentUserId,
                currentSelectedSchedule.ScheduleId,
                txtNotes.Text,
                out Booking createdBooking,
                out string error);

            Cursor.Current = Cursors.Default;
            btnConfirmBooking.Enabled = true;

            if (success && createdBooking != null)
            {
                MessageBox.Show(
                    $"Booking Successful!\n\n" +
                    $"Booking Reference: {createdBooking.BookingReference}\n" +
                    $"Status: Pending Payment\n\n" +
                    $"Your examination admission ticket has been generated. You can download or print it now.",
                    "Booking Confirmed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Open the Admission Ticket form directly (FR-3.3)
                var ticketForm = new AdmissionTicketForm(createdBooking.BookingReference);
                ticketForm.ShowDialog(this);

                // Refresh slots
                LoadAvailableSlots();
            }
            else
            {
                MessageBox.Show(error, "Booking Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetExam(int examId)
        {
            this.preselectedExamId = examId;
            for (int i = 0; i < cmbExams.Items.Count; i++)
            {
                if (cmbExams.Items[i] is ComboBoxItem item && (int)item.Value == examId)
                {
                    cmbExams.SelectedIndex = i;
                    break;
                }
            }
        }

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public object Tag { get; set; }
            public override string ToString() => Text;
        }
    }
}
