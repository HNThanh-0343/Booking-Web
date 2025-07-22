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
    public class quanLyPhongController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public quanLyPhongController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        public IActionResult Index(int idHotel)
        {
            ViewBag.idHotel = idHotel;
            return View();
        }

        public IActionResult childIndex(int idHotel, int? page, string searchValue)
        {
            if (idHotel == 0)
            {
                var referer = HttpContext.Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    var url = new Uri(referer);
                    var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(url.Query);
                    if (queryParams.TryGetValue("idHotel", out var idHotelValue))
                    {
                        if (int.TryParse(idHotelValue.ToString(), out int parsedIdHotel))
                        {
                            idHotel = parsedIdHotel;
                        }
                    }
                }
            }
            var listRoom = _unitOfWork.Repository<SysRoom>().GetAll(filter: h => h.IdHotel == idHotel);

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listRoom = listRoom.Where(h => (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper())).ToList();
            }

            #region ViewBag
            ViewBag.searchValue = searchValue;
            ViewBag.CatTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll(filter: h => h.Status == true).ToList();
            ViewBag.idHotel = idHotel;
            #endregion
            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = listRoom.ToPagedList(page ?? 1, pageSize);
            if (pageListView != null)
            {
                return PartialView("childIndex", pageListView);
            }
            return PartialView("childIndex");
        }

        public IActionResult themMoiPhong(int idHotel)
        {
            try
            {
                SysRoom sysRoom = new SysRoom();
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeBed = _unitOfWork.Repository<CatBedRoom>().GetAll(filter: h => h.Status == "1").ToList();
                ViewBag.getHotel = _unitOfWork.Repository<SysHotel>().GetById(idHotel);
                return PartialView("themMoiPhong", sysRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult themMoiPhong(SysRoom sysRoom, List<string> Amenities, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeBed = _unitOfWork.Repository<CatBedRoom>().GetAll(filter: h => h.Status == "1").ToList();
                ViewBag.getHotel = _unitOfWork.Repository<SysHotel>().GetById(sysRoom.IdHotel ?? throw new InvalidOperationException("IdHotel cannot be null."));

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
                _unitOfWork.Repository<SysRoom>().Insert(sysRoom);

                var getHotel = _unitOfWork.Repository<SysHotel>().GetById((int)sysRoom.IdHotel);
                if (getHotel == null)
                {
                    return BadRequest(ModelState);
                }
                // Cập nhật lại giá
                if (getHotel.PriceMin == null || Convert.ToDecimal(sysRoom.Price) < getHotel.PriceMin)
                {
                    getHotel.PriceMin = Convert.ToDecimal(sysRoom.Price);
                    _unitOfWork.Repository<SysHotel>().Update(getHotel);
                }
                // Lưu ảnh
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Room\\" + sysRoom.Id + "\\";
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
                    _unitOfWork.Repository<SysRoom>().Update(sysRoom);
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
                var sysRoom = _unitOfWork.Repository<SysRoom>().GetById(id);
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
        public IActionResult chinhSuaPhong(SysRoom sysRoom, List<string> Amenities, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.CatTypeBed = _unitOfWork.Repository<CatBedRoom>().GetAll(filter: h => h.Status == "1").ToList();
                var getRoom = _unitOfWork.Repository<SysRoom>().GetById(sysRoom.Id);
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
                    var urlImg = "\\AppData\\Room\\" + getRoom.Id + "\\";
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
                // cập nhật giá nhỏ nếu giá phòng thấp hơn giá hiện tại của khách sạn
                var getHotel = _unitOfWork.Repository<SysHotel>().GetById((int)sysRoom.IdHotel);
                if (getHotel == null)
                {
                    return BadRequest(ModelState);
                }
                // Cập nhật lại giá
                if (getHotel.PriceMin == null || Convert.ToDecimal(sysRoom.Price) < getHotel.PriceMin)
                {
                    getHotel.PriceMin = Convert.ToDecimal(sysRoom.Price);
                    _unitOfWork.Repository<SysHotel>().Update(getHotel);
                }
                getRoom.Name = sysRoom.Name;
                getRoom.Floor = sysRoom.Floor;
                getRoom.NumRoom = sysRoom.NumRoom;
                getRoom.TotalRoom = sysRoom.TotalRoom;
                getRoom.TypeRoom = sysRoom.TypeRoom;
                getRoom.IdTypeBed = sysRoom.IdTypeBed;
                getRoom.AdultsMax = sysRoom.AdultsMax;
                getRoom.ChildrenMax = sysRoom.ChildrenMax;
                getRoom.Price = sysRoom.Price;
                getRoom.Status = sysRoom.Status;
                getRoom.Feature = sysRoom.Feature;
                getRoom.Description = sysRoom.Description;
                getRoom.ContentBed = sysRoom.ContentBed;
                getRoom.RoomFeature = sysRoom.RoomFeature;
                _unitOfWork.Repository<SysRoom>().Update(getRoom);

                return PartialView(getRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietPhong(int id, bool hideBtn = true)
        {
            try
            {
                var getRoom = _unitOfWork.Repository<SysRoom>().GetById(id);
                if (getRoom == null)
                {
                    return BadRequest();
                }
                ViewBag.hideBtn = hideBtn;
                ViewBag.TienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: h => h.Status == true).ToList();
                ViewBag.GetHotel = _unitOfWork.Repository<SysHotel>().GetById((int)getRoom.IdHotel);
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
                var getRoom = _unitOfWork.Repository<SysRoom>().GetById(id);
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
                _unitOfWork.Repository<SysRoom>().Delete(getRoom);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }

        public IActionResult XoaAnh(string imagePath, int idRoom)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return Json(new { result = false, message = "Đường dẫn ảnh không hợp lệ." });
                }

                var getRoom = _unitOfWork.Repository<SysRoom>().GetById(idRoom);
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
                        _unitOfWork.Repository<SysRoom>().Update(getRoom);
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
    }
}
