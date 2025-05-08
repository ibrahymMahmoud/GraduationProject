namespace GraduationProject.Helpers;

public static class ImageValidator
{
    public static bool IsAValidImageFile(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        return FileFormats.AllowedImageFormats.Contains(extension);
    }
}