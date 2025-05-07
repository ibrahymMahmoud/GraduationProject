using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace GraduationProject.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [MinLength(3, ErrorMessage = "The Minimum lenght for FirstName is  3 characters.")]
        [MaxLength(50, ErrorMessage = "The Maximum lenght for FirstName is  50 characters.")]
        public string FirstName { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "The Minimum lenght for LastName is  3 characters.")]
        [MaxLength(50, ErrorMessage = "The Maximum lenght for LastName is  50 characters.")]
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords not matched.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
    }
}
