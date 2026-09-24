using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using Microsoft.AspNetCore.Identity;

namespace FanHubPlus.Services;

public interface ISupportService
{
    // Saves feedback (visitor => UserId null, e-mail folded into the message)
    Task SaveFeedbackAsync(FeedbackFormViewModel form, ApplicationUser? user);

    // Submits fan art for admin review (optional image already uploaded by controller)
    Task SubmitFanArtAsync(FanSubmissionFormViewModel form, string userId, string? imageUrl);

    // Resolves polymorphic bookmarks to real titles/links for My Bookmarks
    Task<MyBookmarksViewModel> GetBookmarksAsync(string userId);
}
