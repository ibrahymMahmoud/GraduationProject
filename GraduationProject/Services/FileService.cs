namespace GraduationProject.Services;

public class FileService : IFileService
{
    public async Task<string?> UploadFileAsync(IFormFile file)
    {
        if (file.Length == 0)
            return null;
            
        var uniqueFileName = $"{DateTimeOffset.Now:yyyyMMdd_HHmmssfff}_{file.FileName}";

        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var filePathToCreate = Path.Combine(directoryPath, uniqueFileName);

        await using var fileStream = new FileStream(filePathToCreate, FileMode.CreateNew);
        await file.CopyToAsync(fileStream);

        return uniqueFileName;
    }
    
    public bool DeleteFileFromPath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        var resourcePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "Uploads",
            filePath);

        if (!File.Exists(resourcePath)) return false;

        File.Delete(resourcePath);

        return true;

    }
}