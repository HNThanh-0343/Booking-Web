using Microsoft.AspNetCore.Mvc;
using WEBSITE_TRAVELBOOKING.Core;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;

namespace WEBSITE_TRAVELBOOKING.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Auth]
    public class trangChuController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public trangChuController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        public IActionResult Index()
        {
            ViewBag.Contry = _unitOfWork.Repository<CatContry>().GetAll(filter: h => h.Featured == true).ToList();
            ViewBag.listHotel = _unitOfWork.Repository<SysHotel>().GetAll(filter: h => h.Status == true).ToList();
            ViewBag.listRoom = _unitOfWork.Repository<SysRoom>().GetAll(includeProperties: "TypeRoomNavigation", filter: h => h.Status == true).ToList();
            ViewBag.listHomeStay = _unitOfWork.Repository<SysVilla>().GetAll(filter: h => h.Status == true).ToList();
            ViewBag.listGuest = _unitOfWork.Repository<SysGuest>().GetAll(filter: h => h.Status == true).ToList();
            return View();
        }
    }
}
