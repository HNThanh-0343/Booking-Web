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
    public class quanlydanhmucController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public quanlydanhmucController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }

        #region Danh mục loại dịch vụ
        public IActionResult Index()
        {
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

                var listCategories = _unitOfWork.Repository<CatCategory>().GetAll();
                if (!string.IsNullOrEmpty(searchValue))
                {
                    listCategories = listCategories.Where(h => (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper())).ToList();
                }

                #region ViewBag
                ViewBag.searchValue = searchValue;
                #endregion

                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 10;
                var pageListView = listCategories.ToPagedList(page ?? 1, pageSize);
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

        public IActionResult themMoiDichVu()
        {
            try
            {
                CatCategory catCategory = new CatCategory();
                return PartialView("themMoiDichVu", catCategory);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult themMoiDichVu(CatCategory catCategory)
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
                    return PartialView("themMoiDichVu", catCategory);
                }

                _unitOfWork.Repository<CatCategory>().Insert(catCategory);
                return PartialView(catCategory);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chinhSuaDichVu(int id)
        {
            try
            {
                var catCategory = _unitOfWork.Repository<CatCategory>().GetById(id);
                if (catCategory == null)
                {
                    return BadRequest();
                }
                return PartialView("chinhSuaDichVu", catCategory);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult chinhSuaDichVu(CatCategory catCategory)
        {
            try
            {
                var getCategories = _unitOfWork.Repository<CatCategory>().GetById(catCategory.Id);
                if (getCategories == null)
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
                    return PartialView("chinhSuaDichVu", catCategory);
                }

                getCategories.Name = catCategory.Name;
                getCategories.Status = catCategory.Status;
                _unitOfWork.Repository<CatCategory>().Update(getCategories);
                return PartialView(getCategories);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IActionResult chiTietDichVu(int id)
        {
            try
            {
                var catCategory = _unitOfWork.Repository<CatCategory>().GetById(id);
                if (catCategory == null)
                {
                    return BadRequest();
                }
                return PartialView(catCategory);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public IActionResult xoaDichVu(int id)
        {
            try
            {
                var catCategory = _unitOfWork.Repository<CatCategory>().GetById(id);
                if (catCategory == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy dịch vụ" });
                }
                _unitOfWork.Repository<CatCategory>().Delete(catCategory);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }
        #endregion

        #region Danh mục tiện nghi của phòng
        public IActionResult IndexTienNghi(int id)
        {
            return View();
        }

        public IActionResult childIndexTienNghi(int? page, string searchValue)
        {
            try
            {
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return BadRequest();
                }
                var listTienNghi = _unitOfWork.Repository<CatAminitieseRoom>().GetAll();
                if (!string.IsNullOrEmpty(searchValue))
                {
                    listTienNghi = listTienNghi.Where(h => (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper())).ToList();
                }
                #region ViewBag
                ViewBag.searchValue = searchValue;
                #endregion

                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 10;
                var pageListView = listTienNghi.ToPagedList(page ?? 1, pageSize);
                if (pageListView != null)
                {
                    return PartialView("childIndexTienNghi", pageListView);
                }
                return PartialView("childIndexTienNghi");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult themMoiTienNghi()
        {
            try
            {
                CatAminitieseRoom catAminitieseRoom = new CatAminitieseRoom();
                return PartialView("themMoiTienNghi", catAminitieseRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult themMoiTienNghi(CatAminitieseRoom catAminitieseRoom)
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
                    return PartialView("themMoiTienNghi", catAminitieseRoom);
                }

                _unitOfWork.Repository<CatAminitieseRoom>().Insert(catAminitieseRoom);
                return PartialView(catAminitieseRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chinhSuaTienNghi(int id)
        {
            try
            {
                var aminitieseRoom = _unitOfWork.Repository<CatAminitieseRoom>().GetById(id);
                if (aminitieseRoom == null)
                {
                    return BadRequest();
                }
                return PartialView("chinhSuaTienNghi", aminitieseRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult chinhSuaTienNghi(CatAminitieseRoom catAminitieseRoom)
        {
            try
            {
                var aminitieseRoom = _unitOfWork.Repository<CatAminitieseRoom>().GetById(catAminitieseRoom.Id);
                if (aminitieseRoom == null)
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
                    return PartialView("chinhSuaDichVu", catAminitieseRoom);
                }

                aminitieseRoom.Name = catAminitieseRoom.Name;
                aminitieseRoom.Icon = catAminitieseRoom.Icon;
                aminitieseRoom.Status = catAminitieseRoom.Status;
                _unitOfWork.Repository<CatAminitieseRoom>().Update(aminitieseRoom);
                return PartialView(aminitieseRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietTienNghi(int id)
        {
            try
            {
                var aminitieseRoom = _unitOfWork.Repository<CatAminitieseRoom>().GetById(id);
                if (aminitieseRoom == null)
                {
                    return BadRequest();
                }
                return PartialView(aminitieseRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaTienNghi(int id)
        {
            try
            {
                var aminitieseRoom = _unitOfWork.Repository<CatAminitieseRoom>().GetById(id);
                if (aminitieseRoom == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy dịch vụ" });
                }
                _unitOfWork.Repository<CatAminitieseRoom>().Delete(aminitieseRoom);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }
        #endregion

        #region Danh mục loại phòng
        public IActionResult IndexLoaiPhong()
        {
            return View();
        }
        public IActionResult childIndexLoaiPhong(int? page, string searchValue)
        {
            try
            {
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return BadRequest();
                }
                var listTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetAll();
                if (!string.IsNullOrEmpty(searchValue))
                {
                    listTypeRoom = listTypeRoom.Where(h => (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper())).ToList();
                }
                #region ViewBag
                ViewBag.searchValue = searchValue;
                #endregion

                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 10;
                var pageListView = listTypeRoom.ToPagedList(page ?? 1, pageSize);
                if (pageListView != null)
                {
                    return PartialView("childIndexLoaiPhong", pageListView);
                }
                return PartialView("childIndexLoaiPhong");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IActionResult themMoiLoaiPhong()
        {
            try
            {
                CatTypeRoom catTypeRoom = new CatTypeRoom();
                return PartialView(catTypeRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult themMoiLoaiPhong(CatTypeRoom catTypeRoom)
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
                    return PartialView("themMoiLoaiPhong", catTypeRoom);
                }

                _unitOfWork.Repository<CatTypeRoom>().Insert(catTypeRoom);
                return PartialView(catTypeRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public IActionResult chinhSuaLoaiPhong(int id)
        {
            try
            {
                var catTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetById(id);
                if (catTypeRoom == null)
                {
                    return PartialView("IndexLoaiPhong");
                }
                return PartialView("chinhSuaLoaiPhong", catTypeRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult chinhSuaLoaiPhong(CatTypeRoom catTypeRoom)
        {
            try
            {
                var getTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetById(catTypeRoom.Id);
                if (getTypeRoom == null)
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
                    return PartialView("chinhSuaDichVu", catTypeRoom);
                }

                getTypeRoom.Name = catTypeRoom.Name;
                getTypeRoom.Icon = catTypeRoom.Icon;
                getTypeRoom.Status = catTypeRoom.Status;
                _unitOfWork.Repository<CatTypeRoom>().Update(getTypeRoom);
                return PartialView(getTypeRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public IActionResult chiTietLoaiPhong(int id)
        {
            try
            {
                var catTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetById(id);
                if (catTypeRoom == null)
                {
                    return BadRequest();
                }
                return PartialView(catTypeRoom);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult xoaLoaiPhong(int id)
        {
            try
            {
                var catTypeRoom = _unitOfWork.Repository<CatTypeRoom>().GetById(id);
                if (catTypeRoom == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy loại phòng" });
                }
                _unitOfWork.Repository<CatTypeRoom>().Delete(catTypeRoom);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }
        #endregion

        #region Danh mục tỉnh thành
        public IActionResult IndexTinhThanh()
        {
            return View();
        }
        public IActionResult childIndexTinhThanh(int? page, string searchValue)
        {
            try
            {
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return BadRequest();
                }
                var listContry = _unitOfWork.Repository<CatContry>().GetAll();
                if (!string.IsNullOrEmpty(searchValue))
                {
                    listContry = listContry.Where(h => (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper())).ToList();
                }
                #region ViewBag
                ViewBag.searchValue = searchValue;
                #endregion

                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 10;
                var pageListView = listContry.ToPagedList(page ?? 1, pageSize);
                if (pageListView != null)
                {
                    return PartialView("childIndexTinhThanh", pageListView);
                }
                return PartialView("childIndexTinhThanh");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IActionResult themMoiTinhThanh()
        {
            try
            {
                CatContry catContry = new CatContry();
                return PartialView("themMoiTinhThanh", catContry);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult themMoiTinhThanh(CatContry catContry)
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
                    return PartialView("themMoiTinhThanh", catContry);
                }

                _unitOfWork.Repository<CatContry>().Insert(catContry);
                return PartialView(catContry);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public IActionResult chinhSuaTinhThanh(int id)
        {
            try
            {
                var catContry = _unitOfWork.Repository<CatContry>().GetById(id);
                if (catContry == null)
                {
                    return BadRequest();
                }
                return PartialView("chinhSuaTinhThanh", catContry);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public IActionResult chinhSuaTinhThanh(CatContry catContry)
        {
            try
            {
                var getContry = _unitOfWork.Repository<CatContry>().GetById(catContry.Id);
                if (getContry == null)
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
                    return PartialView("chinhSuaTinhThanh", catContry);
                }

                getContry.Name = catContry.Name;
                getContry.Status = catContry.Status;
                _unitOfWork.Repository<CatContry>().Update(getContry);
                return PartialView(getContry);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IActionResult chiTietTinhThanh(int id)
        {
            try
            {
                var catContry = _unitOfWork.Repository<CatContry>().GetById(id);
                if (catContry == null)
                {
                    return BadRequest();
                }
                return PartialView(catContry);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IActionResult xoaTinhThanh(int id)
        {
            try
            {
                var catContry = _unitOfWork.Repository<CatContry>().GetById(id);
                if (catContry == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy tỉnh thành" });
                }
                _unitOfWork.Repository<CatContry>().Delete(catContry);
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }
        #endregion
    }
}
