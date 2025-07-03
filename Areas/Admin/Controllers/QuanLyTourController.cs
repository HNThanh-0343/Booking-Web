using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WEBSITE_TRAVELBOOKING.Core;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;

namespace WEBSITE_TRAVELBOOKING.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Auth]
    public class QuanLyTourController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public QuanLyTourController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        #region Quản lý Tour
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult childIndex(int? page, string searchValue)
        {
            var getAccount = Account.GetAccount();
            if (getAccount == null)
            {
                return PartialView("childIndex");
            }
            var listTour = _unitOfWork.Repository<SysTour>().GetAll().ToList();
            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listTour = listTour.Where(h => h.Name.ToUpper().Contains(searchValue.ToUpper())).ToList();
            }
            #region ViewBag
            ViewBag.searchValue = searchValue;
            ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
            #endregion

            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = listTour.ToPagedList(page ?? 1, pageSize);
            if (pageListView != null)
            {
                return PartialView("childIndex", pageListView);
            }
            return PartialView("childIndex");
        }

        public IActionResult themMoiTour()
        {
            try
            {
                SysTour sysTour = new SysTour();
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
                return PartialView("themMoiTour", sysTour);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult themMoiTour(SysTour sysTour, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return PartialView("themMoiTour", sysTour);
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
                    return PartialView("themMoiTour", sysTour);
                }

                sysTour.Time = DateTime.Now;
                sysTour.IdCategory = 3;
                _unitOfWork.Repository<SysTour>().Insert(sysTour);

                // Lưu ảnh
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Tour\\" + sysTour.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysTour.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ",";
                        }
                    }

                    sysTour.ListImg = listIMG;
                    _unitOfWork.Repository<SysTour>().Update(sysTour);
                }
                return PartialView(sysTour);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IActionResult chinhSuaTour(int id)
        {
            try
            {
                var sysTour = _unitOfWork.Repository<SysTour>().GetById(id);
                if (sysTour == null)
                {
                    return PartialView("Index");
                }
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
                return PartialView("chinhSuaTour", sysTour);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult chinhSuaTour(SysTour sysTour, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
                var getTour = _unitOfWork.Repository<SysTour>().GetById(sysTour.Id);
                if (getTour == null)
                {
                    return PartialView("chinhSuaTour", sysTour);
                }
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return PartialView("chinhSuaTour", getTour);
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
                    return PartialView("chinhSuaTour", sysTour);
                }

                // Lưu ảnh
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Tour\\" + sysTour.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysTour.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ",";
                        }
                    }
                    getTour.ListImg += listIMG;
                }

                getTour.Name = sysTour.Name;
                getTour.IdUser = sysTour.IdUser;
                getTour.Status = sysTour.Status;
                getTour.Wifi = sysTour.Wifi;
                getTour.LocalText = sysTour.LocalText;
                getTour.Pickup = sysTour.Pickup;
                getTour.Price = sysTour.Price;
                getTour.Description = sysTour.Description;
                getTour.Phone = sysTour.Phone;
                getTour.MaxPeople = sysTour.MaxPeople;
                getTour.MinAge = sysTour.MinAge;
                getTour.WhattoExpect = sysTour.WhattoExpect;
                getTour.Featured = sysTour.Featured;
                getTour.LocalIframe = sysTour.LocalIframe;

                _unitOfWork.Repository<SysTour>().Update(getTour);
                return PartialView(getTour);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietTour(int id)
        {
            try
            {
                var getTour = _unitOfWork.Repository<SysTour>().GetById(id);
                if (getTour == null)
                {
                    return PartialView("Index");
                }
                ViewBag.getUser = _unitOfWork.Repository<SysUser>().GetById((int)getTour.IdUser);
                return PartialView(getTour);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaTour(int id)
        {
            try
            {
                var getTour = _unitOfWork.Repository<SysTour>().GetById(id);
                if (getTour == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy tour" });
                }
                // Xóa ảnh
                if (!string.IsNullOrEmpty(getTour.ListImg))
                {
                    var listIMG = getTour.ListImg.Split(',');
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
                _unitOfWork.Repository<SysTour>().Delete(getTour);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }

        public IActionResult XoaAnh(string imagePath, int IdTour)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return Json(new { result = false, message = "Đường dẫn ảnh không hợp lệ." });
                }

                var getTour = _unitOfWork.Repository<SysTour>().GetById(IdTour);
                if (getTour == null)
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
                    if (!string.IsNullOrEmpty(getTour.ListImg))
                    {
                        var newList = getTour.ListImg.Split(',').Where(h => h.Trim() != imagePath.Trim()).ToArray();
                        getTour.ListImg = string.Join(",", newList);
                        _unitOfWork.Repository<SysTour>().Update(getTour);
                    }
                    return Json(new { result = true });
                }
                else
                {
                    return Json(new { result = false, message = "Ảnh không tồn tại." });
                }
            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa ảnh." });
            }
        }
        #endregion

        #region Quản lý đặt Tour
        public IActionResult dattour()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public IActionResult childIndexDatTour(int? page, string searchValue, DateTime? searchDate)
        {
            try
            {
                var sysBookings = _unitOfWork.Repository<SysBooking>().GetAll(includeProperties: "IdCategoriesNavigation",
                                    filter: h => h.IdCategories == 3,
                                    orderBy: h => h.OrderByDescending(m => m.Id));

                if (!string.IsNullOrEmpty(searchValue))
                {
                    sysBookings = sysBookings.Where(h => (h.FullNameGuest ?? "").ToUpper().Contains(searchValue.ToUpper())).ToList();
                }
                if (searchDate != null)
                {
                    sysBookings = sysBookings.Where(h => h.StartDate.Date <= searchDate.Value.Date && h.EndDate.Date >= searchDate.Value.Date).ToList();
                }

                #region ViewBag
                ViewBag.searchDate = searchDate;
                ViewBag.searchValue = searchValue;
                ViewBag.listUser = _unitOfWork.Repository<SysGuest>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.listTour = _unitOfWork.Repository<SysTour>().GetAll(filter: h => h.Status == true).ToList();
                #endregion

                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 10;
                var pageListView = sysBookings.ToPagedList(page ?? 1, pageSize);
                if (pageListView != null)
                {
                    return PartialView("childIndexDatTour", pageListView);
                }
                return PartialView("childIndexDatTour");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult chinhSuaDatTour(int id)
        {
            try
            {
                var getBooking = _unitOfWork.Repository<SysBooking>().GetById(id);
                if (getBooking == null)
                {
                    return PartialView();
                }
                #region ViewBag
                ViewBag.DSTrangThai = new List<SelectListItem>();
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "0", Text = "Hủy đơn", });
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "1", Text = "Hoàn thành", });
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "2", Text = "Chờ thanh toán", });
                #endregion
                return PartialView(getBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult chinhSuaDatTour(SysBooking sysBooking)
        {
            try
            {
                var getBooking = _unitOfWork.Repository<SysBooking>().GetById(sysBooking.Id);
                if (getBooking == null)
                {
                    return Content("false");
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
                    return PartialView("chinhSuaDatTour", sysBooking);
                }
                //getBooking.StartDate = sysBooking.StartDate;
                //getBooking.EndDate = sysBooking.EndDate;
                getBooking.Status = sysBooking.Status;

                return PartialView("chinhSuaDatTour", sysBooking);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult chiTietDatTour(int id)
        {
            try
            {
                var getBooking = _unitOfWork.Repository<SysBooking>().GetById(id);
                if (getBooking == null)
                {
                    return View();
                }
                #region ViewBag
                ViewBag.getUser = _unitOfWork.Repository<SysGuest>().GetById((int)getBooking.IdUser);
                ViewBag.getTour = _unitOfWork.Repository<SysTour>().GetAll(filter: h => h.Id == getBooking.BookingItemId && h.Status == true).FirstOrDefault();
                #endregion
                return PartialView(getBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaDatTour(int id)
        {
            try
            {
                var sysBooking = _unitOfWork.Repository<SysBooking>().GetById(id);
                if (sysBooking == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy đặt tour" });
                }
                _unitOfWork.Repository<SysBooking>().Delete(sysBooking);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Xóa thất bại" });
            }
        }
        #endregion

    }
}
