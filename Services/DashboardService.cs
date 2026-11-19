// DashboardService.cs - aggregates KPIs, Chart.js datasets, alerts, and role-based dashboard data.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class DashboardService
{
    private readonly AppDbContext db;
    public DashboardService(AppDbContext db) { this.db = db; }

    public async Task<DashboardViewModel> BuildDashboardAsync(string username, string role)
    {
        var today = DateTime.Today;
        var vm = new DashboardViewModel { Username = username, Role = role };

        var mothers = await db.Mothers
            .Include(m => m.Visits)
            .Include(m => m.Newborns).ThenInclude(n => n.Vaccines)
            .ToListAsync();

        var newborns = mothers.SelectMany(m => m.Newborns).ToList();
        var vaccines = newborns.SelectMany(n => n.Vaccines).ToList();

        // Mother role: show only her own pregnancy & baby summary
        if (role == "Mother")
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            var myMother = user?.MotherId != null
                ? mothers.FirstOrDefault(m => m.Id == user.MotherId)
                : mothers.FirstOrDefault();

            vm.LinkedMother = myMother;
            if (myMother != null)
            {
                vm.LinkedMotherRiskLevel = RiskService.Evaluate(myMother).Level;
                var lastVisit = myMother.Visits.OrderByDescending(v => v.VisitDate).FirstOrDefault();
                vm.NextRecommendedAncDate = lastVisit != null ? lastVisit.VisitDate.AddDays(30) : today.AddDays(7);
                vm.LinkedBaby = myMother.Newborns.OrderByDescending(b => b.Dob).FirstOrDefault();
                vm.NextBabyVaccine = vm.LinkedBaby?.Vaccines
                    .Where(v => v.GivenDate == null)
                    .OrderBy(v => v.DueDate)
                    .FirstOrDefault();
                vm.MotherTips = await db.HealthTips
                    .Where(t => t.IsPublished && (t.Trimester == myMother.Trimester || t.Trimester == 0))
                    .OrderByDescending(t => t.Trimester == myMother.Trimester)
                    .ToListAsync();
            }
            return vm;
        }

        // HealthWorker role: focus on assigned villages
        if (role == "HealthWorker")
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            var villages = (user?.AssignedVillages ?? "Phagwara,Nangal")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            vm.AssignedVillages = villages;

            var myMothers = mothers.Where(m => villages.Contains(m.Village, StringComparer.OrdinalIgnoreCase)).ToList();
            var myNewborns = myMothers.SelectMany(m => m.Newborns).ToList();

            vm.TotalMothers = myMothers.Count;
            vm.NewbornsCount = myNewborns.Count;

            foreach (var m in myMothers)
            {
                var r = RiskService.Evaluate(m);
                if (r.Level == "High") vm.HighRiskCount++;

                var lastVisit = m.Visits.OrderByDescending(v => v.VisitDate).FirstOrDefault();
                bool isOverdue = (lastVisit == null && m.Week > 0) ||
                                 (lastVisit != null && (today - lastVisit.VisitDate).TotalDays > 45 && m.Week < 38);

                if (isOverdue)
                {
                    vm.WorkerOverdue.Add(new FollowUpItem
                    {
                        Title = $"{m.Name} — Overdue ANC Check-up",
                        Subtitle = $"{m.Village} • Week {m.Week} • Phone {m.Phone}",
                        Status = "Overdue",
                        LinkUrl = $"/Mothers/Details/{m.Id}"
                    });
                }
                else if (m.Edd >= today && m.Edd <= today.AddDays(30))
                {
                    vm.WorkerTasks.Add(new FollowUpItem
                    {
                        Title = $"{m.Name} — Birth Prep Visit",
                        Subtitle = $"{m.Village} • EDD {m.Edd:dd MMM yyyy}",
                        Status = "Today",
                        LinkUrl = $"/Mothers/Details/{m.Id}"
                    });
                }
            }

            foreach (var b in myNewborns)
            {
                foreach (var v in b.Vaccines.Where(x => x.GivenDate == null && x.DueDate <= today.AddDays(14)))
                {
                    var item = new FollowUpItem
                    {
                        Title = $"{b.Name} — {v.Name}",
                        Subtitle = $"{b.Mother?.Village} • Due {v.DueDate:dd MMM yyyy}",
                        Status = v.DueDate < today ? "Overdue" : "Due Soon",
                        LinkUrl = $"/Newborns/Details/{b.Id}"
                    };
                    if (v.DueDate < today) vm.WorkerOverdue.Add(item);
                    else vm.WorkerTasks.Add(item);
                }
            }

            vm.OverdueAncCount = vm.WorkerOverdue.Count;
            return vm;
        }

        // Admin / Doctor full analytics dashboard
        vm.TotalMothers = mothers.Count;
        vm.NewbornsCount = newborns.Count;
        vm.UpcomingDeliveriesMonth = mothers.Count(m => m.Edd >= today && m.Edd <= today.AddDays(30));

        int givenCount = vaccines.Count(v => v.GivenDate != null);
        vm.VaccinationCoveragePercent = vaccines.Count == 0 ? 0 : (int)Math.Round(givenCount * 100.0 / vaccines.Count);

        foreach (var m in mothers)
        {
            var r = RiskService.Evaluate(m);
            if (r.Level == "High")
            {
                vm.HighRiskCount++;
                vm.HighRiskAlerts.Add(new RiskAlertItem
                {
                    MotherId = m.Id,
                    MotherName = m.Name,
                    Village = m.Village,
                    Week = m.Week,
                    Reasons = r.Reasons
                });
            }
            else if (r.Level == "Medium") vm.MediumRiskCount++;
            else vm.LowRiskCount++;

            var lastVisit = m.Visits.OrderByDescending(v => v.VisitDate).FirstOrDefault();
            bool overdueAnc = (lastVisit == null && m.Week > 0) ||
                              (lastVisit != null && (today - lastVisit.VisitDate).TotalDays > 45 && m.Week < 38);
            if (overdueAnc)
            {
                vm.OverdueAncCount++;
                vm.TodaysFollowUps.Add(new FollowUpItem
                {
                    Title = $"{m.Name} (ANC Overdue)",
                    Subtitle = $"{m.Village} • Week {m.Week} • {m.Phone}",
                    Status = "Overdue",
                    LinkUrl = $"/Mothers/Details/{m.Id}"
                });
            }
            else if (m.Edd >= today && m.Edd <= today.AddDays(30))
            {
                vm.TodaysFollowUps.Add(new FollowUpItem
                {
                    Title = $"{m.Name} (EDD {m.Edd:dd MMM})",
                    Subtitle = $"{m.Village} • Delivery due within 30 days",
                    Status = "Due Soon",
                    LinkUrl = $"/Mothers/Details/{m.Id}"
                });
            }
        }

        // Chart 1: Monthly registrations (last 6 months)
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            vm.MonthLabels.Add(monthStart.ToString("MMM yyyy"));
            vm.MonthRegistrationCounts.Add(mothers.Count(m => m.RegisteredAt >= monthStart && m.RegisteredAt < monthEnd));
        }

        // Chart 2: Vaccination coverage % by village
        var villagesList = mothers.Select(m => m.Village).Distinct().OrderBy(v => v).ToList();
        foreach (var village in villagesList)
        {
            var vVaccines = mothers.Where(m => m.Village == village)
                .SelectMany(m => m.Newborns)
                .SelectMany(n => n.Vaccines)
                .ToList();
            int pct = vVaccines.Count == 0 ? 0 : (int)Math.Round(vVaccines.Count(v => v.GivenDate != null) * 100.0 / vVaccines.Count);
            vm.VillageLabels.Add(village);
            vm.VillageCoveragePercents.Add(pct);
        }

        // Recent activity feed (latest registrations, visits, given vaccines)
        var activities = new List<ActivityItem>();
        activities.AddRange(mothers.Select(m => new ActivityItem
        {
            Kind = "Registration",
            Text = $"Registered {m.Name} ({m.Village})",
            Date = m.RegisteredAt
        }));
        activities.AddRange(mothers.SelectMany(m => m.Visits.Select(v => new ActivityItem
        {
            Kind = "Visit",
            Text = $"ANC visit for {m.Name} — BP {v.Systolic}/{v.Diastolic}, Hb {v.Hb}",
            Date = v.VisitDate
        })));
        activities.AddRange(newborns.SelectMany(b => b.Vaccines.Where(v => v.GivenDate != null).Select(v => new ActivityItem
        {
            Kind = "Vaccine",
            Text = $"{v.Name} given to {b.Name}",
            Date = v.GivenDate.Value
        })));

        vm.RecentActivity = activities.OrderByDescending(a => a.Date).Take(7).ToList();
        return vm;
    }

    public async Task<List<FollowUpItem>> GetTopbarNotificationsAsync()
    {
        var today = DateTime.Today;
        var alerts = new List<FollowUpItem>();

        var mothers = await db.Mothers.Include(m => m.Visits).ToListAsync();
        foreach (var m in mothers)
        {
            var r = RiskService.Evaluate(m);
            if (r.Level == "High")
            {
                alerts.Add(new FollowUpItem
                {
                    Title = $"High-risk: {m.Name}",
                    Subtitle = $"{m.Village} • {string.Join(", ", r.Reasons.Take(2))}",
                    Status = "High",
                    LinkUrl = $"/Mothers/Details/{m.Id}"
                });
            }
        }

        var overdueVax = await db.Vaccinations
            .Where(v => v.GivenDate == null && v.DueDate < today)
            .OrderBy(v => v.DueDate)
            .Take(5)
            .ToListAsync();

        foreach (var v in overdueVax)
        {
            alerts.Add(new FollowUpItem
            {
                Title = $"Overdue vaccine: {v.Name}",
                Subtitle = $"Was due {v.DueDate:dd MMM yyyy}",
                Status = "Overdue",
                LinkUrl = $"/Newborns/Details/{v.NewbornId}"
            });
        }

        return alerts.Take(8).ToList();
    }
}
