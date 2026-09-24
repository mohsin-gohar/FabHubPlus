using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

// Chatbot: JSON endpoint used by the floating widget + a full-page chat view
public class ChatbotController : Controller
{
    private readonly IChatbotService _chatbot;
    private readonly IRepository<ChatbotQuery> _log;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatbotController(IChatbotService chatbot,
                             IRepository<ChatbotQuery> log,
                             UserManager<ApplicationUser> userManager)
    {
        _chatbot = chatbot;
        _log = log;
        _userManager = userManager;
    }

    // POST /Chatbot/Ask (AJAX JSON) - called by the floating widget in the layout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask([FromForm] ChatRequestViewModel request)
    {
        if (!ModelState.IsValid)
            return Json(new ChatResponseViewModel { Reply = "Please type a message first." });

        var user = await _userManager.GetUserAsync(User); // null => visitor (logged with UserId=null)
        var response = await _chatbot.AskAsync(request.Message, user?.Id);
        return Json(response);
    }

    // GET /Chatbot - full page with my recent chat history
    public async Task<IActionResult> Index()
    {
        var vm = new ChatHistoryViewModel();

        var user = await _userManager.GetUserAsync(User);
        if (user is not null)
        {
            vm.Turns = await _log.Query()
                .Where(q => q.UserId == user.Id)
                .OrderByDescending(q => q.CreatedAt)
                .Take(20)
                .Select(q => new ChatTurn
                {
                    Question = q.Message,
                    Answer = q.Response,
                    At = q.CreatedAt
                })
                .ToListAsync();
            vm.Turns.Reverse(); // oldest first
        }

        return View(vm);
    }
}
