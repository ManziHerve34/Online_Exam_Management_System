using System;

namespace OnlineExamsSystem.Models
{
    public class ExamCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }

    public class Exam
    {
        public int ExamId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ExamCode { get; set; }
        public string Title { get; set; }
        public string Discipline { get; set; }
        public string CertificationLevel { get; set; }
        public string Prerequisites { get; set; }
        public int DurationMinutes { get; set; }
        public int PassingScore { get; set; }
        public decimal Fee { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Venue
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string RoomNumber { get; set; }
        public int TotalCapacity { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public bool IsActive { get; set; }

        public string DisplayName => $"{VenueName} ({City} - {RoomNumber})";
    }

    public class ExamSchedule
    {
        public int ScheduleId { get; set; }
        public int ExamId { get; set; }
        public string ExamCode { get; set; }
        public string ExamTitle { get; set; }
        public decimal ExamFee { get; set; }
        public int DurationMinutes { get; set; }
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public string VenueCity { get; set; }
        public string RoomNumber { get; set; }
        public string VenueAddress { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxSeats { get; set; }
        public int BookedSeats { get; set; }
        public int AvailableSeats => Math.Max(0, MaxSeats - BookedSeats);
        public string AssignedProctor { get; set; }
        public string Status { get; set; }

        public string TimeWindow => $"{DateTime.Today.Add(StartTime):hh:mm tt} - {DateTime.Today.Add(EndTime):hh:mm tt}";
        public string DateString => ExamDate.ToString("yyyy-MM-dd (ddd)");
        public string DisplaySlot => $"{DateString} | {TimeWindow} | Available: {AvailableSeats}/{MaxSeats} Seats";
    }
}
