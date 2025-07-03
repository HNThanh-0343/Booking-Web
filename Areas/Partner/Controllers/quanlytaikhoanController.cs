using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WEBSITE_TRAVELBOOKING.Core;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;

namespace WEBSITE_TRAVELBOOKING.Areas.Partner.Controllers
{
    [Area("partner")]
    [Auth]
    public class quanlytaikhoanController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public quanlytaikhoanController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }

        public IActionResult Index()
        {
            ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true).ToList();
            return View();
        }

        public IActionResult childIndex(int? page, string searchValue, int? IdRole)
        {
            var getAccount = Account.GetAccount();
            if (getAccount == null)
            {
                // lỗi chưa đăng nhâp
            }
            var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var listUser = _unitOfWork.Repository<SysUser>()
                            .GetAll(includeProperties: "IdRoleNavigation", filter: (m => m.PartnerId == getAccount.Id))
                            .ToList()
                            .Select(user =>
                            {
                                if (string.IsNullOrEmpty(user.Avatar) ||
                                    !System.IO.File.Exists(Path.Combine(wwwRootPath, user.Avatar.TrimStart('/'))))
                                {
                                    user.Avatar = "/assets/img/no-avatar.png";
                                }
                                return user;
                            })
                            .ToList();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchValue))
            {
                listUser = listUser.Where(h =>
                    (h.Name ?? "").ToUpper().Contains(searchValue.ToUpper()) ||
                    (h.Username ?? "").ToUpper().Contains(searchValue.ToUpper())
                ).ToList();
            }
            if (IdRole != null)
            {
                listUser = listUser.Where(h => h.IdRole == IdRole).ToList();
            }
            #region ViewBag
            ViewBag.searchValue = searchValue;
            ViewBag.IdRole = IdRole;
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
                    TempData["ErrorMessage"] = "Email đã tồn tại trong hệ thống!";
                    string key = "Email";
                    string errorMessage = "Email đã tồn tại trong hệ thống!";
                    ModelState.AddModelError(key, errorMessage);
                    return ValidationProblem(ModelState);
                }

                TempData["SuccessMessage"] = "Thêm mới thành công!";
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
                if (getUser == null)
                {
                    return PartialView("Index");
                }
                #region ViewBag
                ViewBag.DSTrangThai = new List<SelectListItem>();
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "1", Text = "Kích hoạt", });
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "0", Text = "Chưa kích hoạt", });
                #endregion

                return PartialView("chinhSuaTaiKhoan", getUser);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult chinhSuaTaiKhoan([Bind("Id,IdRole,Username,Password,Email,Local,Name")] SysUser sysUser, int? Status)
        {
            try
            {
                var getUser = _unitOfWork.Repository<SysUser>().GetById(sysUser.Id);
                ViewBag.DSTrangThai = new List<SelectListItem>();
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "1", Text = "Kích hoạt", });
                ViewBag.DSTrangThai.Add(new SelectListItem() { Value = "0", Text = "Chưa kích hoạt", });
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
                getUser.CardName = sysUser.CardName;
                getUser.CardNumber = sysUser.CardNumber;
                getUser.IdRole = sysUser.IdRole;
                getUser.Status = Status == 1 ? true : false;
                _unitOfWork.Repository<SysUser>().Update(getUser);

                TempData["SuccessMessage"] = "Chỉnh sửa thành công!";
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
                if (getUser == null) return PartialView("Index");
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
                if (getUser == null) return PartialView("Index");
                _unitOfWork.Repository<SysUser>().Delete(getUser);
                TempData["SuccessMessage"] = "Xóa thành công!";
                //return PartialView("childIndex");
                return Json(new { result = true, message = "Xóa thành công!" });

            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });

            }
        }
    }
}
