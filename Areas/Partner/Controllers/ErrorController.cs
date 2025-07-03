using Microsoft.AspNetCore.Mvc;
using WEBSITE_TRAVELBOOKING.Core;

namespace WEBSITE_TRAVELBOOKING.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Route("Partner")]
    public class ErrorController : Controller
    {
        [HttpGet("404")]
        public IActionResult Index()
        {
            // Lấy message từ Session
            var message = HttpContext.Session.GetString("ErrorMessagePartNer") ?? "Lỗi không xác định";

            // Xóa message trong session nếu muốn tránh hiển thị lại lần sau
            HttpContext.Session.Remove("ErrorMessagePartNer");
            ViewBag.ErrorMessage = message;
            return View();
        }
    }
}
