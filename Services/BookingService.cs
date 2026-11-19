using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;

namespace OnlineExamsSystem.Services
{
    public class BookingService
    {
        public bool BookExamSlot(int candidateId, int scheduleId, string notes, out Booking createdBooking, out string error)
        {
            createdBooking = null;
            error = string.Empty;

            if (candidateId <= 0 || scheduleId <= 0)
            {
                error = "Invalid candidate or exam schedule selection.";
                return false;
            }

            try
            {
                var schedService = new ScheduleService();
                var schedule = schedService.GetScheduleById(scheduleId);
                if (schedule == null)
                {
                    error = "Selected exam schedule was not found.";
                    return false;
                }

                if (schedule.Status != "Scheduled")
                {
                    error = $"This exam schedule is currently '{schedule.Status}' and cannot accept bookings.";
                    return false;
                }

                if (schedule.ExamDate < DateTime.Today)
                {
                    error = "Cannot book an exam slot that has already passed.";
                    return false;
                }

                // FR-3.1 & FR-3.2: Real-time seat capacity check
                if (schedule.AvailableSeats <= 0)
                {
                    error = "Sorry, this examination session has reached its maximum seat capacity.";
                    return false;
                }

                // FR-3.2: Candidate check 1: Candidate cannot re-register for the same exam if already booked and active
                string duplicateExamSql = @"
                SELECT COUNT(*) FROM `bookings` b
                JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
                WHERE b.`candidate_id` = @CandidateId 
                  AND s.`exam_id` = @ExamId
                  AND b.`booking_status` IN ('Pending Payment', 'Confirmed / Cleared for Exam');";

                long activeExamCount = Convert.ToInt64(DatabaseHelper.ExecuteScalar(duplicateExamSql,
                    new MySqlParameter("@CandidateId", candidateId),
                    new MySqlParameter("@ExamId", schedule.ExamId)));

                if (activeExamCount > 0)
                {
                    error = "You already have an active registration for this certification exam. Double registration for the same exam is prohibited.";
                    return false;
                }

                // FR-3.2: Double-booking validation 2: Candidate cannot schedule two exams at overlapping times on same date
                string overlapSql = @"
                SELECT s.`exam_date`, s.`start_time`, s.`end_time`, e.`exam_code`, e.`title`
                FROM `bookings` b
                JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
                JOIN `exams` e ON s.`exam_id` = e.`exam_id`
                WHERE b.`candidate_id` = @CandidateId
                  AND b.`booking_status` IN ('Pending Payment', 'Confirmed / Cleared for Exam')
                  AND s.`exam_date` = @ExamDate
                  AND NOT (s.`end_time` <= @StartTime OR s.`start_time` >= @EndTime);";

                DataTable overlapDt = DatabaseHelper.ExecuteDataTable(overlapSql,
                    new MySqlParameter("@CandidateId", candidateId),
                    new MySqlParameter("@ExamDate", schedule.ExamDate.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@StartTime", schedule.StartTime),
                    new MySqlParameter("@EndTime", schedule.EndTime));

                if (overlapDt.Rows.Count > 0)
                {
                    DataRow conflictRow = overlapDt.Rows[0];
                    string conflictCode = conflictRow["exam_code"].ToString();
                    error = $"Double-Booking Conflict: You are already scheduled for '{conflictCode}' on {schedule.ExamDate:yyyy-MM-dd} during an overlapping time window. Candidates cannot take two exams simultaneously.";
                    return false;
                }

                // FR-3.3: Generate Unique Exam Booking Reference / Ticket Number
                string bookingReference = GenerateBookingReference();

                string insertSql = @"
                INSERT INTO `bookings`
                (`booking_reference`, `candidate_id`, `schedule_id`, `booking_date`, `booking_status`, `notes`)
                VALUES
                (@Ref, @CandidateId, @ScheduleId, @BookingDate, 'Pending Payment', @Notes);";

                DatabaseHelper.ExecuteNonQuery(insertSql,
                    new MySqlParameter("@Ref", bookingReference),
                    new MySqlParameter("@CandidateId", candidateId),
                    new MySqlParameter("@ScheduleId", scheduleId),
                    new MySqlParameter("@BookingDate", DateTime.Now),
                    new MySqlParameter("@Notes", (object)notes?.Trim() ?? DBNull.Value));

                createdBooking = GetBookingByReference(bookingReference);

                AuditService.LogAction(candidateId, "BOOKING_CREATED", $"Candidate booked exam '{schedule.ExamCode}' (Schedule #{scheduleId}). Booking Ref: {bookingReference}");
                return true;
            }
            catch (Exception ex)
            {
                error = "Booking transaction failed: " + ex.Message;
                return false;
            }
        }

        public List<Booking> GetCandidateBookings(int candidateId, string filter = "All")
        {
            var list = new List<Booking>();
            string sql = @"
            SELECT b.*,
                   u.`full_name` AS `candidate_name`, u.`email` AS `candidate_email`, u.`phone_number` AS `candidate_phone`,
                   s.`exam_id`, s.`venue_id`, s.`exam_date`, s.`start_time`, s.`end_time`, s.`assigned_proctor`,
                   e.`exam_code`, e.`title` AS `exam_title`, e.`fee` AS `exam_fee`, e.`duration_minutes`, e.`passing_score`,
                   v.`venue_name`, v.`address` AS `venue_address`, v.`city` AS `venue_city`, v.`room_number`
            FROM `bookings` b
            JOIN `users` u ON b.`candidate_id` = u.`user_id`
            JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
            JOIN `exams` e ON s.`exam_id` = e.`exam_id`
            JOIN `venues` v ON s.`venue_id` = v.`venue_id`
            WHERE b.`candidate_id` = @CandidateId ";

            if (filter == "Upcoming")
            {
                sql += " AND s.`exam_date` >= CURRENT_DATE() AND b.`booking_status` != 'Cancelled' ";
            }
            else if (filter == "Past")
            {
                sql += " AND (s.`exam_date` < CURRENT_DATE() OR b.`booking_status` = 'Completed') ";
            }
            else if (filter == "Cancelled")
            {
                sql += " AND b.`booking_status` = 'Cancelled' ";
            }

            sql += " ORDER BY s.`exam_date` DESC, s.`start_time` DESC;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, new MySqlParameter("@CandidateId", candidateId));
            foreach (DataRow row in dt.Rows)
                list.Add(MapBooking(row));

            return list;
        }

        public Booking GetBookingByReference(string reference)
        {
            string sql = @"
            SELECT b.*,
                   u.`full_name` AS `candidate_name`, u.`email` AS `candidate_email`, u.`phone_number` AS `candidate_phone`,
                   s.`exam_id`, s.`venue_id`, s.`exam_date`, s.`start_time`, s.`end_time`, s.`assigned_proctor`,
                   e.`exam_code`, e.`title` AS `exam_title`, e.`fee` AS `exam_fee`, e.`duration_minutes`, e.`passing_score`,
                   v.`venue_name`, v.`address` AS `venue_address`, v.`city` AS `venue_city`, v.`room_number`
            FROM `bookings` b
            JOIN `users` u ON b.`candidate_id` = u.`user_id`
            JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
            JOIN `exams` e ON s.`exam_id` = e.`exam_id`
            JOIN `venues` v ON s.`venue_id` = v.`venue_id`
            WHERE b.`booking_reference` = @Ref;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, new MySqlParameter("@Ref", reference.Trim()));
            if (dt.Rows.Count > 0)
                return MapBooking(dt.Rows[0]);
            return null;
        }

        public Booking GetBookingById(int bookingId)
        {
            string sql = @"
            SELECT b.*,
                   u.`full_name` AS `candidate_name`, u.`email` AS `candidate_email`, u.`phone_number` AS `candidate_phone`,
                   s.`exam_id`, s.`venue_id`, s.`exam_date`, s.`start_time`, s.`end_time`, s.`assigned_proctor`,
                   e.`exam_code`, e.`title` AS `exam_title`, e.`fee` AS `exam_fee`, e.`duration_minutes`, e.`passing_score`,
                   v.`venue_name`, v.`address` AS `venue_address`, v.`city` AS `venue_city`, v.`room_number`
            FROM `bookings` b
            JOIN `users` u ON b.`candidate_id` = u.`user_id`
            JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
            JOIN `exams` e ON s.`exam_id` = e.`exam_id`
            JOIN `venues` v ON s.`venue_id` = v.`venue_id`
            WHERE b.`booking_id` = @BookingId;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, new MySqlParameter("@BookingId", bookingId));
            if (dt.Rows.Count > 0)
                return MapBooking(dt.Rows[0]);
            return null;
        }

        // FR-3.5: Reschedule exam booking up to defined cutoff date
        public bool RescheduleBooking(int bookingId, int newScheduleId, int candidateId, out string error)
        {
            error = string.Empty;
            try
            {
                var booking = GetBookingById(bookingId);
                if (booking == null)
                {
                    error = "Booking record not found.";
                    return false;
                }

                if (booking.CandidateId != candidateId && !SessionManager.IsAdmin && !SessionManager.IsStaff)
                {
                    error = "Unauthorized to reschedule this booking.";
                    return false;
                }

                int cutoffHours = DbConfig.RescheduleCutoffHours;
                if (!booking.CanModify(cutoffHours) && !SessionManager.IsAdmin)
                {
                    error = $"Rescheduling cutoff policy error: Bookings cannot be rescheduled less than {cutoffHours} hours prior to the scheduled exam window.";
                    return false;
                }

                var schedService = new ScheduleService();
                var newSchedule = schedService.GetScheduleById(newScheduleId);
                if (newSchedule == null)
                {
                    error = "Target schedule not found.";
                    return false;
                }

                if (newSchedule.ExamId != booking.ExamId)
                {
                    error = "Cannot reschedule to a different certification exam.";
                    return false;
                }

                if (newSchedule.AvailableSeats <= 0)
                {
                    error = "The selected new session is already full.";
                    return false;
                }

                // Check overlap on new schedule
                string overlapSql = @"
                SELECT s.`exam_date`, s.`start_time`, s.`end_time`, e.`exam_code`
                FROM `bookings` b
                JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
                JOIN `exams` e ON s.`exam_id` = e.`exam_id`
                WHERE b.`candidate_id` = @CandidateId
                  AND b.`booking_id` != @CurrentBookingId
                  AND b.`booking_status` IN ('Pending Payment', 'Confirmed / Cleared for Exam')
                  AND s.`exam_date` = @ExamDate
                  AND NOT (s.`end_time` <= @StartTime OR s.`start_time` >= @EndTime);";

                DataTable overlapDt = DatabaseHelper.ExecuteDataTable(overlapSql,
                    new MySqlParameter("@CandidateId", candidateId),
                    new MySqlParameter("@CurrentBookingId", bookingId),
                    new MySqlParameter("@ExamDate", newSchedule.ExamDate.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@StartTime", newSchedule.StartTime),
                    new MySqlParameter("@EndTime", newSchedule.EndTime));

                if (overlapDt.Rows.Count > 0)
                {
                    error = "Cannot reschedule: You have another exam booked during this new time window.";
                    return false;
                }

                string updateSql = @"
                UPDATE `bookings` 
                SET `schedule_id` = @NewScheduleId, 
                    `notes` = CONCAT(IFNULL(`notes`,''), ' | Rescheduled to new date on ', NOW())
                WHERE `booking_id` = @BookingId;";

                DatabaseHelper.ExecuteNonQuery(updateSql,
                    new MySqlParameter("@NewScheduleId", newScheduleId),
                    new MySqlParameter("@BookingId", bookingId));

                AuditService.LogAction(candidateId, "BOOKING_RESCHEDULED", $"Booking Ref {booking.BookingReference} rescheduled from Schedule #{booking.ScheduleId} to Schedule #{newScheduleId}");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        // FR-3.5: Cancel exam booking up to defined cutoff date
        public bool CancelBooking(int bookingId, int candidateId, string reason, out string error)
        {
            error = string.Empty;
            try
            {
                var booking = GetBookingById(bookingId);
                if (booking == null)
                {
                    error = "Booking record not found.";
                    return false;
                }

                if (booking.CandidateId != candidateId && !SessionManager.IsAdmin && !SessionManager.IsStaff)
                {
                    error = "Unauthorized to cancel this booking.";
                    return false;
                }

                int cutoffHours = DbConfig.RescheduleCutoffHours;
                if (!booking.CanModify(cutoffHours) && !SessionManager.IsAdmin)
                {
                    error = $"Cancellation policy restriction: Bookings cannot be cancelled less than {cutoffHours} hours prior to the scheduled exam window.";
                    return false;
                }

                string updateSql = @"
                UPDATE `bookings` 
                SET `booking_status` = 'Cancelled',
                    `notes` = CONCAT(IFNULL(`notes`,''), ' | Cancelled on ', NOW(), ': ', @Reason)
                WHERE `booking_id` = @BookingId;";

                DatabaseHelper.ExecuteNonQuery(updateSql,
                    new MySqlParameter("@Reason", string.IsNullOrWhiteSpace(reason) ? "Candidate requested cancellation" : reason.Trim()),
                    new MySqlParameter("@BookingId", bookingId));

                AuditService.LogAction(candidateId, "BOOKING_CANCELLED", $"Booking Ref {booking.BookingReference} cancelled. Reason: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private string GenerateBookingReference()
        {
            var rand = new Random();
            string refCode;
            bool unique = false;
            do
            {
                int num = rand.Next(10000, 99999);
                refCode = $"EXAM-{DateTime.Now.Year}-{num}";
                string checkSql = "SELECT COUNT(*) FROM `bookings` WHERE `booking_reference` = @Ref;";
                long count = Convert.ToInt64(DatabaseHelper.ExecuteScalar(checkSql, new MySqlParameter("@Ref", refCode)));
                if (count == 0)
                    unique = true;
            } while (!unique);

            return refCode;
        }

        private Booking MapBooking(DataRow row)
        {
            return new Booking
            {
                BookingId = Convert.ToInt32(row["booking_id"]),
                BookingReference = row["booking_reference"].ToString(),
                CandidateId = Convert.ToInt32(row["candidate_id"]),
                CandidateName = row["candidate_name"]?.ToString(),
                CandidateEmail = row["candidate_email"]?.ToString(),
                CandidatePhone = row["candidate_phone"]?.ToString(),
                ScheduleId = Convert.ToInt32(row["schedule_id"]),
                ExamId = Convert.ToInt32(row["exam_id"]),
                ExamCode = row["exam_code"]?.ToString(),
                ExamTitle = row["exam_title"]?.ToString(),
                ExamFee = Convert.ToDecimal(row["exam_fee"]),
                DurationMinutes = Convert.ToInt32(row["duration_minutes"]),
                PassingScore = Convert.ToInt32(row["passing_score"]),
                VenueId = Convert.ToInt32(row["venue_id"]),
                VenueName = row["venue_name"]?.ToString(),
                VenueAddress = row["venue_address"]?.ToString(),
                VenueCity = row["venue_city"]?.ToString(),
                RoomNumber = row["room_number"]?.ToString(),
                AssignedProctor = row["assigned_proctor"]?.ToString(),
                ExamDate = Convert.ToDateTime(row["exam_date"]),
                StartTime = (TimeSpan)row["start_time"],
                EndTime = (TimeSpan)row["end_time"],
                BookingDate = Convert.ToDateTime(row["booking_date"]),
                BookingStatus = row["booking_status"].ToString(),
                Notes = row["notes"]?.ToString(),
                CreatedAt = Convert.ToDateTime(row["created_at"]),
                UpdatedAt = Convert.ToDateTime(row["updated_at"])
            };
        }
    }
}
