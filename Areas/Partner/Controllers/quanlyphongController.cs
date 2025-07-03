using Microsoft.AspNetCore.Mvc;
using WEBSITE_TRAVELBOOKING.Core;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;

namespace WEBSITE_TRAVELBOOKING.Areas.Partner.Controllers
{
    [Area("partner")]
    [Auth]
    public class quanlyphongController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public quanlyphongController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult childIndex(int? page, string searchValue)
        {
            var getAccount = Account.GetAccount();
            
            var listHotel = _unitOfWork.Repository<SysHotel>().GetAll(filter: h => h.IdUser == getAccount.Id).ToList();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listHotel = listHotel.Where(h => (h.Name.ToUpper()).Contains(searchValue.ToUpper()) || (h.Description.ToUpper()).Contains(searchValue.ToUpper())).ToList();
            }
            #region ViewBag
            ViewBag.searchValue = searchValue;
            ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
            #endregion

            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = listHotel.ToPagedList(page ?? 1, pageSize);
            if (pageListView != null)
            {
                return PartialView("childIndex", pageListView);
            }
            return PartialView("childIndex");
        }

        public IActionResult themMoiKhachSan()
        {
            try
            {
                SysHotel sysHotel = new SysHotel();
                return PartialView("themMoiKhachSan", sysHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult themMoiKhachSan(SysHotel sysHotel, List<IFormFile> postedFile)
        {
            try
            {
                var getAccount = Account.GetAccount();
                if(getAccount == null)
                {
                    return PartialView("themMoiKhachSan", sysHotel);
                }
                sysHotel.IdUser = getAccount.Id;
                if (sysHotel.Name == null || sysHotel.Local == null)
                {
                    //TempData["ErrorMessage"] = "Thêm mới thất bại!";
                    return PartialView("themMoiKhachSan", sysHotel);
                }
                if (!ModelState.IsValid)
                {
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            var key = modelStateKey;
                            var errorMessage = error.ErrorMessage;
                            ModelState.AddModelError(key, errorMessage);
                            return BadRequest(ModelState);
                        }
                    }
                    return PartialView("themMoiKhachSan", sysHotel);
                }
                sysHotel.TimeCreate = DateTime.Now;
                _unitOfWork.Repository<SysHotel>().Insert(sysHotel);

                // Lưu ảnh
                if (postedFile != null)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Hotel\\" + sysHotel.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysHotel.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ", ";
                        }
                    }

                    sysHotel.ListImg = listIMG;
                    _unitOfWork.Repository<SysHotel>().Update(sysHotel);
                }
                return PartialView(sysHotel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult chinhSuaKhachSan(int id)
        {
            try
            {
                var sysHotel = _unitOfWork.Repository<SysHotel>().GetById(id);
                if (sysHotel == null)
                {
                    return PartialView("Index");
                }
                return PartialView("chinhSuaKhachSan", sysHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult chinhSuaKhachSan(SysHotel sysHotel, List<IFormFile> postedFile)
        {
            try
            {
                var getHotel = _unitOfWork.Repository<SysHotel>().GetById(sysHotel.Id);
                if (getHotel == null)
                {
                    //TempData["ErrorMessage"] = "Chỉnh sửa thất bại!";
                    return PartialView("chinhSuaTaiKhoan", getHotel);
                }

                if (!ModelState.IsValid)
                {
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            var key = modelStateKey;
                            var errorMessage = error.ErrorMessage;
                            ModelState.AddModelError(key, errorMessage);
                            return BadRequest(ModelState);
                        }
                    }
                    return PartialView("chinhSuaKhachSan", sysHotel);
                }

                // Lưu avatar
                if (postedFile != null)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Hotel\\" + getHotel.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysHotel.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ", ";
                        }
                    }
                    getHotel.ListImg = listIMG;
                }

                getHotel.Name = sysHotel.Name;
                getHotel.Description = sysHotel.Description;
                getHotel.Local = sysHotel.Local;                
                getHotel.Amenities = sysHotel.Amenities;                
                getHotel.Featured = sysHotel.Featured;
                getHotel.Status = sysHotel.Status;
                getHotel.Phone = sysHotel.Phone;

                _unitOfWork.Repository<SysHotel>().Update(getHotel);

                //TempData["SuccessMessage"] = "Chỉnh sửa thành công!";
                return PartialView(sysHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietKhachSan(int id)
        {
            try
            {
                var getHotel = _unitOfWork.Repository<SysHotel>().GetById(id);
                if (getHotel == null)
                {
                    return PartialView("Index");
                }
                ViewBag.getUser = _unitOfWork.Repository<SysUser>().GetById((int)getHotel.IdUser);
                return PartialView(getHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaKhachSan(int id)
        {
            try
            {
                var getHotel = _unitOfWork.Repository<SysHotel>().GetById(id);
                if (getHotel == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy khách sạn" });
                }
                // Xóa ảnh
                if (!string.IsNullOrEmpty(getHotel.ListImg))
                {
                    var listIMG = getHotel.ListImg.Split(',');
                    string wwwPath = this.Environment.WebRootPath;

                    foreach (var img in listIMG)
                    {
                        var fullPath = Path.Combine(wwwPath, img.TrimStart('\\').Replace("/", "\\"));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }
                _unitOfWork.Repository<SysHotel>().Delete(getHotel);
                //TempData["SuccessMessage"] = "Xóa thành công!";
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }

        public IActionResult XoaAnh(string imagePath, int hotelId)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return Json(new { result = false, message = "Đường dẫn ảnh không hợp lệ." });
                }

                var getHotel = _unitOfWork.Repository<SysHotel>().GetById(hotelId);
                if (getHotel == null)
                {
                    return Json(new { result = false, message = "Lỗi khi xóa ảnh." });
                }

                string trimmedPath = imagePath.Trim().TrimStart('/', '\\');
                var rootPath = Directory.GetCurrentDirectory();
                var fullPath = Path.Combine(rootPath, "wwwroot", trimmedPath);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);

                    // Cập nhật lại list ảnh
                    if (!string.IsNullOrEmpty(getHotel.ListImg))
                    {
                        var newList = getHotel.ListImg.Split(',').Where(h => h.Trim() != imagePath.Trim()).ToArray();
                        getHotel.ListImg = string.Join(",", newList);
                        _unitOfWork.Repository<SysHotel>().Update(getHotel);
                    }
                    return Json(new { result = true });
                } else
                {
                    return Json(new { result = false, message = "Ảnh không tồn tại." });
                }
            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa ảnh." });
            }
        }       
    }
}