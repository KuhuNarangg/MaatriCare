// Models.cs - all database entities (one class = one table).
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MaatriCare.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }   // Admin, Doctor, HealthWorker
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
    [DataType(DataType.Date)] public DateTime Lmp { get; set; }   // last menstrual period
    public List<AncVisit> Visits { get; set; } = new();
    public List<Newborn> Newborns { get; set; } = new();

    [NotMapped] public DateTime Edd => Lmp.AddDays(280);                       // expected delivery date
    [NotMapped] public int Week => Math.Clamp((int)((DateTime.Today - Lmp).TotalDays / 7), 0, 40);
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
    public string Notes { get; set; }
}

public class Newborn
{
    public int Id { get; set; }
    public int MotherId { get; set; }
    public Mother Mother { get; set; }
    [Required] public string Name { get; set; }
    [DataType(DataType.Date)] public DateTime Dob { get; set; }
    public string Gender { get; set; }
    public double BirthWeight { get; set; }
    public List<Vaccination> Vaccines { get; set; } = new();
}

public class Vaccination
{
    public int Id { get; set; }
    public int NewbornId { get; set; }
    public string Name { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? GivenDate { get; set; }
    [NotMapped] public string Status => GivenDate != null ? "Given" : DueDate.Date < DateTime.Today ? "Overdue" : "Pending";
}
