// MotherFormViewModel.cs - form model for registering or editing a mother's profile.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class MotherFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter the mother's full name.")]
    [StringLength(100)]
    public string Name { get; set; }

    [Range(12, 55, ErrorMessage = "Please enter an age between 12 and 55.")]
    public int Age { get; set; } = 24;

    [Required(ErrorMessage = "Please enter the village name.")]
    public string Village { get; set; }

    [Required(ErrorMessage = "Please enter a contact phone number.")]
    public string Phone { get; set; }

    public string BloodGroup { get; set; }

    public bool PreviousCSection { get; set; }

    [Range(0, 10, ErrorMessage = "Previous complications must be between 0 and 10.")]
    public int PreviousComplications { get; set; }

    [Required(ErrorMessage = "Please select the Last Menstrual Period (LMP) date.")]
    [DataType(DataType.Date)]
    public DateTime Lmp { get; set; } = DateTime.Today.AddDays(-60);
}
