// ClinicalModels.cs - VisitRecord, PrescriptionItem, FollowUpContact, and AuditLog database entities.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.Models;

public class VisitRecord
{
    public int Id { get; set; }
    public int MotherId { get; set; }
    public Mother Mother { get; set; }
    public int? NewbornId { get; set; }
    public Newborn Newborn { get; set; }
    public int? AppointmentId { get; set; }
    public Appointment Appointment { get; set; }

    public DateTime VisitDate { get; set; } = DateTime.Now;
    public bool IsDraft { get; set; }

    // Vitals (editable by Doctor, Nurse, Admin)
    public int? Systolic { get; set; }
    public int? Diastolic { get; set; }
    public double? WeightKg { get; set; }
    public double? Hb { get; set; }
    public int? BloodSugar { get; set; }
    public double? TemperatureF { get; set; }
    public int? PulseBpm { get; set; }

    // Doctor's clinical section (editable by Doctor, Admin)
    [MaxLength(500)] public string Symptoms { get; set; } = "";
    [MaxLength(400)] public string Diagnosis { get; set; } = "";
    [MaxLength(1000)] public string MedicalNotes { get; set; } = "";
    [MaxLength(400)] public string TestsAdvised { get; set; } = "";
    [MaxLength(600)] public string Advice { get; set; } = "";
    [DataType(DataType.Date)] public DateTime? NextFollowUpDate { get; set; }
    [MaxLength(100)] public string DoctorAuthorName { get; set; } = "";
    [MaxLength(30)] public string DoctorAuthorRole { get; set; } = "";
    public DateTime? DoctorUpdatedAt { get; set; }

    // Nurse's clinical section (editable by Nurse who authored it, or Admin)
    [MaxLength(800)] public string NurseNotes { get; set; } = "";
    [MaxLength(400)] public string MedicationAdministered { get; set; } = "";
    [MaxLength(400)] public string VaccinesGivenSummary { get; set; } = "";
    [MaxLength(60)] public string NurseAuthorUsername { get; set; } = "";
    [MaxLength(100)] public string NurseAuthorName { get; set; } = "";
    [MaxLength(30)] public string NurseAuthorRole { get; set; } = "";
    public DateTime? NurseUpdatedAt { get; set; }

    public List<PrescriptionItem> Prescriptions { get; set; } = new();
}

public class PrescriptionItem
{
    public int Id { get; set; }
    public int VisitRecordId { get; set; }
    public VisitRecord VisitRecord { get; set; }
    [Required, MaxLength(140)] public string MedicineName { get; set; }
    [MaxLength(60)] public string Dose { get; set; } = "";
    [MaxLength(80)] public string Frequency { get; set; } = "";
    [MaxLength(60)] public string Duration { get; set; } = "";
    [MaxLength(200)] public string Instructions { get; set; } = "";
    [MaxLength(100)] public string AuthorName { get; set; } = "";
    [MaxLength(30)] public string AuthorRole { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class FollowUpContact
{
    public int Id { get; set; }
    [Required, MaxLength(80)] public string TargetKey { get; set; }   // e.g. "appt-4", "anc-6", "vax-12"
    [MaxLength(100)] public string ContactedBy { get; set; } = "";
    [MaxLength(300)] public string Note { get; set; } = "";
    public DateTime ContactedAt { get; set; } = DateTime.Now;
}

public class AuditLog
{
    public int Id { get; set; }
    [MaxLength(60)] public string Username { get; set; } = "";
    [MaxLength(30)] public string Role { get; set; } = "";
    [MaxLength(40)] public string Action { get; set; } = "";       // Create, Edit, Delete, StatusChange
    [MaxLength(60)] public string EntityType { get; set; } = "";   // Mother, Appointment, VisitRecord, Referral, Vaccine, User
    public int? EntityId { get; set; }
    [MaxLength(400)] public string Details { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
