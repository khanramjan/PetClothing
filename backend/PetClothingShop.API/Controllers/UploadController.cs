using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClothingShop.Core.Interfaces;

namespace PetClothingShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(IFileService fileService, ILogger<UploadController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpPost("image")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Only images are allowed." });
            }

            // Validate file size (10MB max)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { message = "File size exceeds 10MB limit." });
            }

            // Upload the file
            using var stream = file.OpenReadStream();
            var fileUrl = await _fileService.UploadFileAsync(stream, file.FileName, "products");

            // Return the full URL path
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var fullUrl = $"{baseUrl}/uploads{fileUrl}";

            return Ok(new { url = fullUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, new { message = "An error occurred while uploading the image." });
        }
    }

    [HttpDelete("image")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage([FromQuery] string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest(new { message = "URL is required" });
            }

            // Extract the relative path from the full URL
            var uri = new Uri(url);
            var relativePath = uri.AbsolutePath.Replace("/uploads", "");

            var deleted = await _fileService.DeleteFileAsync(relativePath);

            if (deleted)
            {
                return Ok(new { message = "Image deleted successfully" });
            }
            
            return NotFound(new { message = "Image not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image");
            return StatusCode(500, new { message = "An error occurred while deleting the image." });
        }
    }
}
