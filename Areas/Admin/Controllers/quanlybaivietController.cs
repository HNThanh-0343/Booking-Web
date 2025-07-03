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
    public class quanlybaivietController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public quanlybaivietController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        public IActionResult Index()
        {
            ViewBag.CatCategory = _unitOfWork.Repository<CatCategory>().GetAll().ToList();
            return View();
        }

        public IActionResult childIndex(int? page, string searchValue, int? idCategories)
        {
            var getAccount = Account.GetAccount();
            if (getAccount == null)
            {
                return BadRequest();
            }
            var listBlog = _unitOfWork.Repository<SysBlog>().GetAll().ToList();
            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listBlog = listBlog.Where(h => (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper()) ||
                            (h.ContentsShort ?? "").ToUpper().Contains(searchValue.ToUpper())).ToList();
            }
            if(idCategories != null)
            {
                listBlog = listBlog.Where(h => h.IdTypeBlog == idCategories).ToList();
            }
            #region ViewBag
            ViewBag.searchValue = searchValue;
            ViewBag.idCategories = idCategories;
            ViewBag.CatCategory = _unitOfWork.Repository<CatCategory>().GetAll().ToList();
            ViewBag.listUser = _unitOfWork.Repository<SysUser>().GetAll().ToList();
            #endregion

            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = listBlog.ToPagedList(page ?? 1, pageSize);
            if (pageListView != null)
            {
                return PartialView("childIndex", pageListView);
            }
            return PartialView("childIndex");
        }

        public IActionResult themMoiBlog()
        {
            try
            {
                SysBlog sysBlog = new SysBlog();
                ViewBag.CatCategory = _unitOfWork.Repository<CatCategory>().GetAll().ToList();
                return PartialView("themMoiBlog", sysBlog);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult themMoiBlog(SysBlog sysBlog, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.CatCategory = _unitOfWork.Repository<CatCategory>().GetAll().ToList();
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
                    return PartialView("themMoiBlog", sysBlog);
                }

                sysBlog.IdUser = getAccount.Id;
                sysBlog.DateCreate = DateTime.Now;
                _unitOfWork.Repository<SysBlog>().Insert(sysBlog);

                // Lưu ảnh
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Blog\\" + sysBlog.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysBlog.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ",";
                        }
                    }

                    sysBlog.ListImg = listIMG;
                    _unitOfWork.Repository<SysBlog>().Update(sysBlog);
                }
                return PartialView(sysBlog);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IActionResult chinhSuaBlog(int id)
        {
            try
            {
                var sysBlog = _unitOfWork.Repository<SysBlog>().GetById(id);
                if (sysBlog == null)
                {
                    return PartialView("Index");
                }
                ViewBag.CatCategory = _unitOfWork.Repository<CatCategory>().GetAll().ToList();
                return PartialView("chinhSuaBlog", sysBlog);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult chinhSuaBlog(SysBlog sysBlog, List<IFormFile> postedFile)
        {
            try
            {
                ViewBag.CatCategory = _unitOfWork.Repository<CatCategory>().GetAll().ToList();
                var getBlog = _unitOfWork.Repository<SysBlog>().GetById(sysBlog.Id);
                if (getBlog == null)
                {
                    return BadRequest();
                }
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return PartialView("chinhSuaBlog", getBlog);
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
                    return PartialView("chinhSuaBlog", sysBlog);
                }

                // Lưu ảnh
                if (postedFile.Count > 0)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Blog\\" + getBlog.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(getBlog.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ",";
                        }
                    }
                    getBlog.ListImg += listIMG;
                }
                getBlog.IdTypeBlog = sysBlog.IdTypeBlog;
                getBlog.ContentsShort = sysBlog.ContentsShort;
                getBlog.Contents = sysBlog.Contents;
                getBlog.DateEdit = DateTime.Now;
                getBlog.Tag = sysBlog.Tag;
                getBlog.Status = sysBlog.Status;

                _unitOfWork.Repository<SysBlog>().Update(getBlog);
                return PartialView(getBlog);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietBlog(int id)
        {
            try
            {
                var getBlog = _unitOfWork.Repository<SysBlog>().GetById(id);
                if (getBlog == null)
                {
                    return BadRequest();
                }
                ViewBag.getCategories = _unitOfWork.Repository<CatCategory>().GetById((int)getBlog.IdTypeBlog);
                return PartialView(getBlog);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaBlog(int id)
        {
            try
            {
                var getBlog = _unitOfWork.Repository<SysBlog>().GetById(id);
                if (getBlog == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy bài viết" });
                }
                // Xóa ảnh
                if (!string.IsNullOrEmpty(getBlog.ListImg))
                {
                    var listIMG = getBlog.ListImg.Split(',');
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
                _unitOfWork.Repository<SysBlog>().Delete(getBlog);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }

        public IActionResult XoaAnh(string imagePath, int IdBlog)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return Json(new { result = false, message = "Đường dẫn ảnh không hợp lệ." });
                }

                var getBlog = _unitOfWork.Repository<SysBlog>().GetById(IdBlog);
                if (getBlog == null)
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
                    if (!string.IsNullOrEmpty(getBlog.ListImg))
                    {
                        var newList = getBlog.ListImg.Split(',').Where(h => h.Trim() != imagePath.Trim()).ToArray();
                        getBlog.ListImg = string.Join(",", newList);
                        _unitOfWork.Repository<SysBlog>().Update(getBlog);
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
