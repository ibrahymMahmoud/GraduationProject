using System.Security.Claims;
using GraduationProject.Bases;
using GraduationProject.DTOs;
using GraduationProject.Helpers;
using GraduationProject.Models;
using GraduationProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    IConfiguration config,
    UserManager<AppUser> userManager,
    IFileService fileService) : ControllerBase
{
    [Authorize]
    [HttpGet("Profile")]
    [ProducesResponseType(typeof(BaseResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserProfileAsync()
    {
        var userId = HttpContext.User.FindFirstValue("uid");
        var user = await userManager.FindByIdAsync(userId!);
        
        if (user == null)
            return Unauthorized();

        var result = new UserProfileDto
        {
            Name = string.Concat(user.FirstName, " ", user.LastName),
            DateOfBirth = user.DateOfBirth,
            Email = user.Email!,
            UserName = user.UserName!,
            PhotoUrl = !string.IsNullOrEmpty(user.ImageName) ? $"{config["ApiBaseUrl"]}/Uploads/{user.ImageName}" :null
        };
        
        return Ok(new BaseResponse<UserProfileDto>(result));
    }

    [HttpPost("UploadImage")]
    [ProducesResponseType(typeof(BaseResponse<UserProfileDto>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<UserProfileDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BaseResponse<UserProfileDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImageAsync([FromForm] UploadImageRequestDto request)
    {
        var isValid = ImageValidator.IsAValidImageFile(request.Image);
        if (!isValid) return BadRequest(new BaseResponse<string>("Not a valid image file format."));
        
        var userId = HttpContext.User.FindFirstValue("uid");
        var user = await userManager.FindByIdAsync(userId!);
        
        if (user == null)
            return Unauthorized();

        if (user.ImageName != null)
            fileService.DeleteFileFromPath(user.ImageName);
        
        user.ImageName = await fileService.UploadFileAsync(request.Image);

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description).ToList();
            return BadRequest(new BaseResponse<UserProfileDto>(errors));
        } 
        
        var result = new UserProfileDto
        {
            Name = string.Concat(user.FirstName, " ", user.LastName),
            DateOfBirth = user.DateOfBirth,
            Email = user.Email!,
            UserName = user.UserName!,
            PhotoUrl = !string.IsNullOrEmpty(user.ImageName) ? $"{config["ApiBaseUrl"]}/Uploads/{user.ImageName}" :null
        };

        
        return Ok(new BaseResponse<UserProfileDto>(result, "Image uploaded successfully."));

    }
}