// Program.cs - starts the app, registers services, creates and seeds the database.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Services;

var b = WebApplication.CreateBuilder(args);
b.Services.AddControllersWithViews();
b.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=maatricare.db"));
b.Services.AddScoped<AuthService>();
b.Services.AddScoped<ContactService>();
b.Services.AddScoped<DashboardService>();
b.Services.AddScoped<UserService>();
b.Services.AddScoped<MotherService>();
b.Services.AddScoped<AppointmentService>();
b.Services.AddScoped<NewbornService>();
b.Services.AddScoped<ReminderService>();
b.Services.AddScoped<ReferralService>();
b.Services.AddScoped<HealthTipService>();
b.Services.AddScoped<AuditService>();
b.Services.AddScoped<ConsultationService>();
b.Services.AddAuthentication("Cookies").AddCookie("Cookies", o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/AccessDenied";
});

var app = b.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();   // creates the SQLite file on first run
    db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ContactMessages (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        FullName TEXT NOT NULL,
        Email TEXT NOT NULL,
        Phone TEXT,
        Subject TEXT NOT NULL,
        Message TEXT NOT NULL,
        CreatedAt TEXT NOT NULL)");
    DbSeeder.Seed(db);             // adds demo data
}
app.UseStaticFiles();
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Public}/{action=Index}/{id?}");
app.Run();
