// BabyPortalViewModel.cs - directory, KPI summary, and filters for the dedicated Baby Portal.
using MaatriCare.Models;
namespace MaatriCare.ViewModels;

public class BabyPortalViewModel
{
    public string Query { get; set; } = "";
    public string Village { get; set; } = "";
    public string Flag { get; set; } = "all";   // all, lbw, preterm, overdue
    public List<string> Villages { get; set; } = new();

    public int TotalBabies { get; set; }
    public int LbwCount { get; set; }
    public int PretermCount { get; set; }
    public int OverdueVaccinesBabiesCount { get; set; }

    public List<Newborn> Babies { get; set; } = new();
}
