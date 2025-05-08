namespace GraduationProject.Models;

public class Score
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public AppUser User { get; set; } = null!;

    public int Value { get; set; }
    public DateTime DateAchieved { get; set; } = DateTime.Now;
}