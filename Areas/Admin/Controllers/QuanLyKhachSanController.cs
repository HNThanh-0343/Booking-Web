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
    public class QuanLyKhachSanController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public QuanLyKhachSanController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        #region Quản lý khách sạn
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult childIndex(int? page, string searchValue)
        {
            var listHotel = _unitOfWork.Repository<SysHotel>().GetAll(orderBy: h => h.OrderBy(m => m.Name)).ToList();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listHotel = listHotel.Where(h => (Common.GenerateSlug(h.Name.ToUpper())).Contains(Common.GenerateSlug(searchValue.ToUpper()))).ToList();
            }
            #region ViewBag
            ViewBag.searchValue = searchValue;
            ViewBag.contry = _unitOfWork.Repository<CatContry>().GetAll(filter: h => h.Status == true);
            ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll(filter: h => h.IdRole == 3).ToList();
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

        public IActionResult themMoiKhachSan(int? idUser)
        {
            try
            {
                SysHotel sysHotel = new SysHotel();
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll(filter: h => h.IdRole == 3 && h.Status == true).ToList();
                ViewBag.CatContry = _unitOfWork.Repository<CatContry>().GetAll().ToList();
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                if (idUser != null)
                {
                    ViewBag.getUser = _unitOfWork.Repository<SysUser>().GetById((int)idUser);
                }
                return PartialView("themMoiKhachSan", sysHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult themMoiKhachSan(SysHotel sysHotel,List<string> Amenities, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll(filter: h => h.IdRole == 3 && h.Status == true).ToList();
                ViewBag.CatContry = _unitOfWork.Repository<CatContry>().GetAll().ToList();
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.getUser = _unitOfWork.Repository<SysUser>().GetById((int)sysHotel.IdUser);
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
                if(Amenities.Count > 0)
                {
                    sysHotel.Amenities = string.Join(",", Amenities);
                }
                sysHotel.TimeCreate = DateTime.Now;
                sysHotel.IdCategory = 1;
                _unitOfWork.Repository<SysHotel>().Insert(sysHotel);

                // Lưu ảnh
                if (postedFile.Count > 0)
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
                            listIMG += pathdefault + ",";
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
                    return BadRequest();
                }
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll(filter: h => h.IdRole == 3 && h.Status == true).ToList();
                ViewBag.CatContry = _unitOfWork.Repository<CatContry>().GetAll().ToList();
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                return PartialView("chinhSuaKhachSan", sysHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult chinhSuaKhachSan(SysHotel sysHotel, List<string> Amenities, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll(filter: h => h.IdRole == 3 && h.Status == true).ToList();
                ViewBag.CatContry = _unitOfWork.Repository<CatContry>().GetAll().ToList();
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                var getHotel = _unitOfWork.Repository<SysHotel>().GetById(sysHotel.Id);
                if (getHotel == null)
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
                    return PartialView("chinhSuaKhachSan", sysHotel);
                }

                // Lưu avatar
                if (postedFile.Count > 0)
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
                    getHotel.ListImg += listIMG;
                }
                if (Amenities.Count > 0)
                {
                    getHotel.Amenities = string.Join(",", Amenities);
                }
                getHotel.IdUser = sysHotel.IdUser;
                getHotel.Name = sysHotel.Name;
                getHotel.Description = sysHotel.Description;
                getHotel.Local = sysHotel.Local;
                getHotel.Featured = sysHotel.Featured;
                getHotel.Status = sysHotel.Status;
                getHotel.Localiframe = sysHotel.Localiframe;
                getHotel.IdContry = sysHotel.IdContry;
                getHotel.PriceMin = sysHotel.PriceMin;

                _unitOfWork.Repository<SysHotel>().Update(getHotel);

                //TempData["SuccessMessage"] = "Chỉnh sửa thành công!";
                return PartialView(sysHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietKhachSan(int id, bool hideBtn = true)
        {
            try
            {
                var getHotel = _unitOfWork.Repository<SysHotel>().GetById(id);
                if (getHotel == null)
                {
                    return BadRequest();
                }
                ViewBag.hideBtn = hideBtn;
                ViewBag.getUser = _unitOfWork.Repository<SysUser>().GetById((int)getHotel.IdUser);
                ViewBag.getContry = _unitOfWork.Repository<CatContry>().GetById((int)getHotel.IdContry);
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
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

                // Lấy danh sách phòng
                var listRoom = _unitOfWork.Repository<SysRoom>().GetAll(filter: h => h.IdHotel == getHotel.Id).ToList();
                foreach (var item in listRoom)
                {
                    if (!string.IsNullOrEmpty(item.ListImg))
                    {
                        var listIMG = item.ListImg.Split(',');
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
                    _unitOfWork.Repository<SysRoom>().Delete(item);
                }
                return Json(new { result = true, message = "Thay đổi trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Lỗi khi đổi trạng thái" });
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


        #region Danh sách đặt phòng
        public IActionResult DatPhong()
        {
            return View();
        }
        public IActionResult childIndexDatPhong(int? page, string searchValue, DateTime? searchDate)
        {
            try
            {
                var sysBookings = _unitOfWork.Repository<SysBooking>().GetAll(includeProperties: "IdCategoriesNavigation", 
                                    filter: h => h.IdCategories == 1, 
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
                ViewBag.listRoom = _unitOfWork.Repository<SysRoom>().GetAll(includeProperties: "TypeRoomNavigation", filter: h => h.Status == true).ToList();
                ViewBag.listHotel = _unitOfWork.Repository<SysHotel>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.typeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll(filter: h => h.Status == true).ToList();
                #endregion

                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 10;
                var pageListView = sysBookings.ToPagedList(page ?? 1, pageSize);
                if (pageListView != null)
                {
                    return PartialView("childIndexDatPhong", pageListView);
                }
                return PartialView("childIndexDatPhong");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult chiTietDatPhong(int id)
        {
            try
            {
                var getBooking = _unitOfWork.Repository<SysBooking>().GetById(id);
                if (getBooking == null)
                {
                    return BadRequest();
                }
                var getRoom = _unitOfWork.Repository<SysRoom>().GetAll(
                                    includeProperties: "TypeRoomNavigation", 
                                    filter: h => h.Id == getBooking.BookingItemId && h.Status == true
                                ).FirstOrDefault();
                if (getRoom != null)
                {
                    ViewBag.getHotel = _unitOfWork.Repository<SysHotel>().GetById((int)getRoom.IdHotel);
                }
                if (getBooking.IdUser != null)
                {
                    var getUser = _unitOfWork.Repository<SysGuest>().GetById((int)getBooking.IdUser);
                    if (getUser != null)
                    {
                        ViewBag.getUser = getUser;
                    }
                }
                #region ViewBag
                ViewBag.getRoom = getRoom;
                #endregion
                return PartialView(getBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}