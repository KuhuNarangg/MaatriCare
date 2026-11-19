// AppDbContext.cs - EF Core gateway to the database tables.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Models;
namespace MaatriCare.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) { }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Mother> Mothers { get; set; }
    public DbSet<AncVisit> AncVisits { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<Newborn> Newborns { get; set; }
    public DbSet<Vaccination> Vaccinations { get; set; }
    public DbSet<GrowthRecord> GrowthRecords { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
}
