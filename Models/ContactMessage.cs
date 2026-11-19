// ContactMessage.cs - stores messages submitted through the public Contact page.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.Models;

public class ContactMessage
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string FullName { get; set; }
    [Required, EmailAddress, MaxLength(150)] public string Email { get; set; }
    [MaxLength(20)] public string Phone { get; set; }
    [Required, MaxLength(150)] public string Subject { get; set; }
    [Required, MaxLength(2000)] public string Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
