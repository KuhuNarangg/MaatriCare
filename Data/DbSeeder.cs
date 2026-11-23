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

        // Seeded appointments with times, doctor, status flow (Scheduled, Waiting, In Consultation, Attended, Missed)
        var now = DateTime.Now;
        var slotNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, (now.Minute / 10) * 10, 0).ToString("HH:mm");
        var slotNext = now.AddMinutes(25).ToString("HH:mm");
        var slotLater = now.AddMinutes(55).ToString("HH:mm");

        var seededAppts = new List<Appointment>
        {
            new() { MotherId = mothers[2].Id, DoctorUsername = "doctor", DoctorName = "Dr. Manpreet Gill", ScheduledDate = today, StartTime = slotNow, DurationMinutes = 20, Purpose = "High-Risk BP Follow-Up", Status = "Waiting", CreatedBy = "receptionist", CheckedInAt = now.AddMinutes(-5), Notes = "Checked in at reception; priority review" },
            new() { MotherId = mothers[0].Id, DoctorUsername = "doctor", DoctorName = "Dr. Manpreet Gill", ScheduledDate = today, StartTime = slotNext, DurationMinutes = 20, Purpose = "ANC 2 Routine Check-up", Status = "Scheduled", CreatedBy = "receptionist", Notes = "Check Hb & weight gain" },
            new() { MotherId = mothers[9].Id, DoctorUsername = "doctor", DoctorName = "Dr. Manpreet Gill", ScheduledDate = today, StartTime = slotLater, DurationMinutes = 20, Purpose = "Severe Anaemia Follow-Up", Status = "Scheduled", CreatedBy = "receptionist", Notes = "Repeat Hb test" },
            new() { MotherId = mothers[1].Id, DoctorUsername = "doctor", DoctorName = "Dr. Manpreet Gill", ScheduledDate = today.AddDays(2), StartTime = "10:30", DurationMinutes = 20, Purpose = "Pre-delivery Assessment", Status = "Scheduled", CreatedBy = "receptionist", Notes = "Confirm hospital bag & transport" },
            new() { MotherId = mothers[4].Id, DoctorUsername = "doctor", DoctorName = "Dr. Manpreet Gill", ScheduledDate = today.AddDays(-1), StartTime = "11:00", DurationMinutes = 20, Purpose = "Specialist Referral Review", Status = "Missed", CreatedBy = "receptionist", Notes = "Did not arrive; follow-up call needed" },
            new() { MotherId = mothers[12].Id, DoctorUsername = "doctor", DoctorName = "Dr. Manpreet Gill", ScheduledDate = today, StartTime = "09:00", DurationMinutes = 20, Purpose = "ANC 2 Check-up", Status = "Attended", CreatedBy = "receptionist", Notes = "Completed on time" }
        };
        db.Appointments.AddRange(seededAppts);
        db.SaveChanges();

        // Seeded VisitRecord + Prescriptions so Records tab and Print Prescription work out of the box
        var vr1 = new VisitRecord
        {
            MotherId = mothers[12].Id,
            AppointmentId = seededAppts[5].Id,
            VisitDate = today.AddHours(9).AddMinutes(15),
            IsDraft = false,
            Systolic = 116,
            Diastolic = 74,
            WeightKg = 58.0,
            Hb = 11.9,
            BloodSugar = 91,
            TemperatureF = 98.4,
            PulseBpm = 78,
            Symptoms = "Mild fatigue in evenings; good fetal movements.",
            Diagnosis = "Normal 26-week intrauterine pregnancy",
            MedicalNotes = "Fundal height corresponds to gestational age. Continue routine antenatal supplements.",
            TestsAdvised = "Repeat CBC and OGTT at 28 weeks",
            Advice = "Iron-rich diet, 8 hours rest, hydration, report any headache or swelling immediately.",
            NextFollowUpDate = today.AddDays(21),
            DoctorAuthorName = "Dr. Manpreet Gill",
            DoctorAuthorRole = "Doctor",
            DoctorUpdatedAt = today.AddHours(9).AddMinutes(20),
            NurseNotes = "Vitals recorded; counseled on IFA tablet timing after meals.",
            MedicationAdministered = "TT-2 Booster administered (Left Deltoid)",
            NurseAuthorUsername = "nurse",
            NurseAuthorName = "Gurpreet Kaur (Nurse)",
            NurseAuthorRole = "Nurse",
            NurseUpdatedAt = today.AddHours(9).AddMinutes(10)
        };
        vr1.Prescriptions.AddRange(new[]
        {
            new PrescriptionItem { MedicineName = "Ferrous Sulfate + Folic Acid (IFA)", Dose = "100mg / 500mcg", Frequency = "Once daily", Duration = "30 days", Instructions = "After dinner with lemon water", AuthorName = "Dr. Manpreet Gill", AuthorRole = "Doctor" },
            new PrescriptionItem { MedicineName = "Calcium Carbonate + Vitamin D3", Dose = "500mg", Frequency = "Twice daily", Duration = "30 days", Instructions = "After breakfast and lunch (not with IFA)", AuthorName = "Dr. Manpreet Gill", AuthorRole = "Doctor" }
        });
        db.VisitRecords.Add(vr1);

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
            new AppUser { Username = "nurse", FullName = "Gurpreet Kaur (Nurse)", Phone = "9876500102", PasswordHash = Hash("Pass@123"), Role = "Nurse", IsActive = true, AssignedVillages = "Phagwara,Nangal" },
            new AppUser { Username = "receptionist", FullName = "Priya Sharma", Phone = "9876500103", PasswordHash = Hash("Pass@123"), Role = "Receptionist", IsActive = true },
            new AppUser { Username = "mother", FullName = "Anita Rani", Phone = "9876500004", PasswordHash = Hash("Pass@123"), Role = "Mother", IsActive = true, MotherId = mothers[1].Id }
        );

        db.AuditLogs.AddRange(
            new AuditLog { Username = "receptionist", Role = "Receptionist", Action = "Create", EntityType = "Appointment", EntityId = seededAppts[0].Id, Details = "Booked High-Risk BP Follow-Up for Kiranpreet Kaur", Timestamp = now.AddMinutes(-30) },
            new AuditLog { Username = "doctor", Role = "Doctor", Action = "Create", EntityType = "VisitRecord", EntityId = 1, Details = "Submitted consultation & 2-item prescription for Jaswinder Kaur", Timestamp = today.AddHours(9).AddMinutes(20) },
            new AuditLog { Username = "nurse", Role = "Nurse", Action = "Edit", EntityType = "VisitRecord", EntityId = 1, Details = "Logged vitals and TT-2 booster for Jaswinder Kaur", Timestamp = today.AddHours(9).AddMinutes(10) }
        );

        // Seeded hospital referrals for high-risk mothers
        db.Referrals.AddRange(
            new Referral { MotherId = mothers[2].Id, HospitalName = "Civil Hospital Phagwara (OB-GYN)", Reason = "BP 146/92, Urine Protein +2, Previous C-section", Priority = "Urgent", Status = "Pending", ReferredDate = today.AddDays(-2), Notes = "Pre-eclampsia evaluation needed" },
            new Referral { MotherId = mothers[4].Id, HospitalName = "District Hospital Kapurthala", Reason = "BP 150/95, Blood Sugar 150, Age 37", Priority = "Urgent", Status = "Accepted", ReferredDate = today.AddDays(-4), Notes = "Specialist bed confirmed" },
            new Referral { MotherId = mothers[9].Id, HospitalName = "Civil Hospital Phagwara", Reason = "Severe Anaemia (Hb 6.6) & BP 142/90", Priority = "Urgent", Status = "Completed", ReferredDate = today.AddDays(-6), Notes = "IV iron sucrose started" }
        );

        // Seeded trimester-based health education tips
        db.HealthTips.AddRange(
            new HealthTip { Title = "Start Folic Acid & Early Registration", Category = "First Trimester", Trimester = 1, Content = "Take 400 mcg folic acid daily during the first 12 weeks to protect your baby's neural tube development and complete your baseline Hb and blood group check." },
            new HealthTip { Title = "Iron-Rich Foods & IFA Tablets", Category = "Nutrition", Trimester = 2, Content = "Eat spinach, lentils, jaggery, and chana paired with lemon or amla for vitamin C. Take your prescribed IFA tablet after dinner to prevent anaemia." },
            new HealthTip { Title = "Recognize Third-Trimester Danger Signs", Category = "Clinical Safety", Trimester = 3, Content = "Visit the clinic immediately if you experience severe headache, blurred vision, sudden swelling of hands/face, reduced baby movements, or any bleeding." },
            new HealthTip { Title = "Birth Preparedness & Hospital Bag", Category = "Birth Prep", Trimester = 3, Content = "Keep your MaatriCare ANC card, clean clothes for mother and baby, emergency contact numbers, and transport plan ready by week 34." },
            new HealthTip { Title = "Exclusive Breastfeeding & Birth Vaccines", Category = "Newborn Care", Trimester = 0, Content = "Begin breastfeeding within 1 hour of birth and ensure BCG, OPV 0, and Hepatitis B birth doses are recorded on your baby's UIP schedule." }
        );

        // Seeded simulated SMS notifications
        db.Notifications.AddRange(
            new Notification { RecipientName = mothers[2].Name, Phone = mothers[2].Phone, Category = "Upcoming Visit", Title = "SMS Reminder: High-Risk BP Check", Message = $"Namaste {mothers[2].Name}, please visit Phagwara PHC today for your scheduled BP & urine protein check-up.", LinkUrl = $"/Mothers/Details/{mothers[2].Id}", IsRead = false, CreatedAt = DateTime.Now.AddHours(-3) },
            new Notification { RecipientName = mothers[5].Name, Phone = mothers[5].Phone, Category = "Overdue Visit", Title = "SMS Reminder: Overdue ANC Check-up", Message = $"Namaste {mothers[5].Name}, your antenatal visit is overdue. Please contact your ANM worker in Nangal.", LinkUrl = $"/Mothers/Details/{mothers[5].Id}", IsRead = false, CreatedAt = DateTime.Now.AddHours(-2) },
            new Notification { RecipientName = mothers[10].Name, Phone = mothers[10].Phone, Category = "Due Vaccine", Title = "SMS Reminder: Pentavalent 1 Overdue", Message = $"Namaste {mothers[10].Name}, Baby of Balwinder is due for Pentavalent 1 + Rota 1 + IPV 1 at Hadiabad clinic.", LinkUrl = $"/Newborns/Details/{b4.Id}", IsRead = false, CreatedAt = DateTime.Now.AddHours(-1) }
        );

        db.SaveChanges();
    }
}
