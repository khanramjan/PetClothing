using Microsoft.Extensions.Configuration;
using PetClothingShop.Core.Interfaces;

namespace PetClothingShop.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _uploadPath;

    public FileService(IConfiguration configuration)
    {
        var uploadsRelativePath = configuration["FileUpload:Path"] ?? "wwwroot/uploads";
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsRelativePath);
        
        // Create upload directory if it doesn't exist
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder)
    {
        var folderPath = Path.Combine(_uploadPath, folder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        using (var fileStreamDestination = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamDestination);
        }

        return $"/{folder}/{uniqueFileName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var filePath = Path.Combine(_uploadPath, fileUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
