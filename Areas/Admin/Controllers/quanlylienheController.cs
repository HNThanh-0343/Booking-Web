using Microsoft.AspNetCore.Mvc;
using WEBSITE_TRAVELBOOKING.Core;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;

namespace WEBSITE_TRAVELBOOKING.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Auth]
    public class quanlylienheController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public quanlylienheController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        public IActionResult Index()
        {
            
            return View();
        }
        public IActionResult childIndex()
        {
            try
            {
                var getContact = _unitOfWork.Repository<SysContact>().GetAll().FirstOrDefault();
                if (getContact == null)
                {
                    getContact = new SysContact();
                }
                return PartialView("childIndex", getContact);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public IActionResult chinhSuaLienHe(SysContact sysContact)
        {
            try
            {
                if (sysContact.Id == 0)
                {
                    _unitOfWork.Repository<SysContact>().Insert(sysContact);
                    return Json(new { status = true, message = "Chỉnh sửa thành công" });
                } else
                {
                    var getContect = _unitOfWork.Repository<SysContact>().GetById(sysContact.Id);
                    if (getContect == null)
                    {
                        return Json(new { status = false, message = "Không tìm thấy dữ liệu cập nhật" });
                    }
                    getContect.Phone = sysContact.Phone;
                    getContect.Email = sysContact.Email;
                    getContect.Name = sysContact.Name;
                    getContect.ContentMap = sysContact.ContentMap;
                    getContect.Urlfb = sysContact.Urlfb;
                    getContect.Urlin = sysContact.Urlin;
                    getContect.Urlinta = sysContact.Urlinta;
                    getContect.Urltwi = sysContact.Urltwi;
                    getContect.Status = sysContact.Status;
                    _unitOfWork.Repository<SysContact>().Update(getContect);

                    return Json(new { status = true, message = "Chỉnh sửa thành công" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
