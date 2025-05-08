using GraduationProject.Helpers;

namespace GraduationProject.Services;

public interface IMailService
{
    Task<bool> SendEmailAsync(EmailMessage emailMessage);
}