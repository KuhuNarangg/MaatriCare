// ProfileViewModel.cs - form models for editing profile info and changing password.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class ProfileViewModel
{
    public string Username { get; set; }
    public string Role { get; set; }

    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100, ErrorMessage = "Name should be under 100 characters.")]
    public string FullName { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string Phone { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Please enter your current password.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "Please choose a new password.")]
    [MinLength(6, ErrorMessage = "New password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Please confirm your new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "The two new passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
}
