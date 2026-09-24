namespace FanHubPlus.Models.ViewModels;

// One crumb for the shared _Breadcrumbs partial
public class BreadcrumbItem
{
    public BreadcrumbItem(string text, string? url = null)
    {
        Text = text;
        Url = url;
    }

    public string Text { get; }
    public string? Url { get; }   // null => current page (no link)
}
