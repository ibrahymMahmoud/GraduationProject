namespace GraduationProject.Helpers;

public class GmailSettings
{
    public string SenderEmail { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public string Host { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int Port { get; set; }
}