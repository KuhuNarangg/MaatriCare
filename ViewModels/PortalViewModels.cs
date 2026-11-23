// PortalViewModels.cs - ViewModels for Reception Desk, Doctor/Nurse Portals, Consultation, and Live Portal.
using System.ComponentModel.DataAnnotations;
using MaatriCare.Models;
namespace MaatriCare.ViewModels;

public class ReceptionDeskViewModel
{
    public string Range { get; set; } = "today";   // today, week, all
    public string Status { get; set; } = "";
    public string Doctor { get; set; } = "";
    public List<AppUser> Doctors { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
    public List<Appointment> TodaysCalendar { get; set; } = new();
}

public class RegisterAndBookViewModel
{
    // Mother details
    [Required(ErrorMessage = "Please enter the mother's full name.")]
    public string Name { get; set; }
    [Range(12, 55, ErrorMessage = "Age must be between 12 and 55.")]
    public int Age { get; set; } = 25;
    [Required(ErrorMessage = "Please enter the village.")]
    public string Village { get; set; } = "Phagwara";
    [Required(ErrorMessage = "Please enter a phone number.")]
    public string Phone { get; set; }
    public string BloodGroup { get; set; } = "O+";
    [DataType(DataType.Date)]
    public DateTime Lmp { get; set; } = DateTime.Today.AddDays(-90);
    public bool PreviousCSection { get; set; }

    // Appointment details
    [Required] public string DoctorUsername { get; set; } = "doctor";
    [Required, DataType(DataType.Date)] public DateTime ScheduledDate { get; set; } = DateTime.Today;
    [Required] public string StartTime { get; set; } = DateTime.Now.ToString("HH:mm");
    [Range(5, 120)] public int DurationMinutes { get; set; } = 20;
    [Required] public string Purpose { get; set; } = "Initial ANC Consultation";
    public string Notes { get; set; } = "";
}

public class ConsultationViewModel
{
    public Appointment Appointment { get; set; }
    public Mother Mother { get; set; }
    public string RiskLevel { get; set; }
    public int RiskScore { get; set; }
    public List<string> RiskReasons { get; set; } = new();
    public string RecommendedAction { get; set; }

    public VisitRecord Record { get; set; }
    public List<VisitRecord> PreviousVisits { get; set; } = new();
    public List<Vaccination> BabyPendingVaccines { get; set; } = new();

    // Role permission flags for UI rendering
    public bool IsReadOnly { get; set; }
    public bool CanEditVitals { get; set; }
    public bool CanEditDoctorSection { get; set; }
    public bool CanEditNurseSection { get; set; }
    public bool CanCreateReferral { get; set; }
}

public class ConsultationPostModel
{
    public int AppointmentId { get; set; }
    public int VisitRecordId { get; set; }
    public string SubmitMode { get; set; } = "draft";   // draft, submit, submit_next, nurse_save

    // Vitals
    public int? Systolic { get; set; }
    public int? Diastolic { get; set; }
    public double? WeightKg { get; set; }
    public double? Hb { get; set; }
    public int? BloodSugar { get; set; }
    public double? TemperatureF { get; set; }
    public int? PulseBpm { get; set; }

    // Doctor section
    public string Symptoms { get; set; }
    public string Diagnosis { get; set; }
    public string MedicalNotes { get; set; }
    public string TestsAdvised { get; set; }
    public string Advice { get; set; }
    public DateTime? NextFollowUpDate { get; set; }

    // Prescription rows (parallel arrays bound from dynamic JS form)
    public List<string> RxMedicine { get; set; } = new();
    public List<string> RxDose { get; set; } = new();
    public List<string> RxFrequency { get; set; } = new();
    public List<string> RxDuration { get; set; } = new();
    public List<string> RxInstructions { get; set; } = new();

    // Optional hospital referral from doctor
    public string ReferralHospital { get; set; }
    public string ReferralReason { get; set; }

    // Nurse section
    public string NurseNotes { get; set; }
    public string MedicationAdministered { get; set; }
    public int? VaccineIdGiven { get; set; }
    public string VaccineNameGiven { get; set; }
    public DateTime? VaccineDateGiven { get; set; }
    public string VaccineBatchGiven { get; set; }
}

public class LivePortalViewModel
{
    public int TotalToday { get; set; }
    public int AttendedToday { get; set; }
    public int MissedToday { get; set; }
    public int WaitingCount { get; set; }
    public int InConsultationCount { get; set; }

    public List<Appointment> LiveNow { get; set; } = new();
    public List<Appointment> Waiting { get; set; } = new();
    public List<Appointment> InConsultation { get; set; } = new();
    public List<Appointment> CompletedToday { get; set; } = new();
    public List<Appointment> MissedTodayList { get; set; } = new();
    public List<Appointment> UpcomingToday { get; set; } = new();
}
