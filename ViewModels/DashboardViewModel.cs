// DashboardViewModel.cs - data transfer objects for Admin/Doctor, Nurse, and Mother dashboards.
using MaatriCare.Models;
namespace MaatriCare.ViewModels;

public class DashboardViewModel
{
    public string Role { get; set; }
    public string Username { get; set; }

    // Admin / Doctor KPIs
    public int TotalMothers { get; set; }
    public int HighRiskCount { get; set; }
    public int MediumRiskCount { get; set; }
    public int LowRiskCount { get; set; }
    public int UpcomingDeliveriesMonth { get; set; }
    public int OverdueAncCount { get; set; }
    public int NewbornsCount { get; set; }
    public int VaccinationCoveragePercent { get; set; }

    // Chart.js datasets
    public List<string> MonthLabels { get; set; } = new();
    public List<int> MonthRegistrationCounts { get; set; } = new();
    public List<string> VillageLabels { get; set; } = new();
    public List<int> VillageCoveragePercents { get; set; } = new();

    // Lists & Feeds
    public List<RiskAlertItem> HighRiskAlerts { get; set; } = new();
    public List<FollowUpItem> TodaysFollowUps { get; set; } = new();
    public List<ActivityItem> RecentActivity { get; set; } = new();

    // Nurse specific
    public List<string> AssignedVillages { get; set; } = new();
    public List<FollowUpItem> WorkerTasks { get; set; } = new();
    public List<FollowUpItem> WorkerOverdue { get; set; } = new();

    // Mother specific
    public Mother LinkedMother { get; set; }
    public string LinkedMotherRiskLevel { get; set; }
    public DateTime? NextRecommendedAncDate { get; set; }
    public Newborn LinkedBaby { get; set; }
    public Vaccination NextBabyVaccine { get; set; }
    public List<HealthTip> MotherTips { get; set; } = new();
}

public class RiskAlertItem
{
    public int MotherId { get; set; }
    public string MotherName { get; set; }
    public string Village { get; set; }
    public int Week { get; set; }
    public List<string> Reasons { get; set; } = new();
}

public class FollowUpItem
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string Status { get; set; }   // Overdue, Due Soon, Today
    public string LinkUrl { get; set; }
}

public class ActivityItem
{
    public string Kind { get; set; }     // Registration, Visit, Vaccine
    public string Text { get; set; }
    public DateTime Date { get; set; }
}
