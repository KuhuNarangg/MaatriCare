// NewbornService.cs - manages deliveries, newborn profiles, growth records, and the clinic-wide vaccine tracker.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class NewbornService
{
    private readonly AppDbContext db;
    public NewbornService(AppDbContext db) { this.db = db; }

    public async Task<BabyPortalViewModel> GetBabyPortalAsync(string q = "", string village = "", string flag = "all")
    {
        var all = await db.Newborns
            .Include(n => n.Mother)
            .Include(n => n.Delivery)
            .Include(n => n.Vaccines)
            .Include(n => n.GrowthRecords)
            .OrderByDescending(n => n.Dob)
            .ToListAsync();

        var villages = all.Select(n => n.Mother?.Village)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct()
            .OrderBy(v => v)
            .ToList();

        var filtered = all.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            filtered = filtered.Where(b =>
                b.Name.ToLower().Contains(term) ||
                (b.Mother?.Name ?? "").ToLower().Contains(term) ||
                (b.Mother?.Village ?? "").ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(village))
            filtered = filtered.Where(b => (b.Mother?.Village ?? "").Equals(village, StringComparison.OrdinalIgnoreCase));

        flag = string.IsNullOrWhiteSpace(flag) ? "all" : flag.ToLower();
        filtered = flag switch
        {
            "lbw" => filtered.Where(b => b.IsLowBirthWeight),
            "preterm" => filtered.Where(b => b.IsPreterm),
            "overdue" => filtered.Where(b => b.Vaccines.Any(v => v.Status == "Overdue")),
            _ => filtered
        };

        return new BabyPortalViewModel
        {
            Query = q ?? "",
            Village = village ?? "",
            Flag = flag,
            Villages = villages,
            TotalBabies = all.Count,
            LbwCount = all.Count(b => b.IsLowBirthWeight),
            PretermCount = all.Count(b => b.IsPreterm),
            OverdueVaccinesBabiesCount = all.Count(b => b.Vaccines.Any(v => v.Status == "Overdue")),
            Babies = filtered.ToList()
        };
    }

    public async Task<Mother> GetMotherAsync(int motherId) =>
        await db.Mothers.FirstOrDefaultAsync(m => m.Id == motherId);

    public async Task<List<Delivery>> GetDeliveriesForMotherAsync(int motherId) =>
        await db.Deliveries.Where(d => d.MotherId == motherId).OrderByDescending(d => d.DeliveryDate).ToListAsync();

    public async Task<int> RecordDeliveryAsync(DeliveryFormViewModel vm)
    {
        var d = new Delivery
        {
            MotherId = vm.MotherId,
            DeliveryDate = vm.DeliveryDate,
            DeliveryType = vm.DeliveryType,
            Place = vm.Place.Trim(),
            Outcome = vm.Outcome,
            Complications = vm.Complications?.Trim() ?? "None"
        };
        db.Deliveries.Add(d);
        await db.SaveChangesAsync();
        return d.Id;
    }

    public async Task<int> CreateNewbornAsync(NewbornFormViewModel vm)
    {
        var baby = new Newborn
        {
            MotherId = vm.MotherId,
            DeliveryId = vm.DeliveryId,
            Name = vm.Name.Trim(),
            Dob = vm.Dob,
            Gender = vm.Gender,
            BirthWeight = vm.BirthWeight,
            ApgarScore = vm.ApgarScore,
            GestationalAgeWeeks = vm.GestationalAgeWeeks,
            Vaccines = VaccineService.Schedule(vm.Dob)
        };
        if (vm.BirthWeight > 0)
        {
            baby.GrowthRecords.Add(new GrowthRecord
            {
                RecordedDate = vm.Dob,
                WeightKg = vm.BirthWeight,
                HeightCm = 49.0,
                Notes = "Birth weight recorded at registration"
            });
        }
        db.Newborns.Add(baby);
        await db.SaveChangesAsync();
        return baby.Id;
    }

    public async Task<Newborn> GetNewbornDetailsAsync(int id) =>
        await db.Newborns
            .Include(n => n.Mother)
            .Include(n => n.Delivery)
            .Include(n => n.Vaccines)
            .Include(n => n.GrowthRecords)
            .FirstOrDefaultAsync(n => n.Id == id);

    public async Task<int?> MarkVaccineGivenAsync(int id, DateTime? givenDate, string batchNumber)
    {
        var v = await db.Vaccinations.FindAsync(id);
        if (v == null) return null;
        v.GivenDate = givenDate ?? DateTime.Today;
        v.BatchNumber = batchNumber?.Trim() ?? "";
        await db.SaveChangesAsync();
        return v.NewbornId;
    }

    public async Task AddGrowthRecordAsync(GrowthRecordViewModel vm)
    {
        db.GrowthRecords.Add(new GrowthRecord
        {
            NewbornId = vm.NewbornId,
            RecordedDate = vm.RecordedDate,
            WeightKg = vm.WeightKg,
            HeightCm = vm.HeightCm,
            Notes = vm.Notes?.Trim() ?? ""
        });
        await db.SaveChangesAsync();
    }

    public async Task<VaccineTrackerViewModel> GetVaccineTrackerAsync(string filter = "pending")
    {
        var today = DateTime.Today;
        var weekEnd = today.AddDays(7);

        var all = await db.Vaccinations
            .Include(v => v.Newborn).ThenInclude(n => n.Mother)
            .OrderBy(v => v.DueDate)
            .ToListAsync();

        var vm = new VaccineTrackerViewModel
        {
            Filter = string.IsNullOrWhiteSpace(filter) ? "pending" : filter.ToLower(),
            TotalPending = all.Count(v => v.GivenDate == null && v.DueDate >= today),
            TotalDueThisWeek = all.Count(v => v.GivenDate == null && v.DueDate >= today && v.DueDate <= weekEnd),
            TotalOverdue = all.Count(v => v.GivenDate == null && v.DueDate < today),
            TotalGiven = all.Count(v => v.GivenDate != null)
        };

        vm.Items = vm.Filter switch
        {
            "week" => all.Where(v => v.GivenDate == null && v.DueDate >= today && v.DueDate <= weekEnd).ToList(),
            "overdue" => all.Where(v => v.GivenDate == null && v.DueDate < today).ToList(),
            "given" => all.Where(v => v.GivenDate != null).OrderByDescending(v => v.GivenDate).ToList(),
            "all" => all,
            _ => all.Where(v => v.GivenDate == null).ToList()
        };

        return vm;
    }
}
