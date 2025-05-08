namespace GraduationProject.Services;

public interface IFileService
{
    Task<string?> UploadFileAsync(IFormFile file);
    bool DeleteFileFromPath(string filePath);
}