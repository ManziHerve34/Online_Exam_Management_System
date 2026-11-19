using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;

namespace OnlineExamsSystem.Services
{
    public static class AuditService
    {
        public static void LogAction(int? userId, string action, string details)
        {
            try
            {
                string sql = @"
                INSERT INTO `audit_logs` (`user_id`, `action`, `details`, `ip_address`, `timestamp`)
                VALUES (@UserId, @Action, @Details, '127.0.0.1', NOW());";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new MySqlParameter("@UserId", userId.HasValue && userId.Value > 0 ? (object)userId.Value : DBNull.Value),
                    new MySqlParameter("@Action", action.Trim()),
                    new MySqlParameter("@Details", (object)details ?? DBNull.Value));
            }
            catch
            {
                // Never crash callers on audit failure
            }
        }

        public static List<AuditLog> GetAuditLogs(int? userId = null, string action = null, int limit = 100)
        {
            var list = new List<AuditLog>();
            string sql = @"
            SELECT a.*, u.`email` AS `user_email`
            FROM `audit_logs` a
            LEFT JOIN `users` u ON a.`user_id` = u.`user_id`
            WHERE 1=1 ";

            var parameters = new List<MySqlParameter>();

            if (userId.HasValue && userId.Value > 0)
            {
                sql += " AND a.`user_id` = @UserId ";
                parameters.Add(new MySqlParameter("@UserId", userId.Value));
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                sql += " AND a.`action` LIKE @Action ";
                parameters.Add(new MySqlParameter("@Action", $"%{action.Trim()}%"));
            }

            sql += " ORDER BY a.`timestamp` DESC LIMIT @Limit;";
            parameters.Add(new MySqlParameter("@Limit", limit > 0 ? limit : 100));

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, parameters.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new AuditLog
                {
                    LogId = Convert.ToInt32(row["log_id"]),
                    UserId = row["user_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["user_id"]),
                    UserEmail = row["user_email"]?.ToString() ?? "System / Anonymous",
                    Action = row["action"].ToString(),
                    Details = row["details"]?.ToString(),
                    IpAddress = row["ip_address"]?.ToString(),
                    Timestamp = Convert.ToDateTime(row["timestamp"])
                });
            }

            return list;
        }
    }
}
