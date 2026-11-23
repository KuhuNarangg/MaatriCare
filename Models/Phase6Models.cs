// Phase6Models.cs - database entities for simulated SMS Notifications, hospital Referrals, and HealthTips.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.Models;

public class Notification
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string RecipientName { get; set; }
    [MaxLength(20)] public string Phone { get; set; } = "";
    [Required, MaxLength(40)] public string Category { get; set; }   // Upcoming Visit, Overdue Visit, Due Vaccine
    [Required, MaxLength(140)] public string Title { get; set; }
    [Required, MaxLength(500)] public string Message { get; set; }
    [MaxLength(30)] public string Channel { get; set; } = "Simulated SMS";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [MaxLength(160)] public string LinkUrl { get; set; } = "/Home";
}

public class Referral
{
    public int Id { get; set; }
    public int MotherId { get; set; }
    public Mother Mother { get; set; }
    [Required, MaxLength(140)] public string HospitalName { get; set; }
    [Required, MaxLength(300)] public string Reason { get; set; }
    [MaxLength(20)] public string Priority { get; set; } = "Urgent";   // Urgent, Routine
    [MaxLength(20)] public string Status { get; set; } = "Pending";    // Pending, Accepted, Completed
    [DataType(DataType.Date)] public DateTime ReferredDate { get; set; } = DateTime.Today;
    [MaxLength(100)] public string CreatedBy { get; set; } = "Dr. Manpreet Gill (Doctor)";
    [MaxLength(400)] public string Notes { get; set; } = "";
}

public class HealthTip
{
    public int Id { get; set; }
    [Required, MaxLength(140)] public string Title { get; set; }
    [Required, MaxLength(50)] public string Category { get; set; } = "Nutrition";
    [Range(0, 3)] public int Trimester { get; set; } = 1;   // 0 = General/Newborn, 1 = T1, 2 = T2, 3 = T3
    [Required, MaxLength(1000)] public string Content { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Today;
}
