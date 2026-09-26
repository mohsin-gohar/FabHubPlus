using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Data;

/// <summary>
/// Runs automatically on app startup (called from Program.cs).
/// It only inserts data that is MISSING, so it is safe to run many times.
/// Seeds: 2 roles, 1 admin, 1 demo user, the 8 fandom categories and the demo
/// library (cover art, a playable trailer per title, fan art, ratings, a saved
/// list) so no public page ever renders empty.
/// </summary>
public class DbSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public DbSeeder(RoleManager<IdentityRole> roleManager,
                    UserManager<ApplicationUser> userManager,
                    ApplicationDbContext db)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
    }

    public async Task SeedAsync()
    {
        // ---------- 1) Roles: Admin and User ----------
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }

        // ---------- 2) Admin account (for the Admin panel) ----------
        await EnsureUserAsync("admin@fanhubplus.com", "Admin@123", "Site Admin", "Admin");

        // ---------- 3) Demo normal user (for testing + demo video) ----------
        await EnsureUserAsync("user@fanhubplus.com", "User@123", "Demo Fan", "User");

        // ---------- 4) The 8 fandom categories ----------
        if (!_db.Categories.Any())
        {
            _db.Categories.AddRange(
                new Category { Name = "Anime",    Description = "Japanese animation - series, movies and OVAs." },
                new Category { Name = "Gaming",   Description = "Video games, esports and game franchises." },
                new Category { Name = "Movies",   Description = "Cinematic universes, franchises and blockbusters." },
                new Category { Name = "TV Shows", Description = "Binge-worthy television series and dramas." },
                new Category { Name = "K-Pop",    Description = "Korean pop groups, albums and comebacks." },
                new Category { Name = "Comics",   Description = "Western comics, superheroes and graphic novels." },
                new Category { Name = "Manga",    Description = "Japanese manga series and light novels." },
                new Category { Name = "Cosplay",  Description = "Cosplay culture, costumes and conventions." }
            );
            await _db.SaveChangesAsync();
        }

        // ---------- 5) Demo library data (covers, trailers, fan art, ratings, saved list) ----------
        await SeedLibraryDataAsync();
    }

    /// <summary>
    /// Closes the gaps a fresh checkout starts with: every title gets cover art
    /// and something to play, and the pages that would otherwise be empty (fan
    /// art gallery, rating widget, My Bookmarks) get demo rows.
    /// Every block only fills what is MISSING, so an admin can replace any of it
    /// later and the seeder will not put it back.
    /// </summary>
    private async Task SeedLibraryDataAsync()
    {
        var fan = await _userManager.FindByEmailAsync("user@fanhubplus.com");
        if (fan is null) return; // no demo user => nothing to attach rows to

        // ---- a) Cover art for titles that were saved without a thumbnail ----
        var contents = await _db.Contents.ToListAsync();
        foreach (var content in contents)
        {
            if (!string.IsNullOrWhiteSpace(content.ThumbnailUrl)) continue;

            content.ThumbnailUrl = content.Type switch
            {
                ContentType.Movie => $"/assets/images/movies/movie{(content.ContentId % 24) + 1}.jpg",
                ContentType.Series => $"/assets/images/series/series{(content.ContentId % 6) + 1}.jpg",
                _ => $"/assets/images/featured-movies/featured-movie{(content.ContentId % 6) + 1}.jpg"
            };
        }
        await _db.SaveChangesAsync();

        // ---- b) A playable trailer for every title that has no media row ----
        // The clip is hosted on this site, so the detail-page player and the
        // in-page modal work without ever sending the visitor away. An admin can
        // swap the row for a real trailer link from the Media admin screen.
        var withoutMedia = await _db.Contents
            .Where(c => !c.MediaItems.Any())
            .ToListAsync();

        foreach (var content in withoutMedia)
        {
            _db.MediaItems.Add(new MediaItem
            {
                Content = content,
                MediaType = MediaType.Trailer,
                EmbedUrl = "/assets/videos/movie.mp4",
                Tag = "Official Trailer"
            });
        }

        // ---- c) Fan art, so the community gallery is not a blank wall ----
        if (!await _db.FanSubmissions.AnyAsync())
        {
            var pieces = new (string Title, string Body, string Image)[]
            {
                ("Levi study — ink and wash", "Two hours on the cloak folds, then a very nervous eraser pass. Feedback welcome!",
                    "/assets/images/coverage/coverage1.jpg"),
                ("Elden Ring map sketch", "Redrew the Lands Between from memory after 40 hours of map hoovering.",
                    "/assets/images/coverage/coverage2.jpg"),
                ("Spirited Away — No-Face study", "Watercolour, gold leaf highlights and a lot of patience with the eyes.",
                    "/assets/images/coverage/coverage3.jpg"),
                ("K-Pop comeback stage look", "Costume breakdown from the last concert — which member is who?",
                    "/assets/images/coverage/coverage4.jpg"),
                ("Mecha turnaround sheet", "Six angles to practise the panel lines before the next convention build.",
                    "/assets/images/coverage/coverage5.jpg"),
                ("Comic page — rooftop scene", "Practice page for a superhero series pitch. The inks are still drying.",
                    "/assets/images/coverage/coverage6.jpg")
            };

            var day = 1;
            foreach (var piece in pieces)
            {
                _db.FanSubmissions.Add(new FanSubmission
                {
                    User = fan,
                    Title = piece.Title,
                    Body = piece.Body,
                    ImageUrl = piece.Image,
                    Status = SubmissionStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-day++)
                });
            }
        }

        // ---- d) A few ratings, so the star widget on a detail page has data ----
        if (!await _db.Ratings.AnyAsync())
        {
            var votes = new (int ContentId, int Stars)[] { (1, 5), (2, 5), (3, 4), (4, 4), (6, 5), (7, 5) };

            foreach (var (contentId, stars) in votes)
            {
                var content = contents.FirstOrDefault(c => c.ContentId == contentId);
                if (content is null) continue;

                _db.Ratings.Add(new Rating
                {
                    User = fan,
                    Content = content,
                    Stars = stars,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                });
            }
        }

        // ---- e) A started "My Bookmarks" list for the demo account ----
        if (!await _db.Bookmarks.AnyAsync())
        {
            var article = await _db.Articles.OrderBy(a => a.PublishedAt).FirstOrDefaultAsync();
            var character = await _db.CharacterProfiles.OrderBy(c => c.CharacterId).FirstOrDefaultAsync();

            void Save(BookmarkType type, int? itemId, string? note)
            {
                if (itemId is null) return;

                _db.Bookmarks.Add(new Bookmark
                {
                    User = fan,
                    ItemType = type,
                    ItemId = itemId.Value,
                    Note = note,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                });
            }

            Save(BookmarkType.Content, contents.FirstOrDefault(c => c.ContentId == 1)?.ContentId, "Best fight choreography of the decade.");
            Save(BookmarkType.Content, contents.FirstOrDefault(c => c.ContentId == 7)?.ContentId, "Rewatch before the sequel drops.");
            Save(BookmarkType.Article, article?.ArticleId, "Send this one to the group chat.");
            Save(BookmarkType.Character, character?.CharacterId, "Cosplay list starter.");
        }

        await _db.SaveChangesAsync();

    }

    /// <summary>
    /// Creates a user with password + role only if the email is not registered yet.
    /// Passwords satisfy the Identity rules in Program.cs (8 chars, upper, lower, digit, symbol).
    /// </summary>
    private async Task EnsureUserAsync(string email, string password, string name, string role)
    {
        if (await _userManager.FindByEmailAsync(email) != null)
            return; // already exists

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Name = name,
            EmailConfirmed = true // seed accounts skip email confirmation (easy demo login)
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not seed user {email}: {errors}");
        }
    }
}
