using System;

namespace OnlineExamsSystem.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; }
        public int CandidateId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public string CandidatePhone { get; set; }
        public int ScheduleId { get; set; }
        public int ExamId { get; set; }
        public string ExamCode { get; set; }
        public string ExamTitle { get; set; }
        public decimal ExamFee { get; set; }
        public int DurationMinutes { get; set; }
        public int PassingScore { get; set; }
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public string VenueAddress { get; set; }
        public string VenueCity { get; set; }
        public string RoomNumber { get; set; }
        public string AssignedProctor { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime BookingDate { get; set; }
        public string BookingStatus { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string TimeWindow => $"{DateTime.Today.Add(StartTime):hh:mm tt} - {DateTime.Today.Add(EndTime):hh:mm tt}";
        public bool IsPendingPayment => string.Equals(BookingStatus, "Pending Payment", StringComparison.OrdinalIgnoreCase);
        public bool IsConfirmed => string.Equals(BookingStatus, "Confirmed / Cleared for Exam", StringComparison.OrdinalIgnoreCase);
        public bool IsCancelled => string.Equals(BookingStatus, "Cancelled", StringComparison.OrdinalIgnoreCase);
        public bool IsCompleted => string.Equals(BookingStatus, "Completed", StringComparison.OrdinalIgnoreCase);

        // Can reschedule or cancel up to cutoff window (e.g. 24h prior to exam date + start time)
        public bool CanModify(int cutoffHours)
        {
            if (IsCancelled || IsCompleted) return false;
            DateTime examFullDateTime = ExamDate.Date + StartTime;
            return DateTime.Now.AddHours(cutoffHours) < examFullDateTime;
        }
    }

    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public string BookingReference { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public string ExamCode { get; set; }
        public string ExamTitle { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public string ReceiptNumber { get; set; }
        public string PaymentStatus { get; set; }
        public int VerifiedByUserId { get; set; }
        public string VerifierName { get; set; }
        public DateTime VerifiedAt { get; set; }
        public string Remarks { get; set; }
    }

    public class AuditLog
    {
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string UserEmail { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DashboardStats
    {
        public int TotalCandidates { get; set; }
        public int TotalExams { get; set; }
        public int TotalVenues { get; set; }
        public int TotalSchedules { get; set; }
        public int TotalBookings { get; set; }
        public int PendingClearanceCount { get; set; }
        public int ConfirmedBookingsCount { get; set; }
        public int CancelledBookingsCount { get; set; }
        public decimal TotalRevenueCollected { get; set; }
        public decimal PendingRevenueAmount { get; set; }
        public int TodayRegistrationsCount { get; set; }
    }
}
