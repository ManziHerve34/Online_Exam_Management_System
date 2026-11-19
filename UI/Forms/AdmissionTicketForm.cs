using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Forms
{
    public class AdmissionTicketForm : Form
    {
        private readonly string bookingReference;
        private Booking booking;
        private readonly BookingService bookingService = new BookingService();
        private Panel pnlTicketCanvas;

        public AdmissionTicketForm(string bookingReference)
        {
            this.bookingReference = bookingReference;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Exam Admission Ticket & Scheduling Summary";
            this.Size = new Size(820, 840);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Load data
            booking = bookingService.GetBookingByReference(bookingReference);
            if (booking == null)
            {
                MessageBox.Show("Booking record could not be loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            // Top action toolbar
            var pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = UiTheme.SidebarDark,
                Padding = new Padding(16, 10, 16, 10)
            };

            var lblTopTitle = new Label
            {
                Text = "Official Examination Scheduling Summary & Admission Ticket",
                Font = UiTheme.SectionFont,
                ForeColor = Color.White,
                Location = new Point(16, 16),
                AutoSize = true
            };

            var btnPrint = new Button
            {
                Text = "🖨️ Print Ticket",
                Location = new Point(pnlTopBar.Width - 270, 10),
                Width = 120,
                Height = 34,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            UiTheme.StyleButton(btnPrint, isPrimary: true);
            btnPrint.Click += BtnPrint_Click;

            var btnExportHtml = new Button
            {
                Text = "💾 Save Summary",
                Location = new Point(pnlTopBar.Width - 140, 10),
                Width = 120,
                Height = 34,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            UiTheme.StyleButton(btnExportHtml, isPrimary: false);
            btnExportHtml.Click += BtnExportHtml_Click;

            pnlTopBar.Controls.Add(lblTopTitle);
            pnlTopBar.Controls.Add(btnPrint);
            pnlTopBar.Controls.Add(btnExportHtml);
            this.Controls.Add(pnlTopBar);

            // Scrollable canvas container
            var scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            pnlTicketCanvas = new Panel
            {
                Width = 740,
                Height = 720,
                BackColor = Color.White,
                Location = new Point(20, 20)
            };
            pnlTicketCanvas.Paint += RenderTicketDesign;

            scrollContainer.Controls.Add(pnlTicketCanvas);
            this.Controls.Add(scrollContainer);
        }

        private void RenderTicketDesign(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Outer card border
            using (var borderPen = new Pen(Color.FromArgb(203, 213, 225), 1))
            {
                g.DrawRectangle(borderPen, 0, 0, pnlTicketCanvas.Width - 1, pnlTicketCanvas.Height - 1);
            }

            // Top Header Banner
            using (var headerBrush = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                g.FillRectangle(headerBrush, 0, 0, pnlTicketCanvas.Width, 90);
            }

            // Header Texts
            using (var titleFont = new Font("Segoe UI", 14F, FontStyle.Bold))
            using (var subFont = new Font("Segoe UI", 9F, FontStyle.Regular))
            using (var whiteBrush = new SolidBrush(Color.White))
            using (var mutedBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                g.DrawString("ONLINE PROFESSIONAL EXAMINATIONS COUNCIL", titleFont, whiteBrush, 24, 20);
                g.DrawString("OFFICIAL CANDIDATE ADMISSION TICKET & SCHEDULING CLEARANCE", subFont, mutedBrush, 24, 48);
            }

            // Reference Number & Status Badge
            int yPos = 105;
            using (var refFont = new Font("Segoe UI", 12F, FontStyle.Bold))
            using (var darkBrush = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                g.DrawString($"Booking Reference: {booking.BookingReference}", refFont, darkBrush, 24, yPos);
            }

            // Status Badge Rectangle
            bool isConfirmed = booking.IsConfirmed;
            Color badgeBg = isConfirmed ? Color.FromArgb(236, 253, 245) : Color.FromArgb(254, 243, 199);
            Color badgeBorder = isConfirmed ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11);
            Color badgeText = isConfirmed ? Color.FromArgb(5, 150, 105) : Color.FromArgb(180, 83, 9);
            string statusString = isConfirmed ? "✓ CLEARED FOR EXAM (PAID)" : "⚠ PENDING ON-SITE PAYMENT";

            using (var bgBrush = new SolidBrush(badgeBg))
            using (var pen = new Pen(badgeBorder, 1))
            using (var textBrush = new SolidBrush(badgeText))
            using (var badgeFont = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                Rectangle badgeRect = new Rectangle(pnlTicketCanvas.Width - 260, yPos - 4, 235, 28);
                g.FillRectangle(bgBrush, badgeRect);
                g.DrawRectangle(pen, badgeRect);
                g.DrawString(statusString, badgeFont, textBrush, badgeRect.X + 12, badgeRect.Y + 6);
            }

            // Separator
            yPos += 35;
            using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
            {
                g.DrawLine(pen, 24, yPos, pnlTicketCanvas.Width - 24, yPos);
            }

            // 1. Candidate Information Box
            yPos += 15;
            DrawSectionHeader(g, "CANDIDATE INFORMATION", yPos);
            yPos += 26;

            DrawKeyValue(g, "Candidate Name:", booking.CandidateName, 24, yPos);
            DrawKeyValue(g, "Email Address:", booking.CandidateEmail, 380, yPos);
            yPos += 22;
            DrawKeyValue(g, "Phone Number:", string.IsNullOrEmpty(booking.CandidatePhone) ? "N/A" : booking.CandidatePhone, 24, yPos);
            DrawKeyValue(g, "Candidate ID:", $"CID-{booking.CandidateId:D5}", 380, yPos);

            // Separator
            yPos += 30;
            using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
            {
                g.DrawLine(pen, 24, yPos, pnlTicketCanvas.Width - 24, yPos);
            }

            // 2. Exam Details Box
            yPos += 15;
            DrawSectionHeader(g, "EXAMINATION DETAILS", yPos);
            yPos += 26;

            DrawKeyValue(g, "Exam Code:", booking.ExamCode, 24, yPos);
            DrawKeyValue(g, "Certification Title:", booking.ExamTitle, 200, yPos);
            yPos += 22;
            DrawKeyValue(g, "Allocated Duration:", $"{booking.DurationMinutes} Minutes", 24, yPos);
            DrawKeyValue(g, "Passing Standard:", $"{booking.PassingScore}% minimum score", 200, yPos);
            DrawKeyValue(g, "Registration Fee:", $"{booking.ExamFee:C}", 480, yPos);

            // Separator
            yPos += 30;
            using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
            {
                g.DrawLine(pen, 24, yPos, pnlTicketCanvas.Width - 24, yPos);
            }

            // 3. Testing Center & Slot Box
            yPos += 15;
            DrawSectionHeader(g, "VENUE, DATE & TIME WINDOW", yPos);
            yPos += 26;

            DrawKeyValue(g, "Exam Date:", booking.ExamDate.ToString("dddd, MMMM dd, yyyy"), 24, yPos);
            DrawKeyValue(g, "Scheduled Window:", booking.TimeWindow, 380, yPos);
            yPos += 22;
            DrawKeyValue(g, "Testing Venue:", booking.VenueName, 24, yPos);
            DrawKeyValue(g, "Room / Suite:", booking.RoomNumber ?? "General Exam Lab", 380, yPos);
            yPos += 22;
            DrawKeyValue(g, "Center Address:", $"{booking.VenueAddress}, {booking.VenueCity}", 24, yPos);
            DrawKeyValue(g, "Assigned Proctor:", string.IsNullOrEmpty(booking.AssignedProctor) ? "Authorized Proctor" : booking.AssignedProctor, 380, yPos);

            // Separator
            yPos += 32;
            using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
            {
                g.DrawLine(pen, 24, yPos, pnlTicketCanvas.Width - 24, yPos);
            }

            // 4. Instructions and Rules
            yPos += 15;
            DrawSectionHeader(g, "CRITICAL EXAMINATION INSTRUCTIONS", yPos);
            yPos += 24;

            string[] rules = new string[]
            {
                "1. Arrive at least 30 minutes prior to the scheduled start time for biometric check-in and workstation setup.",
                "2. Present a valid government-issued photo ID (National ID, Passport, or Driver's License) upon check-in.",
                "3. If your registration is 'Pending On-Site Payment', you must finalize payment at the cashier counter prior to entry.",
                "4. All mobile phones, smartwatches, recording devices, and personal notes are strictly prohibited inside the lab.",
                $"5. Rescheduling or cancellation is strictly permitted up to {DbConfig.RescheduleCutoffHours} hours before the start time."
            };

            using (var ruleFont = new Font("Segoe UI", 8.5F, FontStyle.Regular))
            using (var ruleBrush = new SolidBrush(Color.FromArgb(71, 85, 105)))
            {
                foreach (var rule in rules)
                {
                    g.DrawString(rule, ruleFont, ruleBrush, 24, yPos);
                    yPos += 18;
                }
            }

            // Barcode Box Simulation
            yPos += 12;
            DrawBarcodeBox(g, booking.BookingReference, yPos);
        }

        private void DrawSectionHeader(Graphics g, string text, int y)
        {
            using (var font = new Font("Segoe UI", 9.5F, FontStyle.Bold))
            using (var brush = new SolidBrush(UiTheme.Primary))
            {
                g.DrawString(text, font, brush, 24, y);
            }
        }

        private void DrawKeyValue(Graphics g, string label, string value, int x, int y)
        {
            using (var lblFont = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var valFont = new Font("Segoe UI", 9F, FontStyle.Regular))
            using (var lblBrush = new SolidBrush(Color.FromArgb(100, 116, 139)))
            using (var valBrush = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                g.DrawString(label, lblFont, lblBrush, x, y);
                var size = g.MeasureString(label, lblFont);
                g.DrawString(value ?? "", valFont, valBrush, x + size.Width + 4, y);
            }
        }

        private void DrawBarcodeBox(Graphics g, string refCode, int y)
        {
            int boxWidth = 320;
            int boxHeight = 50;
            int startX = (pnlTicketCanvas.Width - boxWidth) / 2;

            using (var pen = new Pen(Color.FromArgb(15, 23, 42), 1))
            using (var brush = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                // Draw vertical bars simulating Code 128 barcode
                var rand = new Random(refCode.GetHashCode());
                int curX = startX + 15;
                while (curX < startX + boxWidth - 15)
                {
                    int w = rand.Next(1, 4);
                    g.FillRectangle(brush, curX, y, w, boxHeight - 16);
                    curX += w + rand.Next(1, 4);
                }

                // Barcode text
                using (var codeFont = new Font("Consolas", 10F, FontStyle.Bold))
                {
                    var size = g.MeasureString($"* {refCode} *", codeFont);
                    g.DrawString($"* {refCode} *", codeFont, brush, startX + (boxWidth - size.Width) / 2, y + boxHeight - 14);
                }
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                var pd = new PrintDocument();
                pd.DocumentName = $"Admission_Ticket_{booking.BookingReference}";
                pd.PrintPage += (s, ev) =>
                {
                    var bmp = new Bitmap(pnlTicketCanvas.Width, pnlTicketCanvas.Height);
                    pnlTicketCanvas.DrawToBitmap(bmp, new Rectangle(0, 0, pnlTicketCanvas.Width, pnlTicketCanvas.Height));
                    ev.Graphics.DrawImage(bmp, ev.MarginBounds.Left, ev.MarginBounds.Top, ev.MarginBounds.Width, (int)(ev.MarginBounds.Width * ((double)pnlTicketCanvas.Height / pnlTicketCanvas.Width)));
                };

                var printDialog = new PrintDialog { Document = pd };
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    pd.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Printing error: " + ex.Message, "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportHtml_Click(object sender, EventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "HTML Document (*.html)|*.html|Text Document (*.txt)|*.txt",
                    FileName = $"Admission_Ticket_{booking.BookingReference}.html",
                    Title = "Export Exam Admission Ticket Summary"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string html = GenerateHtmlTicket();
                    File.WriteAllText(sfd.FileName, html);
                    MessageBox.Show($"Ticket successfully saved to:\n{sfd.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export error: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateHtmlTicket()
        {
            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'/>
    <title>Exam Admission Ticket - {booking.BookingReference}</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: #f1f5f9; padding: 20px; }}
        .ticket {{ max-width: 750px; margin: 0 auto; background: #ffffff; border: 1px solid #cbd5e1; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.05); overflow: hidden; }}
        .header {{ background: #0f172a; color: #ffffff; padding: 24px; }}
        .header h1 {{ margin: 0; font-size: 20px; letter-spacing: 0.5px; }}
        .header p {{ margin: 6px 0 0; font-size: 13px; color: #94a3b8; }}
        .content {{ padding: 24px; }}
        .ref-row {{ display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #e2e8f0; padding-bottom: 12px; }}
        .ref-code {{ font-size: 18px; font-weight: bold; color: #0f172a; }}
        .badge {{ padding: 6px 14px; border-radius: 4px; font-weight: bold; font-size: 13px; }}
        .badge-cleared {{ background: #ecfdf5; color: #059669; border: 1px solid #10b981; }}
        .badge-pending {{ background: #fef3c7; color: #d97706; border: 1px solid #f59e0b; }}
        .section-title {{ font-size: 14px; font-weight: bold; color: #2563eb; margin: 20px 0 10px; text-transform: uppercase; letter-spacing: 0.5px; }}
        .grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 10px; font-size: 14px; }}
        .item span {{ font-weight: bold; color: #64748b; }}
        .rules {{ background: #f8fafc; border-left: 4px solid #2563eb; padding: 12px 16px; margin-top: 20px; font-size: 13px; color: #475569; }}
        .rules ul {{ margin: 6px 0; padding-left: 20px; }}
        .footer {{ text-align: center; margin-top: 24px; padding-top: 16px; border-top: 1px dashed #cbd5e1; font-family: monospace; font-size: 16px; letter-spacing: 4px; }}
    </style>
</head>
<body>
    <div class='ticket'>
        <div class='header'>
            <h1>ONLINE PROFESSIONAL EXAMINATIONS COUNCIL</h1>
            <p>OFFICIAL CANDIDATE ADMISSION TICKET & SCHEDULING CLEARANCE</p>
        </div>
        <div class='content'>
            <div class='ref-row'>
                <div class='ref-code'>Booking Reference: {booking.BookingReference}</div>
                <div class='badge {(booking.IsConfirmed ? "badge-cleared" : "badge-pending")}'>
                    {(booking.IsConfirmed ? "✓ CLEARED FOR EXAM (PAID)" : "⚠ PENDING ON-SITE PAYMENT")}
                </div>
            </div>

            <div class='section-title'>Candidate Information</div>
            <div class='grid'>
                <div class='item'><span>Name:</span> {booking.CandidateName}</div>
                <div class='item'><span>Email:</span> {booking.CandidateEmail}</div>
                <div class='item'><span>Phone:</span> {(string.IsNullOrEmpty(booking.CandidatePhone) ? "N/A" : booking.CandidatePhone)}</div>
                <div class='item'><span>Candidate ID:</span> CID-{booking.CandidateId:D5}</div>
            </div>

            <div class='section-title'>Examination Details</div>
            <div class='grid'>
                <div class='item'><span>Exam Code:</span> {booking.ExamCode}</div>
                <div class='item'><span>Title:</span> {booking.ExamTitle}</div>
                <div class='item'><span>Duration:</span> {booking.DurationMinutes} Minutes</div>
                <div class='item'><span>Passing Standard:</span> {booking.PassingScore}%</div>
                <div class='item'><span>Examination Fee:</span> {booking.ExamFee:C}</div>
            </div>

            <div class='section-title'>Testing Center, Date & Time</div>
            <div class='grid'>
                <div class='item'><span>Exam Date:</span> {booking.ExamDate:dddd, MMMM dd, yyyy}</div>
                <div class='item'><span>Time Window:</span> {booking.TimeWindow}</div>
                <div class='item'><span>Venue:</span> {booking.VenueName} ({booking.RoomNumber})</div>
                <div class='item'><span>Address:</span> {booking.VenueAddress}, {booking.VenueCity}</div>
                <div class='item'><span>Assigned Proctor:</span> {booking.AssignedProctor ?? "Authorized Proctor"}</div>
            </div>

            <div class='rules'>
                <strong>Important Candidate Notice:</strong>
                <ul>
                    <li>Arrive at least 30 minutes before your scheduled window for identification clearance.</li>
                    <li>Bring a valid government-issued photo ID (Passport / National ID).</li>
                    <li>If status is Pending Payment, please complete payment at the test center desk upon arrival.</li>
                    <li>Rescheduling/cancellation cutoff: at least {DbConfig.RescheduleCutoffHours} hours prior to start time.</li>
                </ul>
            </div>

            <div class='footer'>
                ||| | ||||| || |||| ||||| ||| ||||||| ||<br/>
                * {booking.BookingReference} *
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}
