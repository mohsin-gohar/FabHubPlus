using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.ViewModels;

// Chatbot widget request/response (JSON round-trip)
public class ChatRequestViewModel
{
    [Required, StringLength(1000)]
    public string Message { get; set; } = string.Empty;
}

public class ChatResponseViewModel
{
    public string Reply { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new(); // follow-up quick chips
}

// Full-page chat history view
public class ChatHistoryViewModel
{
    public List<ChatTurn> Turns { get; set; } = new();
}

public class ChatTurn
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime At { get; set; }
}
