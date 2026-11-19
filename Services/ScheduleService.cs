using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;

namespace OnlineExamsSystem.Services
{
    public class ScheduleService
    {
        public List<ExamSchedule> GetAllSchedules(bool upcomingOnly = false)
        {
            var list = new List<ExamSchedule>();
            string sql = @"
            SELECT s.*, 
                   e.`exam_code`, e.`title` AS `exam_title`, e.`fee` AS `exam_fee`, e.`duration_minutes`,
                   v.`venue_name`, v.`city` AS `venue_city`, v.`room_number`, v.`address` AS `venue_address`,
                   (SELECT COUNT(*) FROM `bookings` b 
                    WHERE b.`schedule_id` = s.`schedule_id` 
                      AND b.`booking_status` IN ('Pending Payment', 'Confirmed / Cleared for Exam')) AS `booked_seats`
            FROM `exam_schedules` s
            JOIN `exams` e ON s.`exam_id` = e.`exam_id`
            JOIN `venues` v ON s.`venue_id` = v.`venue_id`
            WHERE 1=1 ";

            if (upcomingOnly)
            {
                sql += " AND s.`exam_date` >= CURRENT_DATE() AND s.`status` = 'Scheduled' ";
            }

            sql += " ORDER BY s.`exam_date` ASC, s.`start_time` ASC;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
            foreach (DataRow row in dt.Rows)
                list.Add(MapSchedule(row));

            return list;
        }

        public List<ExamSchedule> GetAvailableSchedulesForExam(int examId, DateTime? minDate = null)
        {
            var list = new List<ExamSchedule>();
            string sql = @"
            SELECT s.*, 
                   e.`exam_code`, e.`title` AS `exam_title`, e.`fee` AS `exam_fee`, e.`duration_minutes`,
                   v.`venue_name`, v.`city` AS `venue_city`, v.`room_number`, v.`address` AS `venue_address`,
                   (SELECT COUNT(*) FROM `bookings` b 
                    WHERE b.`schedule_id` = s.`schedule_id` 
                      AND b.`booking_status` IN ('Pending Payment', 'Confirmed / Cleared for Exam')) AS `booked_seats`
            FROM `exam_schedules` s
            JOIN `exams` e ON s.`exam_id` = e.`exam_id`
            JOIN `venues` v ON s.`venue_id` = v.`venue_id`
            WHERE s.`exam_id` = @ExamId 
              AND s.`status` = 'Scheduled'
              AND s.`exam_date` >= @MinDate
            HAVING (s.`max_seats` - `booked_seats`) > 0
            ORDER BY s.`exam_date` ASC, s.`start_time` ASC;";

            DateTime targetMinDate = minDate ?? DateTime.Today;

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql,
                new MySqlParameter("@ExamId", examId),
                new MySqlParameter("@MinDate", targetMinDate.ToString("yyyy-MM-dd")));

            foreach (DataRow row in dt.Rows)
                list.Add(MapSchedule(row));

            return list;
        }

        public ExamSchedule GetScheduleById(int scheduleId)
        {
            string sql = @"
            SELECT s.*, 
                   e.`exam_code`, e.`title` AS `exam_title`, e.`fee` AS `exam_fee`, e.`duration_minutes`,
                   v.`venue_name`, v.`city` AS `venue_city`, v.`room_number`, v.`address` AS `venue_address`,
                   (SELECT COUNT(*) FROM `bookings` b 
                    WHERE b.`schedule_id` = s.`schedule_id` 
                      AND b.`booking_status` IN ('Pending Payment', 'Confirmed / Cleared for Exam')) AS `booked_seats`
            FROM `exam_schedules` s
            JOIN `exams` e ON s.`exam_id` = e.`exam_id`
            JOIN `venues` v ON s.`venue_id` = v.`venue_id`
            WHERE s.`schedule_id` = @ScheduleId;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, new MySqlParameter("@ScheduleId", scheduleId));
            if (dt.Rows.Count > 0)
                return MapSchedule(dt.Rows[0]);
            return null;
        }

        public bool CreateSchedule(ExamSchedule schedule, out string error)
        {
            error = string.Empty;
            if (schedule.ExamId <= 0 || schedule.VenueId <= 0)
            {
                error = "Please select an Exam and a Venue.";
                return false;
            }
            if (schedule.EndTime <= schedule.StartTime)
            {
                error = "End time must be after start time.";
                return false;
            }
            if (schedule.MaxSeats <= 0)
            {
                error = "Maximum seats capacity must be greater than zero.";
                return false;
            }

            try
            {
                // Check venue overlap conflict
                string overlapSql = @"
                SELECT COUNT(*) FROM `exam_schedules`
                WHERE `venue_id` = @VenueId 
                  AND `exam_date` = @ExamDate
                  AND `status` != 'Cancelled'
                  AND NOT (`end_time` <= @StartTime OR `start_time` >= @EndTime);";

                long overlapCount = Convert.ToInt64(DatabaseHelper.ExecuteScalar(overlapSql,
                    new MySqlParameter("@VenueId", schedule.VenueId),
                    new MySqlParameter("@ExamDate", schedule.ExamDate.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@StartTime", schedule.StartTime),
                    new MySqlParameter("@EndTime", schedule.EndTime)));

                if (overlapCount > 0)
                {
                    error = "A conflicting exam schedule already exists in this venue for the selected date and time window.";
                    return false;
                }

                string sql = @"
                INSERT INTO `exam_schedules`
                (`exam_id`, `venue_id`, `exam_date`, `start_time`, `end_time`, `max_seats`, `assigned_proctor`, `status`)
                VALUES
                (@ExamId, @VenueId, @ExamDate, @StartTime, @EndTime, @MaxSeats, @Proctor, @Status);";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new MySqlParameter("@ExamId", schedule.ExamId),
                    new MySqlParameter("@VenueId", schedule.VenueId),
                    new MySqlParameter("@ExamDate", schedule.ExamDate.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@StartTime", schedule.StartTime),
                    new MySqlParameter("@EndTime", schedule.EndTime),
                    new MySqlParameter("@MaxSeats", schedule.MaxSeats),
                    new MySqlParameter("@Proctor", (object)schedule.AssignedProctor?.Trim() ?? DBNull.Value),
                    new MySqlParameter("@Status", string.IsNullOrEmpty(schedule.Status) ? "Scheduled" : schedule.Status));

                AuditService.LogAction(SessionManager.CurrentUserId, "SCHEDULE_CREATED", $"Scheduled exam ID {schedule.ExamId} at venue ID {schedule.VenueId} on {schedule.ExamDate:yyyy-MM-dd} {schedule.TimeWindow}");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool UpdateSchedule(ExamSchedule schedule, out string error)
        {
            error = string.Empty;
            if (schedule.EndTime <= schedule.StartTime)
            {
                error = "End time must be after start time.";
                return false;
            }

            try
            {
                // Check if new max seats is less than already booked seats
                string bookedSql = @"
                SELECT COUNT(*) FROM `bookings` 
                WHERE `schedule_id` = @ScheduleId 
                  AND `booking_status` IN ('Pending Payment', 'Confirmed / Cleared for Exam');";

                long booked = Convert.ToInt64(DatabaseHelper.ExecuteScalar(bookedSql, new MySqlParameter("@ScheduleId", schedule.ScheduleId)));
                if (schedule.MaxSeats < booked)
                {
                    error = $"Cannot reduce maximum seats to {schedule.MaxSeats} because {booked} candidates are already booked in this slot.";
                    return false;
                }

                string sql = @"
                UPDATE `exam_schedules`
                SET `exam_id` = @ExamId,
                    `venue_id` = @VenueId,
                    `exam_date` = @ExamDate,
                    `start_time` = @StartTime,
                    `end_time` = @EndTime,
                    `max_seats` = @MaxSeats,
                    `assigned_proctor` = @Proctor,
                    `status` = @Status
                WHERE `schedule_id` = @ScheduleId;";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new MySqlParameter("@ExamId", schedule.ExamId),
                    new MySqlParameter("@VenueId", schedule.VenueId),
                    new MySqlParameter("@ExamDate", schedule.ExamDate.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@StartTime", schedule.StartTime),
                    new MySqlParameter("@EndTime", schedule.EndTime),
                    new MySqlParameter("@MaxSeats", schedule.MaxSeats),
                    new MySqlParameter("@Proctor", (object)schedule.AssignedProctor?.Trim() ?? DBNull.Value),
                    new MySqlParameter("@Status", schedule.Status),
                    new MySqlParameter("@ScheduleId", schedule.ScheduleId));

                AuditService.LogAction(SessionManager.CurrentUserId, "SCHEDULE_UPDATED", $"Updated schedule ID {schedule.ScheduleId}");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool DeleteSchedule(int scheduleId, out string error)
        {
            error = string.Empty;
            try
            {
                string checkSql = "SELECT COUNT(*) FROM `bookings` WHERE `schedule_id` = @ScheduleId;";
                long count = Convert.ToInt64(DatabaseHelper.ExecuteScalar(checkSql, new MySqlParameter("@ScheduleId", scheduleId)));
                if (count > 0)
                {
                    string cancelSql = "UPDATE `exam_schedules` SET `status` = 'Cancelled' WHERE `schedule_id` = @ScheduleId;";
                    DatabaseHelper.ExecuteNonQuery(cancelSql, new MySqlParameter("@ScheduleId", scheduleId));
                    AuditService.LogAction(SessionManager.CurrentUserId, "SCHEDULE_CANCELLED", $"Schedule ID {scheduleId} marked as Cancelled (active bookings exist).");
                    return true;
                }

                string deleteSql = "DELETE FROM `exam_schedules` WHERE `schedule_id` = @ScheduleId;";
                DatabaseHelper.ExecuteNonQuery(deleteSql, new MySqlParameter("@ScheduleId", scheduleId));
                AuditService.LogAction(SessionManager.CurrentUserId, "SCHEDULE_DELETED", $"Deleted schedule ID {scheduleId}.");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private ExamSchedule MapSchedule(DataRow row)
        {
            return new ExamSchedule
            {
                ScheduleId = Convert.ToInt32(row["schedule_id"]),
                ExamId = Convert.ToInt32(row["exam_id"]),
                ExamCode = row["exam_code"]?.ToString(),
                ExamTitle = row["exam_title"]?.ToString(),
                ExamFee = Convert.ToDecimal(row["exam_fee"]),
                DurationMinutes = Convert.ToInt32(row["duration_minutes"]),
                VenueId = Convert.ToInt32(row["venue_id"]),
                VenueName = row["venue_name"]?.ToString(),
                VenueCity = row["venue_city"]?.ToString(),
                RoomNumber = row["room_number"]?.ToString(),
                VenueAddress = row["venue_address"]?.ToString(),
                ExamDate = Convert.ToDateTime(row["exam_date"]),
                StartTime = (TimeSpan)row["start_time"],
                EndTime = (TimeSpan)row["end_time"],
                MaxSeats = Convert.ToInt32(row["max_seats"]),
                BookedSeats = Convert.ToInt32(row["booked_seats"]),
                AssignedProctor = row["assigned_proctor"]?.ToString(),
                Status = row["status"]?.ToString()
            };
        }
    }
}
