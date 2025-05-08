using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs;

public class ConfirmEmailRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "Code is required.")]
    public string Code { get; set; } = null!;
}