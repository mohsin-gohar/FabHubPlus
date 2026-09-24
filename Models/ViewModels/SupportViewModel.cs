using System.ComponentModel.DataAnnotations;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.ViewModels;

// Support page: FAQ list + feedback form in one view
public class SupportViewModel
{
    public FeedbackFormViewModel FeedbackForm { get; set; } = new();
    public IEnumerable<FaqItemViewModel> Faqs { get; set; } = Enumerable.Empty<FaqItemViewModel>();
    public bool FeedbackSent { get; set; }
}

public class FeedbackFormViewModel
{
    public FeedbackType Type { get; set; } = FeedbackType.Query;

    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    // Optional contact e-mail for anonymous visitors
    [EmailAddress]
    public string? ContactEmail { get; set; }
}

public class FaqItemViewModel
{
    public int FaqId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
