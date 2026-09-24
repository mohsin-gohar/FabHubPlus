using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Services;

/// <summary>
/// Rule-based chatbot (no paid AI API - works offline for the competition):
///   1) Built-in intents (greeting, help, categories, search, events, contact...)
///   2) FAQ keyword scoring against the ChatFaqs table (admin-editable knowledge base)
///   3) Live database search as a fallback (finds real content titles)
///   4) Friendly "I don't understand" fallback with navigation suggestions
/// Every Q&A is logged to ChatbotQueries for the admin analytics.
/// </summary>
public class ChatbotService : IChatbotService
{
    private readonly IRepository<ChatFaq> _faqs;
    private readonly IRepository<ChatbotQuery> _log;
    private readonly IRepository<Content> _contents;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<EventItem> _events;

    public ChatbotService(IRepository<ChatFaq> faqs,
                          IRepository<ChatbotQuery> log,
                          IRepository<Content> contents,
                          IRepository<Category> categories,
                          IRepository<EventItem> events)
    {
        _faqs = faqs;
        _log = log;
        _contents = contents;
        _categories = categories;
        _events = events;
    }

    public async Task<ChatResponseViewModel> AskAsync(string message, string? userId)
    {
        message = (message ?? string.Empty).Trim();
        var lower = message.ToLowerInvariant();
        var response = await RouteAsync(lower);

        // Persist the conversation (visitors => UserId null)
        _log.Add(new ChatbotQuery
        {
            UserId = userId,
            Message = message.Length > 1000 ? message[..1000] : message,
            Response = response.Reply.Length > 4000 ? response.Reply[..4000] : response.Reply,
            CreatedAt = DateTime.UtcNow
        });
        await _log.SaveChangesAsync();

        return response;
    }

    private async Task<ChatResponseViewModel> RouteAsync(string lower)
    {
        // ---------- 1) Small talk / meta intents ----------
        if (IsAny(lower, "hi", "hello", "hey", "salam", "assalam"))
            return Ok("Hello, fan! 👋 Welcome to Fan Hub Plus. Ask me anything about the portal, " +
                      "or try: \"what can you do\".",
                "Explore content", "Categories", "Contact support");

        if (IsAny(lower, "what can you do", "help", "commands", "options", "how to use"))
            return Ok("I can help you with:\n" +
                      "• \"categories\" – list the fandoms\n" +
                      "• \"search <word>\" – find movies/games/series\n" +
                      "• \"events\" – upcoming fandom events\n" +
                      "• \"rate\", \"bookmark\", \"submit\" – how the portal works\n" +
                      "• Anything about the FAQ – just ask!",
                "Categories", "Search anime", "Upcoming events");

        if (IsAny(lower, "categories", "category", "fandoms", "fandom", "genres", "what fandoms"))
        {
            var names = _categories.Query().OrderBy(c => c.Name).Select(c => c.Name).ToList();
            return Ok($"We cover {names.Count} fandoms: {string.Join(", ", names)}. " +
                      "Open the Explore page to filter content by any of them!",
                "Open Explore", "Search content");
        }

        if (IsAny(lower, "events", "event", "convention", "upcoming events", "calendar"))
        {
            var next = _events.Query()
                .Where(e => e.EventDate >= DateTime.UtcNow)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .Select(e => $"{e.Title} ({e.City}, {e.EventDate:MMM d})")
                .ToList();
            var text = next.Count == 0
                ? "No upcoming events right now – check back soon!"
                : "Upcoming events:\n• " + string.Join("\n• ", next);
            return Ok(text, "View all events", "Explore content");
        }

        if (IsAny(lower, "contact", "support", "feedback", "report bug", "bug"))
            return Ok("You can send feedback or report a bug on the Support page – " +
                      "even visitors can use it (no login needed). Our admins reply from the dashboard.",
                "Open Support", "FAQ");

        if (IsAny(lower, "admin", "login", "register", "sign up", "account", "password"))
            return Ok("Use the Login / Register buttons in the top bar. " +
                      "Password rules: 8+ chars with upper, lower, digit and symbol. " +
                      "Admins get an extra dashboard under /Admin after logging in.",
                "Register now", "Reset password");

        if (IsAny(lower, "rate", "rating", "stars"))
            return Ok("Open any content page and click the stars – you can rate each item once " +
                      "(stored in the Ratings table with a unique user+content key).",
                "Explore content", "Bookmarks");

        if (IsAny(lower, "submit", "fan art", "fanart", "gallery", "post"))
            return Ok("Visit Fan Art Gallery → \"Submit your work\". " +
                      "An admin reviews every submission and approves or rejects it. " +
                      "Approved pieces appear in the public gallery!",
                "Fan Art Gallery", "Support");

        if (IsAny(lower, "merch", "shop", "buy", "purchase", "price", "store"))
            return Ok("The Merchandise section is a showcase only – no payments on this portal. " +
                      "You can browse Limited Edition, Pre-Order and Collectible items.",
                "Browse merch", "Explore content");

        // ---------- 2) "search <word>" intent -> live DB search ----------
        if (lower.StartsWith("search ") || lower.StartsWith("find ") || lower.StartsWith("show me "))
        {
            var term = lower[(lower.IndexOf(' ') + 1)..].Trim();
            return await SearchAsync(term);
        }

        // ---------- 3) FAQ knowledge-base keyword scoring ----------
        var best = await ScoreFaqsAsync(lower);
        if (best is not null)
            return Ok(best.Answer, "Support page");

        // ---------- 4) Generic keyword search over content titles ----------
        var generic = await SearchAsync(lower, relaxed: true);
        if (generic.Suggestions.Count > 0 || !generic.Reply.StartsWith("I'm not sure"))
            return generic;

        // ---------- 5) Fallback ----------
        return Ok("I'm not sure about that one 🤔. Try asking about categories, events, ratings, " +
                  "bookmarks or submissions – or visit the Support page where admins can help you.",
            "Support", "Explore", "Home");
    }

    private async Task<ChatResponseViewModel> SearchAsync(string term, bool relaxed = false)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Ok("Tell me what to search for, e.g. \"search anime\".", "Explore");

        var matches = await _contents.Query()
            .Where(c => c.Title.ToLower().Contains(term) ||
                        (c.Genre != null && c.Genre.ToLower().Contains(term)))
            .OrderByDescending(c => c.PopularityScore)
            .Take(3)
            .Select(c => c.Title)
            .ToListAsync();

        if (matches.Count == 0 && relaxed)
            return Ok("I'm not sure about that one 🤔. Could you rephrase?",
                "Explore all content", "Support");

        if (matches.Count == 0)
            return Ok($"No content matched \"{term}\". Try another word or browse the full Explorer.",
                "Explore");

        return Ok($"I found {matches.Count} match(es):\n• " + string.Join("\n• ", matches) +
                  "\nOpen the Explorer to see details, ratings and trailers.",
            "Open Explorer");
    }

    private async Task<ChatFaq?> ScoreFaqsAsync(string lower)
    {
        var faqs = await _faqs.ListAsync();
        ChatFaq? best = null;
        var bestScore = 0;

        foreach (var faq in faqs)
        {
            var keywords = (faq.Keywords ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var score = keywords.Count(k => lower.Contains(k.ToLowerInvariant()));

            // Also match significant words of the question itself
            foreach (var word in faq.Question.ToLowerInvariant()
                         .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                if (word.Length > 5 && lower.Contains(word)) score++;

            if (score > bestScore) { bestScore = score; best = faq; }
        }

        return bestScore >= 2 ? best : null; // require at least 2 keyword hits
    }

    private static bool IsAny(string input, params string[] phrases)
        => phrases.Any(p => input == p || input.Contains(p));

    private static ChatResponseViewModel Ok(string reply, params string[] suggestions)
        => new() { Reply = reply, Suggestions = suggestions.ToList() };
}

