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
    public class quanlykhuyenmaiController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public quanlykhuyenmaiController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        public IActionResult Index()
        {
            var sysPromotions = _unitOfWork.Repository<SysPromotion>().GetAll(filter: h => h.Status == true);
            // kiểm tra khuyến mãi hết hạn
            foreach (var item in sysPromotions)
            {
                if (item.EndDate < DateTime.Now)
                {
                    item.Status = false;
                    _unitOfWork.Repository<SysPromotion>().Update(item);
                }
            }
            return View();
        }
        public IActionResult childIndex(int? page, string searchValue)
        {
            try
            {
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return BadRequest();
                }

                var sysPromotions = _unitOfWork.Repository<SysPromotion>().GetAll(orderBy: h => h.OrderByDescending(m => m.Id));
                if (!string.IsNullOrEmpty(searchValue))
                {
                    sysPromotions = sysPromotions.Where(h => (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper())).ToList();
                }
                
                #region ViewBag
                ViewBag.searchValue = searchValue;
                #endregion

                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 10;
                var pageListView = sysPromotions.ToPagedList(page ?? 1, pageSize);
                if (pageListView != null)
                {
                    return PartialView("childIndex", pageListView);
                }
                return PartialView("childIndex");
            }
            catch (Exception)
            {

                throw;
            }
        }
        public IActionResult themMoiKhuyenMai()
        {
            try
            {
                SysPromotion sysPromotion = new SysPromotion();
                return PartialView("themMoiKhuyenMai", sysPromotion);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult themMoiKhuyenMai(SysPromotion sysPromotion, IFormFile? postedFile)
        {
            try
            {
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return BadRequest();
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
                    return PartialView("themMoiKhuyenMai", sysPromotion);
                }

                _unitOfWork.Repository<SysPromotion>().Insert(sysPromotion);

                // Lưu ảnh
                if (postedFile != null)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\KhuyenMai\\" + sysPromotion.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";

                    sysPromotion.Image = Common.SaveUrlImg(sysPromotion.Id, wwwPath, urlImg, postedFile);
                    _unitOfWork.Repository<SysPromotion>().Update(sysPromotion);
                }
                return PartialView(sysPromotion);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IActionResult chinhSuaKhuyenMai(int id)
        {
            try
            {
                var sysPromotion = _unitOfWork.Repository<SysPromotion>().GetById(id);
                if (sysPromotion == null)
                {
                    return BadRequest();
                }
                return PartialView("chinhSuaKhuyenMai", sysPromotion);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult chinhSuaKhuyenMai(SysPromotion sysPromotion, IFormFile? postedFile)
        {
            try
            {
                var getPromotion = _unitOfWork.Repository<SysPromotion>().GetById(sysPromotion.Id);
                if (getPromotion == null)
                {
                    return BadRequest();
                }
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return BadRequest();
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
                    return PartialView("chinhSuaKhuyenMai", sysPromotion);
                }

                // Lưu ảnh
                if (postedFile != null)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\KhuyenMai\\" + getPromotion.Id + "\\";
                    var path = wwwPath + urlImg;

                    // Xóa ảnh cũ
                    var fullPath = Path.Combine(wwwPath, getPromotion.Image.TrimStart('\\').Replace("/", "\\"));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    getPromotion.Image = Common.SaveUrlImg(getPromotion.Id, wwwPath, urlImg, postedFile);
                }
                getPromotion.Name = sysPromotion.Name;
                getPromotion.StartDate = sysPromotion.StartDate;
                getPromotion.EndDate = sysPromotion.EndDate;
                getPromotion.Describe = sysPromotion.Describe;
                getPromotion.Condition = sysPromotion.Condition;
                getPromotion.SaleOff = sysPromotion.SaleOff;
                getPromotion.Status = sysPromotion.Status;
                getPromotion.Type = sysPromotion.Type;

                _unitOfWork.Repository<SysPromotion>().Update(getPromotion);
                return PartialView(getPromotion);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult chiTietKhuyenMai(int id)
        {
            try
            {
                var sysPromotion = _unitOfWork.Repository<SysPromotion>().GetById(id);
                if (sysPromotion == null)
                {
                    return BadRequest();
                }
                return PartialView(sysPromotion);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaKhuyenMai(int id)
        {
            try
            {
                var sysPromotion = _unitOfWork.Repository<SysPromotion>().GetById(id);
                if (sysPromotion == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy khuyến mãi" });
                }
                // Xóa ảnh
                if (!string.IsNullOrEmpty(sysPromotion.Image))
                {
                    string wwwPath = this.Environment.WebRootPath;

                    var fullPath = Path.Combine(wwwPath, sysPromotion.Image.TrimStart('\\').Replace("/", "\\"));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
                _unitOfWork.Repository<SysPromotion>().Delete(sysPromotion);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }
    }
}
