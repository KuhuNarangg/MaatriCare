// AncVisitFormViewModel.cs - form model for logging or editing an antenatal check-up visit.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class AncVisitFormViewModel
{
    public int Id { get; set; }
    public int MotherId { get; set; }
    public string MotherName { get; set; }

    [Required(ErrorMessage = "Please select the visit date.")]
    [DataType(DataType.Date)]
    public DateTime VisitDate { get; set; } = DateTime.Today;

    [Range(60, 260, ErrorMessage = "Systolic BP should be between 60 and 260.")]
    public int Systolic { get; set; } = 118;

    [Range(40, 160, ErrorMessage = "Diastolic BP should be between 40 and 160.")]
    public int Diastolic { get; set; } = 78;

    [Range(2, 20, ErrorMessage = "Hemoglobin (Hb) should be between 2 and 20 g/dL.")]
    public double Hb { get; set; } = 11.5;

    [Range(40, 500, ErrorMessage = "Blood sugar should be between 40 and 500 mg/dL.")]
    public int BloodSugar { get; set; } = 95;

    [Range(25, 160, ErrorMessage = "Weight should be between 25 and 160 kg.")]
    public double? WeightKg { get; set; }

    public string UrineProtein { get; set; } = "Nil";

    [Range(60, 220, ErrorMessage = "Fetal heartbeat should be between 60 and 220 bpm.")]
    public int? FetalHeartbeat { get; set; }

    [DataType(DataType.Date)]
    public DateTime? NextVisitDate { get; set; }

    [StringLength(500)]
    public string Notes { get; set; }
}
