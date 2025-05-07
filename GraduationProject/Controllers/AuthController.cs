using GraduationProject.DTOs;
using GraduationProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(UserManager<AppUser> userManager) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var user = new AppUser
            {
                FirstName = requestDto.FirstName,
                LastName = requestDto.LastName,
                Email = requestDto.Email,
                DateOfBirth = requestDto.DateOfBirth,
            };

           var identityResult = await userManager.CreateAsync(user, requestDto.Password);

            if (!identityResult.Succeeded)
            {
                var errors = identityResult.Errors.Select(e => $"{e.Code}: {e.Description}").ToList();
                return BadRequest(errors);
            }

            return Ok(new { Success = true, Message = "Registeration completed successfully." }); 

        }
    }
}
