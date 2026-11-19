// AppointmentFormViewModel.cs - form model for booking or rescheduling an ANC appointment.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class AppointmentFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a mother.")]
    public int MotherId { get; set; }

    [Required(ErrorMessage = "Please choose an appointment date.")]
    [DataType(DataType.Date)]
    public DateTime ScheduledDate { get; set; } = DateTime.Today.AddDays(3);

    [Required(ErrorMessage = "Please enter the appointment purpose.")]
    [StringLength(120)]
    public string Purpose { get; set; } = "Routine ANC Check-up";

    [StringLength(300)]
    public string Notes { get; set; }
}
