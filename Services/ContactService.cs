// ContactService.cs - saves public contact form submissions into the database.
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class ContactService
{
    private readonly AppDbContext db;
    public ContactService(AppDbContext db) { this.db = db; }

    public async Task SaveMessageAsync(ContactViewModel vm)
    {
        var msg = new ContactMessage
        {
            FullName = vm.FullName.Trim(),
            Email = vm.Email.Trim(),
            Phone = vm.Phone?.Trim() ?? "",
            Subject = vm.Subject.Trim(),
            Message = vm.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        db.ContactMessages.Add(msg);
        await db.SaveChangesAsync();
    }
}
