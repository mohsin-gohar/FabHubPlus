using FanHubPlus.Models.ViewModels;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FanHubPlus.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IStatsService _stats;

    public DashboardController(IStatsService stats) => _stats = stats;

    // KPI cards + CSS bar/line charts + recent activity feeds
    public async Task<IActionResult> Index()
    {
        var vm = await _stats.GetDashboardAsync();
        return View(vm);
    }
}
