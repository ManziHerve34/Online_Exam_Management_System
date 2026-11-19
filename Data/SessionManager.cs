using System;

namespace OnlineExamsSystem.Data
{
    public static class SessionManager
    {
        public static int CurrentUserId { get; private set; }
        public static string CurrentUserName { get; private set; }
        public static string CurrentEmail { get; private set; }
        public static int CurrentRoleId { get; private set; }
        public static string CurrentRoleName { get; private set; }
        public static DateTime LoginTime { get; private set; }
        public static bool IsLoggedIn => CurrentUserId > 0;

        public static event EventHandler UserLoggedIn;
        public static event EventHandler UserLoggedOut;

        public static void SetSession(int userId, string fullName, string email, int roleId, string roleName)
        {
            CurrentUserId = userId;
            CurrentUserName = fullName;
            CurrentEmail = email;
            CurrentRoleId = roleId;
            CurrentRoleName = roleName;
            LoginTime = DateTime.Now;

            UserLoggedIn?.Invoke(null, EventArgs.Empty);
        }

        public static void ClearSession()
        {
            CurrentUserId = 0;
            CurrentUserName = string.Empty;
            CurrentEmail = string.Empty;
            CurrentRoleId = 0;
            CurrentRoleName = string.Empty;

            UserLoggedOut?.Invoke(null, EventArgs.Empty);
        }

        public static bool IsAdmin => CurrentRoleId == 1 || (CurrentRoleName != null && CurrentRoleName.IndexOf("Admin", StringComparison.OrdinalIgnoreCase) >= 0 && CurrentRoleName.IndexOf("Staff", StringComparison.OrdinalIgnoreCase) < 0);
        public static bool IsStaff => CurrentRoleId == 2 || (CurrentRoleName != null && CurrentRoleName.IndexOf("Staff", StringComparison.OrdinalIgnoreCase) >= 0);
        public static bool IsCandidate => CurrentRoleId == 3 || (CurrentRoleName != null && CurrentRoleName.IndexOf("Candidate", StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
