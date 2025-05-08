using Microsoft.AspNetCore.Identity;

namespace GraduationProject.Models;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string? Code { get; set; }
    public DateTime? CodeExpiration { get; set; }
    public string? ImageName { get; set; }
}