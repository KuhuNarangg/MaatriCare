// AppointmentFormViewModel.cs - form model for booking or rescheduling a clinic appointment.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class AppointmentFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a mother.")]
    public int MotherId { get; set; }

    public int? NewbornId { get; set; }

    [Required(ErrorMessage = "Please select a doctor.")]
    public string DoctorUsername { get; set; } = "doctor";

    [Required(ErrorMessage = "Please choose an appointment date.")]
    [DataType(DataType.Date)]
    public DateTime ScheduledDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Please choose a start time.")]
    public string StartTime { get; set; } = DateTime.Now.ToString("HH:mm");

    [Range(5, 120, ErrorMessage = "Duration must be between 5 and 120 minutes.")]
    public int DurationMinutes { get; set; } = 20;

    [Required(ErrorMessage = "Please enter the appointment reason.")]
    [StringLength(120)]
    public string Purpose { get; set; } = "Routine ANC Check-up";

    [StringLength(300)]
    public string Notes { get; set; }
}
