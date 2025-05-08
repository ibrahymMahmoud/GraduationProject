using System.Security.Claims;
using GraduationProject.Bases;
using GraduationProject.Data;
using GraduationProject.DTOs;
using GraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(
    ApplicationDbContext context,
    UserManager<AppUser> userManager) : ControllerBase
{
    [Authorize]
    [HttpPost("SubmitScore")]
    public async Task<IActionResult> SubmitScore([FromBody] ScoreRequestDto request)
    {
        var userId = HttpContext.User.FindFirstValue("uid");
        var user = await userManager.FindByIdAsync(userId!);
        
        if (user == null)
            return Unauthorized();

        var game = await context.Games.FirstOrDefaultAsync(g => g.Name == request.GameName);
        if (game == null)
        {
            game = new Game { Id = Guid.NewGuid(), Name = request.GameName };
            context.Games.Add(game);
            await context.SaveChangesAsync();
        }

        var score = new Score
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            UserId = user.Id,
            Value = request.Score
        };

        context.GameScores.Add(score);
        await context.SaveChangesAsync();

        return Ok(new BaseResponse<string>() { Succeeded = true ,Message = "Score submitted successfully" });
    }
    
    [Authorize]
    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<UserGameScoreDto>>> GetUserGameScores()
    {
        var userId = HttpContext.User.FindFirstValue("uid");
        var user = await userManager.FindByIdAsync(userId!);
        
        if (user == null)
            return Unauthorized();

        var userScores = await context.GameScores
            .Include(s => s.Game)
            .Where(s => s.UserId == user.Id)
            .OrderByDescending(s => s.DateAchieved)
            .Select(s => new UserGameScoreDto
            {
                GameName = s.Game.Name,
                Score = s.Value,
                DateAchieved = s.DateAchieved
            })
            .ToListAsync();


        return Ok(new BaseResponse<IEnumerable<UserGameScoreDto>>(userScores));
    }

    
}