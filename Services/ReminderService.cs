// ReminderService.cs - generates simulated SMS notifications and builds Today/Overdue/Upcoming follow-up queues.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class ReminderService
{
    private readonly AppDbContext db;
    public ReminderService(AppDbContext db) { this.db = db; }

    public async Task<List<Notification>> GetNotificationsAsync(bool unreadOnly = false)
    {
        var q = db.Notifications.AsQueryable();
        if (unreadOnly) q = q.Where(n => !n.IsRead);
        return await q.OrderByDescending(n => n.CreatedAt).Take(60).ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync() =>
        await db.Notifications.CountAsync(n => !n.IsRead);

    public async Task MarkReadAsync(int id)
    {
        var n = await db.Notifications.FindAsync(id);
        if (n == null) return;
        n.IsRead = true;
        await db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync()
    {
        var unread = await db.Notifications.Where(n => !n.IsRead).ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await db.SaveChangesAsync();
    }

    public async Task<int> GenerateRemindersAsync()
    {
        var today = DateTime.Today;
        var nextWeek = today.AddDays(7);
        int created = 0;

        // Avoid sending duplicate SMS reminders with the exact same title today
        var todaysTitles = await db.Notifications
            .Where(n => n.CreatedAt >= today)
            .Select(n => n.Title)
            .ToListAsync();
        var seen = new HashSet<string>(todaysTitles, StringComparer.OrdinalIgnoreCase);

        // 1. Upcoming scheduled appointments (next 7 days)
        var upcomingAppts = await db.Appointments
            .Include(a => a.Mother)
            .Where(a => a.Status == "Scheduled" && a.ScheduledDate >= today && a.ScheduledDate <= nextWeek)
            .ToListAsync();

        foreach (var a in upcomingAppts)
        {
            string title = $"Upcoming Visit: {a.Mother.Name} ({a.ScheduledDate:dd MMM})";
            if (!seen.Add(title)) continue;
            db.Notifications.Add(new Notification
            {
                RecipientName = a.Mother.Name,
                Phone = a.Mother.Phone ?? "",
                Category = "Upcoming Visit",
                Title = title,
                Message = $"Namaste {a.Mother.Name}, reminder for your {a.Purpose} on {a.ScheduledDate:dd MMM yyyy}.",
                LinkUrl = $"/Mothers/Details/{a.MotherId}"
            });
            created++;
        }

        // 2. Overdue ANC visits (>45 days since last check-up or no visit recorded yet)
        var mothers = await db.Mothers.Include(m => m.Visits).ToListAsync();
        foreach (var m in mothers)
        {
            var last = m.Visits.OrderByDescending(v => v.VisitDate).FirstOrDefault();
            bool overdue = (last == null && m.Week > 0) ||
                           (last != null && (today - last.VisitDate).TotalDays > 45 && m.Week < 38);
            if (!overdue) continue;

            string title = $"Overdue ANC Visit: {m.Name} ({m.Village})";
            if (!seen.Add(title)) continue;
            db.Notifications.Add(new Notification
            {
                RecipientName = m.Name,
                Phone = m.Phone ?? "",
                Category = "Overdue Visit",
                Title = title,
                Message = $"Namaste {m.Name}, your antenatal check-up is overdue (Week {m.Week}). Please visit {m.Village} health centre soon.",
                LinkUrl = $"/Mothers/Details/{m.Id}"
            });
            created++;
        }

        // 3. Due or overdue vaccines (due within next 7 days or already overdue)
        var dueVax = await db.Vaccinations
            .Include(v => v.Newborn).ThenInclude(n => n.Mother)
            .Where(v => v.GivenDate == null && v.DueDate <= nextWeek)
            .ToListAsync();

        foreach (var v in dueVax)
        {
            string momName = v.Newborn?.Mother?.Name ?? "Parent";
            string phone = v.Newborn?.Mother?.Phone ?? "";
            string title = $"Vaccine Due: {v.Newborn?.Name} — {v.Name}";
            if (!seen.Add(title)) continue;
            db.Notifications.Add(new Notification
            {
                RecipientName = momName,
                Phone = phone,
                Category = "Due Vaccine",
                Title = title,
                Message = $"Namaste {momName}, {v.Newborn?.Name} is due for {v.Name} (due {v.DueDate:dd MMM yyyy}).",
                LinkUrl = $"/Newborns/Details/{v.NewbornId}"
            });
            created++;
        }

        if (created > 0) await db.SaveChangesAsync();
        return created;
    }

    public async Task MarkContactedAsync(string targetKey, string contactedBy, string note)
    {
        db.FollowUpContacts.Add(new FollowUpContact
        {
            TargetKey = targetKey,
            ContactedBy = contactedBy ?? "staff",
            Note = string.IsNullOrWhiteSpace(note) ? "Patient contacted by phone" : note.Trim(),
            ContactedAt = DateTime.Now
        });
        await db.SaveChangesAsync();
    }

    public async Task<FollowUpPageViewModel> GetFollowUpsAsync(string tab = "today")
    {
        var today = DateTime.Today;
        var nextTwoWeeks = today.AddDays(14);
        var allTasks = new List<FollowUpTaskRow>();
        var contacts = await db.FollowUpContacts
            .OrderByDescending(c => c.ContactedAt)
            .ToListAsync();
        var latestByKey = contacts.GroupBy(c => c.TargetKey).ToDictionary(g => g.Key, g => g.First());

        // Scheduled & Missed appointments (Missed automatically appear in Overdue follow-ups!)
        var appts = await db.Appointments
            .Include(a => a.Mother)
            .Where(a => a.Status == "Scheduled" || a.Status == "Waiting" || a.Status == "Missed")
            .ToListAsync();

        foreach (var a in appts)
        {
            string status = a.Status == "Missed" || a.ScheduledDate.Date < today
                ? "Overdue"
                : a.ScheduledDate.Date == today ? "Today" : "Upcoming";
            string key = $"appt-{a.Id}";
            latestByKey.TryGetValue(key, out var c);

            allTasks.Add(new FollowUpTaskRow
            {
                TargetKey = key,
                Kind = a.Status == "Missed" ? "Missed Appt" : "Appointment",
                AppointmentId = a.Id,
                PersonName = a.Mother?.Name ?? "",
                Village = a.Mother?.Village ?? "",
                Phone = a.Mother?.Phone ?? "",
                Description = $"{a.Purpose} ({a.StartTime})",
                DueDate = a.ScheduledDate,
                Status = status,
                DetailUrl = $"/Mothers/Details/{a.MotherId}",
                LastContactNote = c != null ? $"{c.Note} (by {c.ContactedBy})" : null,
                LastContactedAt = c?.ContactedAt
            });
        }

        // Overdue ANC check-ups
        var mothers = await db.Mothers.Include(m => m.Visits).ToListAsync();
        foreach (var m in mothers)
        {
            var last = m.Visits.OrderByDescending(v => v.VisitDate).FirstOrDefault();
            bool overdue = (last == null && m.Week > 0) ||
                           (last != null && (today - last.VisitDate).TotalDays > 45 && m.Week < 38);
            if (overdue)
            {
                string key = $"anc-{m.Id}";
                latestByKey.TryGetValue(key, out var c);
                allTasks.Add(new FollowUpTaskRow
                {
                    TargetKey = key,
                    Kind = "ANC Visit",
                    PersonName = m.Name,
                    Village = m.Village,
                    Phone = m.Phone ?? "",
                    Description = last == null ? $"No ANC visit yet (Week {m.Week})" : $"Last ANC visit {(int)(today - last.VisitDate).TotalDays} days ago",
                    DueDate = last?.VisitDate.AddDays(45) ?? today,
                    Status = "Overdue",
                    DetailUrl = $"/Mothers/Details/{m.Id}",
                    LastContactNote = c != null ? $"{c.Note} (by {c.ContactedBy})" : null,
                    LastContactedAt = c?.ContactedAt
                });
            }
        }

        // Due / overdue vaccines
        var vax = await db.Vaccinations
            .Include(v => v.Newborn).ThenInclude(n => n.Mother)
            .Where(v => v.GivenDate == null && v.DueDate <= nextTwoWeeks)
            .ToListAsync();

        foreach (var v in vax)
        {
            string status = v.DueDate.Date < today ? "Overdue" : v.DueDate.Date == today ? "Today" : "Upcoming";
            string key = $"vax-{v.Id}";
            latestByKey.TryGetValue(key, out var c);
            allTasks.Add(new FollowUpTaskRow
            {
                TargetKey = key,
                Kind = "Vaccine",
                VaccineId = v.Id,
                PersonName = v.Newborn?.Name ?? "",
                Village = v.Newborn?.Mother?.Village ?? "",
                Phone = v.Newborn?.Mother?.Phone ?? "",
                Description = $"Vaccine: {v.Name}",
                DueDate = v.DueDate,
                Status = status,
                DetailUrl = $"/Newborns/Details/{v.NewbornId}",
                LastContactNote = c != null ? $"{c.Note} (by {c.ContactedBy})" : null,
                LastContactedAt = c?.ContactedAt
            });
        }

        tab = string.IsNullOrWhiteSpace(tab) ? "today" : tab.ToLower();
        var vm = new FollowUpPageViewModel
        {
            Tab = tab,
            TodayCount = allTasks.Count(t => t.Status == "Today"),
            OverdueCount = allTasks.Count(t => t.Status == "Overdue"),
            UpcomingCount = allTasks.Count(t => t.Status == "Upcoming")
        };

        vm.Items = tab switch
        {
            "overdue" => allTasks.Where(t => t.Status == "Overdue").OrderBy(t => t.DueDate).ToList(),
            "upcoming" => allTasks.Where(t => t.Status == "Upcoming").OrderBy(t => t.DueDate).ToList(),
            _ => allTasks.Where(t => t.Status == "Today").OrderBy(t => t.DueDate).ToList()
        };

        return vm;
    }
}
