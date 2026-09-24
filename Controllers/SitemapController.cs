using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FanHubPlus.Controllers;

// Sitemap: human-readable HTML map + machine-readable /Sitemap/Xml for crawlers
public class SitemapController : Controller
{
    private readonly IRepository<Content> _contents;
    private readonly IRepository<CharacterProfile> _characters;
    private readonly IRepository<Article> _articles;
    private readonly IRepository<MerchandiseItem> _merch;
    private readonly IRepository<EventItem> _events;

    public SitemapController(IRepository<Content> contents,
                             IRepository<CharacterProfile> characters,
                             IRepository<Article> articles,
                             IRepository<MerchandiseItem> merch,
                             IRepository<EventItem> events)
    {
        _contents = contents;
        _characters = characters;
        _articles = articles;
        _merch = merch;
        _events = events;
    }

    // GET /Sitemap - human-readable page with DB-driven links
    public async Task<IActionResult> Index()
    {
        var vm = new SitemapViewModel
        {
            Sections = new List<SitemapSection>
            {
                new()
                {
                    Title = "Main",
                    Url = "/",
                    Children = new List<SitemapLink>
                    {
                        new() { Title = "Home", Url = "/" },
                        new() { Title = "Explore", Url = "/Explore" },
                        new() { Title = "Characters", Url = "/Characters" },
                        new() { Title = "News & Timeline", Url = "/News" },
                        new() { Title = "Events", Url = "/Events" },
                        new() { Title = "Merchandise", Url = "/Merch" },
                        new() { Title = "Fan Art Gallery", Url = "/FanArt" },
                        new() { Title = "Support & FAQ", Url = "/Support" },
                        new() { Title = "Chatbot", Url = "/Chatbot" },
                        new() { Title = "XML Sitemap", Url = "/Sitemap/Xml" }
                    }
                },
                new()
                {
                    Title = "Content",
                    Url = "/Explore",
                    Children = await _contents.Query()
                        .OrderBy(c => c.Title)
                        .Select(c => new SitemapLink { Title = c.Title, Url = "/Explore/Details/" + c.ContentId })
                        .ToListAsync()
                },
                new()
                {
                    Title = "Characters",
                    Url = "/Characters",
                    Children = await _characters.Query()
                        .OrderBy(c => c.Name)
                        .Select(c => new SitemapLink { Title = c.Name, Url = "/Characters/Details/" + c.CharacterId })
                        .ToListAsync()
                },
                new()
                {
                    Title = "News & Timeline",
                    Url = "/News",
                    Children = await _articles.Query()
                        .OrderByDescending(a => a.PublishedAt)
                        .Select(a => new SitemapLink { Title = a.Title, Url = "/News/Details/" + a.ArticleId })
                        .ToListAsync()
                },
                new()
                {
                    Title = "Merchandise",
                    Url = "/Merch",
                    Children = await _merch.Query()
                        .OrderBy(m => m.Name)
                        .Select(m => new SitemapLink { Title = m.Name, Url = "/Merch/Details/" + m.ItemId })
                        .ToListAsync()
                },
                new()
                {
                    Title = "Events",
                    Url = "/Events",
                    Children = await _events.Query()
                        .OrderBy(e => e.EventDate)
                        .Select(e => new SitemapLink { Title = e.Title + " (" + e.City + ")", Url = "/Events" })
                        .ToListAsync()
                }
            }
        };

        return View(vm);
    }

    // GET /Sitemap/Xml - standard sitemap.xml consumed by search engines
    [HttpGet]
    public async Task<IActionResult> Xml()
    {
        var baseUri = $"{Request.Scheme}://{Request.Host}";
        var urls = new List<string>
        {
            "/", "/Explore", "/Characters", "/News", "/Events", "/Merch",
            "/FanArt", "/Support", "/Chatbot", "/Account/Login", "/Account/Register"
        };

        var dynamicUrls = new List<string>();
        dynamicUrls.AddRange(await _contents.Query()
            .Select(c => "/Explore/Details/" + c.ContentId).ToListAsync());
        dynamicUrls.AddRange(await _characters.Query()
            .Select(c => "/Characters/Details/" + c.CharacterId).ToListAsync());
        dynamicUrls.AddRange(await _articles.Query()
            .Select(a => "/News/Details/" + a.ArticleId).ToListAsync());
        dynamicUrls.AddRange(await _merch.Query()
            .Select(m => "/Merch/Details/" + m.ItemId).ToListAsync());

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        foreach (var path in urls.Concat(dynamicUrls))
            sb.AppendLine($"  <url><loc>{baseUri}{path}</loc></url>");
        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }
}
