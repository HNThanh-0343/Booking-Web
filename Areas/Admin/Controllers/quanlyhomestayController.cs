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
    public class quanlyhomestayController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public quanlyhomestayController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        #region Quản lý homestay
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult childIndex(int? page, string searchValue)
        {
            var getAccount = Account.GetAccount();
            if (getAccount == null)
            {
                return BadRequest();
            }
            var listHomeStay = _unitOfWork.Repository<SysVilla>().GetAll(orderBy: h => h.OrderBy(m => m.Featured)).ToList();
            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listHomeStay = listHomeStay.Where(h => h.Name.ToUpper().Contains(searchValue.ToUpper())).ToList();
            }
            #region ViewBag
            ViewBag.searchValue = searchValue;
            ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
            #endregion

            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = listHomeStay.ToPagedList(page ?? 1, pageSize);
            if (pageListView != null)
            {
                return PartialView("childIndex", pageListView);
            }
            return PartialView("childIndex");
        }

        public IActionResult themMoiHomeStay()
        {
            try
            {
                SysVilla sysVilla = new SysVilla();
                ViewBag.CatContry = _unitOfWork.Repository<CatContry>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll(filter: h => h.IdRole == 3 && h.Status == true).ToList();
                return PartialView("themMoiHomeStay", sysVilla);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult themMoiHomeStay(SysVilla sysVilla, List<string> Amenities, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.CatContry = _unitOfWork.Repository<CatContry>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll(filter: h => h.IdRole == 3 && h.Status == true).ToList();
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
                    return PartialView("themMoiHomeStay", sysVilla);
                }
                if (Amenities.Count > 0)
                {
                    sysVilla.Amenities = string.Join(",", Amenities);
                }
                sysVilla.TimeCreate = DateTime.Now;
                sysVilla.IdCategory = 2;
                _unitOfWork.Repository<SysVilla>().Insert(sysVilla);

                // Lưu ảnh
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\HomeStay\\" + sysVilla.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysVilla.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ",";
                        }
                    }

                    sysVilla.ListImg = listIMG;
                    _unitOfWork.Repository<SysVilla>().Update(sysVilla);
                }
                return PartialView(sysVilla);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IActionResult chinhSuaHomeStay(int id)
        {
            try
            {
                var sysHomeStay = _unitOfWork.Repository<SysVilla>().GetById(id);
                if (sysHomeStay == null)
                {
                    return BadRequest();
                }
                ViewBag.CatContry = _unitOfWork.Repository<CatContry>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll(filter: h => h.IdRole == 3 && h.Status == true).ToList();
                return PartialView("chinhSuaHomeStay", sysHomeStay);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult chinhSuaHomeStay(SysVilla sysVilla, List<string> Amenities, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.CatContry = _unitOfWork.Repository<CatContry>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll(filter: h => h.IdRole == 3 && h.Status == true).ToList();
                var getHomeStay = _unitOfWork.Repository<SysVilla>().GetById(sysVilla.Id);
                if (getHomeStay == null)
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
                    return PartialView("chinhSuaHomeStay", sysVilla);
                }

                // Lưu ảnh
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\HomeStay\\" + getHomeStay.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(getHomeStay.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ",";
                        }
                    }
                    getHomeStay.ListImg += listIMG;
                }
                if (Amenities.Count > 0)
                {
                    getHomeStay.Amenities = string.Join(",", Amenities);
                }
                getHomeStay.Name = sysVilla.Name;
                getHomeStay.IdUser = sysVilla.IdUser;
                getHomeStay.LocalText = sysVilla.LocalText;
                getHomeStay.IdContry = sysVilla.IdContry;
                getHomeStay.TotalRoom = sysVilla.TotalRoom;
                getHomeStay.TotalBed = sysVilla.TotalBed;
                getHomeStay.TotalGuest = sysVilla.TotalGuest;
                getHomeStay.PriceMin = sysVilla.PriceMin;
                getHomeStay.Phone = sysVilla.Phone;
                getHomeStay.Status = sysVilla.Status;
                getHomeStay.Featured = sysVilla.Featured;
                getHomeStay.Description = sysVilla.Description;
                getHomeStay.HouseRules = sysVilla.HouseRules;
                getHomeStay.LocalIframe = sysVilla.LocalIframe;

                _unitOfWork.Repository<SysVilla>().Update(getHomeStay);
                return PartialView(getHomeStay);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietHomeStay(int id)
        {
            try
            {
                var getHomeStay = _unitOfWork.Repository<SysVilla>().GetById(id);
                if (getHomeStay == null)
                {
                    return BadRequest();
                }
                ViewBag.getContry = _unitOfWork.Repository<CatContry>().GetById((int)getHomeStay.IdContry);
                ViewBag.getUser = _unitOfWork.Repository<SysUser>().GetById((int)getHomeStay.IdUser);
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                return PartialView(getHomeStay);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaHomeStay(int id)
        {
            try
            {
                var getHomeStay = _unitOfWork.Repository<SysVilla>().GetById(id);
                if (getHomeStay == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy homestay" });
                }
                // Xóa ảnh
                if (!string.IsNullOrEmpty(getHomeStay.ListImg))
                {
                    var listIMG = getHomeStay.ListImg.Split(',');
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
                _unitOfWork.Repository<SysVilla>().Delete(getHomeStay);

                // Lấy danh sách phòng
                var listRoom = _unitOfWork.Repository<SysRoomHomeStay>().GetAll(filter: h => h.IdHomeStay == getHomeStay.Id).ToList();
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
                    _unitOfWork.Repository<SysRoomHomeStay>().Delete(item);
                }
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }

        public IActionResult XoaAnh(string imagePath, int IdHomeStay)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return Json(new { result = false, message = "Đường dẫn ảnh không hợp lệ." });
                }

                var getHomeStay = _unitOfWork.Repository<SysVilla>().GetById(IdHomeStay);
                if (getHomeStay == null)
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
                    if (!string.IsNullOrEmpty(getHomeStay.ListImg))
                    {
                        var newList = getHomeStay.ListImg.Split(',').Where(h => h.Trim() != imagePath.Trim()).ToArray();
                        getHomeStay.ListImg = string.Join(",", newList);
                        _unitOfWork.Repository<SysVilla>().Update(getHomeStay);
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

        #region Quản lý phòng của homestay
        public IActionResult IndexRoom(int idHomeStay)
        {
            ViewBag.idHomeStay = idHomeStay;
            return View();
        }

        public IActionResult childIndexRoom(int idHomeStay, int? page, string searchValue)
        {
            // Nếu không có idHomeStay thì lấy idHomeStay trên url
            if (idHomeStay == 0)
            {
                var referer = HttpContext.Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    var url = new Uri(referer);
                    var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(url.Query);
                    if (queryParams.TryGetValue("idHomeStay", out var idHomeStayValue))
                    {
                        if (int.TryParse(idHomeStayValue.ToString(), out int parsedIdHomeStay))
                        {
                            idHomeStay = parsedIdHomeStay;
                        }
                    }
                }
            }
            var listRoom = _unitOfWork.Repository<SysRoomHomeStay>().GetAll(filter: h => h.IdHomeStay == idHomeStay);

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listRoom = listRoom.Where(h => (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper())).ToList();
            }

            #region ViewBag
            ViewBag.searchValue = searchValue;
            ViewBag.CatTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll(filter: h => h.Status == true).ToList();
            ViewBag.idHotel = idHomeStay;
            #endregion
            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = listRoom.ToPagedList(page ?? 1, pageSize);
            if (pageListView != null)
            {
                return PartialView("childIndexRoom", pageListView);
            }
            return PartialView("childIndexRoom");
        }

        public IActionResult themMoiPhong(int idHomeStay)
        {
            try
            {
                SysRoomHomeStay sysRoom = new SysRoomHomeStay();
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeBed = _unitOfWork.Repository<CatBedRoom>().GetAll(filter: h => h.Status == "1").ToList();
                ViewBag.idHomeStay = idHomeStay;
                return PartialView("themMoiPhong", sysRoom);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult themMoiPhong(SysRoomHomeStay sysRoom, List<string> Amenities, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeBed = _unitOfWork.Repository<CatBedRoom>().GetAll(filter: h => h.Status == "1").ToList();
                ViewBag.idHomeStay = sysRoom.IdHomeStay;
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
                    return PartialView("themMoiPhong", sysRoom);
                }
                if (Amenities.Count > 0)
                {
                    sysRoom.ListAminities = string.Join(",", Amenities);
                }
                _unitOfWork.Repository<SysRoomHomeStay>().Insert(sysRoom);

                // Lưu ảnh
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\RoomHomeStay\\" + sysRoom.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysRoom.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ", ";
                        }
                    }
                    sysRoom.ListImg = listIMG;
                    _unitOfWork.Repository<SysRoomHomeStay>().Update(sysRoom);
                }
                return PartialView(sysRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chinhSuaPhong(int id)
        {
            try
            {
                var sysRoom = _unitOfWork.Repository<SysRoomHomeStay>().GetById(id);
                if (sysRoom == null)
                {
                    return BadRequest();
                }
                #region ViewBag
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeBed = _unitOfWork.Repository<CatBedRoom>().GetAll(filter: h => h.Status == "1").ToList();
                #endregion
                return PartialView("chinhSuaPhong", sysRoom);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult chinhSuaPhong(SysRoomHomeStay sysRoom, List<string> Amenities, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeBed = _unitOfWork.Repository<CatBedRoom>().GetAll(filter: h => h.Status == "1").ToList();
                var getRoom = _unitOfWork.Repository<SysRoomHomeStay>().GetById(sysRoom.Id);
                if (getRoom == null)
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
                    return PartialView("chinhSuaPhong", sysRoom);
                }

                // Lưu avatar
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\RoomHomeStay\\" + getRoom.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(getRoom.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ", ";
                        }
                    }
                    getRoom.ListImg = listIMG;
                }
                if (Amenities.Count > 0)
                {
                    sysRoom.ListAminities = string.Join(",", Amenities);
                }
                getRoom.Name = sysRoom.Name;
                getRoom.TotalRoom = sysRoom.TotalRoom;
                getRoom.TypeRoom = sysRoom.TypeRoom;
                getRoom.IdTypeBed = sysRoom.IdTypeBed;
                getRoom.AdultsMax = sysRoom.AdultsMax;
                getRoom.ChildrenMax = sysRoom.ChildrenMax;
                getRoom.Price = sysRoom.Price;
                getRoom.Status = sysRoom.Status;
                getRoom.Feature = sysRoom.Feature;
                getRoom.Description = sysRoom.Description;
                _unitOfWork.Repository<SysRoomHomeStay>().Update(getRoom);

                return PartialView(getRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietPhong(int id)
        {
            try
            {
                var getRoom = _unitOfWork.Repository<SysRoomHomeStay>().GetById(id);
                if (getRoom == null)
                {
                    return BadRequest();
                }
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.GetHomeStay = _unitOfWork.Repository<SysVilla>().GetById((int)getRoom.IdHomeStay);
                ViewBag.typeRoom = _unitOfWork.Repository<CatTypeRoom>().GetById((int)getRoom.TypeRoom);
                ViewBag.CatTypeBed = _unitOfWork.Repository<CatBedRoom>().GetById(Convert.ToInt32(getRoom.IdTypeBed));
                return PartialView(getRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaPhong(int id)
        {
            try
            {
                var getRoom = _unitOfWork.Repository<SysRoomHomeStay>().GetById(id);
                if (getRoom == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy phòng" });
                }
                // Xóa ảnh
                if (!string.IsNullOrEmpty(getRoom.ListImg))
                {
                    var listIMG = getRoom.ListImg.Split(',');
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
                _unitOfWork.Repository<SysRoomHomeStay>().Delete(getRoom);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }

        public IActionResult XoaAnhPhong(string imagePath, int idRoom)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return Json(new { result = false, message = "Đường dẫn ảnh không hợp lệ." });
                }

                var getRoom = _unitOfWork.Repository<SysRoomHomeStay>().GetById(idRoom);
                if (getRoom == null)
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
                    if (!string.IsNullOrEmpty(getRoom.ListImg))
                    {
                        var newList = getRoom.ListImg.Split(',').Where(h => h.Trim() != imagePath.Trim()).ToArray();
                        getRoom.ListImg = string.Join(",", newList);
                        _unitOfWork.Repository<SysRoomHomeStay>().Update(getRoom);
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
    }
}
