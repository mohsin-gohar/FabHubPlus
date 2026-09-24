using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FanHubPlus.Services;

/// <summary>
/// Secure image upload: extension + MIME + size whitelist, randomised file names
/// (prevents overwriting / path traversal), atomic move into wwwroot/uploads/{subFolder}.
/// </summary>
public class FileUploadService : IFileUploadService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private static readonly string[] AllowedContentTypes =
        { "image/jpeg", "image/png", "image/webp", "image/gif" };
    private const long MaxBytes = 2 * 1024 * 1000; // 2 MB

    private readonly IWebHostEnvironment _env;

    public FileUploadService(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveImageAsync(IFormFile? file, string subFolder)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("Please choose an image file.");

        if (file.Length > MaxBytes)
            throw new InvalidOperationException("Image is too large (maximum 2 MB).");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("Only jpg, jpeg, png, webp and gif images are allowed.");

        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new InvalidOperationException("File content type is not an allowed image type.");

        // Sanitise the sub-folder name (defense in depth against path traversal)
        subFolder = new string(subFolder.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
        if (string.IsNullOrEmpty(subFolder)) subFolder = "misc";

        var folderAbs = Path.Combine(_env.WebRootPath, "uploads", subFolder);
        Directory.CreateDirectory(folderAbs);

        var fileName = $"{Guid.NewGuid():N}{ext}"; // collision-proof random name
        var fullPath = Path.Combine(folderAbs, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{subFolder}/{fileName}";
    }

    public void DeleteImage(string? webPath)
    {
        if (string.IsNullOrWhiteSpace(webPath) || !webPath.StartsWith("/uploads/", StringComparison.Ordinal))
            return; // never delete outside the uploads folder

        var uploadsRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));
        var target = Path.GetFullPath(Path.Combine(_env.WebRootPath,
            webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

        if (!target.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)) return;
        if (File.Exists(target)) File.Delete(target);
    }
}
