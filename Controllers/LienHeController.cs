using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using System.Net.Mail;
using System.Net;
using System.ComponentModel;

namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class LienHeController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public LienHeController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public IActionResult Index()
        {
            var lienhe = _unitOfWork.Repository<SysContact>().GetAll(filter: (m => m.Status == true));
            return View(lienhe);
        }

        [HttpPost]
        public IActionResult LienHeUser(string name, string email, string subject, string message)
        {
            var lienhe = _unitOfWork.Repository<SysContact>().GetAll(filter: (m => m.Status == true));
            try
            {
                
                var toEmail = "fifa123minh@gmail.com";
                var emailSubject = $"[Liên hệ]: {subject}";
                var emailBody = $@"
                <p>Bạn nhận được một tin nhắn liên hệ mới từ trang web:</p>
                <p><strong>Tên:</strong> {name}</p>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Chủ đề:</strong> {subject}</p>
                <p><strong>Nội dung:</strong></p>
                <p>{message}</p>";

                EmailSender.Send(toEmail, emailSubject, emailBody);

                ViewData["Success"] = "Gửi liên hệ thành công! Chúng tôi sẽ phản hồi sớm nhất.";
            }
            catch (Exception ex)
            {
                ViewData["Failed"] = "Gửi liên hệ thất bại. Vui lòng thử lại.";
            }
            return View("Index", lienhe);
        }

        public static class EmailSender
        {
            public static void Send(string to, string subject, string body)
            {
                var smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("fifa123minh@gmail.com", "jkox tjkr alol rlmn"),
                    EnableSsl = true,
                };

                var mail = new MailMessage("fifa123minh@gmail.com", to, subject, body);
                mail.IsBodyHtml = true;
                smtpClient.Send(mail);
            }
        }

    }
} 