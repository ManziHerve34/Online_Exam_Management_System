using System;
using System.Data;
using MySqlConnector;

namespace OnlineExamsSystem.Data
{
    public static class DatabaseHelper
    {
        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(DbConfig.ConnectionString);
            return conn;
        }

        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT 1;", conn))
                    {
                        cmd.ExecuteScalar();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public static int ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string query, params MySqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    return cmd.ExecuteScalar();
                }
            }
        }

        public static DataTable ExecuteDataTable(string query, params MySqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static void EnsureDatabaseAndTables()
        {
            try
            {
                // First test connection to MySQL server itself
                var builder = new MySqlConnectionStringBuilder(DbConfig.ConnectionString);
                string originalDb = builder.Database;

                // Create database if it does not exist
                builder.Database = "";
                using (var serverConn = new MySqlConnection(builder.ConnectionString))
                {
                    serverConn.Open();
                    using (var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{originalDb}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;", serverConn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Verify tables exist in target database
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SHOW TABLES LIKE 'users';", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            InitializeDefaultSchema(conn);
                        }
                    }
                }
            }
            catch
            {
                // Let callers handle or show connection alert
            }
        }

        private static void InitializeDefaultSchema(MySqlConnection conn)
        {
            string ddl = @"
            CREATE TABLE IF NOT EXISTS `roles` (
                `role_id` INT AUTO_INCREMENT PRIMARY KEY,
                `role_name` VARCHAR(50) NOT NULL UNIQUE,
                `description` VARCHAR(255) NULL
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

            INSERT IGNORE INTO `roles` (`role_id`, `role_name`, `description`) VALUES
            (1, 'System Administrator', 'Full administrative privileges across the entire exam scheduling platform'),
            (2, 'Admin Staff', 'Authorized staff for venue scheduling, booking oversight, and manual on-site payment verification'),
            (3, 'Candidate', 'Registered candidate eligible to browse exams, book seats, manage registrations, and print admission tickets');

            CREATE TABLE IF NOT EXISTS `users` (
                `user_id` INT AUTO_INCREMENT PRIMARY KEY,
                `role_id` INT NOT NULL,
                `full_name` VARCHAR(150) NOT NULL,
                `email` VARCHAR(150) NOT NULL UNIQUE,
                `password_hash` VARCHAR(255) NOT NULL,
                `password_salt` VARCHAR(255) NOT NULL,
                `phone_number` VARCHAR(50) NULL,
                `educational_background` VARCHAR(255) NULL,
                `professional_credentials` VARCHAR(255) NULL,
                `is_email_verified` TINYINT(1) NOT NULL DEFAULT 1,
                `email_verification_token` VARCHAR(100) NULL,
                `password_reset_otp` VARCHAR(10) NULL,
                `password_reset_expiry` DATETIME NULL,
                `is_active` TINYINT(1) NOT NULL DEFAULT 1,
                `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                CONSTRAINT `fk_users_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`role_id`) ON DELETE RESTRICT ON UPDATE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

            CREATE TABLE IF NOT EXISTS `categories` (
                `category_id` INT AUTO_INCREMENT PRIMARY KEY,
                `category_name` VARCHAR(100) NOT NULL UNIQUE,
                `description` VARCHAR(255) NULL
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

            CREATE TABLE IF NOT EXISTS `exams` (
                `exam_id` INT AUTO_INCREMENT PRIMARY KEY,
                `category_id` INT NOT NULL,
                `exam_code` VARCHAR(50) NOT NULL UNIQUE,
                `title` VARCHAR(200) NOT NULL,
                `discipline` VARCHAR(100) NOT NULL,
                `certification_level` VARCHAR(50) NOT NULL,
                `prerequisites` TEXT NULL,
                `duration_minutes` INT NOT NULL DEFAULT 90,
                `passing_score` INT NOT NULL DEFAULT 70,
                `fee` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
                `description` TEXT NULL,
                `is_active` TINYINT(1) NOT NULL DEFAULT 1,
                `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT `fk_exams_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`category_id`) ON DELETE RESTRICT ON UPDATE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

            CREATE TABLE IF NOT EXISTS `venues` (
                `venue_id` INT AUTO_INCREMENT PRIMARY KEY,
                `venue_name` VARCHAR(150) NOT NULL,
                `address` VARCHAR(255) NOT NULL,
                `city` VARCHAR(100) NOT NULL,
                `room_number` VARCHAR(50) NULL,
                `total_capacity` INT NOT NULL DEFAULT 30,
                `contact_person` VARCHAR(100) NULL,
                `contact_phone` VARCHAR(50) NULL,
                `is_active` TINYINT(1) NOT NULL DEFAULT 1
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

            CREATE TABLE IF NOT EXISTS `exam_schedules` (
                `schedule_id` INT AUTO_INCREMENT PRIMARY KEY,
                `exam_id` INT NOT NULL,
                `venue_id` INT NOT NULL,
                `exam_date` DATE NOT NULL,
                `start_time` TIME NOT NULL,
                `end_time` TIME NOT NULL,
                `max_seats` INT NOT NULL,
                `assigned_proctor` VARCHAR(100) NULL,
                `status` VARCHAR(50) NOT NULL DEFAULT 'Scheduled',
                `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT `fk_schedules_exam` FOREIGN KEY (`exam_id`) REFERENCES `exams` (`exam_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
                CONSTRAINT `fk_schedules_venue` FOREIGN KEY (`venue_id`) REFERENCES `venues` (`venue_id`) ON DELETE RESTRICT ON UPDATE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

            CREATE TABLE IF NOT EXISTS `bookings` (
                `booking_id` INT AUTO_INCREMENT PRIMARY KEY,
                `booking_reference` VARCHAR(50) NOT NULL UNIQUE,
                `candidate_id` INT NOT NULL,
                `schedule_id` INT NOT NULL,
                `booking_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                `booking_status` VARCHAR(50) NOT NULL DEFAULT 'Pending Payment',
                `notes` TEXT NULL,
                `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                CONSTRAINT `fk_bookings_candidate` FOREIGN KEY (`candidate_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
                CONSTRAINT `fk_bookings_schedule` FOREIGN KEY (`schedule_id`) REFERENCES `exam_schedules` (`schedule_id`) ON DELETE RESTRICT ON UPDATE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

            CREATE TABLE IF NOT EXISTS `payments` (
                `payment_id` INT AUTO_INCREMENT PRIMARY KEY,
                `booking_id` INT NOT NULL UNIQUE,
                `amount_paid` DECIMAL(10,2) NOT NULL,
                `payment_method` VARCHAR(50) NOT NULL,
                `receipt_number` VARCHAR(100) NOT NULL UNIQUE,
                `payment_status` VARCHAR(50) NOT NULL DEFAULT 'Verified',
                `verified_by_user_id` INT NOT NULL,
                `verified_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                `remarks` TEXT NULL,
                CONSTRAINT `fk_payments_booking` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`booking_id`) ON DELETE CASCADE ON UPDATE CASCADE,
                CONSTRAINT `fk_payments_verifier` FOREIGN KEY (`verified_by_user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT ON UPDATE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

            CREATE TABLE IF NOT EXISTS `audit_logs` (
                `log_id` INT AUTO_INCREMENT PRIMARY KEY,
                `user_id` INT NULL,
                `action` VARCHAR(100) NOT NULL,
                `details` TEXT NULL,
                `ip_address` VARCHAR(50) DEFAULT '127.0.0.1',
                `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT `fk_audit_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE SET NULL ON UPDATE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

            using (var cmd = new MySqlCommand(ddl, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
