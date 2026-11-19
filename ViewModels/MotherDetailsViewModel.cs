// MotherDetailsViewModel.cs - bundles Mother profile, risk panel, auto ANC schedule, and journey milestones.
using MaatriCare.Models;
namespace MaatriCare.ViewModels;

public class MotherDetailsViewModel
{
    public Mother Mother { get; set; }
    public string RiskLevel { get; set; }
    public int RiskScore { get; set; }
    public List<string> RiskReasons { get; set; } = new();
    public string RecommendedAction { get; set; }
    public List<AncScheduleSlot> AncSchedule { get; set; } = new();
    public List<TimelineMilestone> VisitMilestones { get; set; } = new();
}

public class AncScheduleSlot
{
    public string Label { get; set; }      // e.g. "ANC 1 (Before 12 weeks)"
    public string WindowText { get; set; } // e.g. "Weeks 1 – 12"
    public DateTime DueDate { get; set; }
    public string Status { get; set; }     // Done, Overdue, Upcoming
    public DateTime? CompletedOn { get; set; }
}

public class TimelineMilestone
{
    public int VisitId { get; set; }
    public int Week { get; set; }
    public DateTime VisitDate { get; set; }
    public string Summary { get; set; }
}
