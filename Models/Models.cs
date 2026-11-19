// Models.cs - all database entities (one class = one table).
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MaatriCare.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string PasswordHash { get; set; }
    public string Role { get; set; }   // Admin, Doctor, HealthWorker, Mother
    public bool IsActive { get; set; } = true;
    public string AssignedVillages { get; set; } = "";
    public int? MotherId { get; set; }
}

public class Mother
{
    public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Range(12, 55)] public int Age { get; set; }
    [Required] public string Village { get; set; }
    [Required] public string Phone { get; set; }
    public string BloodGroup { get; set; }
    public bool PreviousCSection { get; set; }
    [Range(0, 10)] public int PreviousComplications { get; set; }
    [DataType(DataType.Date)] public DateTime Lmp { get; set; }   // last menstrual period
    public DateTime RegisteredAt { get; set; } = DateTime.Today;
    public List<AncVisit> Visits { get; set; } = new();
    public List<Newborn> Newborns { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
    public List<Delivery> Deliveries { get; set; } = new();

    [NotMapped] public DateTime Edd => Lmp.AddDays(280);                       // expected delivery date
    [NotMapped] public int Week => Math.Clamp((int)((DateTime.Today - Lmp).TotalDays / 7), 0, 40);
    [NotMapped] public int Trimester => Week <= 12 ? 1 : Week <= 26 ? 2 : 3;
    [NotMapped] public int DaysToEdd => (Edd.Date - DateTime.Today).Days;
}

public class AncVisit   // one antenatal check-up
{
    public int Id { get; set; }
    public int MotherId { get; set; }
    [DataType(DataType.Date)] public DateTime VisitDate { get; set; }
    public int Systolic { get; set; }
    public int Diastolic { get; set; }
    public double Hb { get; set; }
    public int BloodSugar { get; set; }
    public double? WeightKg { get; set; }
    public string UrineProtein { get; set; } = "Nil";
    public int? FetalHeartbeat { get; set; }
    [DataType(DataType.Date)] public DateTime? NextVisitDate { get; set; }
    public string Notes { get; set; }
}

public class Appointment   // booked antenatal follow-up appointment
{
    public int Id { get; set; }
    public int MotherId { get; set; }
    public Mother Mother { get; set; }
    [DataType(DataType.Date)] public DateTime ScheduledDate { get; set; }
    [Required, MaxLength(120)] public string Purpose { get; set; } = "Routine ANC Check-up";
    [MaxLength(20)] public string Status { get; set; } = "Scheduled";   // Scheduled, Attended, Missed
    [MaxLength(300)] public string Notes { get; set; }
}

public class Delivery   // delivery outcome linked to a mother and optional newborn
{
    public int Id { get; set; }
    public int MotherId { get; set; }
    public Mother Mother { get; set; }
    [DataType(DataType.Date)] public DateTime DeliveryDate { get; set; }
    [Required, MaxLength(40)] public string DeliveryType { get; set; } = "Normal";   // Normal, C-Section
    [Required, MaxLength(120)] public string Place { get; set; } = "Phagwara PHC";
    [Required, MaxLength(60)] public string Outcome { get; set; } = "Live Birth";    // Live Birth, NICU Referral, Stillbirth
    [MaxLength(400)] public string Complications { get; set; } = "";
}

public class Newborn
{
    public int Id { get; set; }
    public int MotherId { get; set; }
    public Mother Mother { get; set; }
    public int? DeliveryId { get; set; }
    public Delivery Delivery { get; set; }
    [Required] public string Name { get; set; }
    [DataType(DataType.Date)] public DateTime Dob { get; set; }
    public string Gender { get; set; }
    public double BirthWeight { get; set; }
    [Range(0, 10)] public int? ApgarScore { get; set; }
    [Range(24, 43)] public int? GestationalAgeWeeks { get; set; }
    public List<Vaccination> Vaccines { get; set; } = new();
    public List<GrowthRecord> GrowthRecords { get; set; } = new();

    [NotMapped] public bool IsLowBirthWeight => BirthWeight > 0 && BirthWeight < 2.5;
    [NotMapped] public bool IsPreterm => GestationalAgeWeeks.HasValue && GestationalAgeWeeks.Value < 37;
}

public class Vaccination
{
    public int Id { get; set; }
    public int NewbornId { get; set; }
    public Newborn Newborn { get; set; }
    public string Name { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? GivenDate { get; set; }
    [MaxLength(60)] public string BatchNumber { get; set; } = "";
    [NotMapped] public string Status => GivenDate != null ? "Given" : DueDate.Date < DateTime.Today ? "Overdue" : "Pending";
}

public class GrowthRecord   // weight and height check-up plotted against WHO reference bands
{
    public int Id { get; set; }
    public int NewbornId { get; set; }
    public Newborn Newborn { get; set; }
    [DataType(DataType.Date)] public DateTime RecordedDate { get; set; }
    [Range(0.5, 25)] public double WeightKg { get; set; }
    [Range(30, 120)] public double HeightCm { get; set; }
    [MaxLength(200)] public string Notes { get; set; } = "";
}
