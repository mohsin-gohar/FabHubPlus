using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Services;

/// <summary>
/// Support-domain logic: feedback intake, fan-art submission workflow,
/// and polymorphic bookmark resolution (Bookmark -> real title + link).
/// </summary>
public class SupportService : ISupportService
{
    private readonly IRepository<Feedback> _feedbacks;
    private readonly IRepository<FanSubmission> _submissions;
    private readonly IRepository<Bookmark> _bookmarks;
    private readonly IRepository<Content> _contents;
    private readonly IRepository<CharacterProfile> _characters;
    private readonly IRepository<Article> _articles;
    private readonly IRepository<MerchandiseItem> _merch;
    private readonly IRepository<MediaItem> _media;

    public SupportService(IRepository<Feedback> feedbacks,
                          IRepository<FanSubmission> submissions,
                          IRepository<Bookmark> bookmarks,
                          IRepository<Content> contents,
                          IRepository<CharacterProfile> characters,
                          IRepository<Article> articles,
                          IRepository<MerchandiseItem> merch,
                          IRepository<MediaItem> media)
    {
        _feedbacks = feedbacks;
        _submissions = submissions;
        _bookmarks = bookmarks;
        _contents = contents;
        _characters = characters;
        _articles = articles;
        _merch = merch;
        _media = media;
    }

    public async Task SaveFeedbackAsync(FeedbackFormViewModel form, ApplicationUser? user)
    {
        // Visitors leave an e-mail for replies - fold it into the message (Feedback has no e-mail column)
        var message = string.IsNullOrWhiteSpace(form.ContactEmail)
            ? form.Message
            : $"[Reply to: {form.ContactEmail}] {form.Message}";

        await _feedbacks.AddAsync(new Feedback
        {
            UserId = user?.Id,
            Type = form.Type,
            Message = message,
            Status = FeedbackStatus.Open,
            CreatedAt = DateTime.UtcNow
        });
        await _feedbacks.SaveChangesAsync();
    }

    public async Task SubmitFanArtAsync(FanSubmissionFormViewModel form, string userId, string? imageUrl)
    {
        await _submissions.AddAsync(new FanSubmission
        {
            UserId = userId,
            Title = form.Title,
            Body = form.Body,
            ImageUrl = imageUrl,
            Status = SubmissionStatus.Pending, // ALWAYS starts pending - never trust the client
            CreatedAt = DateTime.UtcNow
        });
        await _submissions.SaveChangesAsync();
    }

    public async Task<MyBookmarksViewModel> GetBookmarksAsync(string userId)
    {
        var marks = await _bookmarks.Query()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var vm = new MyBookmarksViewModel();

        foreach (var group in marks.GroupBy(b => b.ItemType))
        {
            var g = new BookmarkGroupViewModel { Type = group.Key };

            foreach (var mark in group)
            {
                var entry = new BookmarkEntryViewModel
                {
                    BookmarkId = mark.BookmarkId,
                    ItemId = mark.ItemId,
                    Note = mark.Note,
                    CreatedAt = mark.CreatedAt
                };

                switch (group.Key)
                {
                    case BookmarkType.Content:
                        var c = await _contents.GetByIdAsync(mark.ItemId);
                        if (c is null) continue;
                        entry.Title = c.Title;
                        entry.Subtitle = c.Category?.Name;
                        entry.LinkUrl = $"/Explore/Details/{c.ContentId}";
                        break;

                    case BookmarkType.Article:
                        var a = await _articles.GetByIdAsync(mark.ItemId);
                        if (a is null) continue;
                        entry.Title = a.Title;
                        entry.Subtitle = a.Category?.Name;
                        entry.LinkUrl = $"/News/Details/{a.ArticleId}";
                        break;

                    case BookmarkType.Character:
                        var ch = await _characters.GetByIdAsync(mark.ItemId);
                        if (ch is null) continue;
                        entry.Title = ch.Name;
                        entry.Subtitle = ch.Fandom ?? ch.Category?.Name;
                        entry.LinkUrl = $"/Characters/Details/{ch.CharacterId}";
                        break;

                    case BookmarkType.Merchandise:
                        var m = await _merch.GetByIdAsync(mark.ItemId);
                        if (m is null) continue;
                        entry.Title = m.Name;
                        entry.Subtitle = m.Category?.Name;
                        entry.LinkUrl = $"/Merch/Details/{m.ItemId}";
                        break;

                    case BookmarkType.Video:
                        var med = await _media.GetByIdAsync(mark.ItemId);
                        if (med is null) continue;
                        entry.Title = med.Content?.Title ?? med.Tag ?? "Video";
                        entry.Subtitle = med.MediaType.ToString();
                        entry.LinkUrl = $"/Explore/Details/{med.ContentId}";
                        break;
                }

                g.Items.Add(entry);
            }

            if (g.Items.Count > 0) vm.Groups.Add(g);
        }

        return vm;
    }
}
