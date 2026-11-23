// UserFormViewModel.cs - form model for Admin user creation, editing, village assignment, and mother linking.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class UserFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter a username.")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 40 characters.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Please enter the person's full name.")]
    [StringLength(100, ErrorMessage = "Full name should be under 100 characters.")]
    public string FullName { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Please select a role.")]
    public string Role { get; set; } = "Nurse";

    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool IsActive { get; set; } = true;

    public List<string> SelectedVillages { get; set; } = new();
    public int? MotherId { get; set; }
}
