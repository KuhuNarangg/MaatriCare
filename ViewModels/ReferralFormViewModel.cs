// ReferralFormViewModel.cs - form model for referring a high-risk mother to a hospital.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class ReferralFormViewModel
{
    [Required(ErrorMessage = "Please select a mother.")]
    public int MotherId { get; set; }

    [Required(ErrorMessage = "Please enter the destination hospital name.")]
    [StringLength(140)]
    public string HospitalName { get; set; } = "Civil Hospital Phagwara (OB-GYN)";

    [Required(ErrorMessage = "Please state the clinical reason for referral.")]
    [StringLength(300)]
    public string Reason { get; set; }

    public string Priority { get; set; } = "Urgent";

    [DataType(DataType.Date)]
    public DateTime ReferredDate { get; set; } = DateTime.Today;

    [StringLength(400)]
    public string Notes { get; set; }
}
