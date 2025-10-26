namespace PetClothingShop.Core.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(int userId, string email, string role);
    string GenerateRefreshToken();
    int? ValidateToken(string token);
}

public interface IFileService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
    Task<bool> DeleteFileAsync(string fileUrl);
}
