using System;
using System.Data;
using MySqlConnector;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;

namespace OnlineExamsSystem.Services
{
    public class ReportService
    {
        public DashboardStats GetDashboardStats()
        {
            var stats = new DashboardStats();
            try
            {
                // Candidates count
                object candObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM `users` WHERE `role_id` = 3;");
                stats.TotalCandidates = Convert.ToInt32(candObj);

                // Exams count
                object examObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM `exams` WHERE `is_active` = 1;");
                stats.TotalExams = Convert.ToInt32(examObj);

                // Venues count
                object venueObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM `venues` WHERE `is_active` = 1;");
                stats.TotalVenues = Convert.ToInt32(venueObj);

                // Upcoming Schedules count
                object schedObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM `exam_schedules` WHERE `exam_date` >= CURRENT_DATE() AND `status` = 'Scheduled';");
                stats.TotalSchedules = Convert.ToInt32(schedObj);

                // Bookings count
                object bookObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM `bookings`;");
                stats.TotalBookings = Convert.ToInt32(bookObj);

                // Pending Clearance count
                object pendingObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM `bookings` WHERE `booking_status` = 'Pending Payment';");
                stats.PendingClearanceCount = Convert.ToInt32(pendingObj);

                // Confirmed Bookings count
                object confObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM `bookings` WHERE `booking_status` = 'Confirmed / Cleared for Exam';");
                stats.ConfirmedBookingsCount = Convert.ToInt32(confObj);

                // Cancelled Bookings count
                object cancObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM `bookings` WHERE `booking_status` = 'Cancelled';");
                stats.CancelledBookingsCount = Convert.ToInt32(cancObj);

                // Revenue Collected
                object revObj = DatabaseHelper.ExecuteScalar("SELECT IFNULL(SUM(`amount_paid`), 0) FROM `payments` WHERE `payment_status` = 'Verified';");
                stats.TotalRevenueCollected = Convert.ToDecimal(revObj);

                // Pending Revenue Amount
                string pendingRevSql = @"
                SELECT IFNULL(SUM(e.`fee`), 0)
                FROM `bookings` b
                JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
                JOIN `exams` e ON s.`exam_id` = e.`exam_id`
                WHERE b.`booking_status` = 'Pending Payment';";
                object pendingRevObj = DatabaseHelper.ExecuteScalar(pendingRevSql);
                stats.PendingRevenueAmount = Convert.ToDecimal(pendingRevObj);

                // Today's Registrations
                object todayObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM `bookings` WHERE DATE(`booking_date`) = CURRENT_DATE();");
                stats.TodayRegistrationsCount = Convert.ToInt32(todayObj);
            }
            catch
            {
                // Return default zeros on database failure
            }

            return stats;
        }

        // FR-4.4: Daily registrations report
        public DataTable GetDailyRegistrationsReport(DateTime startDate, DateTime endDate)
        {
            string sql = @"
            SELECT 
                DATE(b.`booking_date`) AS `RegistrationDate`,
                COUNT(b.`booking_id`) AS `TotalBookings`,
                SUM(CASE WHEN b.`booking_status` = 'Confirmed / Cleared for Exam' THEN 1 ELSE 0 END) AS `ConfirmedCount`,
                SUM(CASE WHEN b.`booking_status` = 'Pending Payment' THEN 1 ELSE 0 END) AS `PendingCount`,
                SUM(CASE WHEN b.`booking_status` = 'Cancelled' THEN 1 ELSE 0 END) AS `CancelledCount`,
                IFNULL(SUM(CASE WHEN b.`booking_status` = 'Confirmed / Cleared for Exam' THEN e.`fee` ELSE 0 END), 0) AS `ConfirmedRevenue`
            FROM `bookings` b
            JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
            JOIN `exams` e ON s.`exam_id` = e.`exam_id`
            WHERE DATE(b.`booking_date`) BETWEEN @StartDate AND @EndDate
            GROUP BY DATE(b.`booking_date`)
            ORDER BY `RegistrationDate` DESC;";

            return DatabaseHelper.ExecuteDataTable(sql,
                new MySqlParameter("@StartDate", startDate.ToString("yyyy-MM-dd")),
                new MySqlParameter("@EndDate", endDate.ToString("yyyy-MM-dd")));
        }

        // FR-4.4: Fee collections and revenue report
        public DataTable GetFeeCollectionsReport(DateTime startDate, DateTime endDate)
        {
            string sql = @"
            SELECT 
                p.`receipt_number` AS `ReceiptNumber`,
                DATE(p.`verified_at`) AS `PaymentDate`,
                p.`amount_paid` AS `Amount`,
                p.`payment_method` AS `PaymentMethod`,
                b.`booking_reference` AS `BookingReference`,
                u.`full_name` AS `CandidateName`,
                u.`email` AS `CandidateEmail`,
                e.`exam_code` AS `ExamCode`,
                e.`title` AS `ExamTitle`,
                v.`full_name` AS `VerifiedBy`
            FROM `payments` p
            JOIN `bookings` b ON p.`booking_id` = b.`booking_id`
            JOIN `users` u ON b.`candidate_id` = u.`user_id`
            JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
            JOIN `exams` e ON s.`exam_id` = e.`exam_id`
            JOIN `users` v ON p.`verified_by_user_id` = v.`user_id`
            WHERE DATE(p.`verified_at`) BETWEEN @StartDate AND @EndDate
            ORDER BY p.`verified_at` DESC;";

            return DatabaseHelper.ExecuteDataTable(sql,
                new MySqlParameter("@StartDate", startDate.ToString("yyyy-MM-dd")),
                new MySqlParameter("@EndDate", endDate.ToString("yyyy-MM-dd")));
        }

        // FR-4.4: Venue occupancy rates
        public DataTable GetVenueOccupancyReport(DateTime? fromDate = null, DateTime? toDate = null)
        {
            string sql = @"
            SELECT 
                v.`venue_id` AS `VenueId`,
                v.`venue_name` AS `VenueName`,
                v.`city` AS `City`,
                v.`room_number` AS `Room`,
                v.`total_capacity` AS `VenueRoomCapacity`,
                COUNT(s.`schedule_id`) AS `TotalSessionsScheduled`,
                IFNULL(SUM(s.`max_seats`), 0) AS `TotalOfferedSeats`,
                IFNULL(SUM((SELECT COUNT(*) FROM `bookings` b 
                            WHERE b.`schedule_id` = s.`schedule_id` 
                              AND b.`booking_status` IN ('Pending Payment', 'Confirmed / Cleared for Exam'))), 0) AS `TotalBookedSeats`,
                CASE 
                    WHEN IFNULL(SUM(s.`max_seats`), 0) > 0 
                    THEN ROUND((IFNULL(SUM((SELECT COUNT(*) FROM `bookings` b 
                                            WHERE b.`schedule_id` = s.`schedule_id` 
                                              AND b.`booking_status` IN ('Pending Payment', 'Confirmed / Cleared for Exam'))), 0) * 100.0) / SUM(s.`max_seats`), 1)
                    ELSE 0.0
                END AS `OccupancyRatePercent`
            FROM `venues` v
            LEFT JOIN `exam_schedules` s ON v.`venue_id` = s.`venue_id` AND s.`status` != 'Cancelled'
            WHERE 1=1 ";

            var parameters = new System.Collections.Generic.List<MySqlParameter>();

            if (fromDate.HasValue)
            {
                sql += " AND (s.`exam_date` >= @FromDate OR s.`exam_date` IS NULL) ";
                parameters.Add(new MySqlParameter("@FromDate", fromDate.Value.ToString("yyyy-MM-dd")));
            }
            if (toDate.HasValue)
            {
                sql += " AND (s.`exam_date` <= @ToDate OR s.`exam_date` IS NULL) ";
                parameters.Add(new MySqlParameter("@ToDate", toDate.Value.ToString("yyyy-MM-dd")));
            }

            sql += @"
            GROUP BY v.`venue_id`, v.`venue_name`, v.`city`, v.`room_number`, v.`total_capacity`
            ORDER BY `OccupancyRatePercent` DESC, v.`venue_name` ASC;";

            return DatabaseHelper.ExecuteDataTable(sql, parameters.ToArray());
        }
    }
}
