// MotherService.cs - manages mothers CRUD, ANC visits CRUD, auto ANC schedule, and filtered directory.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class MotherService
{
    private readonly AppDbContext db;
    public MotherService(AppDbContext db) { this.db = db; }

    public async Task<MotherListViewModel> GetMothersListAsync(string q, string village, string risk, int? trimester, string sort, int page = 1, int pageSize = 8)
    {
        var allMothers = await db.Mothers
            .Include(m => m.Visits)
            .Include(m => m.Appointments)
            .ToListAsync();

        var villages = allMothers.Select(m => m.Village).Distinct().OrderBy(v => v).ToList();

        var rows = allMothers.Select(m =>
        {
            var r = RiskService.Evaluate(m);
            return new MotherRowItem { Mother = m, RiskLevel = r.Level, RiskScore = r.Score };
        });

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            rows = rows.Where(x =>
                x.Mother.Name.ToLower().Contains(term) ||
                x.Mother.Village.ToLower().Contains(term) ||
                (x.Mother.Phone ?? "").Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(village))
            rows = rows.Where(x => x.Mother.Village.Equals(village, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(risk))
            rows = rows.Where(x => x.RiskLevel.Equals(risk, StringComparison.OrdinalIgnoreCase));

        if (trimester.HasValue)
            rows = rows.Where(x => x.Mother.Trimester == trimester.Value);

        rows = (sort ?? "name") switch
        {
            "risk" => rows.OrderByDescending(x => x.RiskScore).ThenBy(x => x.Mother.Name),
            "week" => rows.OrderByDescending(x => x.Mother.Week).ThenBy(x => x.Mother.Name),
            "edd" => rows.OrderBy(x => x.Mother.Edd),
            _ => rows.OrderBy(x => x.Mother.Name)
        };

        var filtered = rows.ToList();
        page = Math.Max(1, page);
        var paged = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new MotherListViewModel
        {
            Items = paged,
            Villages = villages,
            Query = q,
            Village = village,
            Risk = risk,
            Trimester = trimester,
            Sort = sort ?? "name",
            Page = page,
            PageSize = pageSize,
            TotalCount = filtered.Count
        };
    }

    public async Task<MotherDetailsViewModel> GetMotherDetailsAsync(int id)
    {
        var m = await db.Mothers
            .Include(x => x.Visits)
            .Include(x => x.Newborns)
            .Include(x => x.Appointments)
            .Include(x => x.Deliveries)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (m == null) return null;

        var eval = RiskService.Evaluate(m);

        var vm = new MotherDetailsViewModel
        {
            Mother = m,
            RiskLevel = eval.Level,
            RiskScore = eval.Score,
            RiskReasons = eval.Reasons,
            RecommendedAction = eval.RecommendedAction,
            AncSchedule = BuildAncSchedule(m),
            VisitMilestones = m.Visits
                .OrderBy(v => v.VisitDate)
                .Select(v => new TimelineMilestone
                {
                    VisitId = v.Id,
                    Week = Math.Clamp((int)((v.VisitDate - m.Lmp).TotalDays / 7), 1, 40),
                    VisitDate = v.VisitDate,
                    Summary = $"BP {v.Systolic}/{v.Diastolic}, Hb {v.Hb}"
                })
                .ToList()
        };
        return vm;
    }

    private static List<AncScheduleSlot> BuildAncSchedule(Mother m)
    {
        var today = DateTime.Today;
        var windows = new (string Label, string WindowText, int MinW, int MaxW, int DueDay)[]
        {
            ("ANC 1", "Before 12 weeks", 0, 13, 84),
            ("ANC 2", "Weeks 14 – 26", 14, 26, 182),
            ("ANC 3", "Weeks 28 – 34", 27, 35, 238),
            ("ANC 4", "Week 36+", 36, 42, 266)
        };

        var result = new List<AncScheduleSlot>();
        foreach (var w in windows)
        {
            var due = m.Lmp.AddDays(w.DueDay);
            var matchedVisit = m.Visits
                .OrderBy(v => v.VisitDate)
                .FirstOrDefault(v =>
                {
                    int wk = (int)((v.VisitDate - m.Lmp).TotalDays / 7);
                    return wk >= w.MinW && wk <= w.MaxW;
                });

            string status = matchedVisit != null
                ? "Done"
                : today > due ? "Overdue" : "Upcoming";

            result.Add(new AncScheduleSlot
            {
                Label = w.Label,
                WindowText = w.WindowText,
                DueDate = due,
                Status = status,
                CompletedOn = matchedVisit?.VisitDate
            });
        }
        return result;
    }

    public async Task<Mother> GetMotherByIdAsync(int id) =>
        await db.Mothers.FirstOrDefaultAsync(m => m.Id == id);

    public async Task<int> CreateMotherAsync(MotherFormViewModel vm)
    {
        var m = new Mother
        {
            Name = vm.Name.Trim(),
            Age = vm.Age,
            Village = vm.Village.Trim(),
            Phone = vm.Phone.Trim(),
            BloodGroup = vm.BloodGroup?.Trim() ?? "",
            PreviousCSection = vm.PreviousCSection,
            PreviousComplications = vm.PreviousComplications,
            Lmp = vm.Lmp,
            RegisteredAt = DateTime.Today
        };
        db.Mothers.Add(m);
        await db.SaveChangesAsync();
        return m.Id;
    }

    public async Task<bool> UpdateMotherAsync(MotherFormViewModel vm)
    {
        var m = await db.Mothers.FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (m == null) return false;
        m.Name = vm.Name.Trim();
        m.Age = vm.Age;
        m.Village = vm.Village.Trim();
        m.Phone = vm.Phone.Trim();
        m.BloodGroup = vm.BloodGroup?.Trim() ?? "";
        m.PreviousCSection = vm.PreviousCSection;
        m.PreviousComplications = vm.PreviousComplications;
        m.Lmp = vm.Lmp;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMotherAsync(int id)
    {
        var m = await db.Mothers
            .Include(x => x.Visits)
            .Include(x => x.Appointments)
            .Include(x => x.Newborns).ThenInclude(n => n.Vaccines)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (m == null) return false;
        db.Mothers.Remove(m);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task AddVisitAsync(AncVisitFormViewModel vm)
    {
        var v = new AncVisit
        {
            MotherId = vm.MotherId,
            VisitDate = vm.VisitDate,
            Systolic = vm.Systolic,
            Diastolic = vm.Diastolic,
            Hb = vm.Hb,
            BloodSugar = vm.BloodSugar,
            WeightKg = vm.WeightKg,
            UrineProtein = vm.UrineProtein ?? "Nil",
            FetalHeartbeat = vm.FetalHeartbeat,
            NextVisitDate = vm.NextVisitDate,
            Notes = vm.Notes?.Trim() ?? ""
        };
        db.AncVisits.Add(v);
        await db.SaveChangesAsync();
    }

    public async Task<AncVisit> GetVisitByIdAsync(int id) =>
        await db.AncVisits.FirstOrDefaultAsync(v => v.Id == id);

    public async Task<int?> UpdateVisitAsync(AncVisitFormViewModel vm)
    {
        var v = await db.AncVisits.FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (v == null) return null;
        v.VisitDate = vm.VisitDate;
        v.Systolic = vm.Systolic;
        v.Diastolic = vm.Diastolic;
        v.Hb = vm.Hb;
        v.BloodSugar = vm.BloodSugar;
        v.WeightKg = vm.WeightKg;
        v.UrineProtein = vm.UrineProtein ?? "Nil";
        v.FetalHeartbeat = vm.FetalHeartbeat;
        v.NextVisitDate = vm.NextVisitDate;
        v.Notes = vm.Notes?.Trim() ?? "";
        await db.SaveChangesAsync();
        return v.MotherId;
    }

    public async Task<int?> DeleteVisitAsync(int id)
    {
        var v = await db.AncVisits.FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return null;
        int motherId = v.MotherId;
        db.AncVisits.Remove(v);
        await db.SaveChangesAsync();
        return motherId;
    }
}
