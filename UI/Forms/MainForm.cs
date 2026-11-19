using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.UI.Common;
using OnlineExamsSystem.UI.Controls;

namespace OnlineExamsSystem.UI.Forms
{
    public class MainForm : Form
    {
        private Panel pnlSidebar;
        private Panel pnlNavMenu;
        private Panel pnlContentContainer;
        private Label lblCurrentUserName;
        private Label lblCurrentUserRole;
        private readonly List<Button> navButtons = new List<Button>();
        private string currentRoute = "";

        // Cached controls for quick navigation
        private CandidateDashboardControl candidateDashboard;
        private AdminDashboardControl adminDashboard;
        private ExamCatalogControl examCatalog;
        private BookingWizardControl bookingWizard;
        private MyBookingsControl myBookings;
        private PaymentVerificationControl paymentVerification;
        private ExamManagementControl examManagement;
        private VenueManagementControl venueManagement;
        private ScheduleManagementControl scheduleManagement;
        private AdminReportsControl adminReports;
        private UserManagementControl userManagement;
        private AuditLogsControl auditLogs;
        private UserProfileControl userProfile;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Online Professional Exams Scheduling and Management System";
            this.Size = new Size(1280, 820);
            this.MinimumSize = new Size(1024, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UiTheme.Background;

            // Sidebar Panel
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = UiTheme.SidebarDark,
                Padding = new Padding(0)
            };

            // Sidebar Brand Header
            var pnlBrand = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(10, 15, 30),
                Padding = new Padding(20, 18, 20, 18)
            };
            var lblBrandMain = new Label
            {
                Text = "EXAM SCHEDULER",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 18),
                AutoSize = true
            };
            var lblBrandSub = new Label
            {
                Text = "Professional Certification System",
                Font = UiTheme.SmallFont,
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(22, 45),
                AutoSize = true
            };
            pnlBrand.Controls.Add(lblBrandMain);
            pnlBrand.Controls.Add(lblBrandSub);
            pnlSidebar.Controls.Add(pnlBrand);

            // User Info Badge
            var pnlUserBadge = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.FromArgb(20, 30, 48),
                Padding = new Padding(20, 14, 20, 14)
            };
            lblCurrentUserName = new Label
            {
                Text = SessionManager.CurrentUserName,
                Font = UiTheme.BodyBold,
                ForeColor = Color.White,
                Location = new Point(20, 14),
                AutoSize = true
            };
            lblCurrentUserRole = new Label
            {
                Text = SessionManager.CurrentRoleName,
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Primary,
                Location = new Point(20, 38),
                AutoSize = true
            };
            pnlUserBadge.Controls.Add(lblCurrentUserName);
            pnlUserBadge.Controls.Add(lblCurrentUserRole);
            pnlSidebar.Controls.Add(pnlUserBadge);

            // Sidebar Bottom (Logout & Student Credit)
            var pnlSidebarBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 110,
                BackColor = Color.FromArgb(10, 15, 30),
                Padding = new Padding(16)
            };

            var btnLogout = new Button
            {
                Text = "🚪 Sign Out",
                Dock = DockStyle.Top,
                Height = 36
            };
            UiTheme.StyleButton(btnLogout, isPrimary: false, isDanger: true);
            btnLogout.Click += BtnLogout_Click;

            var lblCredit = new Label
            {
                Text = "Developer: Manzi Herve (24605)\nCourse: DotNet (Group A)",
                Font = UiTheme.SmallFont,
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Bottom,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlSidebarBottom.Controls.Add(btnLogout);
            pnlSidebarBottom.Controls.Add(lblCredit);
            pnlSidebar.Controls.Add(pnlSidebarBottom);

            // Navigation Menu Container
            pnlNavMenu = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 12, 0, 12)
            };
            pnlSidebar.Controls.Add(pnlNavMenu);

            // Main Content Container
            pnlContentContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background
            };

            this.Controls.Add(pnlContentContainer);
            this.Controls.Add(pnlSidebar);
            pnlSidebar.SendToBack();
            pnlContentContainer.BringToFront();

            this.Load += (s, e) =>
            {
                BuildRoleBasedNavigation();
                Navigate("Dashboard");
            };
        }

        private void BuildRoleBasedNavigation()
        {
            pnlNavMenu.Controls.Clear();
            navButtons.Clear();

            lblCurrentUserName.Text = SessionManager.CurrentUserName;
            lblCurrentUserRole.Text = SessionManager.CurrentRoleName;

            int y = 10;

            if (SessionManager.IsCandidate)
            {
                AddNavButton("🏠  Dashboard", "Dashboard", ref y);
                AddNavButton("📚  Exam Catalog", "Catalog", ref y);
                AddNavButton("📅  Book Exam Slot", "BookExam", ref y);
                AddNavButton("🎟️  My Exam Bookings", "MyBookings", ref y);
                AddNavButton("👤  My Profile", "Profile", ref y);
            }
            else if (SessionManager.IsStaff)
            {
                AddNavButton("🏠  Staff Overview", "Dashboard", ref y);
                AddNavButton("💳  Payment Clearance", "Payments", ref y);
                AddNavButton("📅  Exam Schedules", "Schedules", ref y);
                AddNavButton("🏛️  Testing Centers", "Venues", ref y);
                AddNavButton("👤  Profile", "Profile", ref y);
            }
            else // System Administrator
            {
                AddNavButton("🏠  Executive Dashboard", "Dashboard", ref y);
                AddNavButton("💳  Payment Clearance", "Payments", ref y);
                AddNavButton("📚  Exam Offerings", "Exams", ref y);
                AddNavButton("🏛️  Testing Venues", "Venues", ref y);
                AddNavButton("📅  Exam Schedules", "Schedules", ref y);
                AddNavButton("📊  System Reports", "Reports", ref y);
                AddNavButton("👥  User Roster & RBAC", "Users", ref y);
                AddNavButton("📜  Security Audit Trail", "Audit", ref y);
                AddNavButton("👤  Admin Profile", "Profile", ref y);
            }
        }

        private void AddNavButton(string text, string route, ref int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(0, y),
                Size = new Size(pnlSidebar.Width, 42),
                FlatStyle = FlatStyle.Flat,
                Font = UiTheme.BodyBold,
                ForeColor = Color.FromArgb(203, 213, 225),
                BackColor = UiTheme.SidebarDark,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0),
                Cursor = Cursors.Hand,
                Tag = route
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = UiTheme.SidebarHover;
            btn.Click += (s, e) => Navigate(route);

            pnlNavMenu.Controls.Add(btn);
            navButtons.Add(btn);
            y += 46;
        }

        public void Navigate(string route)
        {
            Navigate(route, null);
        }

        public void Navigate(string route, object arg)
        {
            currentRoute = route;

            // Highlight selected button
            foreach (var b in navButtons)
            {
                bool isSelected = string.Equals(b.Tag?.ToString(), route, StringComparison.OrdinalIgnoreCase);
                b.BackColor = isSelected ? UiTheme.SidebarSelected : UiTheme.SidebarDark;
                b.ForeColor = isSelected ? Color.White : Color.FromArgb(203, 213, 225);
            }

            pnlContentContainer.Controls.Clear();
            Control targetCtrl = null;

            switch (route.ToLowerInvariant())
            {
                case "dashboard":
                    if (SessionManager.IsCandidate)
                    {
                        if (candidateDashboard == null)
                            candidateDashboard = new CandidateDashboardControl(Navigate);
                        else
                            candidateDashboard.LoadDashboardData();
                        targetCtrl = candidateDashboard;
                    }
                    else
                    {
                        if (adminDashboard == null)
                            adminDashboard = new AdminDashboardControl(Navigate);
                        else
                            adminDashboard.LoadDashboardData();
                        targetCtrl = adminDashboard;
                    }
                    break;

                case "catalog":
                    if (examCatalog == null)
                        examCatalog = new ExamCatalogControl(examId => Navigate("BookExam", examId));
                    targetCtrl = examCatalog;
                    break;

                case "bookexam":
                    int preselectedExamId = (arg is int id) ? id : 0;
                    if (bookingWizard == null)
                        bookingWizard = new BookingWizardControl(preselectedExamId);
                    else if (preselectedExamId > 0)
                        bookingWizard.SetExam(preselectedExamId);
                    targetCtrl = bookingWizard;
                    break;

                case "mybookings":
                    if (myBookings == null)
                        myBookings = new MyBookingsControl();
                    else
                        myBookings.LoadBookings("Upcoming");
                    targetCtrl = myBookings;
                    break;

                case "payments":
                    if (paymentVerification == null)
                        paymentVerification = new PaymentVerificationControl();
                    else
                        paymentVerification.ApplyFilters();
                    targetCtrl = paymentVerification;
                    break;

                case "exams":
                    if (examManagement == null)
                        examManagement = new ExamManagementControl();
                    else
                        examManagement.LoadExams();
                    targetCtrl = examManagement;
                    break;

                case "venues":
                    if (venueManagement == null)
                        venueManagement = new VenueManagementControl();
                    else
                        venueManagement.LoadVenues();
                    targetCtrl = venueManagement;
                    break;

                case "schedules":
                    if (scheduleManagement == null)
                        scheduleManagement = new ScheduleManagementControl();
                    else
                        scheduleManagement.LoadSchedules();
                    targetCtrl = scheduleManagement;
                    break;

                case "reports":
                    if (adminReports == null)
                        adminReports = new AdminReportsControl();
                    targetCtrl = adminReports;
                    break;

                case "users":
                    if (userManagement == null)
                        userManagement = new UserManagementControl();
                    else
                        userManagement.LoadUsers();
                    targetCtrl = userManagement;
                    break;

                case "audit":
                    if (auditLogs == null)
                        auditLogs = new AuditLogsControl();
                    else
                        auditLogs.LoadLogs();
                    targetCtrl = auditLogs;
                    break;

                case "profile":
                    if (userProfile == null)
                        userProfile = new UserProfileControl();
                    targetCtrl = userProfile;
                    break;

                default:
                    targetCtrl = candidateDashboard;
                    break;
            }

            if (targetCtrl != null)
            {
                targetCtrl.Dock = DockStyle.Fill;
                pnlContentContainer.Controls.Add(targetCtrl);
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Are you sure you want to log out of your session?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                SessionManager.ClearSession();
                this.Hide();

                var login = new LoginForm();
                if (login.ShowDialog() == DialogResult.OK)
                {
                    BuildRoleBasedNavigation();
                    this.Show();
                    Navigate("Dashboard");
                }
                else
                {
                    this.Close();
                }
            }
        }
    }
}
