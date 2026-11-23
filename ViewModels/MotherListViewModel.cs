// MotherListViewModel.cs - filter, sort, and pagination model for the Mothers directory.
using MaatriCare.Models;
namespace MaatriCare.ViewModels;

public class MotherListViewModel
{
    public List<MotherRowItem> Items { get; set; } = new();
    public List<string> Villages { get; set; } = new();

    // Active filter & sort state
    public string Stage { get; set; } = "expectant";   // expectant, delivered, all
    public int ExpectantCount { get; set; }
    public int DeliveredCount { get; set; }
    public string Query { get; set; }
    public string Village { get; set; }
    public string Risk { get; set; }
    public int? Trimester { get; set; }
    public string Sort { get; set; } = "name";

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 8;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public class MotherRowItem
{
    public Mother Mother { get; set; }
    public string RiskLevel { get; set; }
    public int RiskScore { get; set; }
}
