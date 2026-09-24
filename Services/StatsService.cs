using FanHubPlus.Data;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Services;

/// <summary>
/// Analytics engine: writes view logs and computes dashboard aggregates
/// with efficient SQL-side GROUP BY / COUNT queries (no in-memory crunching).
/// </summary>
public class StatsService : IStatsService
{
    private readonly ApplicationDbContext _db;

    public StatsService(ApplicationDbContext db) => _db = db;

    public async Task LogViewAsync(string itemType, int itemId, string? userId)
    {
        _db.ViewLogs.Add(new ViewLog
        {
            ItemType = itemType,          // "Content" | "Article" | "Merchandise" ...
            ItemId = itemId,
            UserId = userId,              // null => visitor
            ViewedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var weekStart = now.Date.AddDays(-6);

        var vm = new AdminDashboardViewModel
        {
            TotalUsers = await _db.Users.CountAsync(),
            NewUsersThisMonth = await _db.Users.CountAsync(u => u.CreatedAt >= monthStart),
            TotalContents = await _db.Contents.CountAsync(),
            TotalViews = await _db.ViewLogs.CountAsync(),
            PendingFeedback = await _db.Feedbacks.CountAsync(f => f.Status == Models.Enums.FeedbackStatus.Open),
            PendingSubmissions = await _db.FanSubmissions.CountAsync(s => s.Status == Models.Enums.SubmissionStatus.Pending),
            TotalEvents = await _db.Events.CountAsync(),
            TotalMerch = await _db.MerchandiseItems.CountAsync()
        };

        // Views per day for the last 7 days (chart)
        var rawDays = await _db.ViewLogs
            .Where(v => v.ViewedAt >= weekStart)
            .GroupBy(v => v.ViewedAt.Date)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync();

        // Fill missing days with 0 so the line chart has a continuous axis
        for (var i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            vm.ViewsByDay.Add(new DailyCount
            {
                Day = DateOnly.FromDateTime(day),
                Count = rawDays.FirstOrDefault(r => r.Day == day)?.Count ?? 0
            });
        }

        // Content count per category (horizontal bar chart)
        vm.ContentsByCategory = await _db.Categories
            .Select(c => new CategoryCount
            {
                Name = c.Name,
                Count = c.Contents.Count
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync();

        // Top 5 content by view count
        vm.TopContents = await _db.Contents
            .Include(c => c.Category)
            .OrderByDescending(c => c.ViewCount)
            .Take(5)
            .ToListAsync();

        vm.RecentFeedback = await _db.Feedbacks
            .Include(f => f.User)
            .OrderByDescending(f => f.CreatedAt)
            .Take(5)
            .ToListAsync();

        vm.RecentSubmissions = await _db.FanSubmissions
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .ToListAsync();

        vm.RecentUsers = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .ToListAsync();

        return vm;
    }

    public async Task<(int contents, int members, int events)> GetPortalTotalsAsync()
    {
        var contents = await _db.Contents.CountAsync();
        var members = await _db.Users.CountAsync();
        var events = await _db.Events.CountAsync(e => e.EventDate >= DateTime.UtcNow);
        return (contents, members, events);
    }
}
