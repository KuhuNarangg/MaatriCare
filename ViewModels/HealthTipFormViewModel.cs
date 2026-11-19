// HealthTipFormViewModel.cs - form model for Admin health education CRUD.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class HealthTipFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter a title for the health tip.")]
    [StringLength(140)]
    public string Title { get; set; }

    [Required(ErrorMessage = "Please enter a category (e.g. Nutrition, Clinical Safety).")]
    [StringLength(50)]
    public string Category { get; set; } = "Nutrition";

    [Range(0, 3)]
    public int Trimester { get; set; } = 1;

    [Required(ErrorMessage = "Please write the guidance text.")]
    [StringLength(1000)]
    public string Content { get; set; }

    public bool IsPublished { get; set; } = true;
}
