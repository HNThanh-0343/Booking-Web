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
    public class quanLyThueXeController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public quanLyThueXeController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
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
            if (getAccount == null)
            {
                return BadRequest();
            }

            var listCar = _unitOfWork.Repository<SysCar>().GetAll().ToList();
            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listCar = listCar.Where(h => h.Name.ToUpper().Contains(searchValue.ToUpper())).ToList();
            }
            #region ViewBag
            ViewBag.searchValue = searchValue;
            ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
            #endregion
            #region Page
            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = listCar.ToPagedList(page ?? 1, pageSize);
            #endregion
            if (pageListView != null)
            {
                return PartialView("childIndex", pageListView);
            }
            return PartialView("childIndex");
        }

        public IActionResult themMoiThueXe()
        {
            try
            {
                SysCar sysCar = new SysCar();
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
                return PartialView("themMoiThueXe", sysCar);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult themMoiThueXe(SysCar sysCar, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
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
                    return PartialView("themMoiThueXe", sysCar);
                }

                sysCar.Time = DateTime.Now;
                sysCar.IdCategory = 5;
                _unitOfWork.Repository<SysCar>().Insert(sysCar);

                // Lưu ảnh
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Car\\" + sysCar.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysCar.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ",";
                        }
                    }

                    sysCar.ListImg = listIMG;
                    _unitOfWork.Repository<SysCar>().Update(sysCar);
                }
                return PartialView(sysCar);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IActionResult chinhSuaThueXe(int id)
        {
            try
            {
                var sysCar = _unitOfWork.Repository<SysCar>().GetById(id);
                if (sysCar == null)
                {
                    return BadRequest();
                }
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
                return PartialView("chinhSuaThueXe", sysCar);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult chinhSuaThueXe(SysCar sysCar, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();

                var getCar = _unitOfWork.Repository<SysCar>().GetById(sysCar.Id);
                if (getCar == null)
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
                    return PartialView("chinhSuaThueXe", sysCar);
                }

                // Lưu ảnh
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Car\\" + sysCar.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysCar.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ",";
                        }
                    }
                    getCar.ListImg += listIMG;
                }

                getCar.Name = sysCar.Name;
                getCar.IdUser = sysCar.IdUser;
                getCar.Status = sysCar.Status;
                getCar.LocalText = sysCar.LocalText;
                getCar.LocalIframe = sysCar.LocalIframe;
                getCar.Price = sysCar.Price;
                getCar.Phone = sysCar.Phone;
                getCar.Featured = sysCar.Featured;
                getCar.Description = sysCar.Description;
                getCar.Amenities = sysCar.Amenities;
                getCar.Specifications = sysCar.Specifications;

                _unitOfWork.Repository<SysCar>().Update(getCar);
                return PartialView(getCar);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult chiTietThueXe(int id)
        {
            try
            {
                var getCar = _unitOfWork.Repository<SysCar>().GetById(id);
                if (getCar == null)
                {
                    return BadRequest();
                }
                ViewBag.getUser = _unitOfWork.Repository<SysUser>().GetById((int)getCar.IdUser);
                return PartialView(getCar);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaThueXe(int id)
        {
            try
            {
                var getCar = _unitOfWork.Repository<SysCar>().GetById(id);
                if (getCar == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy tour" });
                }
                // Xóa ảnh
                if (!string.IsNullOrEmpty(getCar.ListImg))
                {
                    var listIMG = getCar.ListImg.Split(',');
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
                _unitOfWork.Repository<SysCar>().Delete(getCar);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }

        public IActionResult XoaAnh(string imagePath, int IdCar)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return Json(new { result = false, message = "Đường dẫn ảnh không hợp lệ." });
                }

                var getCar = _unitOfWork.Repository<SysCar>().GetById(IdCar);
                if (getCar == null)
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
                    if (!string.IsNullOrEmpty(getCar.ListImg))
                    {
                        var newList = getCar.ListImg.Split(',').Where(h => h.Trim() != imagePath.Trim()).ToArray();
                        getCar.ListImg = string.Join(",", newList);
                        _unitOfWork.Repository<SysCar>().Update(getCar);
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
