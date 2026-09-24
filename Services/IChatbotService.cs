using FanHubPlus.Models.ViewModels;

namespace FanHubPlus.Services;

public interface IChatbotService
{
    // Answers a visitor question: FAQ keyword matching + intent detection + DB search.
    // Every exchange is persisted in ChatbotQueries (UserId = null for visitors).
    Task<ChatResponseViewModel> AskAsync(string message, string? userId);
}
