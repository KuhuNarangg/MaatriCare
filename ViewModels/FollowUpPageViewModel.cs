// FollowUpPageViewModel.cs - tabbed follow-up queue (Today, Overdue, Upcoming) with call and mark-done info.
namespace MaatriCare.ViewModels;

public class FollowUpPageViewModel
{
    public string Tab { get; set; } = "today";   // today, overdue, upcoming
    public int TodayCount { get; set; }
    public int OverdueCount { get; set; }
    public int UpcomingCount { get; set; }
    public List<FollowUpTaskRow> Items { get; set; } = new();
}

public class FollowUpTaskRow
{
    public string TargetKey { get; set; }  // e.g. "appt-4", "anc-6", "vax-12"
    public string Kind { get; set; }       // Appointment, ANC Visit, Vaccine
    public int? AppointmentId { get; set; }
    public int? VaccineId { get; set; }
    public string PersonName { get; set; }
    public string Village { get; set; }
    public string Phone { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; }     // Today, Overdue, Upcoming
    public string DetailUrl { get; set; }
    public string LastContactNote { get; set; }
    public DateTime? LastContactedAt { get; set; }
}
