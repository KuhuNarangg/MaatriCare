// NewbornFormViewModel.cs - form model for registering a newborn with APGAR, gestational age, and delivery link.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class NewbornFormViewModel
{
    public int MotherId { get; set; }
    public string MotherName { get; set; }
    public int? DeliveryId { get; set; }

    [Required(ErrorMessage = "Please enter the baby's name.")]
    [StringLength(100)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Please select date of birth.")]
    [DataType(DataType.Date)]
    public DateTime Dob { get; set; } = DateTime.Today;

    [Required]
    public string Gender { get; set; } = "Female";

    [Range(0.5, 6.5, ErrorMessage = "Birth weight must be between 0.5 and 6.5 kg.")]
    public double BirthWeight { get; set; } = 3.0;

    [Range(0, 10, ErrorMessage = "APGAR score must be between 0 and 10.")]
    public int? ApgarScore { get; set; } = 9;

    [Range(24, 43, ErrorMessage = "Gestational age must be between 24 and 43 weeks.")]
    public int? GestationalAgeWeeks { get; set; } = 39;
}
