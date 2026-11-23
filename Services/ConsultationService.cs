// ConsultationService.cs - manages the unified Doctor/Nurse Consultation Portal, VisitRecords, and Prescriptions.
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class ConsultationService
{
    private readonly AppDbContext db;
    private readonly AuditService audit;

    public ConsultationService(AppDbContext db, AuditService audit)
    {
        this.db = db;
        this.audit = audit;
    }

    public async Task<ConsultationViewModel> OpenConsultationAsync(int appointmentId, ClaimsPrincipal user)
    {
        var appt = await db.Appointments
            .Include(a => a.Mother).ThenInclude(m => m.Visits)
            .Include(a => a.Mother).ThenInclude(m => m.Appointments)
            .Include(a => a.Mother).ThenInclude(m => m.Newborns).ThenInclude(n => n.Vaccines)
            .Include(a => a.Newborn).ThenInclude(n => n.Vaccines)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appt == null) return null;

        var role = PermissionService.GetRole(user);
        var username = user.Identity?.Name ?? "";

        // Opening consultation as Doctor transitions Scheduled/Waiting -> In Consultation
        if (role == Roles.Doctor && (appt.Status == "Scheduled" || appt.Status == "Waiting"))
        {
            appt.Status = "In Consultation";
            appt.ConsultationStartedAt ??= DateTime.Now;
            await db.SaveChangesAsync();
            await audit.LogAsync(user, "StatusChange", "Appointment", appt.Id, $"Doctor opened consultation for {appt.Mother?.Name}; status set to In Consultation");
        }

        // Load or initialize VisitRecord linked to this appointment
        var record = await db.VisitRecords
            .Include(v => v.Prescriptions)
            .FirstOrDefaultAsync(v => v.AppointmentId == appt.Id);

        if (record == null)
        {
            record = new VisitRecord
            {
                MotherId = appt.MotherId,
                NewbornId = appt.NewbornId,
                AppointmentId = appt.Id,
                VisitDate = DateTime.Now,
                IsDraft = true
            };
            if (!PermissionService.IsReadOnlyClinical(user))
            {
                db.VisitRecords.Add(record);
                await db.SaveChangesAsync();
            }
        }

        var prevVisits = await db.VisitRecords
            .Include(v => v.Prescriptions)
            .Where(v => v.MotherId == appt.MotherId && v.Id != record.Id)
            .OrderByDescending(v => v.VisitDate)
            .Take(10)
            .ToListAsync();

        var eval = RiskService.Evaluate(appt.Mother);
        var babyVaccines = appt.Mother.Newborns
            .SelectMany(n => n.Vaccines)
            .Where(v => v.GivenDate == null)
            .OrderBy(v => v.DueDate)
            .ToList();

        bool nurseOwnsNote = string.IsNullOrWhiteSpace(record.NurseAuthorUsername) ||
                             record.NurseAuthorUsername.Equals(username, StringComparison.OrdinalIgnoreCase);

        return new ConsultationViewModel
        {
            Appointment = appt,
            Mother = appt.Mother,
            RiskLevel = eval.Level,
            RiskScore = eval.Score,
            RiskReasons = eval.Reasons,
            RecommendedAction = eval.RecommendedAction,
            Record = record,
            PreviousVisits = prevVisits,
            BabyPendingVaccines = babyVaccines,
            IsReadOnly = PermissionService.IsReadOnlyClinical(user),
            CanEditVitals = PermissionService.CanEditClinicalCare(user),
            CanEditDoctorSection = PermissionService.CanWriteDoctorPrescription(user),
            CanEditNurseSection = role == Roles.Admin || (role == Roles.Nurse && nurseOwnsNote),
            CanCreateReferral = PermissionService.CanCreateReferralFromConsultation(user)
        };
    }

    public async Task<(bool Ok, bool Forbidden, string Message, int? VisitRecordId)> SaveConsultationAsync(ConsultationPostModel post, ClaimsPrincipal user)
    {
        var role = PermissionService.GetRole(user);
        var username = user.Identity?.Name ?? "";
        var fullName = user.FindFirstValue("FullName") ?? username;

        // Server-side permission enforcement
        if (PermissionService.IsReadOnlyClinical(user))
            return (false, true, "Read-only role cannot modify consultations.", null);

        bool isDoctorAction = post.SubmitMode is "draft" or "submit" or "submit_next";
        if (isDoctorAction && !PermissionService.CanWriteDoctorPrescription(user))
            return (false, true, "Only Doctors and Admins can write prescriptions or submit consultations.", null);

        if (post.SubmitMode == "nurse_save" && !PermissionService.CanWriteNurseSection(user))
            return (false, true, "Only Nurses and Admins can save nurse notes.", null);

        var appt = await db.Appointments.Include(a => a.Mother).FirstOrDefaultAsync(a => a.Id == post.AppointmentId);
        if (appt == null) return (false, false, "Appointment not found.", null);

        var record = await db.VisitRecords
            .Include(v => v.Prescriptions)
            .FirstOrDefaultAsync(v => v.Id == post.VisitRecordId && v.AppointmentId == appt.Id);

        if (record == null)
        {
            record = new VisitRecord
            {
                MotherId = appt.MotherId,
                NewbornId = appt.NewbornId,
                AppointmentId = appt.Id,
                VisitDate = DateTime.Now,
                IsDraft = true
            };
            db.VisitRecords.Add(record);
        }

        // Vitals (Admin, Doctor, Nurse)
        record.Systolic = post.Systolic;
        record.Diastolic = post.Diastolic;
        record.WeightKg = post.WeightKg;
        record.Hb = post.Hb;
        record.BloodSugar = post.BloodSugar;
        record.TemperatureF = post.TemperatureF;
        record.PulseBpm = post.PulseBpm;

        // Nurse section
        if (post.SubmitMode == "nurse_save" || role == Roles.Admin)
        {
            if (role == Roles.Nurse && !string.IsNullOrWhiteSpace(record.NurseAuthorUsername) &&
                !record.NurseAuthorUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                return (false, true, "You can only edit your own nurse notes.", null);
            }

            if (post.NurseNotes != null || post.MedicationAdministered != null || post.VaccineIdGiven.HasValue || !string.IsNullOrWhiteSpace(post.VaccineNameGiven))
            {
                record.NurseNotes = post.NurseNotes?.Trim() ?? record.NurseNotes;
                record.MedicationAdministered = post.MedicationAdministered?.Trim() ?? record.MedicationAdministered;
                record.NurseAuthorUsername = username;
                record.NurseAuthorName = fullName;
                record.NurseAuthorRole = role;
                record.NurseUpdatedAt = DateTime.Now;
            }

            // Record vaccine given today if selected or entered
            if (post.VaccineIdGiven.HasValue)
            {
                var vax = await db.Vaccinations.FindAsync(post.VaccineIdGiven.Value);
                if (vax != null)
                {
                    vax.GivenDate = post.VaccineDateGiven ?? DateTime.Today;
                    vax.BatchNumber = post.VaccineBatchGiven?.Trim() ?? "";
                    var entry = $"{vax.Name} ({vax.GivenDate:dd MMM yyyy}, Batch {vax.BatchNumber})";
                    record.VaccinesGivenSummary = string.IsNullOrWhiteSpace(record.VaccinesGivenSummary)
                        ? entry
                        : record.VaccinesGivenSummary + "; " + entry;
                    await audit.LogAsync(user, "Edit", "Vaccine", vax.Id, $"Nurse marked {vax.Name} given (Batch {vax.BatchNumber})");
                }
            }
            else if (!string.IsNullOrWhiteSpace(post.VaccineNameGiven))
            {
                var d = post.VaccineDateGiven ?? DateTime.Today;
                var entry = $"{post.VaccineNameGiven.Trim()} ({d:dd MMM yyyy}, Batch {post.VaccineBatchGiven?.Trim()})";
                record.VaccinesGivenSummary = string.IsNullOrWhiteSpace(record.VaccinesGivenSummary)
                    ? entry
                    : record.VaccinesGivenSummary + "; " + entry;
            }
        }

        // Doctor section & Prescriptions
        if (PermissionService.CanWriteDoctorPrescription(user) && isDoctorAction)
        {
            record.Symptoms = post.Symptoms?.Trim() ?? "";
            record.Diagnosis = post.Diagnosis?.Trim() ?? "";
            record.MedicalNotes = post.MedicalNotes?.Trim() ?? "";
            record.TestsAdvised = post.TestsAdvised?.Trim() ?? "";
            record.Advice = post.Advice?.Trim() ?? "";
            record.NextFollowUpDate = post.NextFollowUpDate;
            record.DoctorAuthorName = fullName;
            record.DoctorAuthorRole = role;
            record.DoctorUpdatedAt = DateTime.Now;

            // Replace prescriptions cleanly
            db.PrescriptionItems.RemoveRange(record.Prescriptions);
            record.Prescriptions.Clear();
            int count = post.RxMedicine?.Count ?? 0;
            for (int i = 0; i < count; i++)
            {
                var med = post.RxMedicine[i]?.Trim();
                if (string.IsNullOrWhiteSpace(med)) continue;
                record.Prescriptions.Add(new PrescriptionItem
                {
                    MedicineName = med,
                    Dose = i < post.RxDose.Count ? post.RxDose[i]?.Trim() ?? "" : "",
                    Frequency = i < post.RxFrequency.Count ? post.RxFrequency[i]?.Trim() ?? "" : "",
                    Duration = i < post.RxDuration.Count ? post.RxDuration[i]?.Trim() ?? "" : "",
                    Instructions = i < post.RxInstructions.Count ? post.RxInstructions[i]?.Trim() ?? "" : "",
                    AuthorName = fullName,
                    AuthorRole = role,
                    CreatedAt = DateTime.Now
                });
            }

            // Optional referral from consultation
            if (!string.IsNullOrWhiteSpace(post.ReferralHospital) && !string.IsNullOrWhiteSpace(post.ReferralReason))
            {
                var referral = new Referral
                {
                    MotherId = appt.MotherId,
                    HospitalName = post.ReferralHospital.Trim(),
                    Reason = post.ReferralReason.Trim(),
                    Priority = "Urgent",
                    Status = "Pending",
                    ReferredDate = DateTime.Today,
                    CreatedBy = $"{fullName} ({role})",
                    Notes = $"Created during consultation #{appt.Id}"
                };
                db.Referrals.Add(referral);
                await audit.LogAsync(user, "Create", "Referral", null, $"Referred {appt.Mother?.Name} to {referral.HospitalName}: {referral.Reason}");
            }

            // Final submission marks appointment Attended & syncs ANC visit vitals
            if (post.SubmitMode is "submit" or "submit_next")
            {
                record.IsDraft = false;
                record.VisitDate = DateTime.Now;
                appt.Status = "Attended";

                if (post.Systolic.HasValue || post.Hb.HasValue || post.BloodSugar.HasValue)
                {
                    db.AncVisits.Add(new AncVisit
                    {
                        MotherId = appt.MotherId,
                        VisitDate = DateTime.Today,
                        Systolic = post.Systolic ?? 120,
                        Diastolic = post.Diastolic ?? 80,
                        Hb = post.Hb ?? 11.5,
                        BloodSugar = post.BloodSugar ?? 95,
                        WeightKg = post.WeightKg,
                        NextVisitDate = post.NextFollowUpDate,
                        Notes = string.IsNullOrWhiteSpace(post.Diagnosis) ? "Consultation completed" : post.Diagnosis.Trim()
                    });
                }
            }
        }

        await db.SaveChangesAsync();
        await audit.LogAsync(user, post.SubmitMode == "draft" ? "Edit" : "Create", "VisitRecord", record.Id,
            $"{role} saved consultation ({post.SubmitMode}) for {appt.Mother?.Name} with {record.Prescriptions.Count} medication(s)");

        return (true, false, post.SubmitMode == "draft" ? "Draft saved." :
            post.SubmitMode == "nurse_save" ? "Nurse vitals & notes saved." : "Consultation submitted and appointment marked Attended.", record.Id);
    }

    public async Task<VisitRecord> GetVisitRecordByIdAsync(int id) =>
        await db.VisitRecords
            .Include(v => v.Mother)
            .Include(v => v.Newborn)
            .Include(v => v.Appointment)
            .Include(v => v.Prescriptions)
            .FirstOrDefaultAsync(v => v.Id == id);
}
