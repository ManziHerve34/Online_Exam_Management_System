using System;
using System.Drawing;
using System.Windows.Forms;

namespace OnlineExamsSystem.UI.Common
{
    public static class UiTheme
    {
        // Color Palette
        public static readonly Color Primary = Color.FromArgb(37, 99, 235);         // Royal Blue #2563EB
        public static readonly Color PrimaryHover = Color.FromArgb(29, 78, 216);    // Darker Blue #1D4ED8
        public static readonly Color PrimaryLight = Color.FromArgb(239, 246, 255);  // Soft Blue tint
        public static readonly Color Secondary = Color.FromArgb(71, 85, 105);       // Slate #475569
        public static readonly Color SidebarDark = Color.FromArgb(15, 23, 42);      // Deep Navy #0F172A
        public static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);     // Navy Slate #1E293B
        public static readonly Color SidebarSelected = Color.FromArgb(37, 99, 235); // Accent Selected
        public static readonly Color Background = Color.FromArgb(248, 250, 252);    // Clean off-white #F8FAFC
        public static readonly Color CardBg = Color.White;
        public static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
        public static readonly Color Border = Color.FromArgb(226, 232, 240);
        public static readonly Color Success = Color.FromArgb(16, 185, 129);       // Green #10B981
        public static readonly Color SuccessLight = Color.FromArgb(236, 253, 245);
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);        // Amber #F59E0B
        public static readonly Color WarningLight = Color.FromArgb(254, 243, 199);
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);          // Red #EF4444
        public static readonly Color DangerLight = Color.FromArgb(254, 242, 242);

        // Fonts
        public static readonly Font TitleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font SubtitleFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        public static readonly Font SectionFont = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        public static readonly Font BodyBold = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        public static readonly Font SmallFont = new Font("Segoe UI", 8.25F, FontStyle.Regular);
        public static readonly Font KpiFont = new Font("Segoe UI", 20F, FontStyle.Bold);

        public static void StyleButton(Button btn, bool isPrimary = true, bool isDanger = false, bool isSuccess = false)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = BodyBold;
            btn.Cursor = Cursors.Hand;
            btn.Height = 36;

            if (isDanger)
            {
                btn.BackColor = Danger;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38);
            }
            else if (isSuccess)
            {
                btn.BackColor = Success;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(5, 150, 105);
            }
            else if (isPrimary)
            {
                btn.BackColor = Primary;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.MouseOverBackColor = PrimaryHover;
            }
            else
            {
                btn.BackColor = Color.FromArgb(241, 245, 249);
                btn.ForeColor = TextDark;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Border;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
            }
        }

        public static void StyleDataGrid(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Border;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowTemplate.Height = 38;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextDark;
            dgv.ColumnHeadersDefaultCellStyle.Font = BodyBold;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            dgv.ColumnHeadersHeight = 40;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            dgv.DefaultCellStyle.Font = BodyFont;
            dgv.DefaultCellStyle.ForeColor = TextDark;
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = PrimaryLight;
            dgv.DefaultCellStyle.SelectionForeColor = Primary;
            dgv.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        public static Panel CreateKpiCard(string title, string value, string subtitle, Color accentColor, int width = 220)
        {
            var panel = new Panel
            {
                Width = width,
                Height = 110,
                BackColor = CardBg,
                Margin = new Padding(8)
            };

            // Card border indicator strip
            var strip = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = accentColor
            };

            var lblTitle = new Label
            {
                Text = title.ToUpperInvariant(),
                Font = SmallFont,
                ForeColor = TextMuted,
                Location = new Point(16, 12),
                AutoSize = true
            };

            var lblValue = new Label
            {
                Text = value,
                Font = KpiFont,
                ForeColor = TextDark,
                Location = new Point(14, 32),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = subtitle,
                Font = SmallFont,
                ForeColor = TextMuted,
                Location = new Point(16, 76),
                AutoSize = true
            };

            panel.Controls.Add(lblSub);
            panel.Controls.Add(lblValue);
            panel.Controls.Add(lblTitle);
            panel.Controls.Add(strip);

            // Paint subtle border
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Border, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            };

            return panel;
        }

        public static void PaintCardBorder(object sender, PaintEventArgs e)
        {
            if (sender is Control ctrl)
            {
                using (var pen = new Pen(Border, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, ctrl.Width - 1, ctrl.Height - 1);
                }
            }
        }
    }
}
