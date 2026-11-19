// UserService.cs - manages user accounts, village assignments, mother links, profiles, and password updates.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class UserService
{
    private readonly AppDbContext db;
    public UserService(AppDbContext db) { this.db = db; }

    public async Task<List<AppUser>> GetAllUsersAsync() =>
        await db.Users.OrderBy(u => u.Role).ThenBy(u => u.Username).ToListAsync();

    public async Task<AppUser> GetUserByIdAsync(int id) =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<AppUser> GetUserByUsernameAsync(string username) =>
        await db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<List<string>> GetAllVillagesAsync()
    {
        var defaults = new[] { "Phagwara", "Nangal", "Hadiabad", "Goraya" };
        var dbVillages = await db.Mothers.Select(m => m.Village).Distinct().ToListAsync();
        return defaults.Union(dbVillages, StringComparer.OrdinalIgnoreCase).OrderBy(v => v).ToList();
    }

    public async Task<List<Mother>> GetMothersForSelectAsync() =>
        await db.Mothers.OrderBy(m => m.Name).ToListAsync();

    public async Task<string> CreateUserAsync(UserFormViewModel vm)
    {
        var cleanUser = vm.Username.Trim();
        if (await db.Users.AnyAsync(u => u.Username.ToLower() == cleanUser.ToLower()))
            return "That username is already taken. Please choose another.";

        if (string.IsNullOrWhiteSpace(vm.Password) || vm.Password.Length < 6)
            return "Please set an initial password of at least 6 characters.";

        var user = new AppUser
        {
            Username = cleanUser,
            FullName = vm.FullName.Trim(),
            Phone = vm.Phone?.Trim() ?? "",
            Role = vm.Role,
            PasswordHash = DbSeeder.Hash(vm.Password),
            IsActive = vm.IsActive,
            AssignedVillages = vm.Role == "HealthWorker" ? string.Join(",", vm.SelectedVillages ?? new()) : "",
            MotherId = vm.Role == "Mother" ? vm.MotherId : null
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return null;
    }

    public async Task<string> UpdateUserAsync(UserFormViewModel vm)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == vm.Id);
        if (user == null) return "User not found.";

        user.FullName = vm.FullName.Trim();
        user.Phone = vm.Phone?.Trim() ?? "";
        user.Role = vm.Role;
        user.IsActive = vm.IsActive;
        user.AssignedVillages = vm.Role == "HealthWorker" ? string.Join(",", vm.SelectedVillages ?? new()) : "";
        user.MotherId = vm.Role == "Mother" ? vm.MotherId : null;

        if (!string.IsNullOrWhiteSpace(vm.Password))
        {
            if (vm.Password.Length < 6) return "New password must be at least 6 characters.";
            user.PasswordHash = DbSeeder.Hash(vm.Password);
        }

        await db.SaveChangesAsync();
        return null;
    }

    public async Task<(bool Ok, string Message)> ToggleActiveAsync(int id, string currentUsername)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return (false, "User not found.");
        if (user.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
            return (false, "You cannot deactivate your own admin account.");

        user.IsActive = !user.IsActive;
        await db.SaveChangesAsync();
        return (true, user.IsActive ? $"{user.Username} has been reactivated." : $"{user.Username} has been deactivated.");
    }

    public async Task UpdateProfileAsync(string username, ProfileViewModel vm)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return;
        user.FullName = vm.FullName.Trim();
        user.Phone = vm.Phone?.Trim() ?? "";
        await db.SaveChangesAsync();
    }

    public async Task<string> ChangePasswordAsync(string username, ChangePasswordViewModel vm)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return "Account not found.";
        if (user.PasswordHash != DbSeeder.Hash(vm.CurrentPassword))
            return "Your current password is incorrect.";

        user.PasswordHash = DbSeeder.Hash(vm.NewPassword);
        await db.SaveChangesAsync();
        return null;
    }
}
