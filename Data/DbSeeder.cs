// DbSeeder.cs - demo users, mothers, visits and a newborn so the dashboard is not empty.
using System.Security.Cryptography;
using System.Text;
using MaatriCare.Models;
using MaatriCare.Services;
namespace MaatriCare.Data;

public static class DbSeeder
{
    public static string Hash(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));

    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;
        db.Users.AddRange(
            new AppUser { Username = "admin", PasswordHash = Hash("Pass@123"), Role = "Admin" },
            new AppUser { Username = "doctor", PasswordHash = Hash("Pass@123"), Role = "Doctor" },
            new AppUser { Username = "worker", PasswordHash = Hash("Pass@123"), Role = "HealthWorker" });

        var today = DateTime.Today;
        var m1 = new Mother { Name = "Simran Kaur", Age = 24, Village = "Phagwara", Phone = "9876500001", BloodGroup = "B+", Lmp = today.AddDays(-120) };
        var m2 = new Mother { Name = "Harpreet Kaur", Age = 37, Village = "Nangal", Phone = "9876500002", BloodGroup = "O+", PreviousCSection = true, Lmp = today.AddDays(-200) };
        var m3 = new Mother { Name = "Pooja Devi", Age = 17, Village = "Hadiabad", Phone = "9876500003", BloodGroup = "A+", Lmp = today.AddDays(-60) };
        var m4 = new Mother { Name = "Anita Rani", Age = 28, Village = "Phagwara", Phone = "9876500004", BloodGroup = "AB+", Lmp = today.AddDays(-300) };
        m1.Visits.Add(new AncVisit { VisitDate = today.AddDays(-10), Systolic = 112, Diastolic = 74, Hb = 12.1, BloodSugar = 90, Notes = "Normal" });
        m2.Visits.Add(new AncVisit { VisitDate = today.AddDays(-5), Systolic = 150, Diastolic = 95, Hb = 9.5, BloodSugar = 150, Notes = "Needs referral" });
        m4.Visits.Add(new AncVisit { VisitDate = today.AddDays(-90), Systolic = 118, Diastolic = 78, Hb = 11.5, BloodSugar = 100 });

        var baby = new Newborn { Name = "Baby of Anita", Dob = today.AddDays(-50), Gender = "Female", BirthWeight = 2.9, Mother = m4 };
        baby.Vaccines = VaccineService.Schedule(baby.Dob);
        baby.Vaccines.Where(v => v.DueDate <= today.AddDays(-40)).ToList().ForEach(v => v.GivenDate = v.DueDate);
        db.Mothers.AddRange(m1, m2, m3, m4);
        db.Newborns.Add(baby);
        db.SaveChanges();
    }
}
