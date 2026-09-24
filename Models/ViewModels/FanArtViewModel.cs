using System.ComponentModel.DataAnnotations;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.ViewModels;

// Fan Art gallery (index): approved submissions + the submit form model
public class FanArtViewModel
{
    public List<FanSubmission> Approved { get; set; } = new();
    public FanSubmissionFormViewModel Form { get; set; } = new();
    public List<FanSubmission> MySubmissions { get; set; } = new(); // shown when logged in
}

// The actual submit form (validations + optional image upload)
public class FanSubmissionFormViewModel
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(8000)]
    [Display(Name = "Your fan story / description")]
    public string Body { get; set; } = string.Empty;

    [Display(Name = "Optional image (jpg/png/webp, max 2 MB)")]
    public IFormFile? ImageFile { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending; // set server-side only
}
