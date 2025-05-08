using System.Text;
using GraduationProject.Bases;
using GraduationProject.Data;
using GraduationProject.DTOs;
using GraduationProject.Helpers;
using GraduationProject.Models;
using GraduationProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    UserManager<AppUser> userManager,
    ITokenService tokenService,
    ApplicationDbContext context,
    IMailService mailService,
    IFileService fileService) : ControllerBase
{
    [HttpPost("Register")]
    [ProducesResponseType(typeof(BaseResponse<string>), 200)]
    [ProducesResponseType(typeof(BaseResponse<string>), 400)]
    public async Task<IActionResult> RegisterAsync([FromForm] RegisterRequestDto requestDto)
    {
        if (!ModelState.IsValid)
            return StatusCode(422, ModelState);

        var user = new AppUser
        {
            FirstName = requestDto.FirstName,
            LastName = requestDto.LastName,
            UserName = requestDto.Email,
            Email = requestDto.Email,
            DateOfBirth = requestDto.DateOfBirth,
            EmailConfirmed = false
        };

        if (requestDto.Image != null)
        {
            var isValid = ImageValidator.IsAValidImageFile(requestDto.Image);
            if (!isValid)
                return BadRequest(new BaseResponse<string>("Not a valid image file."));
            
            user.ImageName = await fileService.UploadFileAsync(requestDto.Image);
        }

        var identityResult = await userManager.CreateAsync(user, requestDto.Password);

        if (identityResult.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "User");
               
            return Ok(new BaseResponse<string>(user.Id,  "Registration completed successfully."));
        }
        var errors = identityResult.Errors.Select(e => $"{e.Code}: {e.Description}").ToList();
        return BadRequest(new BaseResponse<string>(errors));
    }
        
    [HttpPost("Login")]
    [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), 400)]
    [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), 401)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto requestDto)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, ModelState);
        
        var existedUser = await userManager.FindByEmailAsync(requestDto.Email);
        if (existedUser is null)
            return NotFound(new BaseResponse<LoginResponseDto>("User not found."));
            
        if (!await userManager.IsEmailConfirmedAsync(existedUser))
            return BadRequest(new BaseResponse<LoginResponseDto>("Email is not confirmed."));
            
        if (!await userManager.CheckPasswordAsync(existedUser, requestDto.Password))
            return Unauthorized(new BaseResponse<LoginResponseDto>("Invalid credentials."));
            
        var token = await tokenService.GenerateJwtTokenAsync(existedUser);
        var userRoles = await userManager.GetRolesAsync(existedUser);
            
        var loginResponse = new LoginResponseDto
        {
            Email = existedUser.Email!,
            UserName = existedUser.UserName!,
            Roles = userRoles.ToList(),
            Token = token
        };
            
        return Ok(new BaseResponse<LoginResponseDto>(loginResponse));
    }

    [HttpPost("SendConfirmEmailCode")]
    [ProducesResponseType(typeof(BaseResponse<string>), 200)]
    [ProducesResponseType(typeof(BaseResponse<string>), 404)]
    [ProducesResponseType(typeof(BaseResponse<string>), 400)]
    [ProducesResponseType(422)]
    [ProducesResponseType(typeof(BaseResponse<string>), 409)]
    public async Task<IActionResult> SendConfirmEmailCodeAsync([FromBody] SendConfirmEmailRequestDto requestDto)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, ModelState);
        
        var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var user = await userManager.FindByEmailAsync(requestDto.Email);
            if (user is null)
                return NotFound(new BaseResponse<string>("User not found."));
            
            if (await userManager.IsEmailConfirmedAsync(user))
                return Conflict(new BaseResponse<string>("Email is confirmed yet."));
            
            var authenticationCode = await userManager.GenerateUserTokenAsync(user, "Email", "AuthenticationCode");
            var encodedCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationCode));
            
            user.Code =  encodedCode;

            user.CodeExpiration = DateTime.Now.AddMinutes(5);
            
            var identityResult = await userManager.UpdateAsync(user);

            if (!identityResult.Succeeded)
            {
                var errors = identityResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(new BaseResponse<string>(errors));
            }

            var emailMessage = new EmailMessage
            {
                To = requestDto.Email,
                Subject = "Activate Account",
                Content =
                    $"Thank you for registering with us. To activate your account, please use the following code:" +
                    $"{authenticationCode}, This code will expire in 5 minutes. "
            };

            await mailService.SendEmailAsync(emailMessage);
            
            await transaction.CommitAsync();
            return Ok(new BaseResponse<string>(user.Code,  "The activate email code sent to your email successfully, check your inbox."));
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return BadRequest(new BaseResponse<string>(e.Message));
        }
    }

    [HttpPost("ConfirmEmail")]
    [ProducesResponseType(typeof(BaseResponse<string>), 200)]
    [ProducesResponseType(typeof(BaseResponse<string>), 404)]
    [ProducesResponseType(typeof(BaseResponse<string>), 400)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequestDto requestDto)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, ModelState);
        
        var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var user = await userManager.FindByEmailAsync(requestDto.Email);
            if (user is null)
                return NotFound(new BaseResponse<string>("User not found."));
            
            var decodedAuthenticationCode = Encoding.UTF8.GetString(Convert.FromBase64String(user.Code!));

            if (decodedAuthenticationCode != requestDto.Code)
                return BadRequest(new BaseResponse<string>("Invalid authentication code."));
            
            if (DateTime.Now > user.CodeExpiration)
                return BadRequest(new BaseResponse<string>("Code is expired."));

            user.EmailConfirmed = true;
            user.Code = null;
            user.CodeExpiration = null;
            var identityResult = await userManager.UpdateAsync(user);

            if (!identityResult.Succeeded)
            {
                var errors = identityResult.Errors
                    .Select(e => e.Description)
                    .ToList();

                return BadRequest(new BaseResponse<string>(errors));
            }

            var emailMessage = new EmailMessage()
            {
                To = requestDto.Email,
                Subject = "Email Confirmation",
                Content = "Congratulations, Your Email is Confirmed!"
            };
            
            await mailService.SendEmailAsync(emailMessage);
            
            await transaction.CommitAsync();
            return Ok(new BaseResponse<string>("Email has been confirmed."));

        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return BadRequest(new BaseResponse<string>(e.Message));
        }
    }

    [HttpPost("ForgetPassword")]
    [ProducesResponseType(typeof(BaseResponse<string>), 200)]
    [ProducesResponseType(typeof(BaseResponse<string>), 404)]
    [ProducesResponseType(typeof(BaseResponse<string>), 400)]
    [ProducesResponseType(422)]
    [ProducesResponseType(typeof(BaseResponse<string>), 409)]
    public async Task<IActionResult> ForgetPasswordAsync([FromBody] ForgetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, ModelState);
        
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return NotFound(new BaseResponse<string>("User not found."));
        
        if (!await userManager.IsEmailConfirmedAsync(user))
            return Conflict(new BaseResponse<string>("Email is confirmed yet."));
        
        var decoded = await userManager.GenerateUserTokenAsync(user, "Email", "Generate Code");
        var authCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(decoded));
        user.Code = authCode;
        user.CodeExpiration = DateTime.Now.AddMinutes(5);
        
        var identityResult = await userManager.UpdateAsync(user);

        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => e.Description).ToList();
            return BadRequest(new BaseResponse<string>(errors));
        }
        
        var emailMessage = new EmailMessage
        {
            To = request.Email,
            Subject = "Reset Password Code",
            Content = $"Password reset code: {decoded}, this code will expire in 5 minutes"
        };
        
        await mailService.SendEmailAsync(emailMessage);
        
        return Ok(new BaseResponse<string>("Password reset code has been sent to your email."));
    }

    [HttpPost("ResetPassword")]
    [ProducesResponseType(typeof(BaseResponse<string>), 200)]
    [ProducesResponseType(typeof(BaseResponse<string>), 404)]
    [ProducesResponseType(typeof(BaseResponse<string>), 400)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, ModelState);
        
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return NotFound(new BaseResponse<string>("User not found."));
        
        var decodedAuthCode = Encoding.UTF8.GetString(Convert.FromBase64String(user.Code!));

        if (decodedAuthCode != request.Code)
            return BadRequest(new BaseResponse<string>("Invalid authentication code."));
        
        if (DateTimeOffset.UtcNow > user.CodeExpiration)
           return BadRequest(new BaseResponse<string>("Code is expired."));
        
        await userManager.RemovePasswordAsync(user);
        var result = await userManager.AddPasswordAsync(user, request.NewPassword);

        return result.Succeeded ? 
            Ok(new BaseResponse<string>("Password has been reset.")) :
            StatusCode(StatusCodes.Status422UnprocessableEntity);
    }
        
}