using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Controls
{
    public class ExamCatalogControl : UserControl
    {
        private TextBox txtSearch;
        private ComboBox cmbCategory;
        private ComboBox cmbLevel;
        private ComboBox cmbDiscipline;
        private DataGridView dgvExams;
        private Panel pnlDetails;
        private Label lblDetailTitle;
        private Label lblDetailCode;
        private Label lblDetailMeta;
        private Label lblDetailPrereq;
        private TextBox txtDetailDesc;
        private Button btnBookExam;

        private readonly ExamService examService = new ExamService();
        private readonly Action<int> onBookExamSelected;

        public ExamCatalogControl(Action<int> onBookExamSelected = null)
        {
            this.onBookExamSelected = onBookExamSelected;
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F)); // Table
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F)); // Details Pane

            // Header & Filter Panel
            var pnlTop = new Panel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 16) };

            var lblTitle = new Label
            {
                Text = "Professional Certification Exam Catalog",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Explore available industry certification examinations, review prerequisites, syllabus details, and fees.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };

            // Filter Bar
            var pnlFilters = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                Location = new Point(0, 65),
                BackColor = Color.White
            };
            pnlFilters.Paint += UiTheme.PaintCardBorder;

            var lblSearch = new Label { Text = "Search:", Font = UiTheme.BodyBold, Location = new Point(12, 12), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(68, 10), Width = 160, Font = UiTheme.BodyFont };
            txtSearch.TextChanged += (s, e) => ApplyFilters();

            var lblCat = new Label { Text = "Category:", Font = UiTheme.BodyBold, Location = new Point(245, 12), AutoSize = true };
            cmbCategory = new ComboBox { Location = new Point(315, 10), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
            cmbCategory.SelectedIndexChanged += (s, e) => ApplyFilters();

            var lblLevel = new Label { Text = "Level:", Font = UiTheme.BodyBold, Location = new Point(490, 12), AutoSize = true };
            cmbLevel = new ComboBox { Location = new Point(535, 10), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
            cmbLevel.SelectedIndexChanged += (s, e) => ApplyFilters();

            var lblDisc = new Label { Text = "Discipline:", Font = UiTheme.BodyBold, Location = new Point(670, 12), AutoSize = true };
            cmbDiscipline = new ComboBox { Location = new Point(745, 10), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
            cmbDiscipline.SelectedIndexChanged += (s, e) => ApplyFilters();

            var btnReset = new Button { Text = "Reset", Location = new Point(900, 8), Width = 70, Height = 28 };
            UiTheme.StyleButton(btnReset, isPrimary: false);
            btnReset.Click += (s, e) => ResetFilters();

            pnlFilters.Controls.Add(lblSearch);
            pnlFilters.Controls.Add(txtSearch);
            pnlFilters.Controls.Add(lblCat);
            pnlFilters.Controls.Add(cmbCategory);
            pnlFilters.Controls.Add(lblLevel);
            pnlFilters.Controls.Add(cmbLevel);
            pnlFilters.Controls.Add(lblDisc);
            pnlFilters.Controls.Add(cmbDiscipline);
            pnlFilters.Controls.Add(btnReset);

            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblSub);
            pnlTop.Controls.Add(pnlFilters);
            mainLayout.Controls.Add(pnlTop, 0, 0);

            // Table Panel
            var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            pnlGrid.Paint += UiTheme.PaintCardBorder;

            dgvExams = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvExams);
            dgvExams.SelectionChanged += DgvExams_SelectionChanged;

            pnlGrid.Controls.Add(dgvExams);
            mainLayout.Controls.Add(pnlGrid, 0, 1);

            // Details Panel
            pnlDetails = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                Margin = new Padding(0, 16, 0, 0)
            };
            pnlDetails.Paint += UiTheme.PaintCardBorder;

            lblDetailCode = new Label
            {
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Primary,
                BackColor = UiTheme.PrimaryLight,
                Padding = new Padding(6, 2, 6, 2),
                AutoSize = true,
                Location = new Point(20, 16)
            };

            lblDetailTitle = new Label
            {
                Font = UiTheme.SubtitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(20, 42)
            };

            lblDetailMeta = new Label
            {
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(20, 72)
            };

            lblDetailPrereq = new Label
            {
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(20, 98)
            };

            var lblDescHeading = new Label
            {
                Text = "Exam Overview & Syllabus Summary:",
                Font = UiTheme.BodyBold,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(20, 126)
            };

            txtDetailDesc = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.None,
                Font = UiTheme.BodyFont,
                Location = new Point(20, 150),
                Size = new Size(700, 75),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            btnBookExam = new Button
            {
                Text = "📅 Book This Exam Now",
                Width = 200,
                Height = 42,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlDetails.Width - 220, 20)
            };
            UiTheme.StyleButton(btnBookExam, isPrimary: true);
            btnBookExam.Click += (s, e) =>
            {
                if (dgvExams.CurrentRow?.Tag is Exam ex)
                {
                    onBookExamSelected?.Invoke(ex.ExamId);
                }
            };

            pnlDetails.Controls.Add(lblDetailCode);
            pnlDetails.Controls.Add(lblDetailTitle);
            pnlDetails.Controls.Add(lblDetailMeta);
            pnlDetails.Controls.Add(lblDetailPrereq);
            pnlDetails.Controls.Add(lblDescHeading);
            pnlDetails.Controls.Add(txtDetailDesc);
            pnlDetails.Controls.Add(btnBookExam);

            mainLayout.Controls.Add(pnlDetails, 0, 2);
            this.Controls.Add(mainLayout);

            this.Load += (s, e) => LoadInitialData();
        }

        private void LoadInitialData()
        {
            // Populate Category filter
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add(new ComboBoxItem { Text = "All Categories", Value = 0 });
            var categories = examService.GetCategories();
            foreach (var c in categories)
                cmbCategory.Items.Add(new ComboBoxItem { Text = c.CategoryName, Value = c.CategoryId });
            cmbCategory.SelectedIndex = 0;

            // Populate Level filter
            cmbLevel.Items.Clear();
            cmbLevel.Items.Add("All Levels");
            foreach (var lvl in examService.GetCertificationLevels())
                cmbLevel.Items.Add(lvl);
            cmbLevel.SelectedIndex = 0;

            // Populate Discipline filter
            cmbDiscipline.Items.Clear();
            cmbDiscipline.Items.Add("All Disciplines");
            foreach (var d in examService.GetDisciplineList())
                cmbDiscipline.Items.Add(d);
            cmbDiscipline.SelectedIndex = 0;

            SetupGridColumns();
            ApplyFilters();
        }

        private void SetupGridColumns()
        {
            dgvExams.Columns.Clear();
            dgvExams.Columns.Add("ExamCode", "Exam Code");
            dgvExams.Columns.Add("Title", "Certification Title");
            dgvExams.Columns.Add("Category", "Category");
            dgvExams.Columns.Add("Discipline", "Discipline");
            dgvExams.Columns.Add("Level", "Level");
            dgvExams.Columns.Add("Duration", "Duration");
            dgvExams.Columns.Add("PassingScore", "Passing Score");
            dgvExams.Columns.Add("Fee", "Fee");

            dgvExams.Columns["ExamCode"].Width = 120;
            dgvExams.Columns["Title"].Width = 260;
            dgvExams.Columns["Duration"].Width = 90;
            dgvExams.Columns["PassingScore"].Width = 100;
            dgvExams.Columns["Fee"].Width = 90;
        }

        private void ApplyFilters()
        {
            string keyword = txtSearch.Text;
            int? catId = null;
            if (cmbCategory.SelectedItem is ComboBoxItem item && (int)item.Value > 0)
                catId = (int)item.Value;

            string level = cmbLevel.SelectedItem?.ToString();
            string discipline = cmbDiscipline.SelectedItem?.ToString();

            var list = examService.SearchAndFilterExams(keyword, catId, level, discipline, activeOnly: true);

            dgvExams.Rows.Clear();
            foreach (var ex in list)
            {
                int idx = dgvExams.Rows.Add(
                    ex.ExamCode,
                    ex.Title,
                    ex.CategoryName,
                    ex.Discipline,
                    ex.CertificationLevel,
                    $"{ex.DurationMinutes} mins",
                    $"{ex.PassingScore}%",
                    $"{ex.Fee:C}"
                );
                dgvExams.Rows[idx].Tag = ex;
            }

            if (dgvExams.Rows.Count > 0)
            {
                dgvExams.Rows[0].Selected = true;
                UpdateDetails(dgvExams.Rows[0].Tag as Exam);
            }
            else
            {
                ClearDetails();
            }
        }

        private void ResetFilters()
        {
            txtSearch.Clear();
            cmbCategory.SelectedIndex = 0;
            cmbLevel.SelectedIndex = 0;
            cmbDiscipline.SelectedIndex = 0;
            ApplyFilters();
        }

        private void DgvExams_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvExams.CurrentRow?.Tag is Exam ex)
            {
                UpdateDetails(ex);
            }
        }

        private void UpdateDetails(Exam ex)
        {
            if (ex == null) return;
            lblDetailCode.Text = ex.ExamCode;
            lblDetailTitle.Text = ex.Title;
            lblDetailMeta.Text = $"Category: {ex.CategoryName}   |   Discipline: {ex.Discipline}   |   Level: {ex.CertificationLevel}   |   Duration: {ex.DurationMinutes} mins   |   Passing Score: {ex.PassingScore}%   |   Exam Fee: {ex.Fee:C}";
            lblDetailPrereq.Text = $"Prerequisites: {(string.IsNullOrEmpty(ex.Prerequisites) ? "None specified." : ex.Prerequisites)}";
            txtDetailDesc.Text = ex.Description ?? "No additional description available.";
            btnBookExam.Enabled = true;
        }

        private void ClearDetails()
        {
            lblDetailCode.Text = string.Empty;
            lblDetailTitle.Text = "No exam selected";
            lblDetailMeta.Text = string.Empty;
            lblDetailPrereq.Text = string.Empty;
            txtDetailDesc.Text = string.Empty;
            btnBookExam.Enabled = false;
        }

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
