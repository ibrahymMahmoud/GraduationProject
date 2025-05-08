namespace GraduationProject.DTOs;

public class UserGameScoreDto
{
    public string GameName { get; set; } = null!;
    public int Score { get; set; }
    public DateTime DateAchieved { get; set; }
}