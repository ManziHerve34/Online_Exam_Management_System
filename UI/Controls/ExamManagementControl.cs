using System;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Models;
using OnlineExamsSystem.Services;
using OnlineExamsSystem.UI.Common;

namespace OnlineExamsSystem.UI.Controls
{
    public class ExamManagementControl : UserControl
    {
        private DataGridView dgvExams;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private TextBox txtSearch;

        private readonly ExamService examService = new ExamService();

        public ExamManagementControl()
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Header
            var pnlHeader = new Panel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 16) };
            var lblTitle = new Label
            {
                Text = "Certification Exam Offerings Management",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var lblSub = new Label
            {
                Text = "Create, modify, and manage official certification exam catalogs, passing scores, fees, and prerequisites.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 32)
            };

            var pnlSearch = new Panel { Dock = DockStyle.Bottom, Height = 45, Location = new Point(0, 65), BackColor = Color.White, Padding = new Padding(12) };
            pnlSearch.Paint += UiTheme.PaintCardBorder;
            var lblS = new Label { Text = "Search Exam:", Font = UiTheme.BodyBold, Location = new Point(12, 14), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(110, 12), Width = 260, Font = UiTheme.BodyFont };
            txtSearch.TextChanged += (s, e) => LoadExams();

            pnlSearch.Controls.Add(lblS);
            pnlSearch.Controls.Add(txtSearch);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Controls.Add(pnlSearch);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Table Panel
            var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            pnlGrid.Paint += UiTheme.PaintCardBorder;
            dgvExams = new DataGridView { Dock = DockStyle.Fill };
            UiTheme.StyleDataGrid(dgvExams);
            dgvExams.SelectionChanged += DgvExams_SelectionChanged;
            dgvExams.CellDoubleClick += (s, e) => EditSelectedExam();

            pnlGrid.Controls.Add(dgvExams);
            mainLayout.Controls.Add(pnlGrid, 0, 1);

            // Action Toolbar
            var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(0, 16, 0, 0) };
            pnlActions.Paint += UiTheme.PaintCardBorder;

            btnAdd = new Button { Text = "+ Add New Exam Offering", Location = new Point(12, 12), Width = 190, Height = 36 };
            UiTheme.StyleButton(btnAdd, isPrimary: true);
            btnAdd.Click += (s, e) => ShowExamEditor(null);

            btnEdit = new Button { Text = "✏️ Edit Exam", Location = new Point(212, 12), Width = 120, Height = 36, Enabled = false };
            UiTheme.StyleButton(btnEdit, isPrimary: false);
            btnEdit.Click += (s, e) => EditSelectedExam();

            btnDelete = new Button { Text = "🗑️ Delete / Deactivate", Location = new Point(342, 12), Width = 160, Height = 36, Enabled = false };
            UiTheme.StyleButton(btnDelete, isPrimary: false, isDanger: true);
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button { Text = "Refresh", Location = new Point(512, 12), Width = 90, Height = 36 };
            UiTheme.StyleButton(btnRefresh, isPrimary: false);
            btnRefresh.Click += (s, e) => LoadExams();

            pnlActions.Controls.Add(btnAdd);
            pnlActions.Controls.Add(btnEdit);
            pnlActions.Controls.Add(btnDelete);
            pnlActions.Controls.Add(btnRefresh);
            mainLayout.Controls.Add(pnlActions, 0, 2);

            this.Controls.Add(mainLayout);
            this.Load += (s, e) =>
            {
                SetupGridColumns();
                LoadExams();
            };
        }

        private void SetupGridColumns()
        {
            dgvExams.Columns.Clear();
            dgvExams.Columns.Add("Code", "Exam Code");
            dgvExams.Columns.Add("Title", "Exam Title");
            dgvExams.Columns.Add("Category", "Category");
            dgvExams.Columns.Add("Discipline", "Discipline");
            dgvExams.Columns.Add("Level", "Level");
            dgvExams.Columns.Add("Duration", "Duration");
            dgvExams.Columns.Add("PassingScore", "Passing Score");
            dgvExams.Columns.Add("Fee", "Fee");
            dgvExams.Columns.Add("Status", "Status");

            dgvExams.Columns["Code"].Width = 110;
            dgvExams.Columns["Title"].Width = 230;
            dgvExams.Columns["Duration"].Width = 90;
            dgvExams.Columns["PassingScore"].Width = 100;
            dgvExams.Columns["Fee"].Width = 80;
            dgvExams.Columns["Status"].Width = 80;
        }

        public void LoadExams()
        {
            var list = examService.SearchAndFilterExams(txtSearch.Text, null, null, null, activeOnly: false);
            dgvExams.Rows.Clear();
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;

            foreach (var ex in list)
            {
                int idx = dgvExams.Rows.Add(
                    ex.ExamCode,
                    ex.Title,
                    ex.CategoryName,
                    ex.Discipline,
                    ex.CertificationLevel,
                    $"{ex.DurationMinutes}m",
                    $"{ex.PassingScore}%",
                    $"{ex.Fee:C}",
                    ex.IsActive ? "Active" : "Inactive"
                );
                dgvExams.Rows[idx].Tag = ex;
                if (!ex.IsActive)
                    dgvExams.Rows[idx].Cells["Status"].Style.ForeColor = UiTheme.Danger;
                else
                    dgvExams.Rows[idx].Cells["Status"].Style.ForeColor = UiTheme.Success;
            }

            if (dgvExams.Rows.Count > 0)
            {
                dgvExams.Rows[0].Selected = true;
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
            }
        }

        private void DgvExams_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvExams.CurrentRow?.Tag is Exam;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        private void EditSelectedExam()
        {
            if (dgvExams.CurrentRow?.Tag is Exam ex)
            {
                ShowExamEditor(ex);
            }
        }

        private void ShowExamEditor(Exam existing)
        {
            using (var dlg = new Form())
            {
                bool isEdit = existing != null;
                dlg.Text = isEdit ? $"Edit Exam Offering: {existing.ExamCode}" : "Create New Exam Offering";
                dlg.Size = new Size(580, 620);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.White;

                var pnlH = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UiTheme.SidebarDark, Padding = new Padding(16, 12, 16, 12) };
                var lblH = new Label { Text = isEdit ? "UPDATE EXAM OFFERING DETAILS" : "REGISTER NEW EXAM OFFERING", Font = UiTheme.SectionFont, ForeColor = Color.White, Location = new Point(16, 16), AutoSize = true };
                pnlH.Controls.Add(lblH);
                dlg.Controls.Add(pnlH);

                int y = 75;

                // Exam Code & Category
                var lblCodeP = new Label { Text = "Exam Code (e.g. AWS-SAA-C03):", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtCode = new TextBox { Text = existing?.ExamCode ?? "", Location = new Point(24, y + 20), Width = 230, Font = UiTheme.BodyFont };

                var lblCatP = new Label { Text = "Category:", Font = UiTheme.BodyBold, Location = new Point(280, y), AutoSize = true };
                var cmbCat = new ComboBox { Location = new Point(280, y + 20), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
                var categories = examService.GetCategories();
                int catSelect = 0;
                for (int i = 0; i < categories.Count; i++)
                {
                    cmbCat.Items.Add(new ComboBoxItem { Text = categories[i].CategoryName, Value = categories[i].CategoryId });
                    if (existing != null && categories[i].CategoryId == existing.CategoryId) catSelect = i;
                }
                if (cmbCat.Items.Count > 0) cmbCat.SelectedIndex = catSelect;

                dlg.Controls.Add(lblCodeP);
                dlg.Controls.Add(txtCode);
                dlg.Controls.Add(lblCatP);
                dlg.Controls.Add(cmbCat);

                // Title
                y += 55;
                var lblTitleP = new Label { Text = "Exam Title:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtTitle = new TextBox { Text = existing?.Title ?? "", Location = new Point(24, y + 20), Width = 496, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblTitleP);
                dlg.Controls.Add(txtTitle);

                // Discipline & Level
                y += 55;
                var lblDiscP = new Label { Text = "Discipline (e.g. Cloud, Security):", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtDisc = new TextBox { Text = existing?.Discipline ?? "", Location = new Point(24, y + 20), Width = 230, Font = UiTheme.BodyFont };

                var lblLvlP = new Label { Text = "Certification Level:", Font = UiTheme.BodyBold, Location = new Point(280, y), AutoSize = true };
                var cmbLvl = new ComboBox { Location = new Point(280, y + 20), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiTheme.BodyFont };
                cmbLvl.Items.AddRange(new object[] { "Associate", "Professional", "Expert", "Specialty" });
                cmbLvl.SelectedItem = existing?.CertificationLevel ?? "Associate";

                dlg.Controls.Add(lblDiscP);
                dlg.Controls.Add(txtDisc);
                dlg.Controls.Add(lblLvlP);
                dlg.Controls.Add(cmbLvl);

                // Duration, Passing Score, Fee
                y += 55;
                var lblDurP = new Label { Text = "Duration (mins):", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var numDur = new NumericUpDown { Location = new Point(24, y + 20), Width = 140, Minimum = 15, Maximum = 480, Value = existing?.DurationMinutes ?? 90, Font = UiTheme.BodyFont };

                var lblPassP = new Label { Text = "Passing Score (%):", Font = UiTheme.BodyBold, Location = new Point(190, y), AutoSize = true };
                var numPass = new NumericUpDown { Location = new Point(190, y + 20), Width = 140, Minimum = 50, Maximum = 100, Value = existing?.PassingScore ?? 70, Font = UiTheme.BodyFont };

                var lblFeeP = new Label { Text = "Exam Fee ($):", Font = UiTheme.BodyBold, Location = new Point(360, y), AutoSize = true };
                var numFee = new NumericUpDown { Location = new Point(360, y + 20), Width = 160, Minimum = 0, Maximum = 10000, DecimalPlaces = 2, Value = existing?.Fee ?? 150, Font = UiTheme.BodyFont };

                dlg.Controls.Add(lblDurP);
                dlg.Controls.Add(numDur);
                dlg.Controls.Add(lblPassP);
                dlg.Controls.Add(numPass);
                dlg.Controls.Add(lblFeeP);
                dlg.Controls.Add(numFee);

                // Prerequisites
                y += 55;
                var lblPrereqP = new Label { Text = "Prerequisites:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtPrereq = new TextBox { Text = existing?.Prerequisites ?? "", Location = new Point(24, y + 20), Width = 496, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblPrereqP);
                dlg.Controls.Add(txtPrereq);

                // Description
                y += 55;
                var lblDescP = new Label { Text = "Description / Syllabus Summary:", Font = UiTheme.BodyBold, Location = new Point(24, y), AutoSize = true };
                var txtDesc = new TextBox { Text = existing?.Description ?? "", Location = new Point(24, y + 20), Width = 496, Height = 50, Multiline = true, Font = UiTheme.BodyFont };
                dlg.Controls.Add(lblDescP);
                dlg.Controls.Add(txtDesc);

                // Active Checkbox
                y += 80;
                var chkActive = new CheckBox { Text = "Active (Available for Candidate Booking)", Checked = existing?.IsActive ?? true, Location = new Point(24, y), AutoSize = true, Font = UiTheme.BodyBold };
                dlg.Controls.Add(chkActive);

                // Buttons
                y += 35;
                var btnSave = new Button { Text = isEdit ? "Update Exam" : "Save Exam", Location = new Point(24, y), Width = 150, Height = 36 };
                UiTheme.StyleButton(btnSave, isPrimary: true);

                var btnCancel = new Button { Text = "Cancel", Location = new Point(190, y), Width = 100, Height = 36 };
                UiTheme.StyleButton(btnCancel, isPrimary: false);
                btnCancel.Click += (s, ev) => dlg.DialogResult = DialogResult.Cancel;

                btnSave.Click += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtTitle.Text))
                    {
                        MessageBox.Show("Exam Code and Title are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int catId = (cmbCat.SelectedItem is ComboBoxItem ci) ? (int)ci.Value : 1;

                    var examObj = new Exam
                    {
                        ExamId = existing?.ExamId ?? 0,
                        CategoryId = catId,
                        ExamCode = txtCode.Text.Trim(),
                        Title = txtTitle.Text.Trim(),
                        Discipline = txtDisc.Text.Trim(),
                        CertificationLevel = cmbLvl.SelectedItem?.ToString() ?? "Associate",
                        DurationMinutes = (int)numDur.Value,
                        PassingScore = (int)numPass.Value,
                        Fee = numFee.Value,
                        Prerequisites = txtPrereq.Text.Trim(),
                        Description = txtDesc.Text.Trim(),
                        IsActive = chkActive.Checked
                    };

                    bool success;
                    string err;
                    if (isEdit)
                        success = examService.UpdateExam(examObj, out err);
                    else
                        success = examService.CreateExam(examObj, out err);

                    if (success)
                    {
                        MessageBox.Show($"Exam {examObj.ExamCode} saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show(err, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                dlg.Controls.Add(btnSave);
                dlg.Controls.Add(btnCancel);

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadExams();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvExams.CurrentRow?.Tag is Exam ex)
            {
                var confirm = MessageBox.Show($"Are you sure you want to delete or deactivate exam '{ex.ExamCode} - {ex.Title}'?", "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    bool ok = examService.DeleteExam(ex.ExamId, out string error);
                    if (ok)
                    {
                        MessageBox.Show("Exam updated successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadExams();
                    }
                    else
                    {
                        MessageBox.Show(error, "Action Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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
