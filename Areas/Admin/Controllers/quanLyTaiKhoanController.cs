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
    public class quanLyTaiKhoanController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public quanLyTaiKhoanController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }

        #region Danh sách quản trị
        public IActionResult Index()
        {
            var getAccount = Account.GetAccount();
            if (getAccount == null)
            {
                // lỗi chưa đăng nhâp
            }
            return View();
        }

        public IActionResult childIndex(int? page, string searchValue)
        {
            var getAccount = Account.GetAccount();
            if (getAccount == null)
            {
                // lỗi chưa đăng nhâp
            }

            var listUser = _unitOfWork.Repository<SysUser>()
                            .GetAll(includeProperties: "IdRoleNavigation", filter: (m => m.PartnerId == null && m.IdRole == 1),orderBy: h => h.OrderBy(m => m.Name))
                            .ToList();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listUser = listUser.Where(h =>
                    (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper()) ||
                    (h.Username ?? "").ToUpper().Contains(searchValue.ToUpper())
                ).ToList();
            }
            #region ViewBag
            ViewBag.searchValue = searchValue;
            #endregion

            #region Page
            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = listUser.ToPagedList(page ?? 1, pageSize);
            #endregion
            if (pageListView != null)
            {
                return PartialView("childIndex", pageListView);
            }

            return PartialView("childIndex");
        }
        #endregion

        #region Danh sách đối tác
        public IActionResult IndexPartner()
        {
            var getAccount = Account.GetAccount();
            if (getAccount == null)
            {
                // lỗi chưa đăng nhâp
            }
            return View();
        }

        public IActionResult childIndexPartner(int? page, string searchValue)
        {
            var getAccount = Account.GetAccount();
            if (getAccount == null)
            {
                // lỗi chưa đăng nhâp
            }

            var listUser = _unitOfWork.Repository<SysUser>()
                            .GetAll(includeProperties: "IdRoleNavigation", filter: (m => m.IdRole == 3), orderBy: h => h.OrderBy(m => m.Name))
                            .ToList();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listUser = listUser.Where(h =>
                    (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper()) ||
                    (h.Username ?? "").ToUpper().Contains(searchValue.ToUpper())
                ).ToList();
            }
            #region ViewBag
            ViewBag.searchValue = searchValue;
            #endregion

            #region Page
            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = listUser.ToPagedList(page ?? 1, pageSize);
            #endregion
            if (pageListView != null)
            {
                return PartialView("childIndexPartner", pageListView);
            }

            return PartialView("childIndexPartner");
        }
        #endregion

        #region Danh sách khách hàng
        public IActionResult IndexGuest()
        {
            var getAccount = Account.GetAccount();
            if (getAccount == null)
            {
                // lỗi chưa đăng nhâp
            }
            return View();
        }

        public IActionResult childIndexGuest(int? page, string searchValue)
        {
            var getAccount = Account.GetAccount();
            if (getAccount == null)
            {
                // lỗi chưa đăng nhâp
            }

            var listUser = _unitOfWork.Repository<SysGuest>()
                            .GetAll(filter: (m => m.IdRole == 2), orderBy: h => h.OrderBy(m => m.Name))
                            .ToList();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listUser = listUser.Where(h =>
                    (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper()) ||
                    (h.Username ?? "").ToUpper().Contains(searchValue.ToUpper())
                ).ToList();
            }
            #region ViewBag
            ViewBag.searchValue = searchValue;
            #endregion

            #region Page
            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = listUser.ToPagedList(page ?? 1, pageSize);
            #endregion
            if (pageListView != null)
            {
                return PartialView("childIndexGuest", pageListView);
            }

            return PartialView("childIndexGuest");
        }
        #endregion

        public IActionResult themMoiTaiKhoan()
        {
            try
            {

                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    // lỗi chưa đăng nhâp
                }
                if (getAccount.IdRole == 1)
                {
                    ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true && h.IdUserPrent == null).ToList();
                }
                else
                {
                    ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true && h.IdUserPrent == getAccount.Id).ToList();
                }
                SysUser sysUser = new SysUser();
                return PartialView(sysUser);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult themMoiTaiKhoan(SysUser sysUser, int? Status)
        {
            try
            {
                // Mã hóa mật khẩu
                CustomPasswordHasher hasher = new CustomPasswordHasher();
                sysUser.Password = hasher.CreateBase64(sysUser.Password);
                sysUser.Status = true;
                sysUser.Avatar = "/assets/img/no-avatar.png";
                ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true).ToList();
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
                            return ValidationProblem(ModelState);
                        }
                    }
                }
                var getEmail = _unitOfWork.Repository<SysUser>().GetAll(m => m.Email.ToLower() == sysUser.Email.ToLower()).FirstOrDefault();
                if (getEmail != null)
                {
                    string key = "Email";
                    string errorMessage = "Email đã tồn tại trong hệ thống!";
                    ModelState.AddModelError(key, errorMessage);
                    return ValidationProblem(ModelState);
                }

                var getRole = _unitOfWork.Repository<SysRole>().GetAll(filter: (m => m.Id == 3));
                if (getRole == null)
                {
                    string key = "Username";
                    string errorMessage = "Thêm mới thất bại, không tìm thấy quyền partner!";
                    ModelState.AddModelError(key, errorMessage);
                    return ValidationProblem(ModelState);
                }
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    // lỗi chưa đăng nhâp
                }
                if (getAccount.IdRole != 1)
                {
                    sysUser.PartnerId = getAccount.Id;
                }

                _unitOfWork.Repository<SysUser>().Insert(sysUser);

                return PartialView("Index");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IActionResult chinhSuaTaiKhoan(int id)
        {
            try
            {
                var getUser = _unitOfWork.Repository<SysUser>().GetById(id);
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return BadRequest();
                }
                if (getAccount.IdRole == 1)
                {
                    ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true && h.IdUserPrent == null).ToList();
                }
                else
                {
                    ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true && h.IdUserPrent == getAccount.Id).ToList();
                }
                if (getUser == null)
                {
                    return PartialView("Index");
                }
                #region ViewBag
                ViewBag.DSTrangThai = new List<SelectListItem>();
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "true", Text = "Kích hoạt", });
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "false", Text = "Chưa kích hoạt", });
                #endregion

                return PartialView("chinhSuaTaiKhoan", getUser);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult chinhSuaTaiKhoan(SysUser sysUser)
        {
            try
            {
                var getUser = _unitOfWork.Repository<SysUser>().GetById(sysUser.Id);
                ViewBag.DSTrangThai = new List<SelectListItem>();
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "true", Text = "Kích hoạt", });
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "false", Text = "Chưa kích hoạt", });
                ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true).ToList();
                if (getUser == null)
                {
                    string key = "Username";
                    string errorMessage = "Tài khoản đã bị xóa hoặc không tồn tại!!";
                    ModelState.AddModelError(key, errorMessage);
                    return ValidationProblem(ModelState);
                }

                // Kiểm tra trường dữ liệu
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
                            return ValidationProblem(ModelState);
                        }
                    }
                }
                // Mã hóa mật khẩu
                if (getUser.Password != sysUser.Password)
                {
                    CustomPasswordHasher hasher = new CustomPasswordHasher();
                    getUser.Password = hasher.CreateBase64(sysUser.Password);
                }
                getUser.Name = sysUser.Name;
                getUser.Email = sysUser.Email;
                getUser.Username = sysUser.Username;
                getUser.Local = sysUser.Local;
                getUser.Phone = sysUser.Phone;
                getUser.IdRole = sysUser.IdRole;
                getUser.Status = sysUser.Status;
                _unitOfWork.Repository<SysUser>().Update(getUser);
                return PartialView("Index");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietTaiKhoan(int id)
        {
            try
            {
                var getUser = _unitOfWork.Repository<SysUser>().GetById(id);
                if (getUser == null) return BadRequest();
                ViewBag.getRole = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Id == getUser.IdRole).FirstOrDefault();
                return PartialView(getUser);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaTaiKhoan(int id)
        {
            try
            {
                ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true).ToList();
                var getUser = _unitOfWork.Repository<SysUser>().GetById(id);
                if (getUser == null) return BadRequest();
                _unitOfWork.Repository<SysUser>().Delete(getUser);
                return Json(new { result = true, message = "Xóa thành công!" });

            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });

            }
        }

        #region CRUD Khách hàng

        public IActionResult chinhSuaGuest(int id)
        {
            try
            {
                var getUser = _unitOfWork.Repository<SysGuest>().GetById(id);
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return BadRequest();
                }
                if (getAccount.IdRole == 1)
                {
                    ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true && h.IdUserPrent == null).ToList();
                }
                else
                {
                    ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true && h.IdUserPrent == getAccount.Id).ToList();
                }
                if (getUser == null)
                {
                    return PartialView("Index");
                }
                #region ViewBag
                ViewBag.DSTrangThai = new List<SelectListItem>();
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "true", Text = "Kích hoạt", });
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "false", Text = "Chưa kích hoạt", });
                #endregion

                return PartialView(getUser);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult chinhSuaGuest(SysGuest sysUser)
        {
            try
            {
                var getUser = _unitOfWork.Repository<SysGuest>().GetById(sysUser.Id);
                ViewBag.DSTrangThai = new List<SelectListItem>();
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "true", Text = "Kích hoạt", });
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "false", Text = "Chưa kích hoạt", });
                ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true).ToList();
                if (getUser == null)
                {
                    string key = "Username";
                    string errorMessage = "Tài khoản đã bị xóa hoặc không tồn tại!!";
                    ModelState.AddModelError(key, errorMessage);
                    return ValidationProblem(ModelState);
                }

                // Kiểm tra trường dữ liệu
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
                            return ValidationProblem(ModelState);
                        }
                    }
                }
                // Mã hóa mật khẩu
                if (getUser.Password != sysUser.Password)
                {
                    CustomPasswordHasher hasher = new CustomPasswordHasher();
                    getUser.Password = hasher.CreateBase64(sysUser.Password);
                }
                getUser.Name = sysUser.Name;
                getUser.Email = sysUser.Email;
                getUser.Username = sysUser.Username;
                getUser.Local = sysUser.Local;
                getUser.Status = sysUser.Status;
                getUser.Phone = sysUser.Phone;
                getUser.IdRole = sysUser.IdRole;
                _unitOfWork.Repository<SysGuest>().Update(getUser);
                return PartialView("Index");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietGuest(int id)
        {
            try
            {
                var getUser = _unitOfWork.Repository<SysGuest>().GetById(id);
                if (getUser == null) return BadRequest();
                ViewBag.getRole = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Id == getUser.IdRole).FirstOrDefault();
                return PartialView(getUser);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaGuest(int id)
        {
            try
            {
                ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true).ToList();
                var getUser = _unitOfWork.Repository<SysGuest>().GetById(id);
                if (getUser == null) return BadRequest();
                _unitOfWork.Repository<SysGuest>().Delete(getUser);
                return Json(new { result = true, message = "Xóa thành công!" });

            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });

            }
        }
        #endregion
    }
}
