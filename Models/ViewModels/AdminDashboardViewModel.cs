using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

// Admin dashboard: stat cards + recent activity + chart series
public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int TotalContents { get; set; }
    public int TotalViews { get; set; }
    public int PendingFeedback { get; set; }
    public int PendingSubmissions { get; set; }
    public int TotalEvents { get; set; }
    public int TotalMerch { get; set; }

    // Last 7 days views (line chart data)
    public List<DailyCount> ViewsByDay { get; set; } = new();

    // Category distribution (bar chart data)
    public List<CategoryCount> ContentsByCategory { get; set; } = new();

    // Top content by views (top 5)
    public List<Content> TopContents { get; set; } = new();

    // Recent activity feeds
    public List<Feedback> RecentFeedback { get; set; } = new();
    public List<FanSubmission> RecentSubmissions { get; set; } = new();
    public List<ApplicationUser> RecentUsers { get; set; } = new();
}

public class DailyCount
{
    public DateOnly Day { get; set; }
    public int Count { get; set; }
}

public class CategoryCount
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
