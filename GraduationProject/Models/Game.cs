namespace GraduationProject.Models;

public class Game
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Score> Scores { get; set; } = new List<Score>();
}