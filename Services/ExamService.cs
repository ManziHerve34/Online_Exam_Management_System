using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;

namespace OnlineExamsSystem.Services
{
    public class ExamService
    {
        public List<ExamCategory> GetCategories()
        {
            var list = new List<ExamCategory>();
            string sql = "SELECT * FROM `categories` ORDER BY `category_name` ASC;";
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ExamCategory
                {
                    CategoryId = Convert.ToInt32(row["category_id"]),
                    CategoryName = row["category_name"].ToString(),
                    Description = row["description"]?.ToString()
                });
            }
            return list;
        }

        public List<Exam> GetAllExams(bool activeOnly = false)
        {
            return SearchAndFilterExams(null, null, null, null, activeOnly);
        }

        public List<Exam> SearchAndFilterExams(string keyword, int? categoryId, string certificationLevel, string discipline, bool activeOnly = false)
        {
            var list = new List<Exam>();
            string sql = @"
            SELECT e.*, c.`category_name`
            FROM `exams` e
            JOIN `categories` c ON e.`category_id` = c.`category_id`
            WHERE 1=1 ";

            var parameters = new List<MySqlParameter>();

            if (activeOnly)
            {
                sql += " AND e.`is_active` = 1 ";
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " AND (e.`exam_code` LIKE @Keyword OR e.`title` LIKE @Keyword OR e.`discipline` LIKE @Keyword OR e.`description` LIKE @Keyword) ";
                parameters.Add(new MySqlParameter("@Keyword", $"%{keyword.Trim()}%"));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                sql += " AND e.`category_id` = @CategoryId ";
                parameters.Add(new MySqlParameter("@CategoryId", categoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(certificationLevel) && certificationLevel != "All Levels")
            {
                sql += " AND e.`certification_level` = @CertLevel ";
                parameters.Add(new MySqlParameter("@CertLevel", certificationLevel.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(discipline) && discipline != "All Disciplines")
            {
                sql += " AND e.`discipline` = @Discipline ";
                parameters.Add(new MySqlParameter("@Discipline", discipline.Trim()));
            }

            sql += " ORDER BY e.`title` ASC;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, parameters.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapExam(row));
            }

            return list;
        }

        public Exam GetExamById(int examId)
        {
            string sql = @"
            SELECT e.*, c.`category_name`
            FROM `exams` e
            JOIN `categories` c ON e.`category_id` = c.`category_id`
            WHERE e.`exam_id` = @ExamId;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, new MySqlParameter("@ExamId", examId));
            if (dt.Rows.Count > 0)
                return MapExam(dt.Rows[0]);
            return null;
        }

        public bool CreateExam(Exam exam, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(exam.ExamCode) || string.IsNullOrWhiteSpace(exam.Title))
            {
                error = "Exam Code and Title are required.";
                return false;
            }

            try
            {
                // Check code uniqueness
                string checkSql = "SELECT COUNT(*) FROM `exams` WHERE `exam_code` = @Code;";
                long count = Convert.ToInt64(DatabaseHelper.ExecuteScalar(checkSql, new MySqlParameter("@Code", exam.ExamCode.Trim())));
                if (count > 0)
                {
                    error = "An exam with this exam code already exists.";
                    return false;
                }

                string sql = @"
                INSERT INTO `exams`
                (`category_id`, `exam_code`, `title`, `discipline`, `certification_level`, `prerequisites`, 
                 `duration_minutes`, `passing_score`, `fee`, `description`, `is_active`)
                VALUES
                (@CategoryId, @Code, @Title, @Discipline, @Level, @Prerequisites, 
                 @Duration, @PassingScore, @Fee, @Description, @Active);";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new MySqlParameter("@CategoryId", exam.CategoryId),
                    new MySqlParameter("@Code", exam.ExamCode.Trim().ToUpperInvariant()),
                    new MySqlParameter("@Title", exam.Title.Trim()),
                    new MySqlParameter("@Discipline", exam.Discipline.Trim()),
                    new MySqlParameter("@Level", exam.CertificationLevel.Trim()),
                    new MySqlParameter("@Prerequisites", (object)exam.Prerequisites ?? DBNull.Value),
                    new MySqlParameter("@Duration", exam.DurationMinutes),
                    new MySqlParameter("@PassingScore", exam.PassingScore),
                    new MySqlParameter("@Fee", exam.Fee),
                    new MySqlParameter("@Description", (object)exam.Description ?? DBNull.Value),
                    new MySqlParameter("@Active", exam.IsActive ? 1 : 0));

                AuditService.LogAction(SessionManager.CurrentUserId, "EXAM_CREATED", $"Created exam: {exam.ExamCode} - {exam.Title}");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool UpdateExam(Exam exam, out string error)
        {
            error = string.Empty;
            try
            {
                string sql = @"
                UPDATE `exams`
                SET `category_id` = @CategoryId,
                    `exam_code` = @Code,
                    `title` = @Title,
                    `discipline` = @Discipline,
                    `certification_level` = @Level,
                    `prerequisites` = @Prerequisites,
                    `duration_minutes` = @Duration,
                    `passing_score` = @PassingScore,
                    `fee` = @Fee,
                    `description` = @Description,
                    `is_active` = @Active
                WHERE `exam_id` = @ExamId;";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new MySqlParameter("@CategoryId", exam.CategoryId),
                    new MySqlParameter("@Code", exam.ExamCode.Trim().ToUpperInvariant()),
                    new MySqlParameter("@Title", exam.Title.Trim()),
                    new MySqlParameter("@Discipline", exam.Discipline.Trim()),
                    new MySqlParameter("@Level", exam.CertificationLevel.Trim()),
                    new MySqlParameter("@Prerequisites", (object)exam.Prerequisites ?? DBNull.Value),
                    new MySqlParameter("@Duration", exam.DurationMinutes),
                    new MySqlParameter("@PassingScore", exam.PassingScore),
                    new MySqlParameter("@Fee", exam.Fee),
                    new MySqlParameter("@Description", (object)exam.Description ?? DBNull.Value),
                    new MySqlParameter("@Active", exam.IsActive ? 1 : 0),
                    new MySqlParameter("@ExamId", exam.ExamId));

                AuditService.LogAction(SessionManager.CurrentUserId, "EXAM_UPDATED", $"Updated exam ID {exam.ExamId}: {exam.ExamCode}");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool DeleteExam(int examId, out string error)
        {
            error = string.Empty;
            try
            {
                // Check if exam is referenced in schedules
                string checkSql = "SELECT COUNT(*) FROM `exam_schedules` WHERE `exam_id` = @ExamId;";
                long count = Convert.ToInt64(DatabaseHelper.ExecuteScalar(checkSql, new MySqlParameter("@ExamId", examId)));
                if (count > 0)
                {
                    // Soft-delete instead of hard delete to preserve integrity
                    string softDelete = "UPDATE `exams` SET `is_active` = 0 WHERE `exam_id` = @ExamId;";
                    DatabaseHelper.ExecuteNonQuery(softDelete, new MySqlParameter("@ExamId", examId));
                    AuditService.LogAction(SessionManager.CurrentUserId, "EXAM_DEACTIVATED", $"Exam ID {examId} has existing schedules; deactivated instead of deleted.");
                    return true;
                }

                string deleteSql = "DELETE FROM `exams` WHERE `exam_id` = @ExamId;";
                DatabaseHelper.ExecuteNonQuery(deleteSql, new MySqlParameter("@ExamId", examId));
                AuditService.LogAction(SessionManager.CurrentUserId, "EXAM_DELETED", $"Deleted exam ID {examId}.");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public List<string> GetDisciplineList()
        {
            var list = new List<string>();
            string sql = "SELECT DISTINCT `discipline` FROM `exams` WHERE `discipline` IS NOT NULL AND `discipline` != '' ORDER BY `discipline` ASC;";
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
            foreach (DataRow row in dt.Rows)
                list.Add(row["discipline"].ToString());
            return list;
        }

        public List<string> GetCertificationLevels()
        {
            return new List<string> { "Associate", "Professional", "Expert", "Specialty" };
        }

        private Exam MapExam(DataRow row)
        {
            return new Exam
            {
                ExamId = Convert.ToInt32(row["exam_id"]),
                CategoryId = Convert.ToInt32(row["category_id"]),
                CategoryName = row["category_name"]?.ToString(),
                ExamCode = row["exam_code"].ToString(),
                Title = row["title"].ToString(),
                Discipline = row["discipline"].ToString(),
                CertificationLevel = row["certification_level"].ToString(),
                Prerequisites = row["prerequisites"]?.ToString(),
                DurationMinutes = Convert.ToInt32(row["duration_minutes"]),
                PassingScore = Convert.ToInt32(row["passing_score"]),
                Fee = Convert.ToDecimal(row["fee"]),
                Description = row["description"]?.ToString(),
                IsActive = Convert.ToBoolean(row["is_active"]),
                CreatedAt = Convert.ToDateTime(row["created_at"])
            };
        }
    }
}
