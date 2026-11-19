using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;

namespace OnlineExamsSystem.Services
{
    public class PaymentService
    {
        // FR-4.2: View all pending and confirmed exam bookings per venue and date
        public List<Booking> GetBookingsForOversight(int? venueId, DateTime? examDate, string statusFilter)
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
            WHERE 1=1 ";

            var parameters = new List<MySqlParameter>();

            if (venueId.HasValue && venueId.Value > 0)
            {
                sql += " AND s.`venue_id` = @VenueId ";
                parameters.Add(new MySqlParameter("@VenueId", venueId.Value));
            }

            if (examDate.HasValue)
            {
                sql += " AND s.`exam_date` = @ExamDate ";
                parameters.Add(new MySqlParameter("@ExamDate", examDate.Value.ToString("yyyy-MM-dd")));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All Statuses")
            {
                sql += " AND b.`booking_status` = @Status ";
                parameters.Add(new MySqlParameter("@Status", statusFilter.Trim()));
            }

            sql += " ORDER BY s.`exam_date` ASC, s.`start_time` ASC, b.`booking_date` DESC;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, parameters.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Booking
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
                });
            }

            return list;
        }

        // FR-4.3: Authorized staff record and verify on-site payments manually
        public bool RecordAndVerifyPayment(int bookingId, decimal amountPaid, string paymentMethod, string remarks, int verifiedByUserId, out string receiptNumber, out string error)
        {
            receiptNumber = string.Empty;
            error = string.Empty;

            if (bookingId <= 0)
            {
                error = "Invalid booking ID.";
                return false;
            }

            if (amountPaid <= 0)
            {
                error = "Payment amount must be greater than zero.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                error = "Please specify a payment method (Cash at Center, Bank Slip, Mobile Money, Credit Card).";
                return false;
            }

            try
            {
                var bookingService = new BookingService();
                var booking = bookingService.GetBookingById(bookingId);
                if (booking == null)
                {
                    error = "Booking record not found.";
                    return false;
                }

                if (booking.IsConfirmed)
                {
                    error = "This booking has already been verified and cleared for the examination.";
                    return false;
                }

                if (booking.IsCancelled)
                {
                    error = "Cannot process payment for a cancelled booking.";
                    return false;
                }

                receiptNumber = GenerateReceiptNumber();

                // Insert payment record
                string paymentSql = @"
                INSERT INTO `payments`
                (`booking_id`, `amount_paid`, `payment_method`, `receipt_number`, `payment_status`, `verified_by_user_id`, `verified_at`, `remarks`)
                VALUES
                (@BookingId, @Amount, @Method, @Receipt, 'Verified', @VerifierId, NOW(), @Remarks)
                ON DUPLICATE KEY UPDATE
                `amount_paid` = @Amount,
                `payment_method` = @Method,
                `payment_status` = 'Verified',
                `verified_by_user_id` = @VerifierId,
                `verified_at` = NOW(),
                `remarks` = @Remarks;";

                DatabaseHelper.ExecuteNonQuery(paymentSql,
                    new MySqlParameter("@BookingId", bookingId),
                    new MySqlParameter("@Amount", amountPaid),
                    new MySqlParameter("@Method", paymentMethod.Trim()),
                    new MySqlParameter("@Receipt", receiptNumber),
                    new MySqlParameter("@VerifierId", verifiedByUserId),
                    new MySqlParameter("@Remarks", (object)remarks?.Trim() ?? DBNull.Value));

                // Update booking status to 'Confirmed / Cleared for Exam'
                string updateBookingSql = @"
                UPDATE `bookings`
                SET `booking_status` = 'Confirmed / Cleared for Exam',
                    `notes` = CONCAT(IFNULL(`notes`,''), ' | On-site payment cleared via Receipt #', @Receipt, ' on ', NOW())
                WHERE `booking_id` = @BookingId;";

                DatabaseHelper.ExecuteNonQuery(updateBookingSql,
                    new MySqlParameter("@Receipt", receiptNumber),
                    new MySqlParameter("@BookingId", bookingId));

                AuditService.LogAction(verifiedByUserId, "PAYMENT_RECORDED_AND_CLEARED", $"Verified payment of {amountPaid:C} for Booking {booking.BookingReference}. Receipt: {receiptNumber}. Method: {paymentMethod}");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public Payment GetPaymentByBookingId(int bookingId)
        {
            string sql = @"
            SELECT p.*, b.`booking_reference`, u.`full_name` AS `candidate_name`, u.`email` AS `candidate_email`,
                   e.`exam_code`, e.`title` AS `exam_title`, v.`full_name` AS `verifier_name`
            FROM `payments` p
            JOIN `bookings` b ON p.`booking_id` = b.`booking_id`
            JOIN `users` u ON b.`candidate_id` = u.`user_id`
            JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
            JOIN `exams` e ON s.`exam_id` = e.`exam_id`
            JOIN `users` v ON p.`verified_by_user_id` = v.`user_id`
            WHERE p.`booking_id` = @BookingId;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, new MySqlParameter("@BookingId", bookingId));
            if (dt.Rows.Count > 0)
                return MapPayment(dt.Rows[0]);
            return null;
        }

        public List<Payment> GetAllPayments()
        {
            var list = new List<Payment>();
            string sql = @"
            SELECT p.*, b.`booking_reference`, u.`full_name` AS `candidate_name`, u.`email` AS `candidate_email`,
                   e.`exam_code`, e.`title` AS `exam_title`, v.`full_name` AS `verifier_name`
            FROM `payments` p
            JOIN `bookings` b ON p.`booking_id` = b.`booking_id`
            JOIN `users` u ON b.`candidate_id` = u.`user_id`
            JOIN `exam_schedules` s ON b.`schedule_id` = s.`schedule_id`
            JOIN `exams` e ON s.`exam_id` = e.`exam_id`
            JOIN `users` v ON p.`verified_by_user_id` = v.`user_id`
            ORDER BY p.`verified_at` DESC;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
            foreach (DataRow row in dt.Rows)
                list.Add(MapPayment(row));

            return list;
        }

        private string GenerateReceiptNumber()
        {
            var rand = new Random();
            string receipt;
            bool unique = false;
            do
            {
                int num = rand.Next(1000, 9999);
                receipt = $"REC-{DateTime.Now.Year}-{num}";
                string check = "SELECT COUNT(*) FROM `payments` WHERE `receipt_number` = @Rec;";
                long count = Convert.ToInt64(DatabaseHelper.ExecuteScalar(check, new MySqlParameter("@Rec", receipt)));
                if (count == 0)
                    unique = true;
            } while (!unique);

            return receipt;
        }

        private Payment MapPayment(DataRow row)
        {
            return new Payment
            {
                PaymentId = Convert.ToInt32(row["payment_id"]),
                BookingId = Convert.ToInt32(row["booking_id"]),
                BookingReference = row["booking_reference"]?.ToString(),
                CandidateName = row["candidate_name"]?.ToString(),
                CandidateEmail = row["candidate_email"]?.ToString(),
                ExamCode = row["exam_code"]?.ToString(),
                ExamTitle = row["exam_title"]?.ToString(),
                AmountPaid = Convert.ToDecimal(row["amount_paid"]),
                PaymentMethod = row["payment_method"].ToString(),
                ReceiptNumber = row["receipt_number"].ToString(),
                PaymentStatus = row["payment_status"].ToString(),
                VerifiedByUserId = Convert.ToInt32(row["verified_by_user_id"]),
                VerifierName = row["verifier_name"]?.ToString(),
                VerifiedAt = Convert.ToDateTime(row["verified_at"]),
                Remarks = row["remarks"]?.ToString()
            };
        }
    }
}
