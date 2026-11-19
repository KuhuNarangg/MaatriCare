// VaccineTrackerViewModel.cs - filterable clinic-wide vaccination tracker model across all newborns.
using MaatriCare.Models;
namespace MaatriCare.ViewModels;

public class VaccineTrackerViewModel
{
    public string Filter { get; set; } = "pending";   // all, pending, week, overdue, given
    public int TotalPending { get; set; }
    public int TotalDueThisWeek { get; set; }
    public int TotalOverdue { get; set; }
    public int TotalGiven { get; set; }
    public List<Vaccination> Items { get; set; } = new();
}
