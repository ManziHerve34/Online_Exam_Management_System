using System;
using System.Configuration;

namespace OnlineExamsSystem.Data
{
    public static class DbConfig
    {
        private static string _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    try
                    {
                        var setting = ConfigurationManager.ConnectionStrings["MySqlConn"];
                        if (setting != null && !string.IsNullOrWhiteSpace(setting.ConnectionString))
                        {
                            _connectionString = setting.ConnectionString;
                        }
                    }
                    catch
                    {
                        // Fall back to default XAMPP connection
                    }

                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        _connectionString = "Server=localhost;Port=3306;Database=online_exams_db;Uid=root;Pwd=;AllowUserVariables=True;CharSet=utf8mb4;";
                    }
                }
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        public static int RescheduleCutoffHours
        {
            get
            {
                try
                {
                    string val = ConfigurationManager.AppSettings["RescheduleCutoffHours"];
                    if (int.TryParse(val, out int hours))
                        return hours;
                }
                catch { }
                return 24;
            }
        }

        public static string CurrencySymbol
        {
            get
            {
                try
                {
                    string val = ConfigurationManager.AppSettings["CurrencySymbol"];
                    if (!string.IsNullOrEmpty(val))
                        return val;
                }
                catch { }
                return "$";
            }
        }
    }
}
