// AuthService.cs - verifies user credentials, checks active status, and maps each role to its dashboard route.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
namespace MaatriCare.Services;

public class AuthService
{
    private readonly AppDbContext db;
    public AuthService(AppDbContext db) { this.db = db; }

    public async Task<(AppUser User, string Error)> ValidateUserAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return (null, "Please enter both your username and password.");

        var hash = DbSeeder.Hash(password);
        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username.Trim() && x.PasswordHash == hash);
        if (user == null)
            return (null, "We couldn't sign you in. Please check your username and password.");

        if (!user.IsActive)
            return (null, "This account has been deactivated. Please contact your clinic administrator.");

        return (user, null);
    }

    public (string Action, string Controller) GetDashboardRouteForRole(string role) =>
        PermissionService.GetLandingRoute(role);
}
