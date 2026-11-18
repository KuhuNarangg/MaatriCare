// Program.cs - starts the app, registers services, creates and seeds the database.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;

var b = WebApplication.CreateBuilder(args);
b.Services.AddControllersWithViews();
b.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=maatricare.db"));
b.Services.AddAuthentication("Cookies").AddCookie("Cookies", o => o.LoginPath = "/Account/Login");

var app = b.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();   // creates the SQLite file on first run
    DbSeeder.Seed(db);             // adds demo data
}
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.Run();
