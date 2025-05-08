using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs;

public class SendConfirmEmailRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = null!;
}