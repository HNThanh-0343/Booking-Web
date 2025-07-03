using System;
using System.Threading.Tasks;

namespace WEBSITE_TRAVELBOOKING.Infrastructure
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
    }
} 