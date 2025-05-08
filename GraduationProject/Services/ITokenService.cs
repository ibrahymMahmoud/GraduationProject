using GraduationProject.Models;

namespace GraduationProject.Services;

public interface ITokenService
{
    Task<string> GenerateJwtTokenAsync(AppUser user);
}