// DbSeeder.cs - seeds 4 role accounts, 15 mothers across 4 Punjab villages, ANC visits, newborns and vaccines.
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

        var today = DateTime.Today;

        var mothers = new List<Mother>
        {
            // Phagwara (4 mothers)
            new() { Name = "Simran Kaur", Age = 24, Village = "Phagwara", Phone = "9876500001", BloodGroup = "B+", Lmp = today.AddDays(-120), RegisteredAt = today.AddDays(-100) },
            new() { Name = "Anita Rani", Age = 28, Village = "Phagwara", Phone = "9876500004", BloodGroup = "AB+", Lmp = today.AddDays(-265), RegisteredAt = today.AddDays(-155) },
            new() { Name = "Kiranpreet Kaur", Age = 36, Village = "Phagwara", Phone = "9876500005", BloodGroup = "O+", PreviousCSection = true, PreviousComplications = 2, Lmp = today.AddDays(-258), RegisteredAt = today.AddDays(-130) },
            new() { Name = "Meena Kumari", Age = 22, Village = "Phagwara", Phone = "9876500006", BloodGroup = "A+", Lmp = today.AddDays(-75), RegisteredAt = today.AddDays(-18) },

            // Nangal (4 mothers)
            new() { Name = "Harpreet Kaur", Age = 37, Village = "Nangal", Phone = "9876500002", BloodGroup = "O+", PreviousCSection = true, Lmp = today.AddDays(-200), RegisteredAt = today.AddDays(-145) },
            new() { Name = "Navjot Kaur", Age = 25, Village = "Nangal", Phone = "9876500007", BloodGroup = "B+", Lmp = today.AddDays(-262), RegisteredAt = today.AddDays(-110) },
            new() { Name = "Sukhdeep Kaur", Age = 19, Village = "Nangal", Phone = "9876500008", BloodGroup = "O-", Lmp = today.AddDays(-140), RegisteredAt = today.AddDays(-65) },
            new() { Name = "Ritu Sharma", Age = 31, Village = "Nangal", Phone = "9876500009", BloodGroup = "A+", Lmp = today.AddDays(-50), RegisteredAt = today.AddDays(-12) },

            // Hadiabad (4 mothers)
            new() { Name = "Pooja Devi", Age = 17, Village = "Hadiabad", Phone = "9876500003", BloodGroup = "A+", Lmp = today.AddDays(-60), RegisteredAt = today.AddDays(-40) },
            new() { Name = "Manjit Kaur", Age = 34, Village = "Hadiabad", Phone = "9876500010", BloodGroup = "B+", PreviousCSection = true, Lmp = today.AddDays(-220), RegisteredAt = today.AddDays(-95) },
            new() { Name = "Balwinder Kaur", Age = 27, Village = "Hadiabad", Phone = "9876500011", BloodGroup = "O+", Lmp = today.AddDays(-268), RegisteredAt = today.AddDays(-78) },
            new() { Name = "Sunita Rani", Age = 29, Village = "Hadiabad", Phone = "9876500012", BloodGroup = "AB-", Lmp = today.AddDays(-95), RegisteredAt = today.AddDays(-25) },

            // Goraya (3 mothers)
            new() { Name = "Jaswinder Kaur", Age = 26, Village = "Goraya", Phone = "9876500013", BloodGroup = "O+", Lmp = today.AddDays(-185), RegisteredAt = today.AddDays(-85) },
            new() { Name = "Paramjit Kaur", Age = 38, Village = "Goraya", Phone = "9876500014", BloodGroup = "A-", Lmp = today.AddDays(-150), RegisteredAt = today.AddDays(-52) },
            new() { Name = "Amandeep Kaur", Age = 23, Village = "Goraya", Phone = "9876500015", BloodGroup = "B+", Lmp = today.AddDays(-42), RegisteredAt = today.AddDays(-6) }
        };

        // ANC visits across mothers (mix of normal, mild/severe anaemia, high BP, urine protein, and overdue)
        mothers[0].Visits.Add(new AncVisit { VisitDate = today.AddDays(-65), Systolic = 110, Diastolic = 72, Hb = 11.9, BloodSugar = 88, WeightKg = 54.5, UrineProtein = "Nil", FetalHeartbeat = 142, Notes = "ANC 1 registration check" });
        mothers[0].Visits.Add(new AncVisit { VisitDate = today.AddDays(-10), Systolic = 112, Diastolic = 74, Hb = 12.1, BloodSugar = 90, WeightKg = 57.2, UrineProtein = "Nil", FetalHeartbeat = 144, NextVisitDate = today.AddDays(20), Notes = "Normal vitals" });
        mothers[1].Visits.Add(new AncVisit { VisitDate = today.AddDays(-190), Systolic = 116, Diastolic = 76, Hb = 11.4, BloodSugar = 94, WeightKg = 56.0, UrineProtein = "Nil", Notes = "ANC 1 normal" });
        mothers[1].Visits.Add(new AncVisit { VisitDate = today.AddDays(-95), Systolic = 118, Diastolic = 78, Hb = 11.6, BloodSugar = 96, WeightKg = 60.5, UrineProtein = "Nil", FetalHeartbeat = 140, Notes = "ANC 2 normal" });
        mothers[1].Visits.Add(new AncVisit { VisitDate = today.AddDays(-8), Systolic = 118, Diastolic = 78, Hb = 11.5, BloodSugar = 98, WeightKg = 64.0, UrineProtein = "Nil", FetalHeartbeat = 138, NextVisitDate = today.AddDays(7), Notes = "Ready for delivery" });
        mothers[2].Visits.Add(new AncVisit { VisitDate = today.AddDays(-4), Systolic = 146, Diastolic = 92, Hb = 8.4, BloodSugar = 148, WeightKg = 68.0, UrineProtein = "+2", FetalHeartbeat = 148, NextVisitDate = today.AddDays(5), Notes = "High BP + proteinuria + C-section history; refer" });
        mothers[3].Visits.Add(new AncVisit { VisitDate = today.AddDays(-14), Systolic = 116, Diastolic = 76, Hb = 11.8, BloodSugar = 88, WeightKg = 52.0, UrineProtein = "Nil", NextVisitDate = today.AddDays(30), Notes = "Healthy first check" });

        mothers[4].Visits.Add(new AncVisit { VisitDate = today.AddDays(-5), Systolic = 150, Diastolic = 95, Hb = 9.5, BloodSugar = 150, WeightKg = 71.0, UrineProtein = "+1", FetalHeartbeat = 152, NextVisitDate = today.AddDays(7), Notes = "Needs specialist referral" });
        mothers[5].Visits.Add(new AncVisit { VisitDate = today.AddDays(-52), Systolic = 122, Diastolic = 80, Hb = 10.4, BloodSugar = 104, WeightKg = 59.0, UrineProtein = "Nil", FetalHeartbeat = 140, Notes = "Overdue for next ANC" });
        mothers[6].Visits.Add(new AncVisit { VisitDate = today.AddDays(-55), Systolic = 114, Diastolic = 72, Hb = 10.8, BloodSugar = 92, WeightKg = 41.0, UrineProtein = "Nil", FetalHeartbeat = 145, Notes = "Low weight; needs nutrition & IFA follow-up" });
        mothers[7].Visits.Add(new AncVisit { VisitDate = today.AddDays(-7), Systolic = 120, Diastolic = 78, Hb = 12.0, BloodSugar = 96, WeightKg = 55.5, UrineProtein = "Nil", NextVisitDate = today.AddDays(28), Notes = "Normal" });

        // mothers[8] (Pooja Devi, age 17) has no ANC visit yet -> Medium/overdue follow-up
        mothers[9].Visits.Add(new AncVisit { VisitDate = today.AddDays(-6), Systolic = 142, Diastolic = 90, Hb = 6.6, BloodSugar = 118, WeightKg = 50.0, UrineProtein = "+2", FetalHeartbeat = 166, NextVisitDate = today.AddDays(4), Notes = "Severe anaemia & BP" });
        mothers[10].Visits.Add(new AncVisit { VisitDate = today.AddDays(-12), Systolic = 124, Diastolic = 82, Hb = 11.2, BloodSugar = 102, WeightKg = 62.0, UrineProtein = "Nil", FetalHeartbeat = 136, NextVisitDate = today.AddDays(10), Notes = "EDD approaching" });
        mothers[11].Visits.Add(new AncVisit { VisitDate = today.AddDays(-50), Systolic = 118, Diastolic = 76, Hb = 10.5, BloodSugar = 95, WeightKg = 54.0, UrineProtein = "Trace", Notes = "Mild anaemia, visit overdue" });

        mothers[12].Visits.Add(new AncVisit { VisitDate = today.AddDays(-9), Systolic = 116, Diastolic = 74, Hb = 11.9, BloodSugar = 91, WeightKg = 58.0, UrineProtein = "Nil", FetalHeartbeat = 142, NextVisitDate = today.AddDays(21), Notes = "Normal growth" });
        mothers[13].Visits.Add(new AncVisit { VisitDate = today.AddDays(-3), Systolic = 144, Diastolic = 92, Hb = 6.8, BloodSugar = 145, WeightKg = 63.0, UrineProtein = "+1", FetalHeartbeat = 150, NextVisitDate = today.AddDays(5), Notes = "Urgent referral advised" });
        mothers[14].Visits.Add(new AncVisit { VisitDate = today.AddDays(-5), Systolic = 110, Diastolic = 70, Hb = 12.4, BloodSugar = 86, WeightKg = 53.0, UrineProtein = "Nil", NextVisitDate = today.AddDays(30), Notes = "Early registration" });

        db.Mothers.AddRange(mothers);
        db.SaveChanges();

        // Seeded appointments (Scheduled, Attended, Missed)
        db.Appointments.AddRange(
            new Appointment { MotherId = mothers[0].Id, ScheduledDate = today.AddDays(3), Purpose = "ANC 2 Routine Check-up", Status = "Scheduled", Notes = "Check Hb & weight gain" },
            new Appointment { MotherId = mothers[1].Id, ScheduledDate = today.AddDays(2), Purpose = "Pre-delivery Assessment", Status = "Scheduled", Notes = "Confirm hospital bag & transport" },
            new Appointment { MotherId = mothers[2].Id, ScheduledDate = today, Purpose = "High-Risk BP Follow-Up", Status = "Scheduled", Notes = "Doctor review required" },
            new Appointment { MotherId = mothers[4].Id, ScheduledDate = today.AddDays(-2), Purpose = "Specialist Referral Review", Status = "Missed", Notes = "Did not arrive; call family" },
            new Appointment { MotherId = mothers[9].Id, ScheduledDate = today.AddDays(1), Purpose = "Severe Anaemia Follow-Up", Status = "Scheduled", Notes = "Repeat Hb test" },
            new Appointment { MotherId = mothers[12].Id, ScheduledDate = today.AddDays(-9), Purpose = "ANC 2 Check-up", Status = "Attended", Notes = "Completed on time" }
        );

        // Seeded deliveries linked to mothers & newborns
        var d1 = new Delivery { MotherId = mothers[1].Id, DeliveryDate = today.AddDays(-50), DeliveryType = "Normal", Place = "Phagwara PHC", Outcome = "Live Birth", Complications = "None" };
        var d2 = new Delivery { MotherId = mothers[0].Id, DeliveryDate = today.AddDays(-105), DeliveryType = "Normal", Place = "Civil Hospital Phagwara", Outcome = "Live Birth", Complications = "None" };
        var d3 = new Delivery { MotherId = mothers[5].Id, DeliveryDate = today.AddDays(-75), DeliveryType = "C-Section", Place = "District Hospital Kapurthala", Outcome = "Live Birth", Complications = "Preterm labour (35 wks)" };
        var d4 = new Delivery { MotherId = mothers[10].Id, DeliveryDate = today.AddDays(-48), DeliveryType = "Normal", Place = "Hadiabad Sub-Centre", Outcome = "Live Birth", Complications = "None" };
        var d5 = new Delivery { MotherId = mothers[12].Id, DeliveryDate = today.AddDays(-102), DeliveryType = "Normal", Place = "Goraya CHC", Outcome = "Live Birth", Complications = "None" };
        db.Deliveries.AddRange(d1, d2, d3, d4, d5);
        db.SaveChanges();

        // Newborns across all 4 villages with APGAR, gestational age, delivery link, vaccines, and growth records
        var b1 = new Newborn { Name = "Baby of Anita", Dob = today.AddDays(-50), Gender = "Female", BirthWeight = 2.9, ApgarScore = 9, GestationalAgeWeeks = 39, MotherId = mothers[1].Id, DeliveryId = d1.Id };
        b1.Vaccines = VaccineService.Schedule(b1.Dob);
        foreach (var v in b1.Vaccines.Take(4)) { v.GivenDate = v.DueDate; v.BatchNumber = "UIP-26A1"; }
        b1.GrowthRecords.AddRange(new[]
        {
            new GrowthRecord { RecordedDate = b1.Dob, WeightKg = 2.9, HeightCm = 48.5, Notes = "Birth measurement" },
            new GrowthRecord { RecordedDate = b1.Dob.AddDays(30), WeightKg = 3.9, HeightCm = 52.8, Notes = "1-month check" },
            new GrowthRecord { RecordedDate = b1.Dob.AddDays(48), WeightKg = 4.5, HeightCm = 55.2, Notes = "Healthy weight gain" }
        });

        var b2 = new Newborn { Name = "Baby of Simran", Dob = today.AddDays(-105), Gender = "Male", BirthWeight = 3.2, ApgarScore = 9, GestationalAgeWeeks = 40, MotherId = mothers[0].Id, DeliveryId = d2.Id };
        b2.Vaccines = VaccineService.Schedule(b2.Dob);
        foreach (var v in b2.Vaccines.Take(6)) { v.GivenDate = v.DueDate; v.BatchNumber = "UIP-26B2"; }
        b2.GrowthRecords.AddRange(new[]
        {
            new GrowthRecord { RecordedDate = b2.Dob, WeightKg = 3.2, HeightCm = 50.0, Notes = "Birth measurement" },
            new GrowthRecord { RecordedDate = b2.Dob.AddDays(30), WeightKg = 4.3, HeightCm = 54.0, Notes = "1-month check" },
            new GrowthRecord { RecordedDate = b2.Dob.AddDays(60), WeightKg = 5.3, HeightCm = 57.5, Notes = "2-month check" },
            new GrowthRecord { RecordedDate = b2.Dob.AddDays(95), WeightKg = 6.2, HeightCm = 61.0, Notes = "3-month check" }
        });

        var b3 = new Newborn { Name = "Baby of Navjot", Dob = today.AddDays(-75), Gender = "Female", BirthWeight = 2.2, ApgarScore = 7, GestationalAgeWeeks = 35, MotherId = mothers[5].Id, DeliveryId = d3.Id };
        b3.Vaccines = VaccineService.Schedule(b3.Dob);
        foreach (var v in b3.Vaccines.Take(3)) { v.GivenDate = v.DueDate; v.BatchNumber = "UIP-26C3"; }
        b3.GrowthRecords.AddRange(new[]
        {
            new GrowthRecord { RecordedDate = b3.Dob, WeightKg = 2.2, HeightCm = 46.0, Notes = "Low birth weight & preterm; KMC advised" },
            new GrowthRecord { RecordedDate = b3.Dob.AddDays(30), WeightKg = 3.0, HeightCm = 49.5, Notes = "Steady catch-up" },
            new GrowthRecord { RecordedDate = b3.Dob.AddDays(65), WeightKg = 4.1, HeightCm = 53.5, Notes = "Improving well" }
        });

        var b4 = new Newborn { Name = "Baby of Balwinder", Dob = today.AddDays(-48), Gender = "Male", BirthWeight = 3.1, ApgarScore = 8, GestationalAgeWeeks = 38, MotherId = mothers[10].Id, DeliveryId = d4.Id };
        b4.Vaccines = VaccineService.Schedule(b4.Dob);
        foreach (var v in b4.Vaccines.Take(3)) { v.GivenDate = v.DueDate; v.BatchNumber = "UIP-26D4"; }
        b4.GrowthRecords.Add(new GrowthRecord { RecordedDate = b4.Dob, WeightKg = 3.1, HeightCm = 49.5, Notes = "Birth measurement" });

        var b5 = new Newborn { Name = "Baby of Jaswinder", Dob = today.AddDays(-102), Gender = "Female", BirthWeight = 3.0, ApgarScore = 9, GestationalAgeWeeks = 39, MotherId = mothers[12].Id, DeliveryId = d5.Id };
        b5.Vaccines = VaccineService.Schedule(b5.Dob);
        foreach (var v in b5.Vaccines.Take(5)) { v.GivenDate = v.DueDate; v.BatchNumber = "UIP-26E5"; }
        b5.GrowthRecords.AddRange(new[]
        {
            new GrowthRecord { RecordedDate = b5.Dob, WeightKg = 3.0, HeightCm = 49.0, Notes = "Birth measurement" },
            new GrowthRecord { RecordedDate = b5.Dob.AddDays(45), WeightKg = 4.4, HeightCm = 54.5, Notes = "6-week visit" },
            new GrowthRecord { RecordedDate = b5.Dob.AddDays(95), WeightKg = 5.7, HeightCm = 59.5, Notes = "3-month check" }
        });

        db.Newborns.AddRange(b1, b2, b3, b4, b5);

        db.Users.AddRange(
            new AppUser { Username = "admin", FullName = "Dr. Rajeshwar Singh", Phone = "9876500100", PasswordHash = Hash("Pass@123"), Role = "Admin", IsActive = true },
            new AppUser { Username = "doctor", FullName = "Dr. Manpreet Gill", Phone = "9876500101", PasswordHash = Hash("Pass@123"), Role = "Doctor", IsActive = true },
            new AppUser { Username = "worker", FullName = "Gurpreet Kaur (ANM)", Phone = "9876500102", PasswordHash = Hash("Pass@123"), Role = "HealthWorker", IsActive = true, AssignedVillages = "Phagwara,Nangal" },
            new AppUser { Username = "mother", FullName = "Anita Rani", Phone = "9876500004", PasswordHash = Hash("Pass@123"), Role = "Mother", IsActive = true, MotherId = mothers[1].Id }
        );

        db.SaveChanges();
    }
}
