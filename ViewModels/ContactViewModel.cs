// ContactViewModel.cs - form model with friendly validation rules for the Contact page.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class ContactViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100, ErrorMessage = "Name should be under 100 characters.")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address (e.g. name@example.com).")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Please tell us the topic of your message.")]
    [StringLength(150, ErrorMessage = "Subject should be under 150 characters.")]
    public string Subject { get; set; }

    [Required(ErrorMessage = "Please write a short message so we can help you.")]
    [MinLength(10, ErrorMessage = "Please write at least 10 characters.")]
    [StringLength(2000, ErrorMessage = "Message should be under 2000 characters.")]
    public string Message { get; set; }
}
