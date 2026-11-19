// DeliveryFormViewModel.cs - form model for recording a mother's delivery outcome.
using System.ComponentModel.DataAnnotations;
namespace MaatriCare.ViewModels;

public class DeliveryFormViewModel
{
    public int MotherId { get; set; }
    public string MotherName { get; set; }

    [Required(ErrorMessage = "Please enter the delivery date.")]
    [DataType(DataType.Date)]
    public DateTime DeliveryDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Please choose the delivery type.")]
    public string DeliveryType { get; set; } = "Normal";

    [Required(ErrorMessage = "Please enter the place of delivery.")]
    [StringLength(120)]
    public string Place { get; set; } = "Phagwara PHC";

    [Required(ErrorMessage = "Please select the delivery outcome.")]
    public string Outcome { get; set; } = "Live Birth";

    [StringLength(400)]
    public string Complications { get; set; } = "None";

    public bool RegisterBabyNext { get; set; } = true;
}
