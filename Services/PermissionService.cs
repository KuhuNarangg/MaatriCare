// PermissionService.cs - centralised role and permission rules for all 5 MaatriCare roles.
using System.Security.Claims;
namespace MaatriCare.Services;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";
    public const string Receptionist = "Receptionist";
    public const string Mother = "Mother";
}

public static class PermissionService
{
    public static string GetRole(ClaimsPrincipal user) =>
        user?.FindFirstValue(ClaimTypes.Role) ?? "";

    public static bool IsAdmin(ClaimsPrincipal u) => GetRole(u) == Roles.Admin;
    public static bool IsDoctor(ClaimsPrincipal u) => GetRole(u) == Roles.Doctor;
    public static bool IsNurse(ClaimsPrincipal u) => GetRole(u) == Roles.Nurse;
    public static bool IsReceptionist(ClaimsPrincipal u) => GetRole(u) == Roles.Receptionist;
    public static bool IsMother(ClaimsPrincipal u) => GetRole(u) == Roles.Mother;

    // Staff roles that can browse all patient records
    public static bool CanViewAllPatients(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Admin or Roles.Doctor or Roles.Nurse or Roles.Receptionist;

    // Register a new mother (Admin, Doctor, Nurse, Receptionist)
    public static bool CanRegisterPatient(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Admin or Roles.Doctor or Roles.Nurse or Roles.Receptionist;

    // Edit existing patient profile or clinical data (NOT Receptionist, NOT Mother)
    public static bool CanEditPatientAndClinical(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Admin or Roles.Doctor or Roles.Nurse;

    // Edit vaccines, medications administered, delivery, growth (Admin, Doctor, Nurse)
    public static bool CanEditClinicalCare(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Admin or Roles.Doctor or Roles.Nurse;

    // Write doctor notes, diagnosis, and prescriptions (Admin, Doctor ONLY)
    public static bool CanWriteDoctorPrescription(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Admin or Roles.Doctor;

    // Write nurse notes & administer meds/vaccines in consultation (Admin, Nurse ONLY)
    public static bool CanWriteNurseSection(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Admin or Roles.Nurse;

    // Schedule, reschedule, cancel, check-in appointments (Admin, Receptionist ONLY)
    public static bool CanScheduleAppointments(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Admin or Roles.Receptionist;

    // View Follow-ups & Referrals pages (Admin, Receptionist ONLY — hidden from Doctor & Nurse)
    public static bool CanViewFollowUpsAndReferrals(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Admin or Roles.Receptionist;

    // Create a referral from inside the consultation page (Admin, Doctor, Receptionist)
    public static bool CanCreateReferralFromConsultation(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Admin or Roles.Doctor;

    // View Admin Live Appointment Portal (Admin full, Receptionist read-only)
    public static bool CanViewLivePortal(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Admin or Roles.Receptionist;

    // Delete clinical/patient records (Admin only)
    public static bool CanDeleteRecords(ClaimsPrincipal u) =>
        GetRole(u) == Roles.Admin;

    // Read-only clinical flag (Receptionist & Mother cannot edit clinical data)
    public static bool IsReadOnlyClinical(ClaimsPrincipal u) =>
        GetRole(u) is Roles.Receptionist or Roles.Mother;

    // Role-specific landing page after login
    public static (string Action, string Controller) GetLandingRoute(string role) => role switch
    {
        Roles.Admin => ("Index", "Home"),
        Roles.Doctor => ("Index", "Doctor"),
        Roles.Nurse => ("Index", "Nurse"),
        Roles.Receptionist => ("Index", "Reception"),
        Roles.Mother => ("Index", "Home"),
        _ => ("Index", "Home")
    };
}
