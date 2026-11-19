// GrowthRecordViewModel.cs - form model for adding a newborn weight & height growth entry.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class GrowthRecordViewModel
{
    public int NewbornId { get; set; }

    [Required(ErrorMessage = "Please choose the measurement date.")]
    [DataType(DataType.Date)]
    public DateTime RecordedDate { get; set; } = DateTime.Today;

    [Range(0.5, 25, ErrorMessage = "Weight must be between 0.5 and 25 kg.")]
    public double WeightKg { get; set; }

    [Range(30, 120, ErrorMessage = "Height must be between 30 and 120 cm.")]
    public double HeightCm { get; set; }

    [StringLength(200)]
    public string Notes { get; set; }
}
