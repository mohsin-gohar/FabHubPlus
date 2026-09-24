using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FanHubPlus.Services;

public interface IFileUploadService
{
    /// <summary>
    /// Validates an uploaded image and stores it under wwwroot/uploads/{subFolder}.
    /// Returns the web path (e.g. /uploads/avatars/xxx.jpg) or null when file is empty.
    /// Throws InvalidOperationException with a user-friendly message when invalid.
    /// </summary>
    Task<string> SaveImageAsync(IFormFile? file, string subFolder);

    // Deletes a previously uploaded file (only inside wwwroot/uploads).
    void DeleteImage(string? webPath);
}
