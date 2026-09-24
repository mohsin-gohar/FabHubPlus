using FanHubPlus.Models.ViewModels;

namespace FanHubPlus.Services;

public interface IStatsService
{
    // Records a detail-page view (ViewLogs table) - powers the dashboard charts
    Task LogViewAsync(string itemType, int itemId, string? userId);

    // Aggregates everything shown on /Admin/Dashboard
    Task<AdminDashboardViewModel> GetDashboardAsync();

    // Simple counters used in the footer / hero section
    Task<(int contents, int members, int events)> GetPortalTotalsAsync();
}
